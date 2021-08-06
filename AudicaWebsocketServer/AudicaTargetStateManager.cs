
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AudicaWebsocketServer {
    class AudicaTargetStateManager {
        // FIXME: Right now this isn't used anywhere in this class.  Delete it? 
        public static TargetTracker targetTracker;

        public static Dictionary<int, AudicaTargetHitState> targetHits = new Dictionary<int, AudicaTargetHitState>();
        public static Dictionary<int, AudicaTargetFailState> targetMisses = new Dictionary<int, AudicaTargetFailState>();
        
        public AudicaTargetStateManager() { }

        // Must be called when a song starts in order to fetch correct targets 
        public void SongStart() {
            this.initialiseStateManagers();
            targetHits.Clear();
            targetMisses.Clear();
        }

        public AudicaTargetHitState? TargetHit(GameplayStats gameplayStats, SongCues.Cue cue, Vector2 targetHitPos) {
            // Check to see if we processed this already or not
            if (AudicaTargetStateManager.targetHits.ContainsKey(cue.index))
            {
                return null;
            }
            AudicaTargetHitState targetHit = new AudicaTargetHitState();
            targetHit.targetIndex = cue.index;
            targetHit.type = cue.behavior.ToString();
            targetHit.hand = cue.handType.ToString();
            targetHit.timingScore = gameplayStats.GetLastTimingScore();
            targetHit.aimScore = gameplayStats.GetLastAimScore();
            targetHit.score = targetHit.timingScore + targetHit.aimScore;       // TODO: may need to multiply by combo? Need to test
            targetHit.tick = cue.tick;
            targetHit.targetHitPosition = targetHitPos.ToString();

            AudicaTargetStateManager.targetHits.Add(cue.index, targetHit);

            return targetHit;
        }

        public AudicaTargetFailState? TargetMiss(SongCues.Cue cue) {
            if (AudicaTargetStateManager.targetMisses.ContainsKey(cue.index))
            {
                return null;
            }
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

            AudicaTargetStateManager.targetMisses.Add(cue.index, targetMiss);

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
