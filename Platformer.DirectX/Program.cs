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
namespace Platformer.DirectX
{
    using Audio;
    using Engine;
    using Engine.Gameplay.Entities;
    using Engine.Resources;
    using Engine.Services;
    using Rendering;
    using Resources;
    using SharpDX.Windows;
    using System;
    using System.Diagnostics;
    using Un4seen.Bass;

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    public class Program
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer");

        private const int GameAreaWidth = 384;
        private const int GameAreaHeight = 240;

        private Game _game;
        private GameForm _form;
        private Renderer _renderer;

        private SoundResourceManager _soundResMan;
        private AudioPlaybackManager _playbackMan;

        private bool _stopping;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var program = new Program();
            program.Run();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Program" /> class.
        /// </summary>
        public Program()
        {
            _stopping = false;
            _game = new Game();
            InitGame();

            _form = new GameForm(this, _game);
            _renderer = new Renderer(_game, _form, GameAreaWidth, GameAreaHeight);

            ChangeScale(2.0f);

            InitAudio();
        }

        /// <summary>
        /// Changes the scale of the windowed mode rendering.
        /// </summary>
        /// <param name="scale">The new scale.</param>
        public void ChangeScale(float scale)
        {
            var width = GameAreaWidth * (int)scale;
            var height = GameAreaHeight * (int)scale;

            _form.Width = width;
            _form.Height = height;
            _renderer.Scale = scale;
        }

        /// <summary>
        /// Starts a level.
        /// </summary>
        /// <param name="map">The name of the map to load.</param>
        public void StartLevel(string map)
        {
            if (_game.IsRunning)
            {
                return;
            }

            var t = new System.Threading.Thread(() =>
            {
                _game.StartLevel(map);

                var entityService = _game.GetService<IEntityService>();
                var camera = entityService.CreateEntity<SonicCamera>(Point.Zero);

                _renderer.Camera = camera;
            });
            t.Start();
        }

        /// <summary>
        /// Stops the currently running level.
        /// </summary>
        public void StopLevel()
        {
            if(_stopping)
            {
                return;
            }

            _stopping = true;
        }

        /// <summary>
        /// Runs the program.
        /// </summary>
        private void Run()
        {
            _form.Show();

            RenderLoop.Run(_form, () =>
            {
                _game.Step(false);

                _renderer.Render();

                if(_stopping)
                {
                    _game.StopLevel();
                    _stopping = false;
                }
            });
        }

        private void Stop()
        {
            Bass.BASS_Free();
        }

        /// <summary>
        /// Initializes the audio playing system.
        /// </summary>
        private void InitAudio()
        {
            _soundResMan = new SoundResourceManager(_game);

            TraceSource.TraceEvent(TraceEventType.Verbose, 0, $"Initializing audio...");
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Failed to init audio!");
            }

            _playbackMan = new AudioPlaybackManager(_game, _soundResMan);
        }

        /// <summary>
        /// Initializes game configuation.
        /// </summary>
        private void InitGame()
        {
            var resourceService = _game.GetService<IResourceService>();
            resourceService.SetResourcePath<Sprite>(@"..\..\..\..\assets\sprites\");
            resourceService.SetResourcePath<Map>(@"..\..\..\..\assets\maps\");
            resourceService.SetResourcePath<Sound>(@"..\..\..\..\assets\sounds\");

            var varService = _game.GetService<IVariableService>();
            varService.GetVar<bool>("r_showtracelines").Value = false;
            varService.GetVar<bool>("r_showcollisionmaps").Value = false;
            varService.GetVar<bool>("r_showentityorigins").Value = false;
            varService.GetVar<bool>("r_showtileframes").Value = false;
            varService.GetVar<bool>("r_showcollisionboxes").Value = false;
        }
    }
}
