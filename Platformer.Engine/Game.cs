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
    using System.Collections.Generic;
    using System.Diagnostics;
    using Entities;
    using Microsoft.Practices.Unity;
    using Services;

    /// <summary>
    /// Class which implements the main game engine.
    /// </summary>
    /// <seealso cref="Platformer.Engine.IGameEngine" />
    public class Game : IGameEngine
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        private const double IdealDeltaTime = 1.0 / 60.0;

        private readonly Stopwatch _gameTimer;

        private IUnityContainer _container;
        private bool _isRunning;
        private bool _paused;
        private bool _resetTime;

        private Map _currentMap;

        private double _accumulator;
        private double _lastTime;
        private double _globalTimeSecs;
        private double _deltaTimeSecs;
        private IGameService[] _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        public Game()
        {
            _container = new UnityContainer();
            RegisterServices();

            _isRunning = false;
            _gameTimer = new Stopwatch();
        }

        /// <summary>
        /// Gets a value indicating whether the game is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets the currently loaded map.
        /// </summary>
        public Map CurrentMap => _currentMap;

        /// <summary>
        /// Gets the amount of time the game has been running, in seconds.
        /// </summary>
        public double GlobalTime => _globalTimeSecs;

        /// <summary>
        /// Gets the amount of time between the last two frames.
        /// </summary>
        public double DeltaTime => _deltaTimeSecs;

        /// <summary>
        /// Gets or sets a value indicating whether the game engine is paused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if paused; otherwise, <c>false</c>.
        /// </value>
        public bool Paused
        {
            get { return _paused; }
            set
            {
                if (_paused != value)
                {
                    if (!value)
                    {
                        _resetTime = true;
                    }

                    _paused = value;
                }
            }
        }

        /// <summary>
        /// Gets a game service.
        /// </summary>
        /// <typeparam name="TService">The type of the service to obtain.</typeparam>
        /// <returns>An instance of a game service.</returns>
        public TService GetService<TService>()
            where TService : IGameService
        {
            return _container.Resolve<TService>();
        }

        /// <summary>
        /// Loads and starts a game level.
        /// </summary>
        /// <param name="levelName">The name of the level.</param>
        public void StartLevel(string levelName)
        {
            if(_isRunning)
            {
                StopLevel();
            }

            var resourceService = GetService<IResourceService>();
            var entityService = GetService<IEntityService>();

            // load the map
            _currentMap = resourceService.LoadResource<Map>(levelName);
            
            // perform main resource load
            resourceService.LoadPreloadedResources();

            // notify game services
            for (var i = 0; i < _services.Length; i++)
            {
                _services[i].MapLoaded(_currentMap);
            }

            // start the game
            _gameTimer.Start();

            // set initial times
            _accumulator = 0.0;
            _lastTime = _gameTimer.ElapsedMilliseconds / 1000.0;
            _globalTimeSecs = 0.0;
            _deltaTimeSecs = 0.0;

            _isRunning = true;
        }

        /// <summary>
        /// Called each frame to run the game logic.
        /// </summary>
        /// <param name="singleStep">If true, does not perform any FPS rate control and simply increments time by a single predefined time step.</param>
        /// <remarks>
        /// http://gafferongames.com/game-physics/fix-your-timestep/
        /// </remarks>
        public void Step(bool singleStep)
        {
            if (_isRunning)
            {
                if(_paused)
                {
                    if (!singleStep)
                    {
                        return;
                    }
                }
                else
                {
                    // can't single step unless paused!
                    singleStep = false;
                }

                double time = _gameTimer.ElapsedMilliseconds / 1000.0;

                if (singleStep)
                {
                    // single step so just increment time by the ideal delta
                    time = _lastTime + IdealDeltaTime;
                }
                else
                {
                    if (_resetTime)
                    {
                        _resetTime = false;
                        _lastTime = time;
                    }
                }

                double frameTime = time - _lastTime;
                _lastTime = time;

                _accumulator += frameTime;

                while (_accumulator >= IdealDeltaTime)
                {
                    _deltaTimeSecs = IdealDeltaTime;

                    // pre-step the game services
                    for (var i = 0; i < _services.Length; i++)
                    {
                        _services[i].PreStep();
                    }

                    // step the game services
                    for (var i = 0; i < _services.Length; i++)
                    {
                        _services[i].Step();
                    }

                    var entityService = GetService<IEntityService>();
                    var collisionService = GetService<ICollisionService>();

                    GameEntity entity;

                    // collide & step the entities
                    var node = entityService.First;
                    while(node != null)
                    {
                        entity = node.Value;

                        if ((entity.Options & EntityOptions.Spawned) == EntityOptions.None)
                        {
                            node = node.Next;
                            continue;
                        }

                        collisionService.PerformCollisions(entity);
                        entity.Step();

                        node = node.Next;
                    }

                    node = entityService.First;
                    while (node != null)
                    {
                        entity = node.Value;

                        if ((entity.Options & EntityOptions.Spawned) == EntityOptions.None)
                        {
                            node = node.Next;
                            continue;
                        }

                        entity.PostStep();
                        node = node.Next;
                    }

                    _accumulator -= IdealDeltaTime;
                    _globalTimeSecs += _deltaTimeSecs;

                    // post-step the game services
                    for (var i = 0; i < _services.Length; i++)
                    {
                        _services[i].PostStep();
                    }
                }
            }
        }

        /// <summary>
        /// Stops the level.
        /// </summary>
        public void StopLevel()
        {
            if(!_isRunning)
            {
                return;
            }

            _isRunning = false;

            // notify game services
            for (var i = 0; i < _services.Length; i++)
            {
                _services[i].MapUnloaded();
            }

            _currentMap = null;

            var resourceService = GetService<IResourceService>();
            resourceService.UnloadAllResources();

            _gameTimer.Stop();
            _gameTimer.Reset();
        }

        /// <summary>
        /// Registers the game services.
        /// </summary>
        private void RegisterServices()
        {
            _container.RegisterInstance<IGameEngine>(this);
            _container.RegisterType<IVariableService, VariableService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IInputService, InputService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IMapService, MapService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IEntityService, EntityService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IResourceService, ResourceService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IRenderService, RenderService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAudioService, AudioService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ICollisionService, CollisionService>(new ContainerControlledLifetimeManager());

            var services = new List<IGameService>();
            services.Add(_container.Resolve<IVariableService>());
            services.Add(_container.Resolve<IInputService>());
            services.Add(_container.Resolve<IMapService>());
            services.Add(_container.Resolve<IEntityService>());
            services.Add(_container.Resolve<IResourceService>());
            services.Add(_container.Resolve<IRenderService>());
            services.Add(_container.Resolve<IAudioService>());
            services.Add(_container.Resolve<ICollisionService>());
            _services = services.ToArray();
        }
    }
}
