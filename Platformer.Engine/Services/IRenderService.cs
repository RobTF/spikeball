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
namespace Platformer.Engine.Services
{
    using System.Collections.Generic;
    using Render;

    /// <summary>
    /// Interface implemented by the render service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.IGameService" />
    public interface IRenderService : IGameService
    {
        /// <summary>
        /// Gets the first renderable in the list.
        /// </summary>
        LinkedListNode<Renderable> First { get; }

        /// <summary>
        /// Draws a line for a specified duration.
        /// </summary>
        /// <param name="line">The line to draw.</param>
        /// <param name="duration">The duration the line should be visible.</param>
        void DrawLine(Line line, double duration);

        /// <summary>
        /// Draws a line for a specified duration.
        /// </summary>
        /// <param name="line">The line to draw.</param>
        /// <param name="r">The red component of the line color.</param>
        /// <param name="g">The green component of the line color.</param>
        /// <param name="b">The blue component of the line color.</param>
        /// <param name="duration">The duration the line should be visible.</param>
        void DrawLine(Line line, byte r, byte g, byte b, double duration);

        /// <summary>
        /// Highlights a geometry tile.
        /// </summary>
        /// <param name="x">The column of the tile.</param>
        /// <param name="y">The row of the tile.</param>
        /// <param name="r">The red component of the line color.</param>
        /// <param name="g">The green component of the line color.</param>
        /// <param name="b">The blue component of the line color.</param>
        /// <param name="duration">The duration the highlight should be visible.</param>
        void HighlightGeometryTile(int x, int y, byte r, byte g, byte b, double duration);

        /// <summary>
        /// Highlights a geometry tile.
        /// </summary>
        /// <param name="x">The column of the tile.</param>
        /// <param name="y">The row of the tile.</param>
        /// <param name="duration">The duration the highlight should be visible.</param>
        void HighlightGeometryTile(int x, int y, double duration);
    }
}
