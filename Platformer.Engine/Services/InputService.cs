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
    using Input;

    /// <summary>
    /// Class which implements the input service.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IInputService" />
    public class InputService : GameService, IInputService
    {
        private ControllerState _currentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputService"/> class.
        /// </summary>
        public InputService()
        {
            _currentState = ControllerState.None;
        }

        /// <summary>
        /// Gets the current state of the control inputs.
        /// </summary>
        public ControllerState CurrentState => _currentState;

        /// <summary>
        /// Marks one or more controls as being "held" (i.e. pushed).
        /// </summary>
        /// <param name="state">The controller state flags to mark as held.</param>
        public void HoldControl(ControllerState state)
        {
            _currentState |= state;
        }

        /// <summary>
        /// Marks one or more controls as being released (no longer held).
        /// </summary>
        /// <param name="state">The controller state flags to mark as released.</param>
        public void ReleaseControl(ControllerState state)
        {
            _currentState &= ~state;
        }
    }
}
