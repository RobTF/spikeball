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
namespace Platformer.Engine.Movement
{
    using System;
    using Entities;
    using Services;

    /// <summary>
    /// Movement controller which allows objects to fall according to a gravity value.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Movement.BasicMoveController" />
    public class FallMoveController : BasicMoveController
    {
        public const double DefaultGravity = 0.21875D * 60.0D;
        public const double DefaultMaxFallSpeed = 16.0;

        private readonly ICollisionService _collisionService;
        private readonly IVariableService _varService;

        protected double _gravity;
        private bool _falling;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMoveController" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public FallMoveController(GameEntity parent, ICollisionService collisionService, IVariableService varService)
            : base(parent)
        {
            if(collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            _varService = varService;
            _collisionService = collisionService;
            _falling = true;

            Gravity = DefaultGravity;
            MaxFallSpeed = DefaultMaxFallSpeed;
            FallOnce = true;
            TerrainOnly = true;
        }

        /// <summary>
        /// Gets or sets the gravity.
        /// </summary>
        public double Gravity
        {
            get
            {
                return _gravity;
            }

            set
            {
                _gravity = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum fall speed.
        /// </summary>
        public double MaxFallSpeed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the entity being controlled is falling.
        /// </summary>
        public bool Falling => _falling;

        /// <summary>
        /// Gets or sets a value indicating whether the controller stops checking for the fall condition once the entity hits the ground.
        /// </summary>
        public bool FallOnce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity will only fall into terrain.
        /// </summary>
        public bool TerrainOnly { get; set; }

        /// <summary>
        /// Steps the movement of the parent entity by the controller logic.
        /// </summary>
        public override void Move()
        {
            if (_falling || !FallOnce)
            {
                _ysp += Gravity * _varService.DeltaTime;

                var position = _parent.Position;
                var size = _parent.CollisionBox.GetSize();

                var tq = new TraceQuery()
                {
                    Line = new Line(position.X, position.Y, position.X, position.Y + size.Y / 2.0),
                    Ignore = _parent,
                    CollisionPath = _parent.CollisionPath,
                    Options = TraceLineOptions.SolidOnly
                };

                if(TerrainOnly)
                {
                    tq.Options |= TraceLineOptions.IgnoreEntities;
                }

                var tr = _collisionService.TraceLine(tq);

                if (!tr.Hit)
                {
                    _falling = true;
                    _ysp = Math.Min(_ysp, MaxFallSpeed);
                }
                else
                {
                    _ysp = 0.0;
                    _parent.Position = new Point(position.X, tr.ContactPoint.Y - size.Y / 2.0);
                    _falling = false;
                }
            }

            base.Move();
        }
    }
}
