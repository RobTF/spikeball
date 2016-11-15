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
    /// <summary>
    /// Class which implements a basic game camera.
    /// </summary>
    [GameEntityDefinition("camera")]
    public class Camera : GameEntity
    {
        private GameEntity _followEntity;

        /// <summary>
        /// Gets or sets the player to follow.
        /// </summary>
        public virtual GameEntity Follow
        {
            get
            {
                return _followEntity;
            }

            set
            {
                _followEntity = value;
                Center();
            }
        }

        /// <summary>
        /// Called after each frame.
        /// </summary>
        protected override void OnPostStep()
        {
            // just stick to the entity we are following
            if (Follow != null)
            {
                Position = Follow.Position;
            }
        }

        /// <summary>
        /// Centers the camera on the entity it is following.
        /// </summary>
        protected void Center()
        {
            if (Follow != null)
            {
                Position = Follow.Position;
            }
        }
    }
}
