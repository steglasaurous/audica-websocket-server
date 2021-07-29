using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public float ticksTotal;

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

    //struct AudicaSongState {

    //    public string songId;
    //    public string songName;
    //    public string songArtist;
    //    public string songAuthor;
    //    public string difficulty;       // "beginner" | "standard" | "advanced" | "expert"
    //    public string classification;   // "ost" | "dlc" | "extra" | "custom"
    //    public string songLength;       // UTC
    //    public string timeElapsed;      // UTC
    //    public string timeRemaining;    // UTC
    //    public float progress;          // 0-1, 0 = start, 1 = end
    //    public float currentTick;       // Hmx.Audio.MidiPlayCursor.GetCurrentTick
    //    public float ticksTotal;
    //    public float songSpeed;         // 1 = 100%
    //    public float health;
    //    public int score;
    //    public int scoreMultiplier;
    //    public int streak;
    //    public int highScore;
    //    public bool isNoFailMode;
    //    public bool isPracticeMode;
    //    public bool isFullComboSoFar;
    //    public List<string> modifiers;
    //}


    struct AudicaSongState {
        public AudicaSongInfo songInfo;
        public AudicaSongProgress songProgress;
        public AudicaSongPlayerStatus songPlayerStatus;
    }

    struct AudicaTargetHitState {
        public int targetIndex;
        public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
        public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
        public float score;
        public float timingScore;
        public float aimScore;
        public float tick;
        //public UnityEngine.Vector2 targetHitPosition;
        public string targetHitPosition;
    }

    struct AudicaTargetFailState {
        public int targetIndex;
        public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
        public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
        public string reason;       // "miss" | "aim" | "early" | "late"
    }
}