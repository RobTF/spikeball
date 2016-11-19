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
    using Engine.Entities;
    using Services;
    using Resources;
    using System;
    using System.Collections.Generic;
    using Collision;

    /// <summary>
    /// Class which implements a basic spring.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("spring")]
    public class Spring : Animatable
    {
        private const double RedPower = 16.0;
        private const double YellowPower = 10.0;

        private readonly IResourceService _resourceService;
        private readonly IAudioService _audioService;
        private readonly IVariableService _variableService;

        private int _idleAnimId;
        private int _openAnimId;
        private int _soundId;

        private bool _isOpen;
        private double _resetTime;
        private double _springPower;
        private Direction _direction;
        private SpringType _springType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spring" /> class.
        /// </summary>
        /// <param name="variableService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public Spring(IVariableService variableService, IResourceService resourceService, IAudioService audioService)
            :base(variableService, resourceService)
        {
            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (variableService == null)
            {
                throw new ArgumentNullException(nameof(variableService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            _variableService = variableService;
            _resourceService = resourceService;
            _audioService = audioService;

            _resourceService.PreloadResource<Sprite>("spring");
            _resourceService.PreloadResource<Sound>("spring");

            _springType = SpringType.Red;
            _isOpen = false;
        }

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
            if(properties.TryGetValue("direction", out val))
            {
                Direction dir;
                if (!Enum.TryParse(val, out dir))
                {
                    dir = Direction.Up;
                }
                _direction = dir;
            }           

            if (properties.TryGetValue("spring_type", out val))
            {
                SpringType sType;
                if (!Enum.TryParse(val, out sType))
                {
                    sType = SpringType.Red;
                }

                _springType = sType;
            }

            base.SetProperties(properties);
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            switch(_direction)
            {
                case Direction.Right:
                    Angle = 270.0;
                    CollisionBox = new BoundingBox(-15.0, -15.0, 0.0, 15.0);
                    break;
                case Direction.Left:
                    Angle = 90.0;
                    CollisionBox = new BoundingBox(0.0, -15.0, 15.0, 15.0);
                    break;
                case Direction.Down:
                    Angle = 180.0;
                    CollisionBox = new BoundingBox(-15.0, -15.0, 15.0, 0.0);
                    break;
                default:
                    Angle = 0.0;
                    CollisionBox = new BoundingBox(-15.0, -5.0, 15.0, 15.0);
                    break;
            }

            _soundId = _resourceService.LoadResource<Sound>("spring").ResourceId;

            var sprite = _resourceService.LoadResource<Sprite>("spring");
            SetSprite(sprite.ResourceId);

            if (_springType == SpringType.Red)
            {
                _idleAnimId = sprite.AnimationByName("redidle").Id;
                _openAnimId = sprite.AnimationByName("redopen").Id;
                _springPower = RedPower;
            }
            else
            {
                _idleAnimId = sprite.AnimationByName("yellowidle").Id;
                _openAnimId = sprite.AnimationByName("yellowopen").Id;
                _springPower = YellowPower;
            }

            SetAnimation(_idleAnimId);
            Options |= EntityOptions.Collidable;
            SolidType = SolidType.FullSolid;

            base.OnSpawn();
        }

        /// <summary>
        /// Called once when this entity collides with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected override void OnColliding(GameEntity other)
        {
            var player = other as Player;

            if(player == null)
            {
                return;
            }

            if (_direction == Direction.Up)
            {
                if ((player.Position.Y < Position.Y) && (player.MoveController.VerticalSpeed > 0.0))
                {
                    SpringPlayer(player, -_springPower, 0.0);
                }
            }
            else if (_direction == Direction.Down)
            {
                if ((player.Position.Y > Position.Y) && (player.MoveController.VerticalSpeed < 0.0))
                {
                    SpringPlayer(player, _springPower, 0.0);
                }
            }
            else if (_direction == Direction.Right)
            {
                if (player.Position.X > Position.X)
                {
                    SpringPlayer(player, 0.0, _springPower);
                }
            }
            else if (_direction == Direction.Left)
            {
                if (player.Position.X < Position.X)
                {
                    SpringPlayer(player, 0.0, -_springPower);
                }
            }

            base.OnColliding(other);
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            if(_isOpen && (_resetTime < _variableService.GlobalTime))
            {
                _isOpen = false;
                SolidType = SolidType.FullSolid;
                SetAnimation(_idleAnimId);
            }

            base.OnStep();
        }

        /// <summary>
        /// Springs the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="verticalSpeed">The vertical speed.</param>
        /// <param name="horizontalSpeed">The horizontal speed.</param>
        private void SpringPlayer(Player player, double verticalSpeed, double horizontalSpeed)
        {
            _audioService.PlaySoundEffect(_soundId);
            _resetTime = _variableService.GlobalTime + 0.1;

            SolidType = SolidType.None;
            SetAnimation(_openAnimId);
            _isOpen = true;

            if ((_direction == Direction.Up) || (_direction == Direction.Down) || !player.MoveController.Falling)
            {
                player.MoveController.Unroll();
            }

            if(verticalSpeed != 0.0)
            {
                player.PushVertically(verticalSpeed, true);
            }

            if (horizontalSpeed != 0.0)
            {
                player.MoveController.SetHorizontalControlLock(0.25);
                player.PushHorizontally(horizontalSpeed, true);
            }
        }
    }

    /// <summary>
    /// Enumeration containing the various spring power types.
    /// </summary>
    public enum SpringType
    {
        /// <summary>
        /// Powerful red spring.
        /// </summary>
        Red,

        /// <summary>
        /// Average yellow spring.
        /// </summary>
        Yellow
    }
}
