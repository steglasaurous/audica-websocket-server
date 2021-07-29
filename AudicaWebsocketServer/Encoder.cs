using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudicaWebsocketServer
{
	struct EventContainer
    {
		public string eventType;
		public Object data;
    }
	struct SongSelectedEvent
    {
		public string songId;
		public string songArtist;
		public string songAuthor;
		public string difficulty;
		public string classification;
		public string songLength; 
		public int highScore;
    }

	struct TargetHitPosition
    {
		public float x;
		public float y;
    }
	struct TargetHitEvent
    {
		public float targetIndex;
		public string targetType;
		public string hand;
		public float score;
		public float timingScore;
		public float aimScore;
		public float tick;
		public TargetHitPosition targetHitPosition;
		public float scoreMultiplier;
		public float health;
		public float streak;

    }
	
	struct SongProgressEvent
    {
		public string songLength;
		public string timeElapsed;
		public string timeRemaining;
		public float progress;
		public float currentTick;
		public float totalTicks;
		public float songSpeed;
		public float health;
		public float score;
		public float scoreMultiplier;
		public float streak;
    }

	class Encoder {

		public string SongSelected(AudicaSongState songState)
        {
			EventContainer container = new EventContainer();
			container.eventType = "SongSelected";
			container.data = songState.songInfo;

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
        }

		public string SongProgress(AudicaGameState gameState, AudicaSongState songState)
        {
			EventContainer container = new EventContainer();
			container.eventType = "SongProgress";
			container.data = songState.songProgress;

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}

		public string SongPlayerStatus(AudicaSongState songState)
        {
			EventContainer container = new EventContainer();
			container.eventType = "SongPlayerStatus";
			container.data = songState.songPlayerStatus;

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}

		public string SongRestart()
        {
			EventContainer container = new EventContainer();
			container.eventType = "SongRestart";
			container.data = "";
			
			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}

		public string ReturnToSongList()
        {
			EventContainer container = new EventContainer();
			container.eventType = "ReturnToSongList";
			container.data = "";

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}

		public string TargetHitEvent(AudicaTargetHitState targetHit, AudicaSongState songState) {
			EventContainer container = new EventContainer();
			container.eventType = "TargetHit";
			container.data = targetHit;
			
			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}
		public string TargetMiss(AudicaTargetFailState targetFail)
        {
			EventContainer container = new EventContainer();
			container.eventType = "TargetMiss";
			container.data = targetFail;

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
		}
	}
}
