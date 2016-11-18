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
    using Engine.Entities;
    using Engine.Services;
    using Engine.Tiles;
    using Resources;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using SharpDX.Mathematics.Interop;
    using System;
    using SharpDX;
    using SharpDX.Direct2D1;
    using Engine.Render;
    using System.Collections.Generic;
    using SharpDX.DirectWrite;
    using Engine.Gameplay.Entities;

    /// <summary>
    /// Class which implements the game renderer.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Renderer : IDisposable
    {
        private readonly TextureResourceManager _texResMan;
        private readonly SpriteResourceManager _spriteResMan;

        private SharpDX.Direct3D11.Device1 _device;
        private SharpDX.Direct3D11.DeviceContext1 _d3dContext;
        private SharpDX.Direct2D1.DeviceContext1 _d2dContext;
        private SwapChain1 _swapChain;
        private SharpDX.Direct2D1.Bitmap1 _d2dTarget;
        private SharpDX.Direct2D1.Factory2 _d2dFactory;
        private Surface _backBuffer;

        // hud
        private SharpDX.DirectWrite.Factory _dwFactory;
        private ResourceFontLoader _fontLoader;
        private FontCollection _fontCollection;
        private SolidColorBrush _hudYellow;
        private SolidColorBrush _hudWhite;
        private TextFormat _hudTextFormat;

        private GameVariable<bool> _varShowTileFrames;
        private GameVariable<bool> _varShowCollisionMaps;
        private GameVariable<bool> _varShowEntityOrigins;
        private GameVariable<bool> _varShowTraceLines;
        private GameVariable<bool> _varShowCollisionBoxes;

        private IMapService _mapService;
        private IVariableService _varService;
        private IEntityService _entityService;
        private IRenderService _renderService;
        private ICollisionService _collisionService;

        private List<Animatable> _spriteRenderList;

        private GameForm _form;

        private bool _resize;
        private RenderContext _rc;
        private IGameEngine _game;
        private int _height;
        private int _width;
        private float _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer" /> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        /// <param name="form">The form.</param>
        /// <param name="width">The primary width of the view in pixels.</param>
        /// <param name="height">The primary height of the view in pixels.</param>
        public Renderer(IGameEngine gameEngine, GameForm form, int width, int height)
        {
            if(gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            if(form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            _form = form;
            _resize = false;
            _scale = 1.0f;
            _width = width;
            _height = height;

            _dwFactory = new SharpDX.DirectWrite.Factory();
            InitFonts();

            // DeviceCreationFlags.BgraSupport must be enabled to allow Direct2D interop.
            SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport);

            // Query the default device for the supported device and context interfaces.
            _device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            _d3dContext = _device.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            // Query for the adapter and more advanced DXGI objects.
            using (var dxgiDevice2 = _device.QueryInterface<SharpDX.DXGI.Device2>())
            {
                _d2dFactory = new SharpDX.Direct2D1.Factory2(SharpDX.Direct2D1.FactoryType.SingleThreaded);

                // Get the default Direct2D device and create a context.
                using (var d2dDevice = new SharpDX.Direct2D1.Device1(_d2dFactory, dxgiDevice2))
                {
                    _d2dContext = new SharpDX.Direct2D1.DeviceContext1(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
                }
            }

            CreateSizeDependentResources();

            _d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Aliased;
            _d2dContext.AntialiasMode = AntialiasMode.Aliased;
            _d2dContext.UnitMode = UnitMode.Pixels;

            _hudYellow = new SolidColorBrush(_d2dContext, new Color4(1.0f, 1.0f, 0.0f, 1.0f));
            _hudWhite = new SolidColorBrush(_d2dContext, new Color4(1.0f, 1.0f, 1.0f, 1.0f));
            _hudTextFormat = new TextFormat(_dwFactory, "Sonic Genesis/Mega Drive Font", _fontCollection, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 14);

            // init game stuff
            _game = gameEngine;

            _texResMan = new TextureResourceManager(gameEngine, _d2dContext);
            _spriteResMan = new SpriteResourceManager(gameEngine, _d2dContext);

            // get the services
            _mapService = _game.GetService<IMapService>();
            _varService = _game.GetService<IVariableService>();
            _entityService = _game.GetService<IEntityService>();
            _renderService = _game.GetService<IRenderService>();
            _collisionService = _game.GetService<ICollisionService>();

            _varShowCollisionMaps = _varService.GetVar<bool>("r_showcollisionmaps");
            _varShowTileFrames = _varService.GetVar<bool>("r_showtileframes");
            _varShowEntityOrigins = _varService.GetVar<bool>("r_showentityorigins");
            _varShowTraceLines = _varService.GetVar<bool>("r_showtracelines");
            _varShowCollisionBoxes = _varService.GetVar<bool>("r_showcollisionboxes");

            _rc = new RenderContext();

            _spriteRenderList = new List<Animatable>(25);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Renderer" /> class.
        /// </summary>
        ~Renderer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the of the renderer.
        /// </summary>
        public float Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    _resize = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the camera used by the renderer.
        /// </summary>
        public Camera Camera { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Renders the game.
        /// </summary>
        public void Render()
        {
            if(_resize)
            {
                CreateSizeDependentResources();
                _resize = false;
            }

            BeginDraw();
            Draw();
            EndDraw();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                _hudTextFormat.Dispose();
                _hudYellow.Dispose();
                _hudWhite.Dispose();

                _fontCollection.Dispose();
                _fontLoader.Dispose();
                _dwFactory.Dispose();

                _texResMan.Dispose();
                _spriteResMan.Dispose();

                _swapChain.Dispose();
                _d2dTarget.Dispose();
                _d2dFactory.Dispose();
                _d3dContext.Dispose();
                _d2dContext.Dispose();
                _device.Dispose();
            }
        }

        /// <summary>
        /// Configures the render context for drawing the specified layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        private void ConfigureContextForLayer(GeometryTileLayer layer)
        {
            if(layer == null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            var map = _mapService.CurrentMap;

            /*
             * Get the number of map tiles each layer tile covers.
             * This is becasue the map layers may use tiles which are larger
             * than the map tiles, map tiles are usually 16x16, but layers 
             * can have tiles up to 128x128.
             * 
             * If we don't take this into account the rendering will clip
             * tiles larger than 16x16 at the wrong time, causing them to
             * "pop" in and out as the player moves.
             */
            var widthMultiplier = (layer.MaxTileWidth / map.GeometryTileWidth);
            var heightMultiplier = (layer.MaxTileHeight / map.GeometryTileHeight);

            // get the width and height of each tile to be rendered
            _rc.RenderTileWidth = map.GeometryTileWidth;
            _rc.RenderTileHeight = map.GeometryTileHeight;

            // maximum number of tiles we will render
            _rc.TilesWide = (int)Math.Ceiling((double)_width / map.GeometryTileWidth) + 1;
            _rc.TilesHigh = (int)Math.Ceiling((double)_height / map.GeometryTileHeight) + 1;

            _rc.TilesWide *= widthMultiplier;
            _rc.TilesHigh *= heightMultiplier;

            // actual x/y render start point - may be slightly beyond screen bounds
            var x = (Camera.Position.X * layer.HorizontalScrollMultipler) + layer.HorizontalOffset;
            var y = (Camera.Position.Y * layer.VerticalScrollMultipler) + layer.VerticalOffset;
            _rc.Origin = new Int32Point((int)x, (int)y);

            // determine the leftmost and topmost boundaries we will be drawing
            _rc.LeftBound = _rc.Origin.X - (_width / 2);
            _rc.TopBound = _rc.Origin.Y - (_height / 2);

            // determine the row and column of the leftmost and topmost tiles we will be drawing
            _rc.FirstTileCol = _rc.LeftBound / _rc.RenderTileWidth - widthMultiplier;
            _rc.FirstTileRow = _rc.TopBound / _rc.RenderTileHeight - heightMultiplier;

            // clamp
            _rc.FirstTileCol = Math.Max(0, _rc.FirstTileCol);
            _rc.FirstTileRow = Math.Max(0, _rc.FirstTileRow);
        }

        /// <summary>
        /// Begins the drawing.
        /// </summary>
        private void BeginDraw()
        {
            _d2dContext.BeginDraw();
        }

        /// <summary>
        /// Performs the game scene drawing.
        /// </summary>
        private void Draw()
        {
            _d2dContext.Clear(SharpDX.Color.Black);

            if(!_game.IsRunning)
            {
                // no game running - just draw a note
                var tf =new TextFormat(_dwFactory, "Sonic Genesis/Mega Drive Font", _fontCollection, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 18);
                tf.TextAlignment = TextAlignment.Center;
                tf.ParagraphAlignment = ParagraphAlignment.Center;

                _d2dContext.Transform = Matrix3x2.Scaling(_scale);

                var rect = new RectangleF(0, 0, _width, _height);
                _d2dContext.DrawText($"No Map Loaded", tf, rect, _hudWhite);

                tf.Dispose();
                return;
            }

            // renders tiles, sprites etc.
            RenderLayers();

            // reposition the drawing based on camera position alone
            _rc.Origin = new Int32Point((int)Camera.Position.X, (int)Camera.Position.Y);
            _rc.LeftBound = _rc.Origin.X - (_width / 2);
            _rc.TopBound = _rc.Origin.Y - (_height / 2);
            _d2dContext.Transform = Matrix3x2.Translation(-_rc.LeftBound, -_rc.TopBound);
            _d2dContext.Transform *= Matrix3x2.Scaling(_scale);

            // temps
            RenderRenderables();

            // for debug purposes only
            RenderDebugMarkers();

            _d2dContext.Transform = Matrix3x2.Identity;
            _d2dContext.Transform *= Matrix3x2.Scaling(_scale);

            RenderHud();
        }

        /// <summary>
        /// Completes drawing.
        /// </summary>
        private void EndDraw()
        {
            _d2dContext.EndDraw();
            _swapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Renders the layers that make up the scene.
        /// </summary>
        private void RenderLayers()
        {
            var map = _mapService.CurrentMap;

            for(var i = 0; i < map.GeometryLayers.Length; i++)
            {
                var layer = map.GeometryLayers[i];

                if (layer.Type == TilesetType.CollisionMap)
                {
                    if (!_varShowCollisionMaps.Value)
                    {
                        continue;
                    }
                }

                ConfigureContextForLayer(layer);
                _d2dContext.Transform = Matrix3x2.Translation(-_rc.LeftBound, -_rc.TopBound);
                _d2dContext.Transform *= Matrix3x2.Scaling(_scale);

                if (layer.Visible)
                {
                    for (var y = _rc.FirstTileRow; y <= _rc.FirstTileRow + _rc.TilesHigh; y++)
                    {
                        for (var x = _rc.FirstTileCol; x <= _rc.FirstTileCol + _rc.TilesWide; x++)
                        {
                            var tile = layer.GetTile(x, y);
                            if (tile == null)
                            {
                                // empty tile
                                continue;
                            }

                            var bitmap = _texResMan.GetTextureForTileSet(tile.Definition.TileSet.Id);

                            var renderX = (tile.GridPosition.X * _rc.RenderTileWidth);
                            var renderY = (tile.GridPosition.Y * _rc.RenderTileHeight) - (tile.Definition.Rect.Size.Y - _rc.RenderTileHeight);

                            float opacity = 1.0f;

                            if (layer.Type == TilesetType.CollisionMap)
                            {
                                opacity = 0.5f;
                            }

                            _d2dContext.DrawBitmap(
                                bitmap,
                                new RectangleF(renderX, renderY, tile.Definition.Rect.Size.X, tile.Definition.Rect.Size.Y),
                                opacity,
                                SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
                                new RectangleF(
                                    tile.Definition.Rect.Position.X,
                                    tile.Definition.Rect.Position.Y,
                                    tile.Definition.Rect.Size.X,
                                    tile.Definition.Rect.Size.Y));
                        }
                    }
                }

                // artwork layer? render the sprites in the same vislayer
                if (layer.Type == TilesetType.Artwork)
                {
                    var visLayer = ((ArtTileLayer)layer).VisLayer;
                    if (visLayer != null)
                    {
                        RenderSprites(visLayer.Value);
                    }
                }

                _d2dContext.Transform = Matrix3x2.Identity;
            }
        }

        /// <summary>
        /// Renders the sprites in a layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        private void RenderSprites(int layer)
        {
            _spriteRenderList.Clear();
            _collisionService.GetEntitiesInBox(new Engine.Rect(_rc.LeftBound, _rc.TopBound, _width, _height), layer, _spriteRenderList);
            _spriteRenderList.Sort((a, b) => a.RenderPriority - b.RenderPriority);

            for(var i = 0; i < _spriteRenderList.Count; i++)
            {
                var entity = _spriteRenderList[i];

                // only render entities that have spawned
                if ((entity.Options & EntityOptions.Spawned) == EntityOptions.None)
                {
                    continue;
                }

                if ((entity.Options & EntityOptions.Visible) == EntityOptions.None)
                {
                    continue;
                }

                RawRectangleF frameRect;
                var bitmap = _spriteResMan.GetImageForSprite(entity.Sprite, entity.Animation, entity.CurrentAnimSequenceIndex, out frameRect);

                var bitmapWidth = (int)(frameRect.Right - frameRect.Left);
                var bitmapHeight = (int)(frameRect.Bottom - frameRect.Top);

                var xPos = entity.RenderPosition.X;
                var yPos = entity.RenderPosition.Y;

                var renderX = xPos - (bitmapWidth / 2);
                var renderY = yPos - (bitmapHeight / 2);

                var oldTransform = _d2dContext.Transform;

                Matrix3x2 trans;

                if (entity.FlipHorizontally)
                {
                    // flipped? perform a negative scale
                    trans = Matrix3x2.Scaling(-1.0f, 1.0f, new Vector2(xPos, yPos));
                }
                else
                {
                    trans = Matrix3x2.Scaling(1.0f, 1.0f, new Vector2(xPos, yPos));
                }

                var rads = (float)(entity.Angle * (Math.PI / 180.0));

                trans = Matrix3x2.Multiply(
                    trans,
                    Matrix3x2.Rotation(
                        -rads,
                        new Vector2(xPos, yPos)));

                trans = Matrix3x2.Multiply(
                    trans,
                    Matrix3x2.Translation(-_rc.LeftBound, -_rc.TopBound));

                trans *= Matrix3x2.Scaling(_scale);

                _d2dContext.Transform = trans;

                _d2dContext.DrawBitmap(
                    bitmap,
                    new RawRectangleF(renderX, renderY, renderX + bitmapWidth, renderY + bitmapHeight),
                    1.0f,
                    SharpDX.Direct2D1.InterpolationMode.NearestNeighbor,
                    frameRect,
                    null);

                _d2dContext.Transform = oldTransform;
            }
        }

        /// <summary>
        /// Renders the temporary renderables.
        /// </summary>
        private void RenderRenderables()
        {
            var node = _renderService.First;
            while(node != null)
            {
                var renderable = node.Value;
                if (renderable is VisLine)
                {
                    var visLine = renderable as VisLine;
                    var line = visLine.Line;

                    using (var brush = new SolidColorBrush(_d2dContext, new Color4(visLine.Color.R / 255, visLine.Color.G / 255, visLine.Color.B / 255, visLine.Color.A / 255)))
                    {
                        _d2dContext.DrawLine(
                            new Vector2((float)line.Start.X, (float)line.Start.Y),
                            new Vector2((float)line.End.X, (float)line.End.Y),
                            brush,
                            1.5f);
                    }
                }
                else if (renderable is VisTileHighlight)
                {
                    var vis = renderable as VisTileHighlight;
                    var renderX = (vis.X * _rc.RenderTileWidth);
                    var renderY = (vis.Y * _rc.RenderTileHeight);

                    using (var brush = new SolidColorBrush(_d2dContext, new Color4(vis.Color.R / 255, vis.Color.G / 255, vis.Color.B / 255, vis.Color.A / 255)))
                    {
                        _d2dContext.DrawRectangle(
                            new RawRectangleF(renderX, renderY, renderX + _rc.RenderTileWidth, renderY + _rc.RenderTileHeight),
                            brush,
                            1.5f);
                    }
                }

                node = node.Next;
            }
        }

        /// <summary>
        /// Renders the debug markers.
        /// </summary>
        private void RenderDebugMarkers()
        {
            var tileFrameBrush = new SolidColorBrush(_d2dContext, new Color4(1.0f, 1.0f, 1.0f, 0.78f));
            var entityOriginBrush = new SolidColorBrush(_d2dContext, new Color4(0.71f, 1.0f, 0.0f, 1.0f));
            var collisionBoxBrush = new SolidColorBrush(_d2dContext, new Color4(0.71f, 1.0f, 0.0f, 1.0f));

            var map = _mapService.CurrentMap;

            if (_varShowTileFrames.Value)
            {
                for (var i = 0; i < map.GeometryLayers.Length; i++)
                {
                    var layer = map.GeometryLayers[i];
                    for (var y = _rc.FirstTileRow; y <= _rc.FirstTileRow + _rc.TilesHigh; y++)
                    {
                        for (var x = _rc.FirstTileCol; x <= _rc.FirstTileCol + _rc.TilesWide; x++)
                        {
                            var tile = layer.GetTile(x, y);
                            if (tile == null)
                            {
                                // empty tile
                                continue;
                            }

                            var renderX = (tile.GridPosition.X * _rc.RenderTileWidth);
                            var renderY = (tile.GridPosition.Y * _rc.RenderTileHeight);

                            if (_varShowTileFrames.Value)
                            {
                                _d2dContext.DrawRectangle(
                                    new RawRectangleF(renderX, renderY, renderX + _rc.RenderTileWidth, renderY + _rc.RenderTileHeight),
                                    tileFrameBrush,
                                    1.5f);
                            }
                        }
                    }
                }
            }

            if (_varShowEntityOrigins.Value || _varShowCollisionBoxes.Value)
            {
                GameEntity entity;
                var node = _entityService.First;
                while(node != null)
                {
                    entity = node.Value;

                    if (_varShowEntityOrigins.Value)
                    {
                        _d2dContext.FillEllipse(
                            new Ellipse(new Vector2((float)entity.Position.X, (float)entity.Position.Y), 2.0f, 2.0f),
                            entityOriginBrush);
                    }

                    if (_varShowCollisionBoxes.Value)
                    {
                        var startX = (float)(entity.Position.X + entity.CollisionBox.Mins.X);
                        var startY = (float)(entity.Position.Y + entity.CollisionBox.Mins.Y);
                        var size = entity.CollisionBox.GetSize();

                        _d2dContext.DrawRectangle(
                            new RawRectangleF(startX, startY, startX + (float)size.X, startY + (float)size.Y),
                            collisionBoxBrush,
                            1.5f);
                    }

                    node = node.Next;
                }
            }

            entityOriginBrush.Dispose();
            collisionBoxBrush.Dispose();
            tileFrameBrush.Dispose();
        }

        /// <summary>
        /// Draws the HUD.
        /// </summary>
        private void RenderHud()
        {
            var rect = new RectangleF(10, 20, 100, 100);
            _d2dContext.DrawText("TIME:", _hudTextFormat, rect, _hudYellow);

            var time = _varService.GlobalTime;
            var mins = Math.Floor((time / 60.0) % 60).ToString("##0");
            var secs = (time % 60).ToString("00");

            rect = new RectangleF(60, 20, 100, 100);
            _d2dContext.DrawText($"{mins}:{secs}", _hudTextFormat, rect, _hudWhite);

            Player player = null;
            var node = _entityService.First;
            while (node != null)
            {
                if (node.Value is Player)
                {
                    player = (Player)node.Value;
                    break;
                }
                node = node.Next;
            }

            if (player != null)
            {
                rect = new RectangleF(10, 35, 100, 100);
                _d2dContext.DrawText("RINGS:", _hudTextFormat, rect, _hudYellow);

                rect = new RectangleF(60, 35, 100, 100);
                _d2dContext.DrawText(player.Rings.ToString(), _hudTextFormat, rect, _hudWhite);
            }
        }

        /// <summary>
        /// Initializes the fonts.
        /// </summary>
        private void InitFonts()
        {
            _fontLoader = new ResourceFontLoader(_dwFactory);
            _fontCollection = new FontCollection(_dwFactory, _fontLoader, _fontLoader.Key);
        }

        /// <summary>
        /// Creates the size dependent drawing resources.
        /// </summary>
        private void CreateSizeDependentResources()
        {
            _d2dContext.Target = null;

            if(_d2dTarget != null)
            {
                _d2dTarget.Dispose();
                _d2dTarget = null;
            }

            if(_backBuffer != null)
            {
                _backBuffer.Dispose();
                _backBuffer = null;
            }

            // Generate a swap chain for our window based on the specified description.
            if (_swapChain == null)
            {
                // Query for the adapter and more advanced DXGI objects.
                using (var dxgiDevice2 = _device.QueryInterface<SharpDX.DXGI.Device2>())
                {
                    using (var dxgiAdapter = dxgiDevice2.Adapter)
                    {
                        using (var dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>())
                        {

                            // Description for our swap chain settings.
                            SwapChainDescription1 description = new SwapChainDescription1()
                            {
                                Width = _width * (int)_scale,
                                Height = _height * (int)_scale,
                                Format = Format.B8G8R8A8_UNorm,
                                Stereo = false,
                                SampleDescription = new SampleDescription(1, 0),
                                Usage = Usage.RenderTargetOutput,
                                BufferCount = 2,
                                Scaling = Scaling.Stretch,
                                SwapEffect = SwapEffect.FlipSequential,
                                Flags = SwapChainFlags.AllowModeSwitch
                            };

                            _swapChain = new SwapChain1(dxgiFactory2, _device, _form.Handle, ref description);
                        }
                    }
                }
            }
            else
            {
                _swapChain.ResizeBuffers(0, _width * (int)_scale, _height * (int)_scale, Format.B8G8R8A8_UNorm, SwapChainFlags.AllowModeSwitch);
            }

            Size2F dpi = _d2dFactory.DesktopDpi;

            // Specify the properties for the bitmap that we will use as the target of our Direct2D operations.
            // We want a 32-bit BGRA surface with premultiplied alpha.
            BitmapProperties1 properties = new BitmapProperties1(
                new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                dpi.Height,
                dpi.Width,
                BitmapOptions.Target | BitmapOptions.CannotDraw);

            // Get the default surface as a backbuffer and create the Bitmap1 that will hold the Direct2D drawing target.
            _backBuffer = Surface.FromSwapChain(_swapChain, 0);
            _d2dTarget = new Bitmap1(_d2dContext, _backBuffer, properties);

            _d2dContext.Target = _d2dTarget;
        }
    }
}
