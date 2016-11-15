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
namespace Platformer.Engine
{
    using System;
    using Resources;

    /// <summary>
    /// Class which encapsulates a single game variable.
    /// </summary>
    public abstract class GameVariable
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        public GameVariable(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Strings.GvarNameRequired, nameof(name));
            }

            _name = name;
        }

        /// <summary>
        /// Gets the variable name.
        /// </summary>
        public string Name => _name;
    }

    /// <summary>
    /// Class which encapsulates a typed game variable.
    /// </summary>
    /// <typeparam name="TValue">The type of the variable value.</typeparam>
    public class GameVariable<TValue> : GameVariable
    {
        private TValue _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameVariable{TValue}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        public GameVariable(string name, TValue initialValue)
            :base(name)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameVariable{TValue}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GameVariable(string name)
            : this(name, default(TValue))
        {
        }

        /// <summary>
        /// Gets or sets the variable value.
        /// </summary>
        public TValue Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }
}
