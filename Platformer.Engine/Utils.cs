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
    public static class Utils
    {
        /// <summary>
        /// Clamps an angle to the 0-360 range.
        /// </summary>
        /// <param name="angle">The angle to be clamped.</param>
        /// <returns>The clamped angle.</returns>
        public static double ClampAngle(double angle)
        {
            while (angle > 360.0)
            {
                angle -= 360.0;
            }

            while (angle < 0.0)
            {
                angle += 360.0;
            }

            return angle;
        }

        /// <summary>
        /// Interpolates between two value.
        /// </summary>
        /// <param name="startVal">The start value.</param>
        /// <param name="endVal">The end value.</param>
        /// <param name="rate">The rate of change.</param>
        /// <param name="dt">The delta time.</param>
        /// <returns>The interpolated value.</returns>
        public static double Lerp(double startVal, double endVal, double rate, double dt)
        {
            double change = rate * dt;
            if(startVal < endVal)
            {
                startVal += change;
                if(startVal > endVal)
                {
                    startVal = endVal;
                }
            }
            else
            {
                startVal -= change;
                if(startVal < endVal)
                {
                    startVal = endVal;
                }
            }

            return startVal;
        }
    }
}
