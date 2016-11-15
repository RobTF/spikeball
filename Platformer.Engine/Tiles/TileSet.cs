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
    using System.Collections.Generic;

    /// <summary>
    /// Class which represents a collection of tile definitions.
    /// </summary>
    public class TileSet
    {
        private readonly int _id;
        private readonly string _name;
        private readonly string _imagePath;
        private Dictionary<int, TileDefinition> _tileDefinitions;
        private TilesetType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileSet"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="imagePath">The image path.</param>
        public TileSet(TilesetType type, int id, string name, string imagePath)
        {
            _type = type;
            _id = id;
            _name = name;
            _imagePath = imagePath;
            _tileDefinitions = new Dictionary<int, TileDefinition>();
        }

        public TilesetType Type => _type;

        public int Id => _id;

        public string Name => _name;

        public string ImagePath => _imagePath;

        public IDictionary<int, TileDefinition> TileDefinitions => _tileDefinitions;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Id: {_id}; Name: {_name}";
        }
    }
}
