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
    using Engine.Resources;
    using SharpDX.Mathematics.Interop;
    using SharpDX.WIC;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class SpriteResourceManager : ResourceManager
    {
        private readonly SharpDX.Direct2D1.DeviceContext _d2dContext;
        private readonly Dictionary<int, SharpDX.Direct2D1.Bitmap> _spriteTextures;
        private readonly Dictionary<int, Dictionary<int, RawRectangleF[]>> _animationRects;

        private SharpDX.Direct2D1.Bitmap _notFound;
        private ImagingFactory _imagingFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteResourceManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine.</param>
        public SpriteResourceManager(IGameEngine gameEngine, SharpDX.Direct2D1.DeviceContext d2dContext)
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
            _spriteTextures = new Dictionary<int, SharpDX.Direct2D1.Bitmap>();
            _animationRects = new Dictionary<int, Dictionary<int, RawRectangleF[]>>();

            var imagePath = Path.Combine(_resourceService.GetResourcePath<Sprite>(), "notfound.png");
            using (var decoder = new BitmapDecoder(_imagingFactory, imagePath, DecodeOptions.CacheOnDemand))
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

                    _notFound = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(_d2dContext, formatConverter);
                }
            }
        }

        /// <summary>
        /// Gets the image for a specific sprite, animation and frame.
        /// </summary>
        /// <param name="spriteResourceId">The sprite resource identifier.</param>
        /// <param name="animationId">The animation identifier.</param>
        /// <param name="seqIndex">The animation sequence index.</param>
        /// <returns>The bitmap to render.</returns>
        public SharpDX.Direct2D1.Bitmap GetImageForSprite(int spriteResourceId, int animationId, int seqIndex, out RawRectangleF rect)
        {
            var retval = _notFound;
            rect = new RawRectangleF(0, 0, 16, 16);

            var sprite = _resourceService.GetResourceById<Sprite>(spriteResourceId);
            if (sprite != null)
            {
                // get the bitmap
                SharpDX.Direct2D1.Bitmap bmp;
                if (_spriteTextures.TryGetValue(spriteResourceId, out bmp))
                {
                    retval = bmp;

                    // now get the source rect for the animation frame
                    Dictionary<int, RawRectangleF[]> rectDict;
                    if (_animationRects.TryGetValue(spriteResourceId, out rectDict))
                    {
                        RawRectangleF[] rects;
                        if (rectDict.TryGetValue(animationId, out rects))
                        {
                            var frame = sprite.Animations[animationId].Sequence[seqIndex];
                            if (frame < rects.Length)
                            {
                                rect = rects[frame];
                            }
                        }
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// Called when a game resource is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Platformer.Engine.Resources.ResourceLoadEventArgs" /> instance containing the event data.</param>
        protected override void OnGameResourceLoaded(object sender, ResourceLoadEventArgs e)
        {
            if (e.ResourceType == typeof(Sprite))
            {
                // get the sprite that has been loaded
                var sprite = e.LoadedResource as Sprite;

                // find, load and store the sprite sheet for the sprite
                var imagePath = Path.Combine(_resourceService.GetResourcePath<Sprite>(), sprite.Name + ".png");
                using (var decoder = new BitmapDecoder(_imagingFactory, imagePath, DecodeOptions.CacheOnDemand))
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
                        _spriteTextures.Add(sprite.ResourceId, bitmap);
                    }
                }

                // create a dictionary to store the sprite animations
                var rectDictionary = new Dictionary<int, RawRectangleF[]>();
                _animationRects.Add(sprite.ResourceId, rectDictionary);

                // enumerate each animation and frame
                foreach (var animation in sprite.Animations)
                {
                    var frameRects = new RawRectangleF[animation.Frames.Length];

                    var curFrame = 0;

                    foreach (var frame in animation.Frames)
                    {
                        var x = frame.Rect.Position.X;
                        var y = frame.Rect.Position.Y;
                        var width = frame.Rect.Size.X;
                        var height = frame.Rect.Size.Y;

                        frameRects[curFrame] = new RawRectangleF(x, y, x + width, y + height);
                        curFrame++;
                    }

                    rectDictionary.Add(animation.Id, frameRects);
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
            if (disposing)
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
            foreach (var bitmap in _spriteTextures.Values)
            {
                bitmap.Dispose();
            }

            _spriteTextures.Clear();
            _animationRects.Clear();
        }
    }
}
