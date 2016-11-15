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
    /// <summary>
    /// Enumeration containing options which effect how entities behave.
    /// </summary>
    public enum EntityOptions
    {
        /// <summary>
        /// No options set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The entity has been spawned.
        /// </summary>
        Spawned = (1 << 0),

        /// <summary>
        /// The entity has been killed.
        /// </summary>
        Killed = (1 << 1),

        /// <summary>
        /// The entity participates in collisions.
        /// </summary>
        Collidable = (1 << 2),

        /// <summary>
        /// The entity is visible and should be rendered.
        /// </summary>
        Visible = (1 << 3)
    }
}
