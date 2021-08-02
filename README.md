# Audica Websocket Server

Exposes a websocket server that emits events about game data.  Useful for stream overlays or processing game data externally from the game itself.  

Note this, like many mods, is a work-in-progress.  There may be bugs :)  Please do file issues in the Github issues tab of this repo if you find them.

## Usage

Copy the DLL file into the Mods folder in your Audica installation and start!  

The Websocket server will be available at `ws://localhost:8085/AudicaStats`.

## Websocket Events

At the moment, events are only emitted from the game.  There aren't any on-demand requests (yet).  The following are the events that
are emitted from the server.

Note that all events follow this object structure:

```json
{
    "eventType": "SomeEventName",
    "data": {}
}
```

The structure of `data` is dependent on the particular event type. 

### SongStart

Emitted when the player starts playing the song.  This also triggers `SongProgress` to start emitting at 1 second intervals.  

```json
{
    "eventType": "SongSelected",
    "data": {
        "songId": "EnvelopeVIP-Continuum_a436d6bf85e13804eae44e072e87c387",
        "songName": "Envelope VIP",
        "songArtist": "TheFatRat",
        "songAuthor": "Continuum",
        "difficulty": "Expert",
        "classification": "extras",
        "songLength": "2:51",
        "ticksTotal": 130400.0
    }
}
```

### SongRestart

```json
{
    "eventType": "SongRestart",
    "data": ""
}
```

### SongProgress

When song playing starts, this is emitted once per second.  Emission stops when the "ReturnToSongList" event is emitted.

```json
{
    "eventType": "SongProgress",
    "data": {
        "progress": 0.0133128827,
        "timeElapsed": "0:02",
        "timeRemaining": "2:49",
        "currentTick": 1736.0
    }
}
```

Notes

* `progress` is a number between 0 and 1, indicating the percentage of the song that's complete.
* `currentTick` is the internal song "tick" position.

### SongPlayerStatus

```json
{
    "eventType": "SongPlayerStatus",
    "data": {
        "health": 1.0,
        "score": 0,
        "scoreMultiplier": 1,
        "streak": 0,
        "highScore": 3434752,
        "isFullComboSoFar": true,
        "isNoFailMode": false,
        "isPracticeMode": false,
        "songSpeed": 1.0,
        "modifiers": []
    }
}
```

Notes

* `health` is a number between 0 and 1 indicating the percentage of health the player has
* This event is emitted whenever any of the data in this object has changed.

### TargetHit

```json
{
    "eventType": "TargetHit",
    "data": {
        "targetIndex": 0,
        "type": "ChainStart",
        "hand": "Left",
        "score": -1.0,
        "timingScore": -1.0,
        "aimScore": 0.0,
        "tick": 6720.0,
        "targetHitPosition": "(-0.2, -0.2)"
    }
}
```

Notes
* `type` can be one of "Melee", "Standard", "Sustain", "Vertical", "Horizontal", "ChainStart", "Chain", "Bomb"
* `hand` can be one of "Left", "Right", "Either", or "None"
* `score`, `timingScore`, `aimScore` and `targetHitPosition` are a bit suspect right now (i.e. I'm not sure what exactly they mean :D), so consider them experimental.


### TargetMiss

```json
{
    "eventType": "TargetMiss",
    "data": {
        "targetIndex": 17,
        "type": "Standard",
        "hand": "Left",
        "reason": "Miss"
    }
}
```

Notes
* See `type` and `hand` in TargetHit notes for the different values these properties can be
* `reason` can be one of "Miss", "ChainBreak", "SustainBreak"


### ReturnToSongList

Emitted when the user hits the "Return to Song List" button.  Note this also stops `SongProgress` from being emitted, since the player has left the song they're in.

```json
{
    "eventType": "ReturnToSongList",
    "data": ""
}
```
