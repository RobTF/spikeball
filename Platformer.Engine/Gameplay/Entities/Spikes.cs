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
    using Collision;
    using Engine.Entities;
    using Resources;
    using Services;

    /// <summary>
    /// A set of spikes which cause damage to the player if the sharp end is touched!
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("spikes")]
    public class Spikes : Animatable
    {
        private readonly IResourceService _resourceService;

        private Direction _direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spikes"/> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public Spikes(IVariableService varService, IResourceService resourceService)
            :base(varService, resourceService)
        {
            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("spikes");
            _direction = Direction.Up;

            Options |= EntityOptions.Collidable;
            SolidType = SolidType.FullSolid;
        }

        /// <summary>
        /// Sets entity properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public override void SetProperties(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string val;
            if (properties.TryGetValue("direction", out val))
            {
                Direction dir;
                if (Enum.TryParse(val, out dir))
                {
                    _direction = dir;
                }
            }

            if(properties.TryGetValue("solid_type", out val))
            {
                SolidType sol;
                if(Enum.TryParse(val, out sol))
                {
                    SolidType = sol;
                }
            }
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var sprite = _resourceService.LoadResource<Sprite>("spikes");
            SetSprite(sprite.ResourceId);
            SetAnimation(sprite.AnimationByName("idle").Id);

            switch(_direction)
            {
                case Direction.Down:
                    CollisionBox = new BoundingBox(-16.0, -16.0, 16.0, 16.0);
                    Angle = 180;
                    break;
                case Direction.Left:
                    CollisionBox = new BoundingBox(-16.0, -16.0, 16.0, 16.0);
                    Angle = 90;
                    break;
                case Direction.Right:
                    CollisionBox = new BoundingBox(-16.0, -16.0, 16.0, 16.0);
                    Angle = 270;
                    break;
                default:
                    CollisionBox = new BoundingBox(-16.0, -16.0, 16.0, 16.0);
                    Angle = 0;
                    break;
            }            

            base.OnSpawn();
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

            var doDamage = false;

            if (_direction == Direction.Up)
            {
                if (player.MoveController.VerticalSpeed >= 0.0)
                {
                    if (player.Position.Y < (Position.Y - (Size.Y / 2)))
                    {
                        doDamage = true;
                    }
                }
            }
            else if (_direction == Direction.Down)
            {
                if (player.MoveController.VerticalSpeed <= 0.0)
                {
                    if (player.Position.Y > Position.Y)
                    {
                        doDamage = true;
                    }
                }
            }
            else if (_direction == Direction.Right)
            {
                if (player.MoveController.HorizontalSpeed <= 0.0)
                {
                    if (player.Position.X > Position.X)
                    {
                        doDamage = true;
                    }
                }
            }
            else if (_direction == Direction.Left)
            {
                if (player.MoveController.HorizontalSpeed >= 0.0)
                {
                    if (player.Position.X < Position.X)
                    {
                        doDamage = true;
                    }
                }
            }

            if (doDamage)
            {
                player.TakeDamage(this);
            }

            base.OnColliding(other);
        }
    }
}
