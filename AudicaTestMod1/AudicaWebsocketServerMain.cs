using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine; // Input is in here, and Input.GetKeyDown requires UnhollowerBaseLib to be referenced as well. 
using HarmonyLib;
using WebSocketSharp.Server;

namespace AudicaWebsocketServer
{
    public class AudicaWebsocketServerMain : MelonMod
    {
        public static SongList.SongData selectedSongData;

        internal static WebSocketServer wssv;
        internal static AudicaGameStateManager AudicaGameState { get; set; }
        internal static AudicaTargetStateManager AudicaTargetState { get; set; }
        
        internal static Encoder encoder;

        private long lastProgressUpdate;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            AudicaWebsocketServerMain.AudicaGameState = new AudicaGameStateManager();
            AudicaWebsocketServerMain.AudicaTargetState = new AudicaTargetStateManager();

            encoder = new Encoder();

            MelonLogger.Msg("Starting websocket server on port 8085.  Access using ws://localhost:8085/AudicaStats.");
            // Startup websocket server here.
            wssv = new WebSocketServer(8085);
            wssv.AddWebSocketService<AudicaStats>("/AudicaStats");
            wssv.Start();

            lastProgressUpdate = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
        public override void OnUpdate()
        {
            List<string> changedStates = AudicaWebsocketServerMain.AudicaGameState.Update();

            if (changedStates.Contains("SongInfo"))
            {
                if (AudicaWebsocketServerMain.AudicaGameState.SongState.songInfo.songId != "")
                {
                    wssv.WebSocketServices.Broadcast(encoder.SongSelected(AudicaWebsocketServerMain.AudicaGameState.SongState));
                }
            }

            if (changedStates.Contains("SongPlayerStatus"))
            {
                wssv.WebSocketServices.Broadcast(encoder.SongPlayerStatus(AudicaWebsocketServerMain.AudicaGameState.SongState));
            }


            // Every 1s, if song is playing, report progress.
            var currentDateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (currentDateTime > lastProgressUpdate && AudicaWebsocketServerMain.AudicaGameState.getSongPlaying())
            {
                wssv.WebSocketServices.Broadcast(encoder.SongProgress(AudicaWebsocketServerMain.AudicaGameState.GameState, AudicaWebsocketServerMain.AudicaGameState.SongState));
                lastProgressUpdate = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            wssv.Stop();
        }

        [HarmonyPatch(typeof(SongSelectItem), "OnSelect", new Type[0])]
        public static class SongSelectPatch
        {
            public static void Postfix(SongSelectItem __instance)
            {
                AudicaWebsocketServerMain.selectedSongData = __instance.mSongData;
                // NOTE: I tried doing AudicaGameState.SongStart() to get all the song deets, but apparently not all song info is
                //       available at this point, especially where song length is concerned.  Needs to happen when the song actually starts.
            }
        }

        [HarmonyPatch(typeof(ScoreKeeper), "Start", new Type[0])]
        public static class StartSongPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("Song Start");
                AudicaWebsocketServerMain.AudicaTargetState.SongStart();
                AudicaWebsocketServerMain.AudicaGameState.SongStart(AudicaWebsocketServerMain.selectedSongData);
            }
        }
        

        [HarmonyPatch(typeof(InGameUI), "Restart", new Type[0])]
        public static class RestartSongPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("Restart Song");
                AudicaWebsocketServerMain.AudicaGameState.SongRestart();
                wssv.WebSocketServices.Broadcast(encoder.SongRestart());
                // FIXME: Emit restart so connected clients know to update stats
            }
        }
        
        [HarmonyPatch(typeof(InGameUI), "ReturnToSongList", new Type[0])]
        public static class EndSongPatch
        {
            public static void Postfix()
            {
                AudicaWebsocketServerMain.AudicaGameState.SongEnd();
                MelonLogger.Msg("Return to Song List");
                wssv.WebSocketServices.Broadcast(encoder.ReturnToSongList());
            }
        }


        // target event handling
        [HarmonyPatch(typeof(GameplayStats), "ReportTargetHit", new Type[] { typeof(SongCues.Cue), typeof(float), typeof(Vector2) })]
        public static class TargetHitPatch
        {
            public static void Postfix(ref GameplayStats __instance, ref SongCues.Cue cue, ref Vector2 targetHitPos)
            {
                MelonLogger.Msg("Target Hit! " + targetHitPos.ToString());
                AudicaTargetHitState targetHit = AudicaWebsocketServerMain.AudicaTargetState.TargetHit(__instance, cue, targetHitPos);
                wssv.WebSocketServices.Broadcast(AudicaWebsocketServerMain.encoder.TargetHitEvent(targetHit, AudicaWebsocketServerMain.AudicaGameState.SongState));
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportShotNothing", new Type[0])]
        public static class ShotNothingPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("Shot nothing!");
                AudicaTargetFailState targetMiss = AudicaWebsocketServerMain.AudicaTargetState.TargetMiss();
                wssv.WebSocketServices.Broadcast(AudicaWebsocketServerMain.encoder.TargetMiss(targetMiss));
                // TODO: feed output into JSON parser then to HTTP server as websocket event
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportTargetAimMiss", new Type[] { typeof(SongCues.Cue), typeof(Vector2) })]
        public static class TargetAimMissPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("Target Miss (aim)!");
                AudicaTargetFailState targetMiss = AudicaWebsocketServerMain.AudicaTargetState.TargetMissAim();
                wssv.WebSocketServices.Broadcast(AudicaWebsocketServerMain.encoder.TargetMiss(targetMiss));
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportTargetEarlyLate", new Type[] { typeof(SongCues.Cue), typeof(float) })]
        public static class TargetEarlyLatePatch
        {
            public static void Postfix(SongCues.Cue cue, float tick)
            {
                MelonLogger.Msg("Target Miss (timing)!");
                // NOTE: seems tick doesn't exist when this is called - getting instance doesn't exist issue
                // for now, just ignoring the tick.
                AudicaTargetFailState targetMiss = AudicaWebsocketServerMain.AudicaTargetState.TargetMissEarlyLate();
                wssv.WebSocketServices.Broadcast(AudicaWebsocketServerMain.encoder.TargetMiss(targetMiss));
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportMisfire", new Type[0])]
        public static class GunMisfirePatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("Misfire!");
                
                // TODO (event with hand that misfired?)
            }
        }

    }

    public class AudicaStats : WebSocketBehavior
    {
        // TODO: Have something that can respond to requests - ex: Get current song, etc

    }

    
}
