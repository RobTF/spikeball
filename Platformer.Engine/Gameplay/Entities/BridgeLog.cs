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
    /// Class which implements a single log that makes up a log bridge.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.Animatable" />
    [GameEntityDefinition("bridge_log")]
    public class BridgeLog : Animatable
    {
        private readonly IResourceService _resourceService;
        private readonly IRenderService _renderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BridgeLog" /> class.
        /// </summary>
        /// <param name="variableService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public BridgeLog(IVariableService variableService, IResourceService resourceService, IRenderService renderService)
                    : base(variableService, resourceService)
        {
            if(variableService == null)
            {
                throw new ArgumentNullException(nameof(variableService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            _renderService = renderService;
            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("bridgelog");

            CollisionBox = new BoundingBox(-8.0, -8.0, 8.0, 8.0);
            Options |= EntityOptions.Collidable;
            SolidType = SolidType.JumpThrough;
        }

        /// <summary>
        /// Gets or sets the bridge this log is a part of.
        /// </summary>
        public Bridge Owner { get; set; }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var sprite = _resourceService.LoadResource<Sprite>("bridgelog");
            SetSprite(sprite.ResourceId);
            SetAnimation(sprite.AnimationByName("idle").Id);

            base.OnSpawn();
        }
    }
}
