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
    using Input;
    using Services;
    using System;
    using System.Diagnostics;
    using Engine.Entities;
    using Collision;
    using Movement;
    using Resources;

    /// <summary>
    /// Class which implements the player in a Sonic the Hedgehog style game.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Entities.GameEntity" />
    public abstract class Player : Animatable
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        public static readonly BoundingBox StandingBox = new BoundingBox(-10.0, -20.0, 10.0, 20.0);
        public static readonly BoundingBox RollingBox = new BoundingBox(-10.0, -15.0, 10.0, 15.0);

        protected const double FallDamageGravity = 0.1875D * 60.0D;

        private readonly IResourceService _resourceService;
        private readonly IRenderService _renderService;
        private readonly IVariableService _varService;
        private readonly IInputService _inputService;
        private readonly IEntityService _entityService;
        private readonly IAudioService _audioService;
        private readonly ICollisionService _collisionService;

        private bool _crouching, _chargingSpindash, _lookup;
        private Direction _screenDirection;
        private PlayerMoveController _moveController;

        protected int _idleAnimId;
        protected int _walkAnimId;
        protected int _runAnimId;
        protected int _rollAnimId;
        protected int _pushAnimId;
        protected int _brakingAnimId;
        protected int _balance1AnimId;
        protected int _balance2AnimId;
        protected int _balance3AnimId;
        protected int _crouchAnimId;
        protected int _spindashAnimId;
        protected int _lookupAnimId;
        protected int _springAnimId;
        protected int _fallAnimId;

        private int _jumpSoundId;
        private int _brakeSoundId;
        private int _rollSoundId;
        private int _spindashChargeSoundId;
        private int _spindashReleaseSoundId;
        private int _ringLossSoundId;

        private SpindashDust _spindashDust;

        private ControllerState _controlState;
        private ControllerState _prevControlState;

        private double _spindashCharge;
        private double _brakeSmokeTime;

        private bool _springing;
        private bool _takingDamage;
        private double _nextFlashTime;

        private double _invulnerabilityTime;
        private bool _invulnerable;

        private double _ringCollectTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="renderService">The render service.</param>
        /// <param name="varService">The variable service.</param>
        /// <param name="inputService">The input service.</param>
        /// <param name="entityService">The entity service.</param>
        /// <param name="audioService">The audio service.</param>
        /// <param name="collisionService">The collision service.</param>
        public Player(
            IResourceService resourceService,
            IRenderService renderService,
            IVariableService varService,
            IInputService inputService,
            IEntityService entityService,
            IAudioService audioService,
            ICollisionService collisionService)
            : base(varService, resourceService)
        {
            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (renderService == null)
            {
                throw new ArgumentNullException(nameof(renderService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (inputService == null)
            {
                throw new ArgumentNullException(nameof(inputService));
            }

            if (entityService == null)
            {
                throw new ArgumentNullException(nameof(entityService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            if (collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            _entityService = entityService;
            _varService = varService;
            _renderService = renderService;
            _inputService = inputService;
            _audioService = audioService;
            _collisionService = collisionService;
            _resourceService = resourceService;

            _resourceService.PreloadResource<Sprite>("brakesmoke");
            _resourceService.PreloadResource<Sprite>("spindashsmoke");

            _resourceService.PreloadResource<Sound>("brake");
            _resourceService.PreloadResource<Sound>("jump");
            _resourceService.PreloadResource<Sound>("roll");
            _resourceService.PreloadResource<Sound>("spindash");
            _resourceService.PreloadResource<Sound>("spindash_release");
            _resourceService.PreloadResource<Sound>("ring_loss");

            MoveController = _moveController = new PlayerMoveController(this, _collisionService, _varService, _inputService, _audioService);
            Options |= EntityOptions.Collidable;
            SolidType = SolidType.None;
            CollisionPath = 0;
            RenderPriority = RenderPriority.High;

            _lookup = false;
            _crouching = false;

            _takingDamage = false;
            _invulnerabilityTime = 0.0;
            _nextFlashTime = 0.0;
            _invulnerable = false;
            _springing = false;

            SetBoundingBox(StandingBox);
        }

        /// <summary>
        /// Gets or sets the number of rings the player is carrying.
        /// </summary>
        public int Rings { get; set; }

        /// <summary>
        /// Gets a value indicating whether the player can collect rings.
        /// </summary>
        public bool CanCollectRings => _ringCollectTime < _varService.GlobalTime;

        /// <summary>
        /// Gets or sets the movement controller.
        /// </summary>
        public new PlayerMoveController MoveController
        {
            get
            {
                return _moveController;
            }

            set
            {
                _moveController = value;
                base.MoveController = value;
            }
        }

        /// <summary>
        /// Gets the state of the player controls.
        /// </summary>
        public ControllerState ControlState => _controlState;

        /// <summary>
        /// Pushes the player vertically.
        /// </summary>
        /// <param name="speed">The speed.</param>
        /// <param name="spring">If set to <c>true</c> will set the player to the spring animation.</param>
        public void PushVertically(double speed, bool spring)
        {
            _moveController.SetSpeed(null, speed);
            if (spring)
            {
                _springing = true;
            }
        }

        /// <summary>
        /// Pushes the player sideways.
        /// </summary>
        /// <param name="speed">The speed to push the player.</param>
        /// <param name="turn">if set to <c>true</c> will always turn the player to face the direction of the push.</param>
        public void PushHorizontally(double speed, bool turn)
        {
            _moveController.SetSpeed(speed, null);

            if (_moveController.Falling)
            {
                if (turn)
                {
                    _screenDirection = _moveController.HorizontalSpeed < 0.0 ? Direction.Left : Direction.Right;
                }
            }
            else
            {
                if (turn)
                {
                    _screenDirection = _moveController.GroundSpeed < 0.0 ? Direction.Left : Direction.Right;
                }
            }
        }

        /// <summary>
        /// Takes damage from another entity.
        /// </summary>
        /// <param name="other">The entity that dealt the damage.</param>
        public void TakeDamage(GameEntity other)
        {
            if (_takingDamage || (_invulnerabilityTime > _varService.GlobalTime))
            {
                return;
            }

            var a = Math.Sign(Position.X - other.Position.X);
            if (a == 0)
            {
                a = 1;
            }

            _moveController.Unroll();
            _moveController.SetSpeed(2.0 * a, -4.0);

            _moveController.ControlsEnabled = false;
            _takingDamage = true;
            _moveController.Gravity = FallDamageGravity;

            _ringCollectTime = _varService.GlobalTime + 1.0;

            if(Rings > 0)
            {
                LoseRings();
            }            
        }

        /// <summary>
        /// Sets the collision box of the player, and adjusts the immediate position to compensate.
        /// </summary>
        /// <param name="box">The new collision box of the player.</param>
        public void SetBoundingBox(BoundingBox box)
        {
            var oldSize = CollisionBox.GetSize();
            CollisionBox = box;
            var newSize = CollisionBox.GetSize();

            // adjust ground position to compensate for size change
            switch (_moveController.MovementMode)
            {
                case PlayerMovementMode.Floor:
                    Position = new Point(Position.X, Position.Y + (oldSize.Y - newSize.Y) / 2);
                    break;
                case PlayerMovementMode.Ceiling:
                    Position = new Point(Position.X, Position.Y - (oldSize.Y - oldSize.Y) / 2);
                    break;
                case PlayerMovementMode.LeftWall:
                    Position = new Point(Position.X - (oldSize.Y - newSize.Y) / 2, Position.Y);
                    break;
                case PlayerMovementMode.RightWall:
                    Position = new Point(Position.X + (oldSize.Y - newSize.Y) / 2, Position.Y);
                    break;
            }
        }

        /// <summary>
        /// Called when the entity is first placed into the game.
        /// </summary>
        protected override void OnSpawn()
        {
            SetAnimation(_idleAnimId);
            AnimSpeed = 0;

            _moveController.JumpSound = _jumpSoundId = _resourceService.LoadResource<Sound>("jump").ResourceId;
            _moveController.BrakeSound = _brakeSoundId = _resourceService.LoadResource<Sound>("brake").ResourceId;
            _moveController.RollSound = _rollSoundId = _resourceService.LoadResource<Sound>("roll").ResourceId;
            _spindashChargeSoundId = _resourceService.LoadResource<Sound>("spindash").ResourceId;
            _spindashReleaseSoundId = _resourceService.LoadResource<Sound>("spindash_release").ResourceId;
            _ringLossSoundId = _resourceService.LoadResource<Sound>("ring_loss").ResourceId;

            base.OnSpawn();
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        protected override void OnStep()
        {
            base.OnStep();

            var falling = _moveController.Falling;

            if (_takingDamage && (!falling || _springing))
            {
                // landed from a damage fall
                _moveController.Gravity = PlayerMoveController.DefaultGravity;
                _takingDamage = false;
                _moveController.ControlsEnabled = true;
                _invulnerable = true;
                _invulnerabilityTime = _varService.GlobalTime + 2.0;
            }

            // stop "springing" if we start descending or hit the ground
            if (_springing && (_moveController.VerticalSpeed > 0.0 || !falling))
            {
                _springing = false;
            }

            // invulverability flash effect
            if (_invulnerable)
            {
                if (_invulnerabilityTime > _varService.GlobalTime)
                {
                    if (_nextFlashTime < _varService.GlobalTime)
                    {
                        _nextFlashTime = _varService.GlobalTime + 0.05;

                        if ((Options & EntityOptions.Visible) > EntityOptions.None)
                        {
                            Options &= ~EntityOptions.Visible;
                        }
                        else
                        {
                            Options |= EntityOptions.Visible;
                        }
                    }
                }
                else
                {
                    _invulnerable = false;
                    Options |= EntityOptions.Visible;
                }
            }

            _controlState = _inputService.CurrentState;

            if (_chargingSpindash)
            {
                SpindashDegrade();
            }

            if ((_controlState & ControllerState.Up) > ControllerState.None)
            {
                LookUp();
            }
            else
            {
                _lookup = false;
            }

            if ((_controlState & ControllerState.Down) > ControllerState.None)
            {
                Crouch();
            }
            else
            {
                Uncrouch();
            }

            if (
                ((_prevControlState & ControllerState.Jump) == ControllerState.None) &&
                ((_controlState & ControllerState.Jump) > ControllerState.None) &&
                _crouching)
            {
                ChargeSpindash();
            }

            // keep kicking out some brake dust if we're braking
            if (_moveController.BrakeState == BrakeState.Braking)
            {
                if (_brakeSmokeTime < _varService.GlobalTime)
                {
                    var brakeSmoke = _entityService.CreateEntity<BrakeDust>(new Point(Position.X, Position.Y + (Size.Y / 2.0)));
                    brakeSmoke.VisLayer = VisLayer;
                    _brakeSmokeTime = _varService.GlobalTime + 0.1;
                }
            }

            UpdateScreenDirection();
            UpdateAnim();

            // keep track of previous control state
            _prevControlState = _controlState;
        }

        /// <summary>
        /// Makes the player look up.
        /// </summary>
        private void LookUp()
        {
            if (
                _moveController.MovementMode != PlayerMovementMode.Floor ||
                Math.Abs(_moveController.GroundSpeed) > 0.0 ||
                _lookup ||
                _moveController.BalanceState != 0 ||
                _moveController.Falling)
            {
                return;
            }

            _lookup = true;
        }

        /// <summary>
        /// Crouches the player.
        /// </summary>
        private void Crouch()
        {
            if (
                _moveController.MovementMode != PlayerMovementMode.Floor ||
                Math.Abs(_moveController.GroundSpeed) > 0.0 ||
                _moveController.BalanceState != 0 ||
                _moveController.Falling)
            {
                if (_crouching)
                {
                    Uncrouch();
                }

                return;
            }

            _moveController.AllowJump = false;
            _crouching = true;
        }

        /// <summary>
        /// Uncrouches the player.
        /// </summary>
        private void Uncrouch()
        {
            if (_crouching)
            {
                _crouching = false;
                _moveController.AllowJump = true;

                if (_chargingSpindash)
                {
                    ReleaseSpindash();
                }
                else
                {
                    SetBoundingBox(StandingBox);
                }
            }
        }

        /// <summary>
        /// Charges a spindash.
        /// </summary>
        private void ChargeSpindash()
        {
            if (!_crouching)
            {
                return;
            }

            if (!_chargingSpindash)
            {
                _spindashCharge = 0.0;

                // spindash can be initiated before crouch is complete so make sure we are the correct size
                SetBoundingBox(RollingBox);

                var offset = _screenDirection == Direction.Left ? 16 : -16;
                var origin = new Point(Position.X + offset, Position.Y + 4);
                _spindashDust = _entityService.CreateEntity<SpindashDust>(origin);
                _spindashDust.VisLayer = VisLayer;
                _spindashDust.RenderPriority = RenderPriority.Highest;
                _spindashDust.FlipHorizontally = FlipHorizontally;
            }
            else
            {
                _spindashCharge += 2.0;
                _spindashDust.AnimSpeed = 20.0 + (_spindashCharge * 4.0);
            }

            _spindashCharge = Math.Min(_spindashCharge, 8.0);
            CurrentAnimSequenceIndex = 0;

            _chargingSpindash = true;
            _audioService.PlaySoundEffect(_spindashChargeSoundId);
        }

        /// <summary>
        /// Degrades the spindash charge
        /// </summary>
        private void SpindashDegrade()
        {
            if (_spindashCharge >= 0.125)
            {
                _spindashCharge *= 0.96875 * 60.0 * _varService.DeltaTime;
            }
        }

        /// <summary>
        /// Releases a spindash.
        /// </summary>
        private void ReleaseSpindash()
        {
            if (!_chargingSpindash)
            {
                return;
            }

            if (_spindashDust != null)
            {
                _entityService.KillEntity(_spindashDust);
                _spindashDust = null;
            }

            _moveController.GroundSpeed = 8 + (Math.Floor(_spindashCharge) / 2.0);

            if (_screenDirection == Direction.Left)
            {
                _moveController.GroundSpeed *= -1;
            }

            _moveController.RollSound = 0; // small hack to cancel out the roll sound when we release the spindash
            _moveController.Roll();
            _moveController.RollSound = _rollSoundId;

            _chargingSpindash = false;
            _audioService.PlaySoundEffect(_spindashReleaseSoundId);
        }

        /// <summary>
        /// Called each time the animation changes frame.
        /// </summary>
        /// <param name="lastSeqIndex">The index of the last sequence position played.</param>
        /// <param name="currentSeqIndex">The index of the current sequence position.</param>
        protected override void AnimationFrameChanged(int lastSeqIndex, int currentSeqIndex)
        {
            // if the braking animation has finished - stop braking
            if (Animation == _brakingAnimId)
            {
                if (lastSeqIndex > currentSeqIndex)
                {
                    // come out of the braking animation, and prevent restarting it for now
                    _moveController.FinishBraking();
                }
            }

            if (Animation == _crouchAnimId)
            {
                // fully crouched - change size
                if (currentSeqIndex == 1)
                {
                    SetBoundingBox(RollingBox);
                }
            }
        }

        /// <summary>
        /// Drops the rings the player has in their posession.
        /// </summary>
        /// <remarks>
        /// http://info.sonicretro.org/SPG:Ring_Loss
        /// </remarks>
        private void LoseRings()
        {
            _audioService.PlaySoundEffect(_ringLossSoundId);

            Rings = Math.Min(Rings, 32);

            var t = 0;
            var angle = 101.25;
            var n = false;
            var speed = 4.0;

            while (t < Rings)
            {
                var ring = _entityService.CreateEntity<Ring>(Position);
                ring.SetLifeTime(4.0);
                ring.VisLayer = VisLayer;
                ring.AnimSpeed = 24.0;
                ring.MoveController = new BounceMoveController(ring, _collisionService, _varService)
                {
                    MaxSpeed = 16,
                    VerticalBounceFactor = 0.75,
                    HorizontalBounceFactor = 0.25,
                    Gravity = 0.09375 * 60.0,
                    TerrainOnly = true
                };

                var mcontroller = (BasicMoveController)ring.MoveController;

                var rad = angle * (Math.PI / 180.0);
                mcontroller.VerticalSpeed = -Math.Sin(rad) * speed;
                mcontroller.HorizontalSpeed = Math.Cos(rad) * speed;

                if (n)
                {
                    mcontroller.HorizontalSpeed *= -1;
                    angle += 22.5;
                }

                n = !n;
                t++;

                if (t == 16)
                {
                    speed = 2.0;
                    angle = 101.25;
                }
            }

            Rings = 0;
        }

        /// <summary>
        /// Updates the current direction of the player in terms of the screen/camera.
        /// </summary>
        private void UpdateScreenDirection()
        {
            if(!_moveController.ControlsEnabled)
            {
                return;
            }

            // determine which direction sonic should be facing
            if (!_crouching)
            {
                var falling = _moveController.Falling;
                var gsp = _moveController.GroundSpeed;
                var xsp = _moveController.HorizontalSpeed;

                if ((_controlState & ControllerState.Left) > ControllerState.None)
                {
                    if ((!falling && gsp <= 0.0) || (falling && xsp <= 0.0D))
                    {
                        _screenDirection = Direction.Left;
                    }
                }
                else if ((_controlState & ControllerState.Right) > ControllerState.None)
                {
                    if ((!falling && gsp >= 0.0) || (falling && xsp >= 0.0D))
                    {
                        _screenDirection = Direction.Right;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the player animation to match the current state.
        /// </summary>
        protected virtual void UpdateAnim()
        {
            var idealAnim = _walkAnimId;
            var idealSpeed = 0.0D;
            var loop = true;

            var gsp = _moveController.GroundSpeed;
            var angle = _moveController.GroundAngle;
            var xsp = _moveController.HorizontalSpeed;
            var brakeState = _moveController.BrakeState;
            var pushing = _moveController.Pushing;
            var balanceState = _moveController.BalanceState;
            
            FlipHorizontally = _screenDirection == Direction.Left;

            if (_moveController.Rolling)
            {
                idealAnim = _rollAnimId;
                idealSpeed = (1.0D / _varService.DeltaTime) / Math.Max(8.0 - Math.Abs(gsp), 1.0);
                Angle = 0.0;
            }
            else if (_moveController.Falling)
            {
                if (_springing)
                {
                    idealAnim = _springAnimId;
                    idealSpeed = 16.0;
                    Angle = 0.0;
                    FlipHorizontally = false;
                }
                else if (_takingDamage)
                {
                    Angle = 0.0;
                    idealAnim = _fallAnimId;
                    idealSpeed = 2.0;
                }
                else
                {
                    if (Math.Abs(gsp) >= PlayerMoveController.MaxRunningSpeed)
                    {
                        idealAnim = _runAnimId;
                    }
                    else
                    {
                        idealAnim = _walkAnimId;
                    }

                    idealSpeed = (1.0D / _varService.DeltaTime) / Math.Max(8.0 - Math.Abs(gsp), 1.0);

                    Angle = (int)(angle / 30) * 30;
                }
            }
            else
            {
                if (gsp != 0.0D)
                {
                    if (brakeState == BrakeState.Braking)
                    {
                        // when braking we always face the way we're going
                        FlipHorizontally = gsp < 0.0;
                        idealAnim = _brakingAnimId;
                        idealSpeed = (1.0D / _varService.DeltaTime) / 8.0;
                    }
                    else
                    {
                        if (pushing)
                        {
                            idealAnim = _pushAnimId;
                            idealSpeed = 1.5;
                        }
                        else
                        {
                            if (Math.Abs(gsp) >= PlayerMoveController.MaxRunningSpeed)
                            {
                                idealAnim = _runAnimId;
                            }
                            else
                            {
                                idealAnim = _walkAnimId;
                            }

                            idealSpeed = (1.0D / _varService.DeltaTime) / Math.Max(8.0 - Math.Abs(gsp), 1.0);
                        }
                    }
                }
                else
                {
                    if (balanceState == BalanceState.Forward)
                    {
                        idealAnim = _balance1AnimId;
                        idealSpeed = 5.0;
                    }
                    else if (balanceState == BalanceState.Backward)
                    {
                        idealAnim = _balance2AnimId;
                        idealSpeed = 3.0;
                    }
                    else if (balanceState == BalanceState.ForwardVeryEdge)
                    {
                        idealAnim = _balance3AnimId;
                        idealSpeed = 10.0;
                    }
                    else if (_chargingSpindash)
                    {
                        idealAnim = _spindashAnimId;
                        idealSpeed = 1.0 / _varService.DeltaTime;
                    }
                    else if (_crouching)
                    {
                        idealAnim = _crouchAnimId;
                        idealSpeed = 15.0;
                        loop = false;
                    }
                    else if (_lookup)
                    {
                        idealAnim = _lookupAnimId;
                        idealSpeed = 15.0;
                        loop = false;
                    }
                    else
                    {
                        idealAnim = _idleAnimId;
                        idealSpeed = 0;
                    }
                }

                Angle = angle;
            }

            if (idealSpeed != AnimSpeed)
            {
                AnimSpeed = idealSpeed;
            }

            if (idealAnim != Animation)
            {
                SetAnimation(idealAnim);
            }

            if (!loop)
            {
                LoopAnimation = false;
            }
        }
    }
}
