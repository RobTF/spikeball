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
    using Engine;
    using Engine.Resources;
    using Engine.Services;
    using System;

    /// <summary>
    /// Class which implements base functionality for resource loaders.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class ResourceManager : IDisposable
    {
        protected readonly IGameEngine _gameEngine;
        protected readonly IResourceService _resourceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        public ResourceManager(IGameEngine gameEngine)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            _gameEngine = gameEngine;
            _resourceService = _gameEngine.GetService<IResourceService>();
            _resourceService.ResourceLoaded += OnGameResourceLoaded;
            _resourceService.ResourcesUnloaded += OnGameResourcesUnloaded;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        ~ResourceManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _resourceService.ResourceLoaded -= OnGameResourceLoaded;
                _resourceService.ResourcesUnloaded -= OnGameResourcesUnloaded;
            }
        }

        /// <summary>
        /// Called when a game resource is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Platformer.Engine.Resources.ResourceLoadEventArgs" /> instance containing the event data.</param>
        protected virtual void OnGameResourceLoaded(object sender, ResourceLoadEventArgs e)
        {

        }

        /// <summary>
        /// Called when all game resources are unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnGameResourcesUnloaded(object sender, EventArgs e)
        {

        }
    }
}
