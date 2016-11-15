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
    using System;
    using System.IO;
    using Render;
    using Resources;

    /// <summary>
    /// Class which parses a simple 2 bit bitmap.
    /// </summary>
    public class Bitmap
    {
        private Int32Point _size;
        private Color _color1;
        private Color _color2;
        private bool[,] _pixels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="reader">The reader containing the bitmap data.</param>
        public Bitmap(BinaryReader reader)
        {
            if(reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var magic = reader.ReadBytes(2);
            if((magic[0] != 'B') || (magic[1] != 'M'))
            {
                throw new InvalidDataException(Strings.BitmapDataInvalid);
            }

            reader.ReadInt32(); // file size
            reader.ReadInt16(); // reserved 1
            reader.ReadInt16(); // reserved 2

            var pixelStart = reader.ReadInt32();
            reader.ReadInt32(); // header size
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();

            _size = new Int32Point(width, height);

            reader.ReadBytes(28); // skip the rest of the header

            // color table
            var b = reader.ReadByte();
            var g = reader.ReadByte();
            var r = reader.ReadByte();
            reader.ReadByte();
            _color1 = new Color(255, r, g, b);

            b = reader.ReadByte();
            g = reader.ReadByte();
            r = reader.ReadByte();
            reader.ReadByte();
            _color2 = new Color(255, r, g, b);

            // read pixels
            _pixels = new bool[_size.X, _size.Y];
            for(var y = _size.Y - 1; y >= 0; y--)
            {
                for(var x = 0; x < _size.X; x += 8)
                {
                    var pbyte = reader.ReadByte();
                    for (var i = 0; i < 8; i++)
                    {
                        _pixels[x + i, y] = ((pbyte & (1 << 7 - i)) > 0);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size of the bitmap.
        /// </summary>
        public Int32Point Size => _size;

        /// <summary>
        /// Gets the first color of the bitmap.
        /// </summary>
        public Color Color1 => _color1;

        /// <summary>
        /// Gets the second color of the bitmap.
        /// </summary>
        public Color Color2 => _color2;

        /// <summary>
        /// Calculates a vertical height map from a piece of the collision map bitmap.
        /// </summary>
        /// <param name="ignoreColor">Color which acts as "empty" in the map.</param>
        /// <param name="rect">The section of the bitmap to parse.</param>
        /// <returns>An array containing the heightmap</returns>
        public bool[,] CalculateCollisionMap(Color ignoreColor, Int32Rect rect)
        {
            bool ignoreVal = (ignoreColor == Color2); // true = color2, false = color1
            var retval = new bool[rect.Size.X, rect.Size.Y];

            for (var x = rect.Position.X; x < (rect.Position.X + rect.Size.X); x++)
            {
                var localX = x - rect.Position.X;
                for (var y = rect.Position.Y; y < (rect.Position.Y + rect.Size.Y); y++)
                {
                    var localY = y - rect.Position.Y;

                    if (_pixels[x, y] != ignoreVal)
                    {
                        retval[localX, localY] = true;
                    }
                }
            }

            return retval;
        }
    }
}
