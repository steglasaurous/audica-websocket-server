using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
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

        // For moderating how often to send SongProgress events.  This is a unix timestamp,
        // but could be changed from seconds to milliseconds if more frequent song progress updates
        // are desired.
        // Consider loading this value from a config file?  With some sensible limits so game/websocket performance
        // isn't adversely affected.
        private long lastProgressUpdate;

        // For use by other mods - allows others to emit a websocket event.
        public static void EmitWebsocketEvent(EventContainer eventContainer)
        {
            wssv.WebSocketServices.Broadcast(Newtonsoft.Json.JsonConvert.SerializeObject(eventContainer));
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            AudicaWebsocketServerMain.AudicaGameState = new AudicaGameStateManager();
            AudicaWebsocketServerMain.AudicaTargetState = new AudicaTargetStateManager();

            encoder = new Encoder();

            // FIXME: Make this configurable via config file
            MelonLogger.Msg("Starting websocket server on port 8085.  Access using ws://localhost:8085/AudicaStats.");
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
            // FIXME: Add hook for when pause screen is shown so progress isn't emitted whiled paused.
            // FIXME: Add hook for when song is failed to stop emitting progress
            // FIXME: Add hook for when song is finished to stop emitting progress.
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
                AudicaWebsocketServerMain.AudicaTargetState.SongStart();
                AudicaWebsocketServerMain.AudicaGameState.SongStart(AudicaWebsocketServerMain.selectedSongData);
            }
        }


        [HarmonyPatch(typeof(InGameUI), "Restart", new Type[0])]
        public static class RestartSongPatch
        {
            public static void Postfix()
            {
                AudicaWebsocketServerMain.AudicaGameState.SongRestart();
                wssv.WebSocketServices.Broadcast(encoder.SongRestart());
            }
        }

        [HarmonyPatch(typeof(InGameUI), "ReturnToSongList", new Type[0])]
        public static class EndSongPatch
        {
            public static void Postfix()
            {
                AudicaWebsocketServerMain.AudicaGameState.SongEnd();
                wssv.WebSocketServices.Broadcast(encoder.ReturnToSongList());
            }
        }


        // target event handling
        [HarmonyPatch(typeof(GameplayStats), "ReportTargetHit", new Type[] { typeof(SongCues.Cue), typeof(float), typeof(Vector2) })]
        public static class TargetHitPatch
        {
            public static void Postfix(ref GameplayStats __instance, ref SongCues.Cue cue, ref Vector2 targetHitPos)
            {
                AudicaTargetHitState? targetHit = AudicaWebsocketServerMain.AudicaTargetState.TargetHit(__instance, cue, targetHitPos);
                if (targetHit != null)
                {
                    wssv.WebSocketServices.Broadcast(AudicaWebsocketServerMain.encoder.TargetHitEvent((AudicaTargetHitState)targetHit, AudicaWebsocketServerMain.AudicaGameState.SongState));
                }
            }
        }

        [HarmonyPatch(typeof(ScoreKeeper), "OnFailure", new Type[] { typeof(SongCues.Cue), typeof(bool), typeof(bool) })]
        private static class PatchScoreKeeperOnFailure
        {
            private static void Postfix(ScoreKeeper __instance, ref SongCues.Cue cue)
            {
                if (cue is null)
                {
                    // Ignore if there's nothing there.
                    return;
                }
                
                AudicaTargetFailState? targetMiss = AudicaWebsocketServerMain.AudicaTargetState.TargetMiss(cue);
                if (targetMiss != null)
                {
                    wssv.WebSocketServices.Broadcast(encoder.TargetMiss((AudicaTargetFailState)targetMiss));
                }
            }
        }
    }

    public class AudicaStats : WebSocketBehavior
    {
        // FIXME: Have something that can respond to requests - ex: Get current song, etc
        // FIXME: On connect, emit current song data, progress and player stats so new clients are up-to-date.
    }
}
