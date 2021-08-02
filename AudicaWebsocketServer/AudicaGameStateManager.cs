using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AudicaWebsocketServer.util;

namespace AudicaWebsocketServer {

	class AudicaGameStateManager {

		// Audica game classes
		public static ScoreKeeper scoreKeeper;
        public static GameplayModifiers modifiers;
        public static PlayerPreferences prefs;
        public static KataConfig config;
        public static SongCues songCues;
        public static GameplayStats gameplayStats;

		// State containers
		private AudicaGameState gameState;
        private AudicaSongState songState;

        // Util
        private SongLengthCalculator songCalculator;

        // FIXME: Might make sense to make this flag an indicator that it should be reading progress regularly, and add something
        //        to the SongProgress struct to indicate if the song is playing vs paused or finished.
        private bool songPlaying = false;
        public SongList.SongData songData { get; set; } 

        public AudicaGameStateManager() {
            this.initialiseStateManagers();
            this.clearGameState();
            this.clearSongState();
        }

		public AudicaGameState GameState {
			get {
				return this.gameState;
			}
		}

		public AudicaSongState SongState {
			get {
				return this.songState;
			}
		}

        // FIXME: Should this be like the above with a getter thing? 
        public bool getSongPlaying()
        {
            return this.songPlaying;
        }

        // Called every tick, don't do anything too heavy in here!
		public List<string> Update() {
            this.pollGameState();
            return this.pollSongState();
		}

		public void SongStart(SongList.SongData song) {
            this.initialiseStateManagers();
            this.songCalculator = new SongLengthCalculator();
            this.songData = song;
            this.songPlaying = true;
        }

        public void SongRestart() {
            this.initialiseStateManagers();
            this.clearSongState();
		}

		public void SongEnd() {
            this.songPlaying = false;
            this.clearSongState();
        }

        private void initialiseStateManagers() {
            AudicaGameStateManager.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
            AudicaGameStateManager.modifiers = UnityEngine.Object.FindObjectOfType<GameplayModifiers>();
            AudicaGameStateManager.prefs = UnityEngine.Object.FindObjectOfType<PlayerPreferences>();
            AudicaGameStateManager.config = UnityEngine.Object.FindObjectOfType<KataConfig>();
            AudicaGameStateManager.songCues = UnityEngine.Object.FindObjectOfType<SongCues>();
        }

		private void clearGameState() {
			this.gameState = new AudicaGameState();
        }

        private void clearSongState() {
            this.songState = new AudicaSongState();

            this.songState.songInfo = new AudicaSongInfo();
            this.songState.songPlayerStatus = new AudicaSongPlayerStatus();
            this.songState.songProgress = new AudicaSongProgress();
            this.songState.gameStats = new GameStats();

            // this is potentially read all the time but only updates if a song is playing so we should initialise everything to actual sane values ...
            this.songState.songInfo.songId = "";
            this.songState.songInfo.songName = "";
            this.songState.songInfo.songArtist = "";
            this.songState.songInfo.songAuthor = "";
            this.songState.songInfo.difficulty = "";
            this.songState.songInfo.classification = "";
            this.songState.songInfo.songLength = TimeSpan.FromSeconds(0).ToString();
            this.songState.songProgress.timeElapsed = TimeSpan.FromSeconds(0).ToString();
            this.songState.songProgress.timeRemaining = TimeSpan.FromSeconds(0).ToString();
            this.songState.songProgress.progress = 0;
            this.songState.songProgress.currentTick = 0;
            this.songState.songInfo.ticksTotal = 0;
            this.songState.songPlayerStatus.songSpeed = 1;
            this.songState.songPlayerStatus.health = 1;
            this.songState.songPlayerStatus.score = 0;
            this.songState.songPlayerStatus.scoreMultiplier = 1;
            this.songState.songPlayerStatus.streak = 0;
            this.songState.songPlayerStatus.highScore = 0;
            this.songState.songPlayerStatus.isNoFailMode = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.NoFail.mVal : false;
            this.songState.songPlayerStatus.isPracticeMode = AudicaGameStateManager.config ? AudicaGameStateManager.config.practiceMode : false;
            this.songState.songPlayerStatus.isFullComboSoFar = true;
            this.songState.songPlayerStatus.modifiers = AudicaGameStateManager.modifiers ? AudicaGameStateManager.modifiers.GetCurrentModifiers()
                .Select((GameplayModifiers.Modifier mod) => GameplayModifiers.GetModifierString(mod))
                .ToList<string>()
                : new List<string>();
            this.songState.gameStats.aimMissCount = 0;
            this.songState.gameStats.chainBreakCount = 0;
            this.songState.gameStats.earlyLateCount = 0;
            this.songState.gameStats.misfireCount = 0;
            this.songState.gameStats.shotMeleeCount = 0;
            this.songState.gameStats.shotNothingCount = 0;
            this.songState.gameStats.successCount = 0;
            this.songState.gameStats.sustainBreakCount = 0;
            this.songState.gameStats.wrongHandCount = 0;
            this.songState.gameStats.wrongOrientationCount = 0;
        }

        private void pollGameState() {
            this.gameState.leftColor = AudicaGameStateManager.prefs ? ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorLeft.mVal) : "#000000";
            this.gameState.rightColor = AudicaGameStateManager.prefs ? ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorRight.mVal) : "#000000";
            this.gameState.targetSpeed = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.TargetSpeedMultiplier.mVal : 1;
            this.gameState.meleeSpeed = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.MeleeSpeedMultiplier.mVal : 1;
            this.gameState.aimAssist = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.AimAssistAmount.mVal : 1;
        }

        private List<string> pollSongState() {
            var changedStates = new List<string>();

            if (this.songPlaying) {
                string songClass = "custom";
                if (this.songData.IsCoreSong()) {
                    songClass = "ost";
                }
                if (this.songData.dlc) {
                    songClass = "dlc";
                }
                if (this.songData.extrasSong) {
                    songClass = "extras";
                }

                // We don't want to calculate the ticks to the end of the song, it keeps playing!
                // Instead get the last target (plus its length) as the end ticks
                UnhollowerBaseLib.Il2CppReferenceArray<SongCues.Cue> cues = AudicaGameStateManager.songCues.mCues.cues;
                SongCues.Cue endCue = cues[cues.Length - 1];
                float songEndTicks = endCue.tick + endCue.tickLength;

                float currentTick = AudicaGameStateManager.scoreKeeper.mLastTick;

                float totalTimeMs = this.songCalculator.SongLengthMilliseconds;
                float currentTimeMs = this.songCalculator.GetSongPositionMilliseconds(currentTick);
                float remainingTimeMs = totalTimeMs - currentTimeMs;

                AudicaSongInfo newSongInfo = new AudicaSongInfo();
                newSongInfo.songId = this.songData.songID;
                newSongInfo.songName = this.songData.title;
                newSongInfo.songArtist = this.songData.artist;
                newSongInfo.songAuthor = this.songData.author;
                newSongInfo.difficulty = KataConfig.GetDifficultyName(AudicaGameStateManager.config.GetDifficulty());
                newSongInfo.classification = songClass;
                newSongInfo.songLength = TimeSpan.FromMilliseconds(Convert.ToInt64(totalTimeMs)).ToString(@"m\:ss");
                newSongInfo.ticksTotal = songEndTicks;

                AudicaSongProgress newSongProgress = new AudicaSongProgress();
                newSongProgress.timeElapsed = TimeSpan.FromMilliseconds(Convert.ToInt64(currentTimeMs)).ToString(@"m\:ss");
                newSongProgress.timeRemaining = TimeSpan.FromMilliseconds(Convert.ToInt64(remainingTimeMs)).ToString(@"m\:ss");
                newSongProgress.progress = currentTimeMs / totalTimeMs;
                newSongProgress.currentTick = currentTick;

                // Changes as player progresses
                AudicaSongPlayerStatus newSongPlayerStatus = new AudicaSongPlayerStatus();
                newSongPlayerStatus.health = (float)Math.Round(AudicaGameStateManager.scoreKeeper.GetHealth(), 2);
                newSongPlayerStatus.score = AudicaGameStateManager.scoreKeeper.mScore;
                newSongPlayerStatus.scoreMultiplier = AudicaGameStateManager.scoreKeeper.GetRawMultiplier();
                newSongPlayerStatus.streak = AudicaGameStateManager.scoreKeeper.GetStreak();
                newSongPlayerStatus.highScore = AudicaGameStateManager.scoreKeeper.GetHighScore();
                newSongPlayerStatus.isFullComboSoFar = AudicaGameStateManager.scoreKeeper.GetIsFullComboSoFar();
                newSongPlayerStatus.isNoFailMode = AudicaGameStateManager.prefs.NoFail.mVal;
                newSongPlayerStatus.isPracticeMode = AudicaGameStateManager.config.practiceMode;

                // Changes if twitch modifiers happen (or in-game stuff)
                newSongPlayerStatus.songSpeed = KataConfig.GetCueDartSpeedMultiplier();      // TODO: not a clue what this value actually is but it's not the speed multiplier!
                newSongPlayerStatus.modifiers = AudicaGameStateManager.modifiers.GetCurrentModifiers() // Confirm? Maybe I'm wrong this can alter while in a song?
                    .Select((GameplayModifiers.Modifier mod) => GameplayModifiers.GetModifierString(mod))
                    .ToList<string>();

                // FIXME: Consider whether to resurrect the gameStats stuff below.  Could be useful for cases where websocket disconnects/reconnects mid-song and needs
                //        context of current state.
                //AudicaGameStateManager.gameplayStats = UnityEngine.Object.FindObjectOfType<GameplayStats>();
                //if (AudicaGameStateManager.gameplayStats)
                //{
                //    MelonLoader.MelonLogger.Msg("Got gameplayStats");
                //    GameStats newGameStats = new GameStats();
                //    newGameStats.misfireCount = gameplayStats.mMisfireCount;
                //    newGameStats.earlyLateCount = gameplayStats.mEarlyLateCount;
                //    newGameStats.aimMissCount = gameplayStats.mAimMissCount;
                //    newGameStats.chainBreakCount = gameplayStats.mChainBreakCount;
                //    newGameStats.shotMeleeCount = gameplayStats.mShotMeleeCount;
                //    newGameStats.shotNothingCount = gameplayStats.mShotNothingCount;
                //    newGameStats.successCount = gameplayStats.mSuccessCount;
                //    newGameStats.sustainBreakCount = gameplayStats.mSustainBreakCount;
                //    newGameStats.wrongHandCount = gameplayStats.mWrongHandCount;
                //    newGameStats.wrongOrientationCount = gameplayStats.mWrongOrientationCount;
                //    if (!this.SongState.gameStats.Equals(newGameStats))
                //    {
                //        changedStates.Add("GameStats");
                //    }
                //    this.songState.gameStats = newGameStats;
                //}

                // As we update the songState overall, determine which pieces changed, and let the caller know
                // what changed so websocket events can be emitted.
                // FIXME: Is this indeed the most efficient means of doing this?  Or should we look at a pub/sub / Observer pattern here?
                if (!this.songState.songInfo.Equals(newSongInfo))
                {
                    changedStates.Add("SongInfo");
                }

                if (!this.songState.songPlayerStatus.Equals(newSongPlayerStatus))
                {
                    changedStates.Add("SongPlayerStatus");
                }


                this.songState.songInfo = newSongInfo;
                this.songState.songPlayerStatus = newSongPlayerStatus;
                this.songState.songProgress = newSongProgress;
                
            }

            return changedStates;
        }
    }
}
