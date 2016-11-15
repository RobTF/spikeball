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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class which represents a tile layer in the map.
    /// </summary>
    public abstract class GeometryTileLayer
    {
        private Map _map;
        private TilesetType _type;
        private GeometryTile[,] _tiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTileLayer" /> class.
        /// </summary>
        /// <param name="map">The map which will contain the layer.</param>
        protected GeometryTileLayer(TilesetType type, Map map)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            _type = type;
            _map = map;
            _tiles = new GeometryTile[map.Width, map.Height];

            HorizontalScrollMultipler = VerticalScrollMultipler = 1.0f;
            HorizontalOffset = VerticalOffset = 0.0f;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type of the layer.
        /// </summary>
        public TilesetType Type => _type;

        /// <summary>
        /// Gets the tiles that make up the layer.
        /// </summary>
        public GeometryTile[,] Tiles => _tiles;

        /// <summary>
        /// Gets or sets the greatest height of any tile in this layer.
        /// </summary>
        public int MaxTileHeight { get; set; }

        /// <summary>
        /// Gets or sets the greatest width of any tile in this layer.
        /// </summary>
        public int MaxTileWidth { get; set; }

        /// <summary>
        /// Gets or sets the horizontal scroll multipler.
        /// </summary>
        public float HorizontalScrollMultipler { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset.
        /// </summary>
        public float HorizontalOffset { get; set; }

        /// <summary>
        /// Gets or sets the vertical scroll multipler.
        /// </summary>
        public float VerticalScrollMultipler { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset.
        /// </summary>
        public float VerticalOffset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GeometryTileLayer"/> is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Sets the tiles which make up the layer.
        /// </summary>
        /// <param name="tiles">The tiles.</param>
        public void SetTiles(IEnumerable<GeometryTile> tiles)
        {
            if(tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            var height = 0;
            var width = 0;

            foreach(var tile in tiles)
            {
                _tiles[tile.GridPosition.X, tile.GridPosition.Y] = tile;

                height = Math.Max(tile.Definition.Rect.Size.Y, height);
                width = Math.Max(tile.Definition.Rect.Size.X, width);
            }

            MaxTileWidth = width;
            MaxTileHeight = height;
        }

        /// <summary>
        /// Sets layer properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public virtual void SetProperties(IDictionary<string, string> properties)
        {
            if(properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string val;
            float fVal;

            if(properties.TryGetValue("horiz_scroll_multiplier", out val))
            {
                if(float.TryParse(val, out fVal))
                {
                    HorizontalScrollMultipler = fVal;
                }
            }

            if (properties.TryGetValue("horiz_offset", out val))
            {
                if (float.TryParse(val, out fVal))
                {
                    HorizontalOffset = fVal;
                }
            }

            if (properties.TryGetValue("vert_scroll_multiplier", out val))
            {
                if (float.TryParse(val, out fVal))
                {
                    VerticalScrollMultipler = fVal;
                }
            }

            if (properties.TryGetValue("vert_offset", out val))
            {
                if (float.TryParse(val, out fVal))
                {
                    VerticalOffset = fVal;
                }
            }
        }
    }
}
