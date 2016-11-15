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
namespace Platformer.Engine
{
    using System;
    using Services;

    /// <summary>
    /// Interface implemented by the main game engine.
    /// </summary>
    public interface IGameEngine
    {
        /// <summary>
        /// Gets a value indicating whether the game is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the currently loaded map.
        /// </summary>
        Map CurrentMap { get; }

        /// <summary>
        /// Gets the amount of time the game has been running, in seconds.
        /// </summary>
        double GlobalTime { get; }

        /// <summary>
        /// Gets the amount of time between the last two frames.
        /// </summary>
        double DeltaTime { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the game engine is paused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if paused; otherwise, <c>false</c>.
        /// </value>
        bool Paused { get; set; }

        /// <summary>
        /// Gets a game service.
        /// </summary>
        /// <typeparam name="TService">The type of the service to obtain.</typeparam>
        /// <returns>An instance of a game service.</returns>
        TService GetService<TService>() where TService : IGameService;

        /// <summary>
        /// Loads and starts a game level.
        /// </summary>
        /// <param name="levelPath">The level path.</param>
        void StartLevel(string levelPath);

        /// <summary>
        /// Called each frame to run the game logic.
        /// </summary>
        /// <param name="singleStep">If true, does not perform any FPS rate control and simply increments time by a single predefined time step.</param>
        void Step(bool singleStep);

        /// <summary>
        /// Stops the level.
        /// </summary>
        void StopLevel();
    }
}
