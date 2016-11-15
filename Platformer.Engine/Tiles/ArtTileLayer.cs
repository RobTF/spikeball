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
    /// Class which represents a map layer that contains level artwork.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Tiles.GeometryTileLayer" />
    public class ArtTileLayer : GeometryTileLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtTileLayer" /> class.
        /// </summary>
        /// <param name="map">The map containing the layer.</param>
        public ArtTileLayer(Map map)
            : base(TilesetType.Artwork, map)
        {
        }

        /// <summary>
        /// Gets or sets the sprite vis layer.
        /// </summary>
        public int? VisLayer { get; set; }

        /// <summary>
        /// Sets layer properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public override void SetProperties(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string value;

            if (properties.TryGetValue("vis_layer", out value))
            {
                int ival;
                if (int.TryParse(value, out ival))
                {
                    VisLayer = ival;
                }
            }

            base.SetProperties(properties);
        }
    }
}
