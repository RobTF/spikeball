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
namespace Platformer.Engine.Gameplay.Entities
{
    using Engine.Entities;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class which implements an entity which facilitates player path swapping.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.GameEntity" />
    [GameEntityDefinition("trigger_pathswitch")]
    public class TriggerPathSwitch : GameEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerPathSwitch"/> class.
        /// </summary>
        public TriggerPathSwitch()
        {
            Options |= EntityOptions.Collidable;
        }

        /// <summary>
        /// Gets or sets the first collision path.
        /// </summary>
        public int CollisionPath1 { get; set; }

        /// <summary>
        /// Gets or sets the second collision path.
        /// </summary>
        public int CollisionPath2 { get; set; }

        public int VisLayer1 { get; set; }

        public int VisLayer2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player must be on the ground to trigger a switch.
        /// </summary>
        public bool GroundOnly { get; set; }

        /// <summary>
        /// Sets layer properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public override void SetProperties(IDictionary<string, string> properties)
        {
            if(properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string val;
            int ival;
            bool bval;

            if (properties.TryGetValue("collision_path1", out val))
            {
                if(int.TryParse(val, out ival))
                {
                    CollisionPath1 = ival;
                }
            }

            if (properties.TryGetValue("collision_path2", out val))
            {
                if (int.TryParse(val, out ival))
                {
                    CollisionPath2 = ival;
                }
            }

            if (properties.TryGetValue("vis_layer1", out val))
            {
                if (int.TryParse(val, out ival))
                {
                    VisLayer1 = ival;
                }
            }

            if (properties.TryGetValue("vis_layer2", out val))
            {
                if (int.TryParse(val, out ival))
                {
                    VisLayer2 = ival;
                }
            }

            if (properties.TryGetValue("ground_only", out val))
            {
                if (bool.TryParse(val, out bval))
                {
                    GroundOnly = bval;
                }
            }
        }

        /// <summary>
        /// Called once when this entity collides with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected override void OnStartColliding(GameEntity other)
        {
            var player = other as Player;
            if(player == null)
            {
                return;
            }
            
            if(player.MoveController.Falling && GroundOnly)
            {
                return;
            }

            if(player.CollisionPath == CollisionPath1)
            {
                player.CollisionPath = CollisionPath2;
            }
            else
            {
                player.CollisionPath = CollisionPath1;
            }

            if (player.VisLayer == VisLayer1)
            {
                player.VisLayer = VisLayer2;
            }
            else
            {
                player.VisLayer = VisLayer1;
            }
        }
    }
}
