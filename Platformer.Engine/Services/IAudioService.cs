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
namespace Platformer.Engine.Services
{
    using Audio;
    using System;

    /// <summary>
    /// Interface implemented by the audio service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.IGameService" />
    public interface IAudioService : IGameService
    {
        /// <summary>
        /// Occurs when a sound effect is played back.
        /// </summary>
        event EventHandler<SoundEffectPlaybackEventArgs> SoundEffectPlayback;

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        void PlaySoundEffect(int resourceId);

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="channel">The specific audio channel to use.</param>
        void PlaySoundEffect(int resourceId, int channel);
    }
}
