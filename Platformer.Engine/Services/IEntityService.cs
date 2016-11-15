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
    using System.Collections.ObjectModel;
    using Entities;

    /// <summary>
    /// Interface implemented by the entity service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.IGameService" />
    public interface IEntityService : IGameService
    {
        /// <summary>
        /// Gets the first entity in the entities list.
        /// </summary>
        LinkedListNode<GameEntity> First { get; }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="typeName">Name of the entity type as defined by the entity definition.</param>
        /// <param name="position">The position at which the entity will be created.</param>
        /// <returns>The entity that was created.</returns>
        GameEntity CreateEntity(string typeName, Point position);

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to create.</typeparam>
        /// <param name="position">The position at which the entity will be created.</param>
        /// <returns>The entity that was created.</returns>
        TEntity CreateEntity<TEntity>(Point position) where TEntity : GameEntity;

        /// <summary>
        /// Removes an entity from the game.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void KillEntity(GameEntity entity);
    }
}
