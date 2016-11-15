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
namespace Platformer.Engine.Tiles
{
    /// <summary>
    /// Class which encapsulates the definition of a map tile.
    /// </summary>
    public class TileDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileDefinition"/> class.
        /// </summary>
        public TileDefinition()
        {
            Angles = new int[4];
        }

        /// <summary>
        /// Gets or sets the identifier of the tile definition.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the tile set that contains this definition.
        /// </summary>
        public TileSet TileSet { get; set; }

        /// <summary>
        /// Gets or sets the position and size of the tile definition within the tile set.
        /// </summary>
        public Int32Rect Rect { get; set; }

        /// <summary>
        /// Gets or sets the collision map.
        /// </summary>
        public bool[,] CollisionMap { get; set; }

        /// <summary>
        /// Gets or sets the tile angles, one per movement mode.
        /// </summary>
        public int[] Angles { get; set; }

        /// <summary>
        /// Gets or sets the solidity type of the tile.
        /// </summary>
        public SolidType SolidType { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Id: {Id}, Rect: {Rect}";
        }
    }
}
