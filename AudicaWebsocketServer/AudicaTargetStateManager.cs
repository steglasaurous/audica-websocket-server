
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
            targetHit.type = cue.behavior.ToString();
            targetHit.hand = cue.handType.ToString();
            targetHit.timingScore = gameplayStats.GetLastTimingScore();
            targetHit.aimScore = gameplayStats.GetLastAimScore();
            targetHit.score = targetHit.timingScore + targetHit.aimScore;       // TODO: may need to multiply by combo? Need to test
            targetHit.tick = cue.tick;
            targetHit.targetHitPosition = targetHitPos.ToString();
            return targetHit;
        }

        public AudicaTargetFailState TargetMiss(SongCues.Cue cue) {
            AudicaTargetFailState targetMiss = new AudicaTargetFailState();

            targetMiss.targetIndex = cue.index;
            targetMiss.type = cue.behavior.ToString();
            targetMiss.hand = cue.handType.ToString();

            if (cue.behavior == Target.TargetBehavior.Chain)
            {
                targetMiss.reason = "ChainBreak";
            }
            else if (cue.behavior == Target.TargetBehavior.Hold && cue.target.mSustainFailed)
            {
                targetMiss.reason = "SustainBreak";
            }
            else
            {
                targetMiss.reason = "Miss";
            }
            
            return targetMiss;
        }

        private void initialiseStateManagers() {
            AudicaTargetStateManager.targetTracker = UnityEngine.Object.FindObjectOfType<TargetTracker>();
        }

        // FIXME: Not sure we need to do this translation?  ToString() seems to convert these values nicely.
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
