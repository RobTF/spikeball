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
namespace Platformer.Engine.Collision
{
    using Entities;
    using System;
    using System.Collections.Generic;
    using Resources;

    /// <summary>
    /// Class which implements a quadtree.
    /// </summary>
    public class Quadtree
    {
        private const int MaxObjects = 10;
        private const int MaxLevels = 8;

        private int _level;
        private List<GameEntity> _entities;
        private Rect _bounds;
        private Quadtree[] _nodes;
        private QuadtreePool _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quadtree"/> class.
        /// </summary>
        /// <param name="level">The level of the tree.</param>
        /// <param name="bounds">The tree bounds.</param>
        public Quadtree()
        {
            _entities = new List<GameEntity>();
            _nodes = new Quadtree[4];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quadtree"/> class.
        /// </summary>
        /// <param name="level">The level of the tree.</param>
        /// <param name="bounds">The tree bounds.</param>
        public Quadtree(int level, Rect bounds)
        {
            _level = level;
            _bounds = bounds;
            _entities = new List<GameEntity>();
            _nodes = new Quadtree[4];
        }

        public void Init(int level, Rect bounds, QuadtreePool pool)
        {
            _level = level;
            _bounds = bounds;
            _pool = pool;
            Clear();
        }

        /// <summary>
        /// Clears data from the quad tree.
        /// </summary>
        public void Clear()
        {
            _entities.Clear();
            for(var i = 0; i < _nodes.Length; i++)
            {
                if(_nodes[i] != null)
                {
                    _nodes[i].Clear();
                    _pool.PutObject(_nodes[i]);
                    _nodes[i] = null;
                }
            }
        }

        /// <summary>
        /// Inserts the specified entity into the tree.
        /// </summary>
        /// <param name="entity">A game entity.</param>
        public void Insert(GameEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (_nodes[0] != null)
            {
                var index = GetIndex(entity.WorldRect);
                if(index != -1)
                {
                    _nodes[index].Insert(entity);
                    return;
                }
            }

            _entities.Add(entity);

            if(_entities.Count > MaxObjects && _level < MaxLevels)
            {
                if(_nodes[0] == null)
                {
                    Split();
                }

                var i = 0;
                while(i < _entities.Count)
                {
                    var ent = _entities[i];
                    var index = GetIndex(ent.WorldRect);
                    if(index != -1)
                    {
                        _entities.RemoveAt(i);
                        _nodes[index].Insert(ent);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of entities that potentially contact the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle in which to search.</param>
        /// <param name="entityList">A list which will contain the entities found.</param>
        public void PossibleEntitiesInBox(Rect rect, List<GameEntity> entityList)
        {
            if(_nodes[0] != null)
            {
                var index = GetIndex(rect);
                if(index != -1)
                {
                    _nodes[index].PossibleEntitiesInBox(rect, entityList);
                }
                else
                {
                    for(var i = 0; i < _nodes.Length; i++)
                    {
                        _nodes[i].PossibleEntitiesInBox(rect, entityList);
                    }
                }
            }

            entityList.AddRange(_entities);
        }

        /// <summary>
        /// Splits the quad tree.
        /// </summary>
        private void Split()
        {
            var subWidth = _bounds.Size.X / 2;
            var subHeight = _bounds.Size.Y / 2;
            var x = _bounds.Position.X;
            var y = _bounds.Position.Y;

            _nodes[0] = _pool.GetObject(_level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
            _nodes[1] = _pool.GetObject(_level + 1, new Rect(x, y, subWidth, subHeight));
            _nodes[2] = _pool.GetObject(_level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
            _nodes[3] = _pool.GetObject(_level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
        }

        /// <summary>
        /// Gets the node index of .
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>THe node index, or -1 if the rectangle does not completely fit into a child node.</returns>
        private int GetIndex(Rect rect)
        {
            var index = -1;
            double verticalMidpoint = _bounds.Position.X + (_bounds.Size.X / 2.0);
            double horizontalMidpoint = _bounds.Position.Y + (_bounds.Size.Y / 2.0);

            // Object can completely fit within the top quadrants
            bool topQuadrant = (rect.Position.Y < horizontalMidpoint && rect.Position.Y + rect.Size.Y < horizontalMidpoint);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = (rect.Position.Y > horizontalMidpoint);

            // Object can completely fit within the left quadrants
            if (rect.Position.X < verticalMidpoint && rect.Position.X + rect.Size.X < verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 1;
                }
                else if (bottomQuadrant)
                {
                    index = 2;
                }
            }
            // Object can completely fit within the right quadrants
            else if (rect.Position.X > verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 0;
                }
                else if (bottomQuadrant)
                {
                    index = 3;
                }
            }

            return index;
        }
    }

    public class QuadtreePool : ObjectPool<Quadtree>
    {
        public QuadtreePool()
            :base(() => new Quadtree())
        {
        }

        public Quadtree GetObject(int level, Rect bounds)
        {
            var qt = GetObject();
            qt.Init(level, bounds, this);
            return qt;
        }
    }
}
