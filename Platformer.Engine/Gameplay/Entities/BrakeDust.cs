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

    /// <summary>
    /// Class which implements a small puff of dust created by a player braking.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("env_brakedust")]
    public class BrakeDust : Animatable
    {
        private readonly IResourceService _resourceService;
        private readonly IEntityService _entityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrakeDust"/> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public BrakeDust(IVariableService varService, IResourceService resourceService, IEntityService entityService)
            :base(varService, resourceService)
        {
            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            _entityService = entityService;
            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("brakesmoke");
        }

        /// <summary>
        /// Called when the entity is first placed into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var spr = _resourceService.LoadResource<Sprite>("brakesmoke");
            SetSprite(spr.ResourceId);
            AnimSpeed = 30.0;
            SetAnimation(spr.AnimationByName("idle").Id);

            base.OnSpawn();
        }

        /// <summary>
        /// Called each time the animation changes frame.
        /// </summary>
        /// <param name="lastSeqIndex">The index of the last sequence position played.</param>
        /// <param name="currentSeqIndex">The index of the current sequence position.</param>
        protected override void AnimationFrameChanged(int lastSeqIndex, int currentSeqIndex)
        {
            // if the braking animation has finished - stop braking
            if (lastSeqIndex > currentSeqIndex)
            {
                // we're done
                _entityService.KillEntity(this);
            }
        }
    }
}
