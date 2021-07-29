using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

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
		public string songLength; // TODO: Confirm this is an int
		public int highScore; // TODO: Confirm this is an int
    }

	//JSONObject targetHitPosition = new JSONObject();
	//targetHitPosition["x"] = targetHit.targetHitPosition.x;
	//		targetHitPosition["y"] = targetHit.targetHitPosition.y;

	//		JSONObject eventJSON = new JSONObject();
	//eventJSON["event"] = "target-hit";
	//		eventJSON["targetIndex"] = targetHit.targetIndex;
	//		eventJSON["targetType"] = targetHit.type;
	//		eventJSON["hand"] = targetHit.hand;
	//		eventJSON["score"] = targetHit.score;
	//		eventJSON["timingScore"] = targetHit.timingScore;
	//		eventJSON["aimScore"] = targetHit.aimScore;
	//		eventJSON["tick"] = targetHit.tick;
	//		eventJSON["targetHitPosition"] = targetHitPosition;

	//		return eventJSON.ToString();
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
			//songSelectedEvent.songId = songState.songId;
			//songSelectedEvent.songArtist = songState.songArtist;
			//songSelectedEvent.songAuthor = songState.songAuthor;
			//songSelectedEvent.difficulty = songState.difficulty;
			//songSelectedEvent.classification = songState.classification;
			//songSelectedEvent.songLength = songState.songLength;
			//songSelectedEvent.highScore = songState.highScore;

			EventContainer container = new EventContainer();
			container.eventType = "SongSelected";
			container.data = songState.songInfo;

			return Newtonsoft.Json.JsonConvert.SerializeObject(container);
        }

		public string SongProgress(AudicaGameState gameState, AudicaSongState songState)
        {
			//var songProgressEvent = new SongProgressEvent();
			//songProgressEvent.currentTick = songState.currentTick;
			//songProgressEvent.health = songState.health;
			//songProgressEvent.progress = songState.progress;
			//songProgressEvent.score = songState.score;
			//songProgressEvent.scoreMultiplier = songState.scoreMultiplier;
			//songProgressEvent.songLength = songState.songLength;
			//songProgressEvent.songSpeed = songState.songSpeed;
			//songProgressEvent.streak = songState.streak;
			//songProgressEvent.timeElapsed = songState.timeElapsed;
			//songProgressEvent.timeRemaining = songState.timeRemaining;

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

		//public string Status(AudicaGameState gameState, AudicaSongState songState) {
		//	JSONObject gameStatus = new JSONObject();
		//	JSONObject gameStateJSON = new JSONObject();
		//	JSONObject songStateJSON = new JSONObject();

		//	JSONArray modifiers = new JSONArray();
		//	songState.modifiers.ForEach((string modifier) => {
		//		modifiers.Add(modifier);
		//	});

		//	gameStateJSON["leftColor"] = gameState.leftColor;
		//	gameStateJSON["rightColor"] = gameState.rightColor;
		//	gameStateJSON["targetSpeed"] = gameState.targetSpeed;
		//	gameStateJSON["meleeSpeed"] = gameState.meleeSpeed;
		//	gameStateJSON["aimAssist"] = gameState.aimAssist;
  //          // TODO: timing assist value?

  //          songStateJSON["songId"] = songState.songId;
		//	songStateJSON["songName"] = songState.songName;
		//	songStateJSON["songArtist"] = songState.songArtist;
		//	songStateJSON["songAuthor"] = songState.songAuthor;
		//	songStateJSON["difficulty"] = songState.difficulty;
		//	songStateJSON["classification"] = songState.classification;
		//	songStateJSON["songLength"] = songState.songLength;
		//	songStateJSON["timeElapsed"] = songState.timeElapsed;
		//	songStateJSON["timeRemaining"] = songState.timeRemaining;
		//	songStateJSON["progress"] = songState.progress;
		//	songStateJSON["currentTick"] = songState.currentTick;
		//	songStateJSON["totalTicks"] = songState.ticksTotal;
		//	songStateJSON["songSpeed"] = songState.songSpeed;
		//	songStateJSON["health"] = songState.health;
		//	songStateJSON["score"] = songState.score;
		//	songStateJSON["scoreMultiplier"] = songState.scoreMultiplier;
		//	songStateJSON["streak"] = songState.streak;
		//	songStateJSON["highScore"] = songState.highScore;
		//	songStateJSON["isNoFailMode"] = songState.isNoFailMode;
		//	songStateJSON["isPracticeMode"] = songState.isPracticeMode;
		//	songStateJSON["isFullComboSoFar"] = songState.isFullComboSoFar;
		//	songStateJSON["modifiers"] = modifiers;

		//	gameStatus["gameSettings"] = gameStateJSON;
		//	gameStatus["songStatus"] = songStateJSON;

		//	return gameStatus.ToString();
		//}

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
