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
    using System.Windows.Forms;
    using SharpDX.Windows;
    using Engine;
    using System;
    using Engine.Services;
    using Engine.Input;
    using System.IO;

    /// <summary>
    /// The main game window.
    /// </summary>
    /// <seealso cref="SharpDX.Windows.RenderForm" />
    public partial class GameForm : RenderForm
    {
        private readonly IGameEngine _gameEngine;
        private readonly IInputService _inputService;
        private readonly Program _prg;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameForm"/> class.
        /// </summary>
        public GameForm(Program prg, IGameEngine gameEngine)
        {
            if(prg == null)
            {
                throw new ArgumentNullException(nameof(prg));
            }

            if(gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            InitializeComponent();

            _prg = prg;
            _gameEngine = gameEngine;
            _inputService = _gameEngine.GetService<IInputService>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            loadMapToolStripMenuItem.DropDownItems.Clear();

            var resService = _gameEngine.GetService<IResourceService>();
            var files = Directory.GetFiles(resService.GetResourcePath<Map>());
            foreach(var file in files)
            {
                var menuItem = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(file));
                menuItem.Click += OnLoadMapItemClicked;
                loadMapToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Up:
                    _inputService.HoldControl(ControllerState.Up);
                    break;
                case Keys.Down:
                    _inputService.HoldControl(ControllerState.Down);
                    break;
                case Keys.Left:
                    _inputService.HoldControl(ControllerState.Left);
                    break;
                case Keys.Right:
                    _inputService.HoldControl(ControllerState.Right);
                    break;
                case Keys.Z:
                    _inputService.HoldControl(ControllerState.Jump);
                    break;
                case Keys.P:
                    _gameEngine.Paused = !_gameEngine.Paused;
                    break;
                case Keys.Q:
                    _gameEngine.Step(true);
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.KeyCode)
            {
                case Keys.Up:
                    _inputService.ReleaseControl(ControllerState.Up);
                    break;
                case Keys.Down:
                    _inputService.ReleaseControl(ControllerState.Down);
                    break;
                case Keys.Left:
                    _inputService.ReleaseControl(ControllerState.Left);
                    break;
                case Keys.Right:
                    _inputService.ReleaseControl(ControllerState.Right);
                    break;
                case Keys.Z:
                    _inputService.ReleaseControl(ControllerState.Jump);
                    break;
            }
        }

        /// <summary>
        /// Called when one of the toggleable debug menu items is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if(menuItem == null)
            {
                return;
            }

            var varService = _gameEngine.GetService<IVariableService>();
            varService.GetVar<bool>((string)menuItem.Tag).Value = menuItem.Checked;
        }

        /// <summary>
        /// Called when exit is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void OnExitClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Called when the stop game menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnStopGameClicked(object sender, EventArgs e)
        {
            _prg.StopLevel();
        }

        /// <summary>
        /// Called when a load map item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnLoadMapItemClicked(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
            {
                return;
            }

            _prg.StartLevel(menuItem.Text);
        }

        /// <summary>
        /// Called when a scale menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnScaleItemClicked(object sender, EventArgs e)
        {
            var newScale = 1.0f;
            if(sender == scale2xToolStripMenuItem)
            {
                newScale = 2.0f;
            }
            else if (sender == scale3xToolStripMenuItem)
            {
                newScale = 3.0f;
            }

            _prg.ChangeScale(newScale);
        }
    }
}
