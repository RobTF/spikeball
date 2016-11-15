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
using System.Collections.Generic;

namespace Platformer.Engine.Resources
{
    /// <summary>
    /// Class which encapsulates the data about a sprite.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Resources.GameResource" />
    public class Sprite : GameResource
    {
        private readonly Dictionary<string, SpriteAnimation> _animations;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class.
        /// </summary>
        public Sprite()
        {
            _animations = new Dictionary<string, SpriteAnimation>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the animations.
        /// </summary>
        public SpriteAnimation[] Animations { get; set; }

        /// <summary>
        /// Gets the animation with the given name.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <returns>The animation, or null if no animation was found.</returns>
        public SpriteAnimation AnimationByName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            for (var i = 0; i < Animations.Length; i++)
            {
                var anim = Animations[i];
                if(anim.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return anim;
                }
            }

            return null;
        }
    }
}
