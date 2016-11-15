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
    using Microsoft.Practices.Unity;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class which implements the resource service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IResourceService" />
    public class ResourceService : GameService, IResourceService
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        private readonly Dictionary<string, HashSet<string>> _resourcePreloadList;
        private readonly Dictionary<string, Dictionary<string, GameResource>> _resources;
        private readonly Dictionary<string, string> _resourcePaths;
        private readonly Dictionary<int, GameResource> _resourcesById;

        private readonly IGameEngine _engine;
        private readonly IUnityContainer _container;

        private readonly object _resourceLock;

        private bool _preloadComplete;
        private int _nextResourceId;

        /// <summary>
        /// Event which is raised when a resource is loaded by the game engine.
        /// </summary>
        public event EventHandler<ResourceLoadEventArgs> ResourceLoaded;

        /// <summary>
        /// Event which is raised when all resources are unloaded.
        /// </summary>
        public event EventHandler ResourcesUnloaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceService" /> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="container">The container.</param>
        public ResourceService(IGameEngine engine, IUnityContainer container)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            _nextResourceId = 1;
            _preloadComplete = false;
            _engine = engine;
            _container = container;
            _resourceLock = new object();
            _resourcePreloadList = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            _resources = new Dictionary<string, Dictionary<string, GameResource>>(StringComparer.OrdinalIgnoreCase);
            _resourcePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _resourcesById = new Dictionary<int, GameResource>();
        }

        /// <summary>
        /// Sets the relative path to a type of resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="path">A relative path.</param>
        public void SetResourcePath<TResource>(string path)
        {
            var resourceTypeName = typeof(TResource).Name;
            _resourcePaths[resourceTypeName] = path;
        }

        /// <summary>
        /// Gets the relative path to a type of resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <returns>A relative path.</returns>
        public string GetResourcePath<TResource>()
        {
            var resourceTypeName = typeof(TResource).Name;

            string retval;
            _resourcePaths.TryGetValue(resourceTypeName, out retval);
            return retval;
        }

        /// <summary>
        /// Preloads a resource.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public void PreloadResource<TResource>(string resourceName)
            where TResource : GameResource
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return;
            }

            lock (_resourceLock)
            {
                var resourceTypeName = typeof(TResource).FullName;

                if (_preloadComplete)
                {
                    // can't preload as it's already done; just have to load whatever it is now
                    // TraceSource.TraceEvent(TraceEventType.Warning, 0, Strings.ResourceLatePreload, resourceTypeName, resourceName);
                    LoadResource<TResource>(resourceName);
                    return;
                }

                // get the preload list for this type
                HashSet<string> preloadList;

                if (!_resourcePreloadList.TryGetValue(resourceTypeName, out preloadList))
                {
                    preloadList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _resourcePreloadList.Add(resourceTypeName, preloadList);
                }

                TraceSource.TraceEvent(TraceEventType.Verbose, 0, Strings.AddingResourceToPreloadQueue, resourceTypeName, resourceName);

                // add the resource to the list
                if (typeof(TResource) == typeof(Sprite))
                {
                    preloadList.Add(resourceName);
                }
            }
        }

        /// <summary>
        /// Loads the resource data.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public TResource LoadResource<TResource>(string resourceName)
            where TResource : GameResource
        {
            return LoadResource(typeof(TResource), resourceName) as TResource;
        }

        /// <summary>
        /// Loads the resource data.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public GameResource LoadResource(Type resourceType, string resourceName)
        {
            GameResource retval = null;

            if (string.IsNullOrEmpty(resourceName))
            {
                return retval;
            }

            lock (_resourceLock)
            {
                var resourceTypeName = resourceType.FullName;

                // get the resource list
                Dictionary<string, GameResource> typeResources;
                if (!_resources.TryGetValue(resourceTypeName, out typeResources))
                {
                    typeResources = new Dictionary<string, GameResource>(StringComparer.OrdinalIgnoreCase);
                    _resources.Add(resourceTypeName, typeResources);
                }

                if (!typeResources.TryGetValue(resourceName, out retval))
                {
                    var args = new ResourceLoadEventArgs();
                    args.ResourceName = resourceName;
                    args.ResourceType = resourceType;

                    TraceSource.TraceEvent(TraceEventType.Verbose, 0, Strings.LoadingResource, resourceTypeName, resourceName);

                    if (resourceType == typeof(Sprite))
                    {
                        // load the sprite
                        retval = LoadSprite(resourceName);
                    }
                    else if(resourceType == typeof(Map))
                    {
                        var mapService = _engine.GetService<IMapService>();
                        retval = mapService.LoadMap(Path.Combine(GetResourcePath<Map>(), resourceName + ".tmx"));
                    }
                    else if(resourceType == typeof(Sound))
                    {
                        var sound = new Sound() { Name = resourceName };
                        retval = sound;
                    }

                    args.LoadedResource = retval;
                    retval.ResourceId = _nextResourceId++;

                    typeResources.Add(resourceName, retval);
                    _resourcesById.Add(retval.ResourceId, retval);

                    // the engine has loaded what it needs, but the renderer might want to load image files etc.
                    ResourceLoaded?.Invoke(this, args);
                }
            }

            return retval;
        }

        /// <summary>
        /// Gets a loaded resource by identifier.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>The resource.</returns>
        public TResource GetResourceById<TResource>(int resourceId)
            where TResource : GameResource
        {
            GameResource retval;
            if(!_resourcesById.TryGetValue(resourceId, out retval))
            {
                return null;
            }
            else
            {
                return retval as TResource;
            }
        }

        /// <summary>
        /// Unloads/frees all game resources.
        /// </summary>
        public void UnloadAllResources()
        {
            lock (_resourceLock)
            {
                _resourcePreloadList.Clear();
                _resources.Clear();
                _resourcesById.Clear();
                _preloadComplete = false;
                _nextResourceId = 1;
                ResourcesUnloaded?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Performs a load on any resources in the preload queue.
        /// </summary>
        public void LoadPreloadedResources()
        {
            lock (_resourceLock)
            {
                // stop any more preloading
                _preloadComplete = true;

                // enumerate each type of resource we have a preload list for
                foreach (var resourceType in _resourcePreloadList.Keys)
                {
                    var resourceCount = _resourcePreloadList[resourceType].Count;
                    TraceSource.TraceEvent(
                        TraceEventType.Verbose,
                        0,
                        Strings.PreloadingResources,
                        resourceCount,
                        resourceCount != 1 ? "s" : string.Empty,
                        resourceType);

                    // enumerate each resource
                    foreach (var resourceName in _resourcePreloadList[resourceType])
                    {
                        // perform the load
                        var type = Type.GetType(resourceType);
                        LoadResource(type, resourceName);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a sprite.
        /// </summary>
        /// <param name="spriteName">Name of the sprite.</param>
        /// <returns>A new <see cref="Sprite"/> instance.</returns>
        private Sprite LoadSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                throw new ArgumentException(Strings.ResourceNameRequired, nameof(spriteName));
            }

            // load the sprite definition file
            var filePath = Path.Combine(GetResourcePath<Sprite>(), spriteName + ".txt");
            var spriteData = File.ReadAllLines(filePath);
            
            var sequences = LoadAnimationSequences(spriteName);

            // create a new sprite instance
            var sprite = new Sprite();
            sprite.Name = spriteName;

            var animationFrames = new Dictionary<string, List<SpriteAnimationFrame>>();
            List<SpriteAnimationFrame> currentFrameList;

            // move through each of the frame definitions, and create the animations
            var i = 1;
            foreach (var line in spriteData)
            {
                var tokens = line.Split(' ');

                if (tokens.Length != 6)
                {
                    TraceSource.TraceEvent(TraceEventType.Warning, 0, Strings.SpriteDefinitionBadTokenCount, filePath, i);
                    continue;
                }

                var splitName = tokens[0].Split('_');
                var animationName = splitName[1];
                var animationFrame = Int32.Parse(splitName[2]);

                var rect = new Int32Rect(
                    Int32.Parse(tokens[2]),
                    Int32.Parse(tokens[3]),
                    Int32.Parse(tokens[4]),
                    Int32.Parse(tokens[5]));

                if (!animationFrames.TryGetValue(animationName, out currentFrameList))
                {
                    currentFrameList = new List<SpriteAnimationFrame>();
                    animationFrames.Add(animationName, currentFrameList);
                }

                currentFrameList.Add(new SpriteAnimationFrame(animationFrame, rect));

                i++;
            }

            // write the animations to the sprite
            var animations = new List<SpriteAnimation>();
            var animId = 0;
            foreach (var animation in animationFrames.Keys)
            {
                var anim = new SpriteAnimation
                {
                    Name = animation,
                    Id = animId++,
                    Frames = animationFrames[animation].OrderBy(f => f.FrameNumber).ToArray(),
                    Sequence = sequences[animation]
                };

                animations.Add(anim);
            }

            sprite.Animations = animations.ToArray();
            return sprite;
        }

        /// <summary>
        /// Loads the animation frame sequences for a sprite.
        /// </summary>
        /// <param name="spriteName">Name of the sprite.</param>
        /// <returns>A dictionary containing the animation maps.</returns>
        private Dictionary<string, int[]> LoadAnimationSequences(string spriteName)
        {
            if(string.IsNullOrWhiteSpace(spriteName))
            {
                throw new ArgumentException(Strings.SpriteNameRequired, nameof(spriteName));
            }

            var filePath = Path.Combine(GetResourcePath<Sprite>(), spriteName + ".anm.txt");
            var data = File.ReadAllLines(filePath);

            var retval = new Dictionary<string, int[]>();

            foreach(var line in data)
            {
                var tokens = line.Split('=');
                if(tokens.Length != 2)
                {
                    continue;
                }

                var name = tokens[0].Trim();
                var frames = tokens[1].Trim().Split(',');
                retval.Add(name, frames.Select(f => int.Parse(f)).ToArray());
            }

            return retval;
        }
    }
}
