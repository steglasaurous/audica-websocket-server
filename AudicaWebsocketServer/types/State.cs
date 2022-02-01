using System;
using System.Collections.Generic;

namespace AudicaWebsocketServer
{
    struct AudicaGameState {
        public string leftColor;        // hex value
        public string rightColor;       // hex value
        public float targetSpeed;       // 1 = 100%
        public float meleeSpeed;        // 1 = 100%
        public float aimAssist;         // 1 = 100%
    }

    struct AudicaSongProgress : IEquatable<AudicaSongProgress>
    {
        public float progress;
        public string timeElapsed;
        public string timeRemaining;
        public float timeElapsedSeconds;
        public float timeRemainingSeconds;
        public float currentTick;

        public bool Equals(AudicaSongProgress other)
        {
            return this.progress == other.progress &&
                this.timeElapsed == other.timeElapsed &&
                this.timeRemaining == other.timeRemaining &&
                this.currentTick == other.currentTick;
        }
    }

    struct AudicaSongPlayerStatus : IEquatable<AudicaSongPlayerStatus>
    {
        public float health;
        public int score;
        public int scoreMultiplier;
        public int streak;
        public int highScore;
        public bool isFullComboSoFar;
        public bool isNoFailMode;
        public bool isPracticeMode;
        public float songSpeed;         // 1 = 100%
        public List<string> modifiers;

        public bool Equals(AudicaSongPlayerStatus other)
        {
            return this.health == other.health &&
                this.score == other.score &&
                this.scoreMultiplier == other.scoreMultiplier &&
                this.streak == other.streak &&
                this.highScore == other.highScore &&
                this.isFullComboSoFar == other.isFullComboSoFar &&
                this.isNoFailMode == other.isNoFailMode &&
                this.isPracticeMode == other.isPracticeMode &&
                this.songSpeed == other.songSpeed;
            // FIXME: Should compare against modifiers as well, but need to figure out how comparing lists work.

        }
    }

    struct AudicaSongInfo : IEquatable<AudicaSongInfo>
    {
        public string songId;
        public string songName;
        public string songArtist;
        public string songAuthor;
        public string difficulty;       // "beginner" | "standard" | "advanced" | "expert"
        public string classification;   // "ost" | "dlc" | "extra" | "custom"
        public string songLength;       // UTC
        public float songLengthSeconds;
        public float ticksTotal;
        public string albumArtData;

        public bool Equals(AudicaSongInfo other)
        {
            return this.songId == other.songId &&
                this.songName == other.songName &&
                this.songArtist == other.songArtist &&
                this.songAuthor == other.songAuthor &&
                this.difficulty == other.difficulty &&
                this.classification == other.classification &&
                this.songLength == other.songLength &&
                this.ticksTotal == other.ticksTotal;
        }
    }

    struct GameStats : IEquatable<GameStats> {
        public int shotNothingCount;
        public int wrongHandCount;
        public int wrongOrientationCount;
        public int shotMeleeCount;
        public int chainBreakCount;
        public int sustainBreakCount;
        public int aimMissCount;
        public int earlyLateCount;
        public int misfireCount;
        public int successCount;

        public bool Equals(GameStats other)
        {
            return this.shotNothingCount == other.shotMeleeCount &&
                this.wrongHandCount == other.wrongHandCount &&
                this.wrongOrientationCount == other.wrongOrientationCount &&
                this.shotMeleeCount == other.shotMeleeCount &&
                this.chainBreakCount == other.chainBreakCount &&
                this.sustainBreakCount == other.sustainBreakCount &&
                this.aimMissCount == other.aimMissCount &&
                this.earlyLateCount == other.earlyLateCount &&
                this.misfireCount == other.misfireCount &&
                this.successCount == other.successCount;
        }
    }



    struct AudicaSongState {
        public AudicaSongInfo songInfo;
        public AudicaSongProgress songProgress;
        public AudicaSongPlayerStatus songPlayerStatus;
        
        // NOTE: Not implemented right now, maybe consider later?
        public GameStats gameStats;
    }

    struct AudicaTargetHitState {
        public int targetIndex;
        public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
        public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
        public float score;
        public float timingScore;
        public float aimScore;
        public float tick;
        // Storing a string representation, as the JSON converter runs into circular references if trying to encode the Vector2 directly.
        public string targetHitPosition;
        public float zOffset;
    }

    struct AudicaTargetFailState {
        public int targetIndex;
        public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
        public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
        public string reason;       // "miss" | "aim" | "early" | "late"
        public float zOffset;
    }
}