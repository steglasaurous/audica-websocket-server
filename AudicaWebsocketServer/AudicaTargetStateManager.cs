
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AudicaWebsocketServer {
    class AudicaTargetStateManager {
        public static TargetTracker targetTracker;

        public AudicaTargetStateManager() { }

        // Must be called when a song starts in order to fetch correct targets 
        public void SongStart() {
            this.initialiseStateManagers();
        }

        public AudicaTargetHitState TargetHit(GameplayStats gameplayStats, SongCues.Cue cue, Vector2 targetHitPos) {
            AudicaTargetHitState targetHit = new AudicaTargetHitState();

            targetHit.targetIndex = cue.index;
            targetHit.type = this.cueToTargetType(cue);
            targetHit.hand = this.cueToHand(cue);
            targetHit.timingScore = gameplayStats.GetLastTimingScore();
            targetHit.aimScore = gameplayStats.GetLastAimScore();
            targetHit.score = targetHit.timingScore + targetHit.aimScore;       // TODO: may need to multiply by combo? Need to test
            targetHit.tick = cue.tick;
            targetHit.targetHitPosition = targetHitPos.ToString();

            return targetHit;
        }

        public AudicaTargetFailState TargetMiss() {
            AudicaTargetFailState targetMiss = new AudicaTargetFailState();
            SongCues.Cue cue = AudicaTargetStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

            targetMiss.targetIndex = cue.index;
            targetMiss.type = this.cueToTargetType(cue);
            targetMiss.hand = this.cueToHand(cue);
            targetMiss.reason = "miss";
            return targetMiss;
        }

        public AudicaTargetFailState TargetMissAim() {
            AudicaTargetFailState targetMiss = new AudicaTargetFailState();
            SongCues.Cue cue = AudicaTargetStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

            targetMiss.targetIndex = cue.index;
            targetMiss.type = this.cueToTargetType(cue);
            targetMiss.hand = this.cueToHand(cue);
            targetMiss.reason = "aim";
            return targetMiss;
        }

        public AudicaTargetFailState TargetMissEarlyLate() {
            // FIXME: Find out how I might get more target info so early/late can be determined
            
            AudicaTargetFailState targetMiss = new AudicaTargetFailState();

            // FIXME: Seems to be blowing up here too complaining the instance doesn't exist
            SongCues.Cue cue = AudicaTargetStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

            targetMiss.targetIndex = cue.index;
            targetMiss.type = this.cueToTargetType(cue);
            targetMiss.hand = this.cueToHand(cue);
            //targetMiss.reason = tick < cue.tick ? "early" : "late";
            targetMiss.reason = "early";

            return targetMiss;
        }

        private void initialiseStateManagers() {
            AudicaTargetStateManager.targetTracker = UnityEngine.Object.FindObjectOfType<TargetTracker>();
        }

        private string cueToTargetType(SongCues.Cue cue) {
            string type = "";
            switch (cue.behavior) {
                case Target.TargetBehavior.Melee: type = "melee"; break;
                case Target.TargetBehavior.Standard: type = "standard"; break;
                case Target.TargetBehavior.Hold: type = "sustain"; break;
                case Target.TargetBehavior.Vertical: type = "vertical"; break;
                case Target.TargetBehavior.Horizontal: type = "horizontal"; break;
                case Target.TargetBehavior.ChainStart: type = "chain-start"; break;
                case Target.TargetBehavior.Chain: type = "chain"; break;
                case Target.TargetBehavior.Dodge: type = "bomb"; break;
            }
            return type;
        }

        private string cueToHand(SongCues.Cue cue) {
            string hand = "";
            switch (cue.handType) {
                case Target.TargetHandType.Left: hand = "left"; break;
                case Target.TargetHandType.Right: hand = "right"; break;
                case Target.TargetHandType.Either: hand = "either"; break;
                case Target.TargetHandType.None: hand = "none"; break;
            }
            return hand;
        }
    }
}
