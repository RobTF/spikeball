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
    using Collision;
    using Engine.Entities;
    using Resources;
    using Services;

    /// <summary>
    /// Class which implements a collectable ring.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("item_ring")]
    public class Ring : Animatable
    {
        private readonly IEntityService _entityService;
        private readonly IResourceService _resourceService;
        private readonly IAudioService _audioService;
        private readonly IVariableService _variableService;

        private int _soundId;
        private int _collectedAnimId;
        private double _killTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ring" /> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="audioService">The audio service.</param>
        public Ring(IVariableService varService, IResourceService resourceService, IAudioService audioService, IEntityService entityService)
            : base(varService, resourceService)
        {
            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            _variableService = varService;
            _entityService = entityService;
            _audioService = audioService;
            _resourceService = resourceService;

            _resourceService.PreloadResource<Sprite>("ring");
            _resourceService.PreloadResource<Sound>("ring");

            _killTime = 0.0;
        }

        /// <summary>
        /// Sets an amount of time the ring has before it is destroyed.
        /// </summary>
        /// <param name="time">The number of seconds the ring has before it is destroyed.</param>
        public void SetLifeTime(double time)
        {
            _killTime = _variableService.GlobalTime + time;
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            CollisionBox = new BoundingBox(-6, -6, 6, 6);
            Options |= EntityOptions.Collidable;
            SolidType = SolidType.None;

            var sprite = _resourceService.LoadResource<Sprite>("ring");
            SetSprite(sprite.ResourceId);
            AnimSpeed = _killTime > 0.0 ? 24.0 : 10.0;
            SetAnimation(sprite.AnimationByName("idle").Id);

            _collectedAnimId = sprite.AnimationByName("collect").Id;

            _soundId = _resourceService.LoadResource<Sound>("ring").ResourceId;

            base.OnSpawn();
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            if(_killTime != 0.0)
            {
                AnimSpeed = Utils.Lerp(AnimSpeed, 2.0, 10, _variableService.DeltaTime);

                if(_killTime < _variableService.GlobalTime)
                {
                    _entityService.KillEntity(this);
                }
            }

            base.OnStep();
        }

        /// <summary>
        /// Called each frame for every entity currently colliding with this one.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected override void OnColliding(GameEntity other)
        {
            var player = other as Player;

            if(player != null)
            {
                if (player.CanCollectRings)
                {
                    Collected(player);
                }
            }
        }

        /// <summary>
        /// Called each time the animation changes frame.
        /// </summary>
        /// <param name="lastSeqIndex">The index of the last sequence position played.</param>
        /// <param name="currentSeqIndex">The index of the current sequence position.</param>
        protected override void AnimationFrameChanged(int lastSeqIndex, int currentSeqIndex)
        {
            if(Animation == _collectedAnimId)
            {
                // kill after we've finished the "collected" animation
                if(lastSeqIndex > currentSeqIndex)
                {
                    _entityService.KillEntity(this);
                }
            }
            base.AnimationFrameChanged(lastSeqIndex, currentSeqIndex);
        }

        /// <summary>
        /// Gives the ring to a player.
        /// </summary>
        /// <param name="player">the player that collected the ring.</param>
        private void Collected(Player player)
        {
            if(player == null)
            {
                return;
            }

            _killTime = 0.0;
            AnimSpeed = 14.0;

            // stop any movement
            MoveController = null;

            Options &= ~EntityOptions.Collidable;
            RenderPriority = RenderPriority.Highest;

            _audioService.PlaySoundEffect(_soundId, 0);
            SetAnimation(_collectedAnimId);

            player.Rings++;
        }
    }
}
