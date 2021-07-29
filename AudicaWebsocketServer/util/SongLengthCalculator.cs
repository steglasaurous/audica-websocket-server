using System.Collections.Generic;

namespace AudicaWebsocketServer.util {

    /**
     * Calculates arbitrary time periods within a song. THIS CLASS SHOULD BE RECONSTRUCTED ON SONG START!
     * Note that on certain maps (you know who you are) calculating the lengths can be extremely expensive. 
     * Use with caution and cache where appropriate.
     * 
     * Class originally written by jukibom as part of the audica-http-status mod
     * https://github.com/jukibom/audica-http-status
     */
    class SongLengthCalculator {

        public static SongDataHolder songDataHolder;
        public static SongCues songCues;
        public static ScoreKeeperDisplay scoreKeeperDisplay;

        private SongList.SongData song;
        private List<ChunkCache> chunkCache = new List<ChunkCache>();
        private float songLengthMs;
        
        public SongLengthCalculator() {
            SongLengthCalculator.songDataHolder = UnityEngine.Object.FindObjectOfType<SongDataHolder>();
            SongLengthCalculator.songCues = UnityEngine.Object.FindObjectOfType<SongCues>();
            SongLengthCalculator.scoreKeeperDisplay = UnityEngine.Object.FindObjectOfType<ScoreKeeperDisplay>();

            this.Precalc();
        }

        public float SongLengthMilliseconds {
            get { return this.songLengthMs; }
        }

        public float GetSongPositionMilliseconds(float tick) {
            float accumulator = 0;
            
            if (tick < 0)
            {
                return 0;
            }

            // find the chunk, adding up ms along the way
            foreach (ChunkCache chunkCache in this.chunkCache) {
                // once we find the chunk, return % proportion through that chunk and break out
                if (tick > chunkCache.startTick && tick < chunkCache.endTick) {
                    accumulator += chunkCache.lengthMs * (
                        // take the period position of the tick ...
                        (tick - chunkCache.startTick) / 
                        // divide it by the total period to get a factor of the length
                        (chunkCache.endTick - chunkCache.startTick)
                    );
                    break;
                }
                else {
                    // include all previous chunks
                    accumulator += chunkCache.lengthMs;
                }
            }
            
            return accumulator;
        }

        private void Precalc() {
            MelonLoader.MelonLogger.Msg("Entering Precalc");

            this.song = SongDataHolder.I.songData;
            MelonLoader.MelonLogger.Msg("Got SongDataHolder.I.songData");

            UnhollowerBaseLib.Il2CppReferenceArray<SongCues.Cue> cues = AudicaGameStateManager.songCues.mCues.cues;
            MelonLoader.MelonLogger.Msg("Got cues");

            SongCues.Cue endCue = cues[cues.Length - 1];
            MelonLoader.MelonLogger.Msg("Got endCue");

            float endTick = endCue.tick + endCue.tickLength;
            this.songLengthMs = 0;

            for (int i = 0; i < song.tempos.Length; i++) {

                float startChunkTick = song.tempos[i].tick;
                float endChunkTick = endTick;

                // if it's NOT the last tempo change, grab the tick from the next one instead.
                if (i != song.tempos.Length - 1) {
                    endChunkTick = song.tempos[i + 1].tick;
                }
                
                // complete chunk length in ticks
                float chunkTickLength = endChunkTick - startChunkTick;

                // ms accumulator
                float chunkMilliseconds = GetTicksMillisconds(chunkTickLength, song.tempos[i].tempo);
                this.songLengthMs += chunkMilliseconds;
                
                // cache
                ChunkCache chunk = new ChunkCache();
                chunk.startTick = startChunkTick;
                chunk.endTick = endChunkTick;
                chunk.lengthMs = chunkMilliseconds;
                this.chunkCache.Add(chunk);
            }
        }

        private float GetTicksMillisconds(float ticks, float tempo) {
            return ticks * this.GetTickMilliseconds(tempo);
        }

        private float GetTickMilliseconds(float tempo) {
            return 60000 / (tempo * 480);
        }
    }

    struct ChunkCache {
        public float startTick;
        public float endTick;
        public float lengthMs;
    }
}
