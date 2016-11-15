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
    using System;
    using System.Collections.Generic;
    using Render;

    /// <summary>
    /// Service which allows the rendering of temporary items that don't directly interact with the game.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IRenderService" />
    public class RenderService : GameService, IRenderService
    {
        private readonly IVariableService _variableService;
        private readonly LinkedList<Renderable> _renderables;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderService"/> class.
        /// </summary>
        /// <param name="engine">The game engine.</param>
        public RenderService(IVariableService variableService)
        {
            if(variableService == null)
            {
                throw new ArgumentNullException(nameof(variableService));
            }

            _variableService = variableService;
            _renderables = new LinkedList<Renderable>();
        }

        /// <summary>
        /// Gets the first renderable in the list.
        /// </summary>
        public LinkedListNode<Renderable> First => _renderables.First;

        /// <summary>
        /// Draws a line for a specified duration.
        /// </summary>
        /// <param name="line">The line to draw.</param>
        /// <param name="duration">The duration the line should be visible.</param>
        public void DrawLine(Line line, double duration)
        {
            DrawLine(line, 255, 0, 0, duration);
        }

        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="duration">The duration.</param>
        public void DrawLine(Line line, byte r, byte g, byte b, double duration)
        {
            var visLine = new VisLine();
            visLine.Color = new Color(r, g, b);
            visLine.Line = line;
            visLine.KillTime = _variableService.GlobalTime + duration;
            _renderables.AddLast(visLine);
        }

        /// <summary>
        /// Highlights a geometry tile.
        /// </summary>
        /// <param name="x">The column of the tile.</param>
        /// <param name="y">The row of the tile.</param>
        /// <param name="r">The red component of the line color.</param>
        /// <param name="g">The green component of the line color.</param>
        /// <param name="b">The blue component of the line color.</param>
        /// <param name="duration">The duration the highlight should be visible.</param>
        public void HighlightGeometryTile(int x, int y, byte r, byte g, byte b, double duration)
        {
            var vis = new VisTileHighlight();
            vis.Color = new Color(r, g, b);
            vis.KillTime = _variableService.GlobalTime + duration;
            vis.X = x;
            vis.Y = y;
            _renderables.AddLast(vis);
        }

        /// <summary>
        /// Highlights a geometry tile.
        /// </summary>
        /// <param name="x">The column of the tile.</param>
        /// <param name="y">The row of the tile.</param>
        /// <param name="duration">The duration the highlight should be visible.</param>
        public void HighlightGeometryTile(int x, int y, double duration)
        {
            HighlightGeometryTile(x, y, 255, 0, 0, duration);
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        public override void Step()
        {
            RemoveOldRenderables();
        }

        /// <summary>
        /// Called once when a map is unloaded.
        /// </summary>
        public override void MapUnloaded()
        {
            _renderables.Clear();
            base.MapUnloaded();
        }

        /// <summary>
        /// Walks the list of current renderables, and removes any dead ones.
        /// </summary>
        private void RemoveOldRenderables()
        {
            var time = _variableService.GlobalTime;

            var node = _renderables.First;
            while(node != null)
            {
                if(node.Value.KillTime <= time)
                {
                    var toRemove = node;
                    node = node.Next;
                    _renderables.Remove(toRemove);
                }
                else
                {
                    node = node.Next;
                }
            }
        }
    }
}
