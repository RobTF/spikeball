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
namespace Platformer.DirectX.Rendering
{
    using Engine;

    public class RenderContext
    {
        /// <summary>
        /// Gets or sets the height each tile will be rendered at.
        /// </summary>
        public int RenderTileHeight { get; set; }

        /// <summary>
        /// Gets or sets the width each tile will be rendered at.
        /// </summary>
        public int RenderTileWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum horizontal number of tiles that will fit on screen at once.
        /// </summary>
        public int TilesWide { get; set; }

        /// <summary>
        /// Gets or sets the maximum vertical number of tiles that will fit on screen at once.
        /// </summary>
        public int TilesHigh { get; set; }

        /// <summary>
        /// Gets or sets the render origin.
        /// </summary>
        public Int32Point Origin { get; set; }

        /// <summary>
        /// Gets or sets the leftmost boundary that will be rendered.
        /// </summary>
        public int LeftBound { get; set; }

        /// <summary>
        /// Gets or sets the topmost boundary that will be rendered.
        /// </summary>
        public int TopBound { get; set; }

        /// <summary>
        /// Gets or sets the first column of tiles that will be rendered, that is the leftmost column.
        /// </summary>
        public int FirstTileCol { get; set; }

        /// <summary>
        /// Gets or sets the first row of tiles that will be rendered, that is the topmost column.
        /// </summary>
        public int FirstTileRow { get; set; }
    }
}
