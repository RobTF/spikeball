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
    /// <summary>
    /// Base type for game services.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.IGameService" />
    public abstract class GameService : IGameService
    {
        /// <summary>
        /// Called once when a map is loaded.
        /// </summary>
        /// <param name="map">The map that was loaded.</param>
        public virtual void MapLoaded(Map map)
        {

        }

        /// <summary>
        /// Called before each game frame.
        /// </summary>
        public virtual void PreStep()
        {

        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        public virtual void Step()
        {

        }

        /// <summary>
        /// Called after each game frame.
        /// </summary>
        public virtual void PostStep()
        {

        }

        /// <summary>
        /// Called once when a map is unloaded.
        /// </summary>
        public virtual void MapUnloaded()
        {

        }
    }
}
