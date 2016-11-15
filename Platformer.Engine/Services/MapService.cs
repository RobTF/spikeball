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
    using System.Diagnostics;
    using System.Linq;
    using Resources;
    using TiledSharp;
    using Tiles;
    using System.IO;
    using Render;
    using Collision;
    using Entities;
    using Movement;

    /// <summary>
    /// Class which implements the map service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IMapService" />
    public class MapService : GameService, IMapService
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        private readonly IEntityService _entityService;
        private readonly IGameEngine _engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapService" /> class.
        /// </summary>
        /// <param name="entityService">The entity service.</param>
        /// <param name="engine">The engine.</param>
        public MapService(IEntityService entityService, IGameEngine engine)
        {
            if(entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            _entityService = entityService;
            _engine = engine;
        }

        /// <summary>
        /// Gets the current map, if a map is being played.
        /// </summary>
        public Map CurrentMap => _engine.CurrentMap;

        /// <summary>
        /// Loads a map.
        /// </summary>
        /// <param name="fileName">Name of the map file.</param>
        public Map LoadMap(string fileName)
        {
            var rawMap = new TmxMap(fileName);

            var map = new Map();
            map.Name = string.Empty;
            map.Width = rawMap.Width;
            map.Height = rawMap.Height;
            map.GeometryTileHeight = rawMap.TileHeight;
            map.GeometryTileWidth = rawMap.TileWidth;

            var tileSets = new List<TileSet>();
            var tileDefinitions = new Dictionary<int, TileDefinition>();
            var geometryLayers = new List<GeometryTileLayer>();

            // process tilesets and the tile definitions
            foreach (var rawTileSet in rawMap.Tilesets)
            {
                TilesetType setType;
                var nameUpper = rawTileSet.Name.ToUpperInvariant();

                if(nameUpper.Contains("COLLISION"))
                {
                    setType = TilesetType.CollisionMap;
                }
                else
                {
                    setType = TilesetType.Artwork;
                }

                var tileSet = new TileSet(setType, tileSets.Count, rawTileSet.Name, rawTileSet.Image.Source);
                ProcessTileSet(rawTileSet, tileSet);
                tileSets.Add(tileSet);

                // add the definitions to the global dictionary
                foreach (var tileDefinition in tileSet.TileDefinitions)
                {
                    tileDefinitions.Add(tileDefinition.Key, tileDefinition.Value);
                }
            }

            map.TileSets = tileSets.ToArray();

            // process geometry layers
            foreach (var rawLayer in rawMap.Layers)
            {
                var nameUpper = rawLayer.Name.ToUpperInvariant();

                GeometryTileLayer layer;

                if (nameUpper.Contains("COLLISION"))
                {
                    layer = new CollisionTileLayer(map);
                }
                else
                {
                    layer = new ArtTileLayer(map);
                }

                layer.Name = rawLayer.Name;
                layer.SetProperties(rawLayer.Properties);
                layer.Visible = rawLayer.Visible;
                ProcessGeometryLayer(rawLayer, layer, map, tileDefinitions);
                geometryLayers.Add(layer);
            }

            map.GeometryLayers = geometryLayers.ToArray();
            map.ArtLayers = geometryLayers.OfType<ArtTileLayer>().ToArray();
            map.CollisionLayers = geometryLayers.OfType<CollisionTileLayer>().ToArray();

            // process object layers
            foreach (var objGroup in rawMap.ObjectGroups)
            {
                ProcessObjectGroup(objGroup, map);
            }

            rawMap = null;
            GC.Collect();
            return map;
        }

        /// <summary>
        /// Processes a Tiled tile set into a geometry tile set used by the engine.
        /// </summary>
        /// <param name="rawTileSet">A Tiled tile set.</param>
        /// <param name="tileSet">The game engine tile set.</param>
        private void ProcessTileSet(TmxTileset rawTileSet, TileSet tileSet)
        {
            if (rawTileSet == null)
            {
                throw new ArgumentNullException(nameof(rawTileSet));
            }

            TraceSource.TraceEvent(TraceEventType.Verbose, 0, $"Processing tileset {rawTileSet.Name}...");

            if (rawTileSet.Image == null)
            {
                throw new LevelLoadException(Strings.TilesetHasNoImage);
            }

            var tileDefinitions = new List<TileDefinition>();

            Bitmap collisionBitmap = null;
            if (tileSet.Type == TilesetType.CollisionMap)
            {
                using (var file = File.Open(rawTileSet.Image.Source, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var br = new BinaryReader(file))
                    {
                        collisionBitmap = new Bitmap(br);
                    }
                }
            }

            // now process each of the individual tile definitions
            var id = rawTileSet.FirstGid;
            for (var y = 0; y < rawTileSet.Image.Height; y += rawTileSet.TileHeight)
            {
                // make sure we don't treat leftover space as a tile if the tiles didn't fit exactly
                if((rawTileSet.Image.Height - y) < rawTileSet.TileHeight)
                {
                    continue;
                }

                for (var x = 0; x < rawTileSet.Image.Width; x += rawTileSet.TileWidth)
                {
                    if ((rawTileSet.Image.Width - x) < rawTileSet.TileWidth)
                    {
                        continue;
                    }

                    if (rawTileSet.TileCount.HasValue)
                    {
                        if (id == (rawTileSet.FirstGid + rawTileSet.TileCount.Value))
                        {
                            return;
                        }
                    }

                    var tile = new TileDefinition();
                    tile.Id = id++;
                    tile.TileSet = tileSet;
                    tile.Rect = new Int32Rect(x, y, rawTileSet.TileWidth, rawTileSet.TileHeight);
                    tile.SolidType = SolidType.FullSolid; // tiles are fully solid by default

                    if (collisionBitmap != null)
                    {
                        tile.CollisionMap = collisionBitmap.CalculateCollisionMap(new Color(255, 255, 255), tile.Rect);
                    }

                    var rawTileDefinition = rawTileSet.Tiles.SingleOrDefault(t => t.Id == tile.Id - rawTileSet.FirstGid);
                    if (rawTileDefinition != null)
                    {
                        // general angle value which is true for all movement modes
                        string val;
                        if (rawTileDefinition.Properties.TryGetValue("angle", out val))
                        {
                            int angleVal;
                            if (int.TryParse(val, out angleVal))
                            {
                                for (var i = 0; i < 4; i++)
                                {
                                    tile.Angles[i] = angleVal;
                                }
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Bad angle value on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }

                        if (rawTileDefinition.Properties.TryGetValue("angle_floor", out val))
                        {
                            int angleVal;
                            if (int.TryParse(val, out angleVal))
                            {
                                tile.Angles[(int)PlayerMovementMode.Floor] = angleVal;
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Bad angle_floor value on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }

                        if (rawTileDefinition.Properties.TryGetValue("angle_rightwall", out val))
                        {
                            int angleVal;
                            if (int.TryParse(val, out angleVal))
                            {
                                tile.Angles[(int)PlayerMovementMode.RightWall] = angleVal;
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Bad angle_rightwall value on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }

                        if (rawTileDefinition.Properties.TryGetValue("angle_leftwall", out val))
                        {
                            int angleVal;
                            if (int.TryParse(val, out angleVal))
                            {
                                tile.Angles[(int)PlayerMovementMode.LeftWall] = angleVal;
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Bad angle_leftwall value on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }

                        if (rawTileDefinition.Properties.TryGetValue("angle_ceiling", out val))
                        {
                            int angleVal;
                            if (int.TryParse(val, out angleVal))
                            {
                                tile.Angles[(int)PlayerMovementMode.Ceiling] = angleVal;
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Bad angle_ceiling value on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }

                        if (rawTileDefinition.Properties.TryGetValue("solid_type", out val))
                        {
                            int optsVal;
                            if (int.TryParse(val, out optsVal))
                            {
                                tile.SolidType = (SolidType)int.Parse(val);
                            }
                            else
                            {
                                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Invalid solid options configured on tile [{tile}] in tileset [{tileSet}].");
                            }
                        }
                    }

                    tileSet.TileDefinitions.Add(tile.Id, tile);
                }
            }
        }

        /// <summary>
        /// Processes the geometry layer, creating the game representation.
        /// </summary>
        /// <param name="rawLayer">The layer.</param>
        /// <param name="map">The map.</param>
        /// <param name="tileDefinitions">A global dictionary of all tile definitions.</param>
        /// <returns>
        /// An instance of a new <see cref="GeometryTileLayer" />.
        /// </returns>
        private void ProcessGeometryLayer(TmxLayer rawLayer, GeometryTileLayer layer, Map map, IDictionary<int, TileDefinition> tileDefinitions)
        {
            if (rawLayer == null)
            {
                throw new ArgumentNullException(nameof(rawLayer));
            }

            if (layer == null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (tileDefinitions == null)
            {
                throw new ArgumentNullException(nameof(tileDefinitions));
            }

            TraceSource.TraceEvent(TraceEventType.Verbose, 0, $"Processing layer {rawLayer.Name}");

            var tiles = new List<GeometryTile>();

            foreach (var tile in rawLayer.Tiles)
            {
                if (tile.Gid != 0)
                {
                    var geoTile = new GeometryTile();
                    geoTile.GridPosition = new Int32Point(tile.X, tile.Y);
                    geoTile.WorldPosition = new Int32Point(tile.X * map.GeometryTileWidth, tile.Y * map.GeometryTileHeight);
                    geoTile.Definition = tileDefinitions[tile.Gid];
                    geoTile.DefinitionId = tile.Gid;
                    tiles.Add(geoTile);
                }
            }

            layer.SetTiles(tiles);
        }

        /// <summary>
        /// Processes the object group, and creates the entities the objects represent.
        /// </summary>
        /// <param name="rawGroup">The raw object group data.</param>
        /// <param name="map">The map.</param>
        private void ProcessObjectGroup(TmxObjectGroup rawGroup, Map map)
        {
            if (rawGroup == null)
            {
                throw new ArgumentNullException(nameof(rawGroup));
            }

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            string val;

            int i;
            int visLayer = 0;
            int? collisionPath = null;
            if(rawGroup.Properties.TryGetValue("vis_layer", out val))
            {
                if(int.TryParse(val, out i))
                {
                    visLayer = i;
                }
            }

            if (rawGroup.Properties.TryGetValue("collision_path", out val))
            {
                if (int.TryParse(val, out i))
                {
                    collisionPath = i;
                }
            }

            foreach (var obj in rawGroup.Objects)
            {
                var centerOrigin = new Point(obj.X + (obj.Width / 2.0D), obj.Y - (obj.Height / 2.0D));
                var collisionBox = new BoundingBox(-(obj.Width / 2.0), -(obj.Height / 2.0), (obj.Width / 2.0), (obj.Height / 2.0));

                if (string.IsNullOrEmpty(obj.Type))
                {
                    TraceSource.TraceEvent(TraceEventType.Warning, 0, Strings.EntityBadTypeName, obj.Type, centerOrigin.X, centerOrigin.Y);
                }
                else
                {
                    var ent = _entityService.CreateEntity(obj.Type, centerOrigin);
                    if (ent != null)
                    {
                        var animatable = ent as Animatable;
                        ent.VisLayer = visLayer;
                        ent.CollisionPath = collisionPath;
                        ent.CollisionBox = collisionBox;
                        ent.SetProperties(obj.Properties); // load key/value pairs from map data
                    }
                }
            }
        }
    }
}
