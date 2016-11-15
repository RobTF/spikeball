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
namespace Platformer.Engine.Entities
{
    using Collision;
    using System;
    using System.Collections.Generic;
    using Movement;

    /// <summary>
    /// Base class for all game entities.
    /// </summary>
    public abstract class GameEntity
    {
        private int _id;
        private SolidType _solidType;
        private EntityOptions _options;
        private Point _position;
        private string _typeName;

        private Rect _worldRect;

        private IMoveController _moveController;
        private BoundingBox _collisionBox;
        private Point _size;

        private List<GameEntity> _prevTouchList;
        private List<GameEntity> _touchList;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEntity"/> class.
        /// </summary>
        public GameEntity()
        {
            CollisionBox = BoundingBox.Zero;
            CollisionPath = null;

            _touchList = new List<GameEntity>();

            _options = EntityOptions.None;
            _solidType = SolidType.None;
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity options.
        /// </summary>
        public EntityOptions Options
        {
            get
            {
                return _options;
            }

            set
            {
                _options = value;
            }
        }

        /// <summary>
        /// Gets or sets the solidity type of the entity.
        /// </summary>
        public SolidType SolidType
        {
            get
            {
                return _solidType;
            }

            set
            {
                _solidType = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of the entity in the world.
        /// </summary>
        public Point Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
                RecalculateCollisionBoxData();
            }
        }

        /// <summary>
        /// Gets or sets the collision mins/maxs.
        /// </summary>
        public BoundingBox CollisionBox
        {
            get
            {
                return _collisionBox;
            }

            set
            {
                _collisionBox = value;
                _size = _collisionBox.GetSize();
                RecalculateCollisionBoxData();
            }
        }

        /// <summary>
        /// Gets or sets the movement controller.
        /// </summary>
        public IMoveController MoveController
        {
            get
            {
                return _moveController;
            }

            set
            {
                _moveController = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the layer the player will be rendered directly on top of.
        /// </summary>
        public int VisLayer { get; set; }

        /// <summary>
        /// Gets or sets the angle of the entity, in degrees.
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Gets or sets the collision path of the entity.
        /// </summary>
        public int? CollisionPath { get; set; }

        /// <summary>
        /// Gets or sets the logical "size" of the entity in world units.
        /// </summary>
        public Point Size => _size;

        /// <summary>
        /// Gets a rectangle calculated from the current position and size (upper left world position and size).
        /// </summary>
        public Rect WorldRect => _worldRect;

        /// <summary>
        /// Updates the current collisions.
        /// </summary>
        /// <param name="current">A list of entities currently colliding with this one.</param>
        public void UpdateCollisions(List<GameEntity> current)
        {
            if(current == null)
            {
                throw new ArgumentNullException(nameof(current));   
            }

            int i;

            for(i = 0; i < current.Count; i++)
            {
                var ent = current[i];
                var alreadyTouching = _touchList.Contains(ent);
                if(!alreadyTouching)
                {
                    StartColliding(ent);
                }

                OnColliding(ent);
            }

            for(i = 0; i < _touchList.Count; i++)
            {
                var touching = _touchList[i];
                if(!current.Contains(touching))
                {
                    StopColliding(touching);
                }
            }

            _touchList.Clear();
            _touchList.AddRange(current);
        }

        /// <summary>
        /// Sets entity properties from a key/value dictionary.
        /// </summary>
        /// <param name="properties">Dictionary containing the properties.</param>
        public virtual void SetProperties(IDictionary<string, string> properties)
        {
            if(properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }
        }

        /// <summary>
        /// Called when the entity is first placed into the game.
        /// </summary>
        public void Spawn()
        {
            OnSpawn();
            Options |= EntityOptions.Spawned;
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        public void Step()
        {
            if(_moveController != null)
            {
                _moveController.Move();
            }

            OnStep();
        }

        /// <summary>
        /// Called after each frame.
        /// </summary>
        public void PostStep()
        {
            OnPostStep();
        }

        /// <summary>
        /// Marks the entity as removed from the game.
        /// </summary>
        public void Killed()
        {
            Options |= EntityOptions.Killed;
            OnKilled();
        }

        /// <summary>
        /// Called once when this entity collides with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected void StartColliding(GameEntity other)
        {
            OnStartColliding(other);
        }

        /// <summary>
        /// Called each frame for every entity currently colliding with this one.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected void Colliding(GameEntity other)
        {
            OnColliding(other);
        }

        /// <summary>
        /// Called once when this entity stops colliding with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected void StopColliding(GameEntity other)
        {
            OnStopColliding(other);
        }

        /// <summary>
        /// Called when the entity is spawned into the game.
        /// </summary>
        protected virtual void OnSpawn()
        {

        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected virtual void OnStep()
        {

        }

        /// <summary>
        /// Called after each frame.
        /// </summary>
        protected virtual void OnPostStep()
        {

        }

        /// <summary>
        /// Called when the entity is killed.
        /// </summary>
        protected virtual void OnKilled()
        {

        }

        /// <summary>
        /// Called once when this entity collides with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected virtual void OnStartColliding(GameEntity other)
        {

        }

        /// <summary>
        /// Called each frame for every entity currently colliding with this one.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected virtual void OnColliding(GameEntity other)
        {

        }

        /// <summary>
        /// Called once when this entity stops colliding with another.
        /// </summary>
        /// <param name="other">The other entity.</param>
        protected virtual void OnStopColliding(GameEntity other)
        {

        }

        /// <summary>
        /// Recalculates various values used in collision detection.
        /// </summary>
        private void RecalculateCollisionBoxData()
        {
            _worldRect = new Rect(Position.X + CollisionBox.Mins.X, Position.Y + CollisionBox.Mins.Y, Size.X, Size.Y);
        }
    }
}
