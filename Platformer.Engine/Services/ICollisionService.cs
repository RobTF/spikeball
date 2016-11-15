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
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface implemented by the collision service.
    /// </summary>
    public interface ICollisionService : IGameService
    {
        /// <summary>
        /// Fires a traceline and collects collision information.
        /// </summary>
        /// <param name="trace">Object containing the trace line parameters.</param>
        /// <returns>
        /// A value containing the result of the trace.
        /// </returns>
        TraceResult TraceLine(TraceQuery trace);

        /// <summary>
        /// Performs collision resolution for an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void PerformCollisions(GameEntity entity);

        /// <summary>
        /// Gets a list of entities inside a box.
        /// </summary>
        /// <param name="rect">A rectangle in world coordinates.</param>
        /// <param name="entityList">A list that will contain the results.</param>
        void GetEntitiesInBox(Rect rect, List<GameEntity> entityList);

        /// <summary>
        /// Gets a list of entities inside a box.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="rect">A rectangle in world coordinates.</param>
        /// <param name="layer">If specified, only entities in the specified vis layer will be returned.</param>
        /// <param name="entityList">A list that will contain the results.</param>
        void GetEntitiesInBox<TEntity>(Rect rect, int? layer, List<TEntity> entityList) where TEntity : GameEntity;
    }
}
