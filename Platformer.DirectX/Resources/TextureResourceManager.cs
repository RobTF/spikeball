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
namespace Platformer.DirectX.Resources
{
    using Engine;
    using System;
    using System.Collections.Generic;
    using Engine.Resources;
    using SharpDX.WIC;

    public class TextureResourceManager : ResourceManager
    {
        private readonly SharpDX.Direct2D1.DeviceContext _d2dContext;
        private readonly Dictionary<int, SharpDX.Direct2D1.Bitmap> _tileSetTextures;

        private ImagingFactory _imagingFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureResourceManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        public TextureResourceManager(IGameEngine gameEngine, SharpDX.Direct2D1.DeviceContext d2dContext)
            : base(gameEngine)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            if (d2dContext == null)
            {
                throw new ArgumentNullException(nameof(d2dContext));
            }

            _d2dContext = d2dContext;
            _imagingFactory = new ImagingFactory();
            _tileSetTextures = new Dictionary<int, SharpDX.Direct2D1.Bitmap>();
        }

        /// <summary>
        /// Gets the texture for tile set.
        /// </summary>
        /// <param name="tileSetId">The tile set identifier.</param>
        /// <returns>The texture.</returns>
        public SharpDX.Direct2D1.Bitmap GetTextureForTileSet(int tileSetId)
        {
            return _tileSetTextures[tileSetId];
        }

        /// <summary>
        /// Called when a game resource is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Platformer.Engine.Resources.ResourceLoadEventArgs" /> instance containing the event data.</param>
        protected override void OnGameResourceLoaded(Object sender, ResourceLoadEventArgs e)
        {
            if (e.ResourceType == typeof(Map))
            {
                var map = e.LoadedResource as Map;
                foreach (var tileSet in map.TileSets)
                {
                    using (var decoder = new BitmapDecoder(_imagingFactory, tileSet.ImagePath, DecodeOptions.CacheOnDemand))
                    {
                        using (var formatConverter = new FormatConverter(_imagingFactory))
                        {
                            formatConverter.Initialize(
                                    decoder.GetFrame(0),
                                    PixelFormat.Format32bppPBGRA,
                                    BitmapDitherType.DualSpiral8x8,
                                    null,
                                    0.0,
                                    BitmapPaletteType.Custom);

                            var bitmap = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(_d2dContext, formatConverter);
                            _tileSetTextures.Add(tileSet.Id, bitmap);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when all game resources are unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void OnGameResourcesUnloaded(object sender, EventArgs e)
        {
            ClearResources();
            base.OnGameResourcesUnloaded(sender, e);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(Boolean disposing)
        {
            if(disposing)
            {
                ClearResources();
                _imagingFactory.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears the resources.
        /// </summary>
        private void ClearResources()
        {
            foreach (var bitmap in _tileSetTextures.Values)
            {
                bitmap.Dispose();
            }

            _tileSetTextures.Clear();
        }
    }
}
