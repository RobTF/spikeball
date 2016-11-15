/*
 *  Copyright (c) 2016 Rob Harwood <rob@codemlia.com>
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 *  and associated documentation files (the "Software"), to deal in the Software without
 *  restriction, including without limitation the rights to use, copy, modify, merge, publish,
 *  distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all copies or
 *  substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 *  BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 *  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 *  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
namespace Platformer.DirectX.Audio
{
    using Engine;
    using Engine.Audio;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Un4seen.Bass;

    /// <summary>
    /// Class which controls audio playback.
    /// </summary>
    public class AudioPlaybackManager : IDisposable
    {
        public const int NumChannels = 5;

        private static readonly TraceSource TraceSource = new TraceSource("Platformer");

        private readonly IGameEngine _gameEngine;
        private readonly SoundResourceManager _soundResMan;
        private readonly AudioChannel[] _channels;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlaybackManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        /// <param name="soundResMan">The sound resource manager.</param>
        public AudioPlaybackManager(IGameEngine gameEngine, SoundResourceManager soundResMan)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            if (soundResMan == null)
            {
                throw new ArgumentNullException(nameof(soundResMan));
            }

            _gameEngine = gameEngine;
            _soundResMan = soundResMan;
            _channels = new AudioChannel[NumChannels];

            for (var i = 0; i < NumChannels; i++)
            {
                _channels[i] = new AudioChannel();
            }

            _gameEngine.GetService<Engine.Services.IAudioService>().SoundEffectPlayback += OnGameSoundPlayback;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AudioPlaybackManager"/> class.
        /// </summary>
        ~AudioPlaybackManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameEngine.GetService<Engine.Services.IAudioService>().SoundEffectPlayback -= OnGameSoundPlayback;
            }
        }

        /// <summary>
        /// Called when a sound effect is played in the game.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Engine.Audio.SoundEffectPlaybackEventArgs"/> instance containing the event data.</param>
        private void OnGameSoundPlayback(object sender, SoundEffectPlaybackEventArgs e)
        {
            int handle = _soundResMan.GetSoundResourceHandle(e.ResourceId);
            if (handle == 0)
            {
                return;
            }

            if (e.Channel < 0 || e.Channel >= _channels.Length)
            {
                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Attempted to play back sample on invalid channel {e.Channel}.");
            }
            else
            {
                var channel = _channels[e.Channel];
                channel.PlaySound(handle);
            }
        }

        /// <summary>
        /// Class which represents a single audio channel.
        /// </summary>
        private class AudioChannel
        {
            private readonly object _lock;
            private readonly Dictionary<int, int> _sampleChannels;

            /// <summary>
            /// Initializes a new instance of the <see cref="AudioChannel"/> class.
            /// </summary>
            public AudioChannel()
            {
                _lock = new object();
                _sampleChannels = new Dictionary<int, int>();
            }

            /// <summary>
            /// Plays a sound.
            /// </summary>
            public void PlaySound(int sampleId)
            {
                lock (_lock)
                {
                    int channelId;
                    if (!_sampleChannels.TryGetValue(sampleId, out channelId))
                    {
                        channelId = Bass.BASS_SampleGetChannel(sampleId, true);
                        _sampleChannels.Add(sampleId, channelId);
                    }

                    Bass.BASS_ChannelPlay(channelId, true);
                }
            }
        }
    }
}
