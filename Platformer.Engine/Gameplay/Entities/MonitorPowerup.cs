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
    using Resources;
    using Services;
    using System;

    /// <summary>
    /// Class which implements a powerup that's held in an item monitor until it is smashed.
    /// </summary>
    [GameEntityDefinition("monitor_powerup")]
    public class MonitorPowerup : Animatable
    {
        private readonly IVariableService _variableService;
        private readonly IResourceService _resourceService;
        private readonly IEntityService _entityService;
        private readonly IAudioService _audioService;

        private bool _collected;
        private double _raiseTime;
        private double _killTime;
        private Player _collectedPlayer;

        private int _ringSoundId;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorPowerup" /> class.
        /// </summary>
        /// <param name="variableService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="entityService">The entity service.</param>
        public MonitorPowerup(IVariableService variableService, IResourceService resourceService, IEntityService entityService, IAudioService audioService)
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
            _variableService = variableService;
            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("mpowerup");

            _resourceService.PreloadResource<Sound>("ring");
        }

        /// <summary>
        /// Gets or sets the type of the powerup.
        /// </summary>
        public PowerupType PowerupType { get; set; }

        /// <summary>
        /// Collects the powerup, giving it to the player.
        /// </summary>
        /// <param name="player">The player that collected the powerup.</param>
        public void Collect(Player player)
        {
            _raiseTime = _variableService.GlobalTime + 0.5;
            _killTime = _variableService.GlobalTime + 0.8;
            _collected = true;
            _collectedPlayer = player;
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            if(_collected)
            {
                if(_raiseTime > _variableService.GlobalTime)
                {
                    Position = new Point(Position.X, Position.Y - 2.0);
                }

                if(_killTime <= _variableService.GlobalTime)
                {
                    ApplyPowerup();
                    _entityService.KillEntity(this);
                }
            }

            base.OnStep();
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            _ringSoundId = _resourceService.LoadResource<Sound>("ring").ResourceId;

            var sprite = _resourceService.LoadResource<Sprite>("mpowerup");

            SetSprite(sprite.ResourceId);

            string animName = "none";
            switch(PowerupType)
            {
                case PowerupType.ExtraLife:
                    animName = "sonic";
                    break;
                case PowerupType.TenRings:
                    animName = "ring";
                    break;
            }

            SetAnimation(sprite.AnimationByName(animName).Id);

            base.OnSpawn();
        }

        /// <summary>
        /// Applies the powerup to the player.
        /// </summary>
        private void ApplyPowerup()
        {
            if(_collectedPlayer == null)
            {
                return;
            }

            switch(PowerupType)
            {
                case PowerupType.TenRings:
                    _audioService.PlaySoundEffect(_ringSoundId);
                    _collectedPlayer.Rings += 10;
                    break;
            }
        }
    }
}
