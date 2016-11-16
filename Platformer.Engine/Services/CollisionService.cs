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
    using Collision;
    using Entities;
    using Resources;
    using Tiles;

    /// <summary>
    /// Class which implements the collision service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.ICollisionService" />
    public class CollisionService : GameService, ICollisionService
    {
        private readonly IEntityService _entityService;
        private readonly IMapService _mapService;
        private readonly IRenderService _renderService;
        private readonly IVariableService _varService;

        private readonly QuadtreePool _quadPool;
        private readonly ObjectPool<List<GameEntity>> _entListPool;

        private readonly GameVariable<bool> _varShowTracelines;

        private Quadtree _quadTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollisionService" /> class.
        /// </summary>
        /// <param name="entityService">The entity service.</param>
        /// <param name="mapService">The map service.</param>
        /// <param name="renderService">The render service.</param>
        /// <param name="varService">The variable service.</param>
        public CollisionService(IEntityService entityService, IMapService mapService, IRenderService renderService, IVariableService varService)
        {
            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (mapService == null)
            {
                throw new ArgumentNullException(nameof(mapService));
            }

            if (renderService == null)
            {
                throw new ArgumentNullException(nameof(renderService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            _entityService = entityService;
            _mapService = mapService;
            _renderService = renderService;
            _varService = varService;

            _quadPool = new QuadtreePool();
            _entListPool = new ObjectPool<List<GameEntity>>(() => new List<GameEntity>(10));

            _varShowTracelines = _varService.GetVar<bool>("r_showtracelines");
        }

        /// <summary>
        /// Called once when a map is loaded.
        /// </summary>
        /// <param name="map">The map that was loaded.</param>
        public override void MapLoaded(Map map)
        {
            _quadTree = _quadPool.GetObject(0, new Rect(0.0, 0.0, map.Width * map.GeometryTileWidth, map.Height * map.GeometryTileHeight));
            base.MapLoaded(map);
        }

        /// <summary>
        /// Called once when a map is unloaded.
        /// </summary>
        public override void MapUnloaded()
        {
            _quadTree.Clear();
            _quadPool.PutObject(_quadTree);
            _quadTree = null;
            base.MapUnloaded();
        }

        /// <summary>
        /// Called before each game frame.
        /// </summary>
        public override void PreStep()
        {
            _quadTree.Clear();

            var node = _entityService.First;
            while(node != null)
            {
                _quadTree.Insert(node.Value);
                node = node.Next;
            }

            base.PreStep();
        }

        /// <summary>
        /// Gets a list of entities inside a box.
        /// </summary>
        /// <param name="rect">A rectangle in world coordinates.</param>
        /// <param name="entityList">A list that will contain the results.</param>
        public void GetEntitiesInBox(Rect rect, List<GameEntity> entityList)
        {
            GetEntitiesInBox(rect, null, entityList);
        }

        /// <summary>
        /// Gets a list of entities inside a box.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="rect">A rectangle in world coordinates.</param>
        /// <param name="layer">If specified, only entities in the specified vis layer will be returned.</param>
        /// <param name="entityList">A list that will contain the results.</param>
        public void GetEntitiesInBox<TEntity>(Rect rect, int? layer, List<TEntity> entityList)
            where TEntity : GameEntity
        {
            var possibles = _entListPool.GetObject();

            _quadTree.PossibleEntitiesInBox(rect, possibles);

            for(var i = 0; i < possibles.Count; i++)
            {
                var possible = possibles[i];

                if((layer != null) && (possible.VisLayer != layer))
                {
                    continue;
                }

                if (possible is TEntity)
                {
                    if (TestAABB(possible.WorldRect, rect))
                    {
                        entityList.Add((TEntity)possible);
                    }
                }
            }

            possibles.Clear();
            _entListPool.PutObject(possibles);
        }

        /// <summary>
        /// Performs collision resolution for an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void PerformCollisions(GameEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // ignore non-collidables
            if ((entity.Options & EntityOptions.Collidable) == EntityOptions.None)
            {
                return;
            }

            var possibles = _entListPool.GetObject();
            _quadTree.PossibleEntitiesInBox(entity.WorldRect, possibles);

            var collided = _entListPool.GetObject();

            for(var i = 0; i < possibles.Count; i++)
            {
                var found = possibles[i];

                // cant collide with ourselves!
                if(found == entity)
                {
                    continue;
                }

                // check again as the other entity may have switched collision modes since being added to the quad tree
                if ((found.Options & EntityOptions.Collidable) == EntityOptions.None)
                {
                    continue;
                }

                // entities on different collision paths cannot collide
                if((found.CollisionPath != null) && (entity.CollisionPath != null) && (found.CollisionPath != entity.CollisionPath))
                {
                    continue;
                }

                var colliding = TestAABB(entity.WorldRect, found.WorldRect);
                if (colliding)
                {
                    collided.Add(found);
                }
            }

            entity.UpdateCollisions(collided);

            possibles.Clear();
            _entListPool.PutObject(possibles);

            collided.Clear();
            _entListPool.PutObject(collided);
        }

        /// <summary>
        /// Fires a traceline and collects collision information.
        /// </summary>
        /// <param name="trace">Object containing the trace line parameters.</param>
        /// <returns>
        /// A value containing the result of the trace.
        /// </returns>
        public TraceResult TraceLine(TraceQuery trace)
        {
            double x0, y0, x1, y1;
            List<GameEntity> entities = _entListPool.GetObject();

            x0 = trace.Line.Start.X;
            y0 = trace.Line.Start.Y;
            x1 = trace.Line.End.X;
            y1 = trace.Line.End.Y;

            if((trace.Options & TraceLineOptions.IgnoreEntities) == TraceLineOptions.None)
            {
                GetPossibleCollidersForTraceline(trace, entities);
            }

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);

            int x = (int)Math.Floor(x0);
            int y = (int)Math.Floor(y0);

            int n = 1;
            int x_inc, y_inc;
            double error = 0.0D;

            if (dx == 0)
            {
                x_inc = 0;
                error = double.MaxValue;
            }
            else if (x1 > x0)
            {
                x_inc = 1;
                n += (int)Math.Floor(x1) - x;
                error = (Math.Floor(x0) + 1 - x0) * dy;
            }
            else
            {
                x_inc = -1;
                n += x - (int)Math.Floor(x1);
                error = (x0 - Math.Floor(x0)) * dy;
            }

            if (dy == 0)
            {
                y_inc = 0;
                error -= double.MaxValue;
            }
            else if (y1 > y0)
            {
                y_inc = 1;
                n += (int)Math.Floor(y1) - y;
                error -= (Math.Floor(y0) + 1 - y0) * dx;
            }
            else
            {
                y_inc = -1;
                n += y - (int)Math.Floor(y1);
                error -= (y0 - Math.Floor(y0)) * dx;
            }

            var hit = false;
            GeometryTile tile = null;
            GameEntity entity = null;

            for (; n > 0; --n)
            {
                // visit
                if (entities.Count > 0)
                {
                    hit = IsPointInEntity(entities, x, y, trace, out entity);
                }

                if (!hit && ((trace.Options & TraceLineOptions.IgnoreTiles) == TraceLineOptions.None))
                {
                    hit = IsPointOnSolidTile(x, y, tile, trace, out tile);
                }

                if (hit)
                {
                    break;
                }

                if (error > 0)
                {
                    y += y_inc;
                    error -= dx;
                }
                else
                {
                    x += x_inc;
                    error += dy;
                }
            }

            if (_varShowTracelines.Value)
            {
                if (hit)
                {
                    _renderService.DrawLine(new Line(trace.Line.Start.X, trace.Line.Start.Y, x, y), 0);
                    _renderService.DrawLine(new Line(x, y, trace.Line.End.X, trace.Line.End.Y), 0, 0, 255, 0);
                }
                else
                {
                    _renderService.DrawLine(trace.Line, 0);
                }
            }

            entities.Clear();
            _entListPool.PutObject(entities);

            var retval = new TraceResult();
            retval.Hit = hit;
            retval.ContactPoint = new Point(x, y);
            retval.Entity = entity;
            retval.Tile = tile;
            return retval;
        }

        /// <summary>
        /// Determines whether a
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="lastTile">The last tile hit.</param>
        /// <param name="trace">The trace line parameters.</param>
        /// <param name="hitTile">The tile that was hit.</param>
        /// <returns>
        ///   <c>true</c> if point is on a solid tile; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPointOnSolidTile(int x, int y, GeometryTile lastTile, TraceQuery trace, out GeometryTile hitTile)
        {
            hitTile = null;
            bool hit = false;

            var map = _mapService.CurrentMap;

            if (map == null)
            {
                return hit;
            }

            var tileCol = (int)Math.Floor((double)x / map.GeometryTileWidth);
            var tileRow = (int)Math.Floor((double)y / map.GeometryTileHeight);

            if ((tileCol < 0) || (tileRow < 0))
            {
                return hit;
            }

            for(var i = 0; i < map.CollisionLayers.Length; i++)
            {
                var layer = map.CollisionLayers[i];

                // if the trace has specific a collision path, ignore layers that are not on the path
                if (trace.CollisionPath != null)
                {
                    if ((layer.CollisionPath != null) && (layer.CollisionPath != trace.CollisionPath))
                    {
                        continue;
                    }
                }

                var tile = layer.GetTile(tileCol, tileRow);

                if (tile != null)
                {
                    // solidity filters
                    if (
                        (tile.Definition.SolidType == SolidType.None) &&
                        ((trace.Options & TraceLineOptions.SolidOnly) > TraceLineOptions.None))
                    {
                        continue;
                    }

                    if (
                        (tile.Definition.SolidType == SolidType.JumpThrough) &&
                        ((trace.Options & TraceLineOptions.IgnoreJumpThrough) > TraceLineOptions.None))
                    {
                        continue;
                    }

                    var collisionMap = tile.Definition.CollisionMap;
                    if (collisionMap != null)
                    {
                        var localX = x - tile.WorldPosition.X;
                        var localY = y - tile.WorldPosition.Y;

                        if (!hit)
                        {
                            if (collisionMap[localX, localY])
                            {
                                hitTile = tile;
                                hit = true;
                            }
                        }
                    }

                    if (hit)
                    {
                        if (_varShowTracelines.Value)
                        {
                            if ((lastTile == null) || (lastTile.GridPosition.X != tile.GridPosition.X) || (lastTile.GridPosition.Y != tile.GridPosition.Y))
                            {
                                _renderService.HighlightGeometryTile(tileCol, tileRow, 0, 255, 0, 0);
                            }
                        }
                    }
                }
            }

            return hit;
        }

        /// <summary>
        /// Gets the possible set of collidable entities which may contact a trace line.
        /// </summary>
        /// <param name="trace">The trace query.</param>
        /// <param name="entityList">A list that will contain the results.</param>
        private void GetPossibleCollidersForTraceline(TraceQuery trace, List<GameEntity> entityList)
        {
            if(entityList == null)
            {
                throw new ArgumentNullException(nameof(entityList));
            }

            // firstly get a bounding rect that covers the trace line
            double width = Math.Abs(trace.Line.Start.X - trace.Line.End.X);
            double height = Math.Abs(trace.Line.Start.Y - trace.Line.End.Y);

            Point upperLeft = new Point(Math.Min(trace.Line.Start.X, trace.Line.End.X), Math.Min(trace.Line.Start.Y, trace.Line.End.Y));

            var rect = new Rect(upperLeft.X, upperLeft.Y, width, height);

            // query the quad tree
            var possibles = _entListPool.GetObject();
            _quadTree.PossibleEntitiesInBox(rect, possibles);

            // filter results based on trace options
            for(var i = 0; i < possibles.Count; i++)
            {
                var ent = possibles[i];

                if (ent == trace.Ignore)
                {
                    continue;
                }

                // ignore non collidables
                if ((ent.Options & EntityOptions.Collidable) == EntityOptions.None)
                {
                    continue;
                }

                // solidity filters
                if (
                    (ent.SolidType == SolidType.None) &&
                    ((trace.Options & TraceLineOptions.SolidOnly) > TraceLineOptions.None))
                {
                    continue;
                }

                if (ent.SolidType == SolidType.JumpThrough)
                {
                    if ((trace.Options & TraceLineOptions.IgnoreJumpThrough) > TraceLineOptions.None)
                    {
                        continue;
                    }
                }

                // type filter
                if ((trace.EntityType != null) && (ent.GetType().IsAssignableFrom(trace.EntityType)))
                {
                    continue;
                }

                // collision path filter
                if((ent.CollisionPath != null) && (trace.CollisionPath != null) && (ent.CollisionPath != trace.CollisionPath))
                {
                    continue;
                }

                entityList.Add(ent);
            }

            possibles.Clear();
            _entListPool.PutObject(possibles);
        }

        /// <summary>
        /// Determines whether the given point is within the bounding box of an entity.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="trace">The trace.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if [is point in entity] [the specified x]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPointInEntity(List<GameEntity> entityList, int x, int y, TraceQuery trace, out GameEntity entity)
        {
            entity = null;

            for(var i = 0; i < entityList.Count; i++)
            {
                var ent = entityList[i];
                var rect = ent.WorldRect;
                if(
                    (x >= rect.Position.X && x <= rect.Position.X + rect.Size.X) &&
                    (y >= rect.Position.Y && y <= rect.Position.Y + rect.Size.Y))
                {
                    entity = ent;
                    break;
                }
            }

            return entity != null;
        }

        /// <summary>
        /// Tests whether two bounding boxes are overlapping.
        /// </summary>
        /// <param name="a">The first bounding box.</param>
        /// <param name="b">The second bounding box.</param>
        /// <returns><c>true</c> if the boxes are overlapping, else <c>false</c>.</returns>
        /// <remarks>https://developer.mozilla.org/en-US/docs/Games/Techniques/2D_collision_detection</remarks>
        private bool TestAABB(Rect a, Rect b)
        {
            if (a.Position.X <= b.Position.X + b.Size.X &&
               a.Position.X + a.Size.X >= b.Position.X &&
               a.Position.Y <= b.Position.Y + b.Size.Y &&
               a.Size.Y + a.Position.Y >= b.Position.Y)
            {
                return true;
            }

            return false;
        }
    }
}
