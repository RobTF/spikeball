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
    using Engine.Entities;
    using Services;
    using Resources;

    /// <summary>
    /// Class which implements the Sonic player character.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Gameplay.Entities.Player" />
    [GameEntityDefinition("player")]
    public class Sonic : Player
    {
        private readonly IResourceService _resourceService;
        private readonly IRenderService _renderService;
        private readonly IVariableService _varService;
        private readonly IInputService _inputService;
        private readonly IEntityService _entityService;
        private readonly IAudioService _audioService;
        private readonly ICollisionService _collisionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="renderService">The render service.</param>
        /// <param name="varService">The variable service.</param>
        /// <param name="inputService">The input service.</param>
        /// <param name="entityService">The entity service.</param>
        /// <param name="audioService"></param>
        /// <param name="collisionService"></param>
        public Sonic(
            IResourceService resourceService,
            IRenderService renderService,
            IVariableService varService,
            IInputService inputService,
            IEntityService entityService,
            IAudioService audioService,
            ICollisionService collisionService)
            : base(
                  resourceService,
                  renderService,
                  varService,
                  inputService,
                  entityService,
                  audioService,
                  collisionService)
        {
            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (renderService == null)
            {
                throw new ArgumentNullException(nameof(renderService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (inputService == null)
            {
                throw new ArgumentNullException(nameof(inputService));
            }

            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            if (collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            _entityService = entityService;
            _varService = varService;
            _renderService = renderService;
            _inputService = inputService;
            _audioService = audioService;
            _collisionService = collisionService;

            _resourceService = resourceService;
            _resourceService.PreloadResource<Sprite>("sonic");
        }

        /// <summary>
        /// Called when the entity is first placed into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            var sprite = _resourceService.LoadResource<Sprite>("sonic");
            SetSprite(sprite.ResourceId);

            _idleAnimId = sprite.AnimationByName("idle").Id;
            _walkAnimId = sprite.AnimationByName("walk").Id;
            _runAnimId = sprite.AnimationByName("run").Id;
            _rollAnimId = sprite.AnimationByName("roll").Id;
            _pushAnimId = sprite.AnimationByName("push").Id;
            _brakingAnimId = sprite.AnimationByName("brake").Id;
            _balance1AnimId = sprite.AnimationByName("balance1").Id;
            _balance2AnimId = sprite.AnimationByName("balance2").Id;
            _balance3AnimId = sprite.AnimationByName("balance3").Id;
            _crouchAnimId = sprite.AnimationByName("crouch").Id;
            _spindashAnimId = sprite.AnimationByName("spindash").Id;
            _lookupAnimId = sprite.AnimationByName("lookup").Id;
            _springAnimId = sprite.AnimationByName("spring").Id;
            _fallAnimId = sprite.AnimationByName("fall").Id;

            // just have all cameras following the player for now
            var node = _entityService.First;
            while(node != null)
            {
                if(node.Value is Camera)
                {
                    var camera = node.Value as Camera;
                    camera.Follow = this;
                }

                node = node.Next;
            }

            base.OnSpawn();
        }
    }
}
