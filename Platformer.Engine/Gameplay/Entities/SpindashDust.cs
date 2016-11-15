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
    /// Class which implements a dust spray created by a player spindashing.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("env_spindashdust")]
    public class SpindashDust : Animatable
    {
        private readonly IResourceService _resourceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpindashDust"/> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public SpindashDust(IVariableService varService, IResourceService resourceService)
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

            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("spindashsmoke");
        }

        /// <summary>
        /// Called when the entity is first placed into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var spr = _resourceService.LoadResource<Sprite>("spindashsmoke");
            SetSprite(spr.ResourceId);
            AnimSpeed = 30.0;
            SetAnimation(spr.AnimationByName("idle").Id);

            base.OnSpawn();
        }
    }
}
