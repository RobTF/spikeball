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
namespace Platformer.Engine.Collision
{
    /// <summary>
    /// Structure which encapsulates an axis aligned bounding box.
    /// </summary>
    public struct BoundingBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
        /// </summary>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minY">The minimum y.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxY">The maximum y.</param>
        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            Mins = new Point(minX, minY);
            Maxs = new Point(maxX, maxY);
        }

        /// <summary>
        /// Gets or sets the minimums.
        /// </summary>
        public Point Mins { get; set; }

        /// <summary>
        /// Gets or sets the maximums.
        /// </summary>
        public Point Maxs { get; set; }

        /// <summary>
        /// Gets the size of the bounding box.
        /// </summary>
        /// <returns>The size of the bounding box.</returns>
        public Point GetSize()
        {
            return new Point(Maxs.X - Mins.X, Maxs.Y - Mins.Y);
        }

        /// <summary>
        /// Gets a bounding box with no size.
        /// </summary>
        public static BoundingBox Zero
        {
            get
            {
                return new BoundingBox(0.0, 0.0, 0.0, 0.0);
            }
        }
    }
}
