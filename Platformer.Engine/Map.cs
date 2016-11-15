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
namespace Platformer.Engine
{
    using Resources;
    using Tiles;

    /// <summary>
    /// Class which encapsulates map data.
    /// </summary>
    public class Map : GameResource
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the width of the map, in tiles.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the map, in tiles.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets the width of geometry tiles.
        /// </summary>
        public int GeometryTileWidth { get; set; }

        /// <summary>
        /// Gets the height of geometry tiles.
        /// </summary>
        public int GeometryTileHeight { get; set; }

        public TileSet[] TileSets { get; set; }

        /// <summary>
        /// Gets or sets all tile layers.
        /// </summary>
        public GeometryTileLayer[] GeometryLayers { get; set; }

        /// <summary>
        /// Gets or sets the art layers.
        /// </summary>
        public ArtTileLayer[] ArtLayers { get; set; }

        /// <summary>
        /// Gets or sets the collision layers.
        /// </summary>
        public CollisionTileLayer[] CollisionLayers { get; set; }
    }
}
