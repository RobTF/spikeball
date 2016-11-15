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
    /// Class which implements a very basic falling movement with rebound/bounce effect.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Movement.FallMoveController" />
    public class BounceMoveController : BasicMoveController
    {
        public const double DefaultMaxSpeed = 16.0;
        public const double DefaultGravity = 0.21875D * 60.0D;

        private readonly ICollisionService _collisionService;
        private readonly IVariableService _varService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMoveController" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public BounceMoveController(GameEntity parent, ICollisionService collisionService, IVariableService varService)
            : base(parent)
        {
            if (collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            _varService = varService;
            _collisionService = collisionService;

            MaxSpeed = DefaultMaxSpeed;
            Gravity = DefaultGravity;
            VerticalBounceFactor = HorizontalBounceFactor = 0.5;
            TerrainOnly = false;
        }

        /// <summary>
        /// Gets or sets the gravity.
        /// </summary>
        public double Gravity { get; set; }

        /// <summary>
        /// Gets or sets the maximum speed of the entity.
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// Gets or sets the vertical bounce factor.
        /// </summary>
        public double VerticalBounceFactor { get; set; }

        /// <summary>
        /// Gets or sets the horizontal bounce factor.
        /// </summary>
        public double HorizontalBounceFactor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity will only move against the terrain.
        /// </summary>
        public bool TerrainOnly { get; set; }

        /// <summary>
        /// Steps the movement of the parent entity by the controller logic.
        /// </summary>
        public override void Move()
        {
            var position = _parent.Position;
            var size = _parent.CollisionBox.GetSize();

            _ysp += Gravity * _varService.DeltaTime;

            if (_ysp < 0.0)
            {
                var tr = _collisionService.TraceLine(GetTraceQuery(new Line(position.X, position.Y, position.X, position.Y - size.Y / 2.0)));
                if(tr.Hit)
                {
                    _ysp *= -VerticalBounceFactor;
                }
            }
            else if (_ysp > 0.0)
            {
                var tr = _collisionService.TraceLine(GetTraceQuery(new Line(position.X, position.Y, position.X, position.Y + size.Y / 2.0)));
                if (tr.Hit)
                {
                    _ysp *= -VerticalBounceFactor;
                }
            }

            if (_xsp < 0.0)
            {
                var tr = _collisionService.TraceLine(GetTraceQuery(new Line(position.X, position.Y, position.X - size.X / 2.0, position.Y)));
                if (tr.Hit)
                {
                    _xsp *= -HorizontalBounceFactor;
                }
            }
            else if (_xsp > 0.0)
            {
                var tr = _collisionService.TraceLine(GetTraceQuery(new Line(position.X, position.Y, position.X + size.X / 2.0, position.Y)));
                if (tr.Hit)
                {
                    _xsp *= -HorizontalBounceFactor;
                }
            }

            // clamp speed
            if (_ysp > 0.0)
            {
                _ysp = Math.Min(_ysp, MaxSpeed);
            }
            else if (_ysp < 0.0)
            {
                _ysp = Math.Max(_ysp, -MaxSpeed);
            }

            if (_xsp > 0.0)
            {
                _xsp = Math.Min(_xsp, MaxSpeed);
            }
            else if (_xsp < 0.0)
            {
                _xsp = Math.Max(_xsp, -MaxSpeed);
            }

            base.Move();
        }

        /// <summary>
        /// Gets a trace query for use in collision traces during movement.
        /// </summary>
        /// <param name="line">The line to use for the trace..</param>
        /// <returns>A trace query.</returns>
        private TraceQuery GetTraceQuery(Line line)
        {
            var tq = new TraceQuery()
            {
                Line = line,
                Ignore = _parent,
                CollisionPath = _parent.CollisionPath,
                Options = TraceLineOptions.SolidOnly
            };

            if (TerrainOnly)
            {
                tq.Options |= TraceLineOptions.IgnoreEntities;
            }

            return tq;
        }
    }
}
