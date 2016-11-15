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
namespace Platformer.Engine.Entities
{
    using Resources;
    using Services;
    using System;

    /// <summary>
    /// Base class for game entities that are visible sprites with animation.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.GameEntity" />
    public abstract class Animatable : GameEntity
    {
        private readonly IVariableService _varService;
        private readonly IResourceService _resourceService;

        private bool _loopAnimation;
        private double _nextFrameTime;
        private int _animationId;
        private double _animSpeed;
        private int _spriteId;
        private SpriteAnimation _anim;

        /// <summary>
        /// Initializes a new instance of the <see cref="Animatable" /> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        /// <param name="resourceService">The resource service.</param>
        public Animatable(IVariableService varService, IResourceService resourceService)
        {
            if(varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            _varService = varService;
            _resourceService = resourceService;

            _loopAnimation = true;
            _spriteId = -1;
            _animationId = -1;
            _animSpeed = 1.0;
            CurrentAnimSequenceIndex = 0;
            VisLayer = 0;
            Options |= EntityOptions.Visible;
            RenderPriority = RenderPriority.Normal;
        }

        /// <summary>
        /// Gets the aligned position used for rendering the sprite.
        /// </summary
        public Int32Point RenderPosition => new Int32Point((int)Math.Floor(Position.X), (int)Math.Floor(Position.Y));

        /// <summary>
        /// Gets the identifier of the sprite used by the entity.
        /// </summary>
        public int Sprite => _spriteId;

        /// <summary>
        /// Gets the identifier of the current animation.
        /// </summary>
        public int Animation => _animationId;

        /// <summary>
        /// Gets or sets the current animation frame.
        /// </summary>
        public int CurrentAnimSequenceIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity should be flipped horizontally when rendered.
        /// </summary>
        public bool FlipHorizontally { get; set; }

        public RenderPriority RenderPriority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current animation should loop.
        /// </summary>
        public bool LoopAnimation
        {
            get
            {
                return _loopAnimation;
            }

            set
            {
                _loopAnimation = value;
            }
        }

        /// <summary>
        /// Gets or sets the animation speed in frames per second.
        /// </summary>
        public double AnimSpeed
        {
            get
            {
                return _animSpeed;
            }

            set
            {
                _animSpeed = value;
            }
        }

        /// <summary>
        /// Sets the sprite of the entitiy.
        /// </summary>
        /// <param name="spriteId">The sprite identifier.</param>
        public void SetSprite(int spriteId)
        {
            if(_spriteId == spriteId)
            {
                return;
            }

            var sprite = _resourceService.GetResourceById<Sprite>(spriteId);
            if(sprite != null)
            {
                _spriteId = sprite.ResourceId;
            }
        }

        /// <summary>
        /// Sets the animation to play.
        /// </summary>
        /// <param name="animationId">The animation identifier.</param>
        public void SetAnimation(int animationId)
        {
            if(_animationId == animationId)
            {
                return;
            }

            _animationId = animationId;
            CurrentAnimSequenceIndex = 0;
            LoopAnimation = true;
            _anim = null;
            _nextFrameTime = _varService.GlobalTime + (1.0D / _animSpeed);

            var sprite = _resourceService.GetResourceById<Sprite>(Sprite);
            if (sprite != null)
            {
                _anim = sprite.Animations[animationId];
            }
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            base.OnStep();

            if (_nextFrameTime <= _varService.GlobalTime)
            {
                if(_anim == null)
                {
                    return;
                }

                if (!LoopAnimation)
                {
                    if (CurrentAnimSequenceIndex >= (_anim.Sequence.Length - 1))
                    {
                        return;
                    }
                }

                var lastSeqIndex = CurrentAnimSequenceIndex;

                // increment frame
                CurrentAnimSequenceIndex = ++CurrentAnimSequenceIndex % _anim.Sequence.Length;

                AnimationFrameChanged(lastSeqIndex, CurrentAnimSequenceIndex);

                _nextFrameTime = _varService.GlobalTime + (1.0D / _animSpeed);
            }
        }

        /// <summary>
        /// Called each time the animation changes frame.
        /// </summary>
        /// <param name="lastSeqIndex">The index of the last sequence position played.</param>
        /// <param name="currentSeqIndex">The index of the current sequence position.</param>
        protected virtual void AnimationFrameChanged(int lastSeqIndex, int currentSeqIndex)
        {
        }
    }
}
