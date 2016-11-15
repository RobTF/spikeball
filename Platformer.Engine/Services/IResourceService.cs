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
    using Resources;
    using System;

    /// <summary>
    /// Interface implemented by the resource service.
    /// </summary>
    public interface IResourceService : IGameService
    {
        /// <summary>
        /// Event which is raised when a resource is loaded by the game engine.
        /// </summary>
        event EventHandler<ResourceLoadEventArgs> ResourceLoaded;

        /// <summary>
        /// Event which is raised when all resources are unloaded.
        /// </summary>
        event EventHandler ResourcesUnloaded;

        /// <summary>
        /// Sets the relative path to a type of resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="path">A relative path.</param>
        void SetResourcePath<TResource>(string path);

        /// <summary>
        /// Gets the relative path to a type of resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <returns>A relative path.</returns>
        string GetResourcePath<TResource>();

        /// <summary>
        /// Preloads a resource.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        void PreloadResource<TResource>(string resourceName) where TResource : GameResource;

        /// <summary>
        /// Loads the resource data.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        TResource LoadResource<TResource>(string resourceName) where TResource : GameResource;

        /// <summary>
        /// Gets a loaded resource by identifier.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>The resource.</returns>
        TResource GetResourceById<TResource>(int resourceId) where TResource : GameResource;

        /// <summary>
        /// Performs a load on any resources in the preload queue.
        /// </summary>
        void LoadPreloadedResources();

        /// <summary>
        /// Unloads/frees all game resources.
        /// </summary>
        void UnloadAllResources();
    }
}
