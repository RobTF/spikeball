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

    /// <summary>
    /// Class which implements a very basic movement controller.
    /// </summary>
    public class BasicMoveController : IMoveController
    {
        protected readonly GameEntity _parent;
        protected double _xsp, _ysp;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMoveController" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public BasicMoveController(GameEntity parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _parent = parent;
            _xsp = 0.0;
            _ysp = 0.0;
        }

        /// <summary>
        /// Gets or sets the vertical speed.
        /// </summary>
        public double VerticalSpeed
        {
            get
            {
                return _ysp;
            }

            set
            {
                _ysp = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal speed.
        /// </summary>
        public double HorizontalSpeed
        {
            get
            {
                return _xsp;
            }

            set
            {
                _xsp = value;
            }
        }

        /// <summary>
        /// Steps the movement of the parent entity by the controller logic.
        /// </summary>
        public virtual void Move()
        {
            _parent.Position = new Point(_parent.Position.X + _xsp, _parent.Position.Y + _ysp);
        }
    }
}
