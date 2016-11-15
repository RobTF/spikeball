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
    using Services;
    using Engine.Entities;

    /// <summary>
    /// Class which implements a game camera which roughly emulates the Genesis Sonic games.
    /// </summary>
    [GameEntityDefinition("sonic_camera")]
    public class SonicCamera : Camera
    {
        private const double LeftBorder = 8.0;
        private const double RightBorder = 8.0;

        private const double TopAirBorder = 32;
        private const double BottomAirBorder = 32;

        private const double CameraFastSpeed = 16.0;
        private const double CameraSlowSpeed = 6.0;

        private Player _followPlayer;
        private IVariableService _varService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SonicCamera" /> class.
        /// </summary>
        /// <param name="varService">The variable service.</param>
        public SonicCamera(IVariableService varService)
        {
            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            _varService = varService;
        }

        /// <summary>
        /// Gets or sets the player to follow.
        /// </summary>
        public override GameEntity Follow
        {
            get
            {
                return _followPlayer;
            }

            set
            {
                _followPlayer = value as Player;
                base.Follow = value;
            }
        }

        /// <summary>
        /// Called after each frame.
        /// </summary>
        protected override void OnPostStep()
        {
            if (_followPlayer == null)
            {
                return;
            }

            double xDelta = 0.0;
            double yDelta = 0.0;

            if (_followPlayer.Position.X > Position.X + RightBorder)
            {
                xDelta = Math.Min(_followPlayer.Position.X - (Position.X + RightBorder), CameraFastSpeed);
            }

            if (_followPlayer.Position.X < Position.X - LeftBorder)
            {
                xDelta = -Math.Min((Position.X - LeftBorder) - _followPlayer.Position.X, CameraFastSpeed);
            }

            if (_followPlayer.MoveController.Falling)
            {
                var yOrigin = _followPlayer.Position.Y + (_followPlayer.Size.Y / 2.0);
                if (yOrigin < Position.Y - TopAirBorder)
                {
                    yDelta = -Math.Min((Position.Y - TopAirBorder) - yOrigin, CameraFastSpeed);
                }

                if (yOrigin > Position.Y + BottomAirBorder)
                {
                    yDelta = Math.Min(yOrigin - (Position.Y + BottomAirBorder), CameraFastSpeed);
                }
            }
            else
            {
                var yOrigin = _followPlayer.Position.Y + (_followPlayer.Size.Y / 2.0);
                if (yOrigin != Position.Y)
                {
                    var diff = yOrigin - Position.Y;
                    var maxSpeed = Math.Abs(_followPlayer.MoveController.VerticalSpeed) > 6.0 ? CameraFastSpeed : CameraSlowSpeed;

                    yDelta = Math.Min(Math.Abs(diff), maxSpeed);
                    yDelta *= Math.Sign(diff);
                }
            }

            Position = new Point(Position.X + xDelta, Position.Y + yDelta);
        }
    }
}
