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
    using Microsoft.Practices.Unity;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class which implements the entity management service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IEntityService" />
    public class EntityService : GameService, IEntityService
    {
        /// <summary>
        /// The maximum number of entities allowed in the game at any time.
        /// </summary>
        public static readonly int MaxEntities = 2000;

        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        private readonly IUnityContainer _container;
        private readonly IEnumerable<Type> _entityTypes;

        private readonly LinkedList<GameEntity> _entitiesLinked;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityService"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public EntityService(IUnityContainer container)
        {
            if(container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            _container = container;

            _entityTypes =
                from t in Assembly.GetExecutingAssembly().GetTypes()
                where
                    t.IsSubclassOf(typeof(GameEntity))
                select t;

            _entitiesLinked = new LinkedList<GameEntity>();
        }

        /// <summary>
        /// Gets the first entity in the entities list.
        /// </summary>
        public LinkedListNode<GameEntity> First => _entitiesLinked.First;

        /// <summary>
        /// Called before each game frame.
        /// </summary>
        public override void PreStep()
        {
            // spawn any entities waiting
            var node = _entitiesLinked.First;
            while(node != null)
            {
                var ent = node.Value;
                if ((ent.Options & EntityOptions.Spawned) == 0)
                {
                    ent.Spawn();
                }
                node = node.Next;
            }

            base.PreStep();
        }

        /// <summary>
        /// Called after each game frame.
        /// </summary>
        public override void PostStep()
        {
            // remove any killed entities
            var node = _entitiesLinked.First;
            while (node != null)
            {
                var ent = node.Value;
                var next = node.Next;
                if ((ent.Options & EntityOptions.Killed) > 0)
                {
                    _entitiesLinked.Remove(node);
                }

                node = next;
            }

            base.PostStep();
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to create.</typeparam>
        /// <param name="position">The position at which the entity will be created.</param>
        /// <returns>
        /// The entity that was created.
        /// </returns>
        public TEntity CreateEntity<TEntity>(Point position)
            where TEntity : GameEntity
        {
            TEntity retval = null;

            retval = _container.Resolve<TEntity>();
            retval.Id = 0;
            retval.Position = new Point(position.X, position.Y);
            _entitiesLinked.AddLast(retval);
            return retval;
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="typeName">Name of the entity type as defined by the entity definition.</param>
        /// <param name="position">The position at which the entity will be created.</param>
        /// <returns>
        /// The entity that was created.
        /// </returns>
        public GameEntity CreateEntity(string typeName, Point position)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException(Strings.EntityTypeNameMustBeSpecified, nameof(typeName));
            }

            typeName = typeName.ToLower();

            GameEntity retval = null;
            Type entityType = null;

            if (string.IsNullOrEmpty(typeName) || ((entityType = GetEntityTypeByTypeName(typeName)) == null))
            {
                TraceSource.TraceEvent(TraceEventType.Warning, 0, Strings.EntityBadTypeName, typeName, position.X, position.Y);
                return retval;
            }

            Debug.Assert(entityType != null, "Entity should have a type.");

            retval = _container.Resolve(entityType) as GameEntity;
            retval.Id = 0;
            retval.Position = new Point(position.X, position.Y);

            _entitiesLinked.AddLast(retval);
            return retval;
        }

        /// <summary>
        /// Removes an entity from the game.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void KillEntity(GameEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.Killed();
        }

        /// <summary>
        /// Called once when a map is unloaded.
        /// </summary>
        public override void MapUnloaded()
        {
            _entitiesLinked.Clear();
            base.MapUnloaded();
        }

        /// <summary>
        /// Gets the class of an entity represented by the specified type name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns>An entity type, or null if no type was found.</returns>
        private Type GetEntityTypeByTypeName(string typeName)
        {
            foreach (var t in _entityTypes)
            {
                var entityDef = t.GetCustomAttribute<GameEntityDefinitionAttribute>();
                if (string.Equals(entityDef?.TypeName, typeName, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }

            return null;
        }
    }
}
