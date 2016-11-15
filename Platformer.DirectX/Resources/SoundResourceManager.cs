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
namespace Platformer.DirectX.Resources
{
    using Audio;
    using Engine;
    using Engine.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Un4seen.Bass;

    /// <summary>
    /// Class which manages sound loading.
    /// </summary>
    public class SoundResourceManager : ResourceManager
    {
        private readonly Dictionary<int, int> _sounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteResourceManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        public SoundResourceManager(IGameEngine gameEngine)
            : base(gameEngine)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            _sounds = new Dictionary<int, int>();
        }

        /// <summary>
        /// Gets the BASS handle to a sound resource.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>The identifier of the associated sample, or zero if not found.</returns>
        public int GetSoundResourceHandle(int resourceId)
        {
            int retval;
            if (_sounds.TryGetValue(resourceId, out retval))
            {
                return retval;
            }
            return 0;
        }

        /// <summary>
        /// Called when a game resource is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Platformer.Engine.Resources.ResourceLoadEventArgs" /> instance containing the event data.</param>
        protected override void OnGameResourceLoaded(Object sender, ResourceLoadEventArgs e)
        {
            if (e.ResourceType == typeof(Sound))
            {
                var sound = e.LoadedResource as Sound;
                var path = Path.Combine(_resourceService.GetResourcePath<Sound>(), sound.Name + ".wav");

                var sample = Bass.BASS_SampleLoad(path, 0L, 0, AudioPlaybackManager.NumChannels, BASSFlag.BASS_DEFAULT);
                if (sample != 0)
                {
                    _sounds.Add(e.LoadedResource.ResourceId, sample);
                }
            }
        }

        /// <summary>
        /// Called when all game resources are unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void OnGameResourcesUnloaded(Object sender, EventArgs e)
        {
            ClearResources();
            base.OnGameResourcesUnloaded(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                ClearResources();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears the resources.
        /// </summary>
        private void ClearResources()
        {
            foreach (var handle in _sounds.Values)
            {
                var channels = Bass.BASS_SampleGetChannels(handle);
                foreach (var channel in channels)
                {
                    Bass.BASS_ChannelStop(channel);
                }
            }

            _sounds.Clear();
        }
    }
}
