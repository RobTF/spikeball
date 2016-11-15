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
    using Resources;
    using Services;
    using Engine.Entities;
    using System.Collections.Generic;
    using Collision;
    using Movement;

    /// <summary>
    /// Class which implements an item monitor.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.GameEntity" />
    [GameEntityDefinition("item_monitor")]
    public class Monitor : Animatable
    {
        private readonly ICollisionService _collisionService;
        private readonly IResourceService _resourceService;
        private readonly IEntityService _entityService;
        private readonly IAudioService _audioService;

        private int _explodeSoundId;
        private bool _exploded;
        private MonitorPowerup _powerup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Monitor" /> class.
        /// </summary>
        /// <param name="collisionService">The collision service.</param>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="variableService">The variable service.</param>
        /// <param name="entityService">The entity service.</param>
        /// <param name="audioService">The audio service.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public Monitor(ICollisionService collisionService, IResourceService resourceService, IVariableService variableService, IEntityService entityService, IAudioService audioService)
            : base(variableService, resourceService)
        {
            if(collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (variableService == null)
            {
                throw new ArgumentNullException(nameof(variableService));
            }

            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            _audioService = audioService;
            _entityService = entityService;
            _resourceService = resourceService;
            _collisionService = collisionService;

            _resourceService.PreloadResource<Sprite>("monitor");
            _resourceService.PreloadResource<Sprite>("mpowerup");
            _resourceService.PreloadResource<Sprite>("explosion1");
            _resourceService.PreloadResource<Sound>("small_explosion");

            PowerupType = PowerupType.None;
            RenderPriority = RenderPriority.Low;
            MoveController = new FallMoveController(this, _collisionService, variableService)
            {
                FallOnce = true,
                TerrainOnly = true
            };
        }

        /// <summary>
        /// Gets or sets the type of the powerup held within the monitor.
        /// </summary>
        public PowerupType PowerupType { get; set; }

        /// <summary>
        /// Sets entity properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public override void SetProperties(IDictionary<String, String> properties)
        {
            string val;
            if(properties.TryGetValue("powerup_type", out val))
            {
                PowerupType type;
                if(!Enum.TryParse(val, out type))
                {
                    type = PowerupType.None;
                }

                PowerupType = type;
            }

            base.SetProperties(properties);
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var sprite = _resourceService.LoadResource<Sprite>("monitor");

            _explodeSoundId = _resourceService.LoadResource<Sound>("small_explosion").ResourceId;

            Options |= EntityOptions.Collidable;
            SolidType = SolidType.FullSolid;
            CollisionBox = new BoundingBox(-15.0, -15.0, 15.0, 15.0);

            SetSprite(sprite.ResourceId);
            SetAnimation(sprite.AnimationByName("idle").Id);
            AnimSpeed = 24.0;

            // create the powerup
            if (PowerupType != PowerupType.None)
            {
                _powerup = _entityService.CreateEntity<MonitorPowerup>(new Point(Position.X, Position.Y));
                _powerup.PowerupType = PowerupType;
                _powerup.VisLayer = VisLayer;
            }
        }

        /// <summary>
        /// Called each frame for every entity currently colliding with this one.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected override void OnColliding(GameEntity other)
        {
            var player = other as Player;
            if (player == null)
            {
                return;
            }

            if(player.MoveController.Rolling)
            {
                if (player.MoveController.VerticalSpeed > 0.0)
                {
                    Explode(player);
                    player.MoveController.VerticalSpeed *= -1;
                }
                else if(player.MoveController.VerticalSpeed == 0.0 && player.MoveController.HorizontalSpeed != 0.0)
                {
                    Explode(player);
                }
            }

            base.OnColliding(other);
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            if(!_exploded)
            {
                _powerup.Position = new Point(Position.X, Position.Y - 3.0);
            }

            base.OnStep();
        }

        /// <summary>
        /// Called each time the animation changes frame.
        /// </summary>
        /// <param name="lastSeqIndex">The index of the last sequence position played.</param>
        /// <param name="currentSeqIndex">The index of the current sequence position.</param>
        protected override void AnimationFrameChanged(int lastSeqIndex, int currentSeqIndex)
        {
            if (!_exploded && (_powerup != null))
            {
                // flash the powerup
                if ((currentSeqIndex % 2) > 0)
                {
                    _powerup.Options |= EntityOptions.Visible;
                }
                else
                {
                    _powerup.Options &= ~EntityOptions.Visible;
                }
            }

            base.AnimationFrameChanged(lastSeqIndex, currentSeqIndex);
        }

        /// <summary>
        /// Explodes the monitor and releases the powerup.
        /// </summary>
        private void Explode(Player player)
        {
            if(_exploded)
            {
                return;
            }

            _exploded = true;

            var sprite = _resourceService.GetResourceById<Sprite>(Sprite);
            SetAnimation(sprite.AnimationByName("broken").Id);
            Options &= ~EntityOptions.Collidable;
            SolidType = SolidType.None;

            var explosion = _entityService.CreateEntity<Explosion>(new Point(Position.X, Position.Y - 5.0));
            explosion.VisLayer = VisLayer;

            _audioService.PlaySoundEffect(_explodeSoundId);

            // powerup always visible
            if (_powerup != null)
            {
                _powerup.Options |= EntityOptions.Visible;
                _powerup.Collect(player);
            }
        }
    }
}
