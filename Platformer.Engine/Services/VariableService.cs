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

    /// <summary>
    /// Class which implements a service that provides game-wide variables.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Services.GameService" />
    /// <seealso cref="Platformer.Engine.Services.IVariableService" />
    public class VariableService : GameService, IVariableService
    {
        private readonly IGameEngine _engine;
        private readonly object _lock;
        private readonly Dictionary<string, GameVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableService"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public VariableService(IGameEngine engine)
        {
            if(engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            _lock = new object();
            _engine = engine;
            _variables = new Dictionary<string, GameVariable>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the amount of time the game has been running, in seconds.
        /// </summary>
        public double GlobalTime => _engine.GlobalTime;

        /// <summary>
        /// Gets the amount of time between the last two frames.
        /// </summary>
        public double DeltaTime => _engine.DeltaTime;

        /// <summary>
        /// Gets a game variable.
        /// </summary>
        /// <typeparam name="TValue">The type of the variable value.</typeparam>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// An instance of <see cref="GameVariable" />, or null if no variable was found, or the value was the wrong type.
        /// </returns>
        public GameVariable<TValue> GetVar<TValue>(string name)
        {
            lock (_lock)
            {
                GameVariable retval;
                if (!_variables.TryGetValue(name, out retval))
                {
                    retval = new GameVariable<TValue>(name);
                    _variables.Add(name, retval);
                }

                return retval as GameVariable<TValue>;
            }
        }
    }
}
