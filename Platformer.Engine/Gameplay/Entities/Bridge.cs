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
namespace Platformer.Engine.Gameplay.Entities
{
    using System;
    using System.Collections.Generic;
    using Engine.Entities;
    using Resources;
    using Services;

    /// <summary>
    /// Class which implements a bridge with depression effect.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.GameEntity" />
    [GameEntityDefinition("bridge")]
    public class Bridge : GameEntity
    {
        private readonly IVariableService _variableService;
        private readonly IEntityService _entityService;
        private readonly IResourceService _resourceService;
        private readonly ICollisionService _collisionService;

        private BridgeLog[] _logs;
        private double[] _maxDepressions;
        private double _depressionFactor;

        private int _standIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bridge" /> class.
        /// </summary>
        /// <param name="entityService">The entity service.</param>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="variableService">The variable service.</param>
        /// <param name="collisionService">The collision service.</param>
        public Bridge(IEntityService entityService, IResourceService resourceService, IVariableService variableService, ICollisionService collisionService)
        {
            if(entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (variableService == null)
            {
                throw new ArgumentNullException(nameof(variableService));
            }

            if (collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            _collisionService = collisionService;
            _variableService = variableService;
            _entityService = entityService;
            _resourceService = resourceService;

            _resourceService.PreloadResource<Sprite>("bridgelog");

            _depressionFactor = 0.0;
            _standIndex = 4;
        }

        /// <summary>
        /// Gets or sets the length of the bridge.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Sets entity properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public override void SetProperties(IDictionary<string, string> properties)
        {
            if(properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string val;
            int iVal;

            if(properties.TryGetValue("length", out val))
            {
                if(int.TryParse(val, out iVal))
                {
                    Length = iVal;
                }
            }
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var logs = new List<BridgeLog>();
            for(int i = 0; i < Length; i++)
            {
                var log = _entityService.CreateEntity<BridgeLog>(Position);
                log.Owner = this;
                log.Position = new Point(Position.X + (log.Size.X * (i + 1)), Position.Y);
                logs.Add(log);
            }

            _logs = logs.ToArray();
            CalculateMaxDepressions();

            base.OnSpawn();
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            if(_logs == null)
            {
                return;
            }

            // determine if player is standing on a log by firing a trace across it
            var start = new Point(Position.X, Position.Y - 12.0);
            var end = new Point(Position.X + _logs[0].Size.X * Length, Position.Y - 12.0);
            var tq = new TraceQuery
            {
                Line = new Line(start, end),
                Ignore = this,
                EntityType = typeof(Player),
                Options = TraceLineOptions.IgnoreTiles
            };
            var tr = _collisionService.TraceLine(tq);

            var idx = -1;

            if (tr.Hit)
            {
                var player = tr.Entity as Player;
                if ((player != null) && !player.MoveController.Falling)
                {
                    // is any log being stood on?
                    for (var i = 0; i < _logs.Length; i++)
                    {
                        var log = _logs[i];
                        if(Math.Abs(player.Position.X - log.Position.X) <= (log.Size.X / 2.0))
                        {
                            idx = i;
                            break;
                        }
                    }
                }
            }

            if(idx != -1)
            {
                _standIndex = idx;
            }

            // step the depression factor
            _depressionFactor = Utils.Lerp(_depressionFactor, (idx != -1) ? 1.0 : 0.0, 4.0, _variableService.DeltaTime);

            var maxDepression = _maxDepressions[_standIndex];

            for (var i = 0; i <= _standIndex; i++)
            {
                var log = _logs[i];

                var x = i + 1.0;
                var y = _standIndex + 1.0;

                log.Position = new Point(log.Position.X, Position.Y + maxDepression * Math.Sin(x / y) * _depressionFactor);
            }

            for (var i = _logs.Length - 1; i > _standIndex; i--)
            {
                var log = _logs[i];

                var x = _logs.Length - (double)i;
                var y = _logs.Length - (double)_standIndex;

                log.Position = new Point(log.Position.X, Position.Y + maxDepression * Math.Sin(x / y) * _depressionFactor);
            }

            base.OnStep();
        }

        /// <summary>
        /// Calculates the maximum log depressions.
        /// </summary>
        private void CalculateMaxDepressions()
        {
            _maxDepressions = new double[_logs.Length];
            for (var i = 0; i < _logs.Length; i++)
            {
                if (i < _logs.Length / 2)
                {
                    _maxDepressions[i] = (2 * (i + 1));
                }
                else
                {
                    _maxDepressions[i] = (2 * (_logs.Length - i));
                }
            }
        }
    }
}
