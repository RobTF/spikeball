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
namespace Platformer.Engine.Movement
{
    using System;
    using System.Diagnostics;
    using Gameplay.Entities;
    using Input;
    using Services;

    /// <summary>
    /// Movement controller which implements Sonic the Hedgehog movement physics.
    /// </summary>
    /// <seealso cref="Platformer.Engine.Movement.BasicMoveController" />
    public class PlayerMoveController : BasicMoveController
    {
        private static readonly TraceSource TraceSource = new TraceSource("Platformer.Engine");

        public const double MaxFallSpeed = 16.0D; // topfall
        public const double SlopeFactor = 0.125D * 60.0D; // slp
        public const double Accelleration = 0.046875D * 60.0D; // acc
        public const double AirAccelleration = 0.09375D * 60.0D; // airacc
        public const double RollDecelleration = 0.125D * 60.0D; //rolldec
        public const double Decelleration = 0.5D * 60.0D; // dec
        public const double Friction = 0.046875D * 60.0D; // frc
        public const double MaxRunningSpeed = 6.0D; // top
        public const double MaxRollSpeed = 16.0D; // rolltop
        public const double DefaultGravity = 0.21875D * 60.0D; // grv
        public const double JumpSpeed = -6.5D; // jmp

        private readonly Player _player;

        private readonly ICollisionService _collisionService;
        private readonly IVariableService _varService;
        private readonly IInputService _inputService;
        private readonly IAudioService _audioService;

        protected double _gravity;
        protected bool _controlsEnabled, _allowJump;

        private bool _falling, _pushing, _jumping, _rolling, _turning;

        private ControllerState _controlState;
        private ControllerState _prevControlState;
        private PlayerMovementMode _movementMode;
        private double _groundAngle;
        private double _controlLockTime;
        private double _gsp;
        private BalanceState _balanceState;
        private BrakeState _brakeState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerMoveController" /> class.
        /// </summary>
        /// <param name="player">The player entity to control.</param>
        /// <param name="collisionService">The collision service.</param>
        /// <param name="varService">The variable service.</param>
        /// <param name="inputService">The input service.</param>
        /// <param name="audioService">The audio service.</param>
        public PlayerMoveController(Player player, ICollisionService collisionService, IVariableService varService, IInputService inputService, IAudioService audioService)
            :base(player)
        {
            if(collisionService == null)
            {
                throw new ArgumentNullException(nameof(collisionService));
            }

            if (varService == null)
            {
                throw new ArgumentNullException(nameof(varService));
            }

            if (inputService == null)
            {
                throw new ArgumentNullException(nameof(inputService));
            }

            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            _player = player;

            _audioService = audioService;
            _inputService = inputService;
            _varService = varService;
            _collisionService = collisionService;

            _falling = true;
            _rolling = false;
            _jumping = false;
            _pushing = false;
            _turning = false;
            _allowJump = true;
            _controlsEnabled = true;

            _gravity = DefaultGravity;

            _controlLockTime = 0.0;
            _gsp = 0.0;
        }

        /// <summary>
        /// Gets or sets the resource identifier of the roll sound.
        /// </summary>
        public int RollSound { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier of the jump sound.
        /// </summary>
        public int JumpSound { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier of the brake sound.
        /// </summary>
        public int BrakeSound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether player controls are enabled.
        /// </summary>
        public bool ControlsEnabled
        {
            get
            {
                return _controlsEnabled;
            }

            set
            {
                _controlsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the player may jump.
        /// </summary>
        public bool AllowJump
        {
            get
            {
                return _allowJump;
            }

            set
            {
                _allowJump = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the player is falling.
        /// </summary>
        public bool Falling => _falling;

        /// <summary>
        /// Gets a value indicating whether this <see cref="PlayerMoveController"/> is rolling.
        /// </summary>
        public bool Rolling => _rolling;

        /// <summary>
        /// Gets the current ground angle.
        /// </summary>
        public double GroundAngle => _groundAngle;

        /// <summary>
        /// Gets a value indicating whether the player is balancing, and in what way.
        /// </summary>
        public BalanceState BalanceState => _balanceState;

        /// <summary>
        /// Gets the braking state of the player.
        /// </summary>
        public BrakeState BrakeState => _brakeState;

        /// <summary>
        /// Gets a value indicating whether the player is pushing.
        /// </summary>
        public bool Pushing => _pushing;

        /// <summary>
        /// Gets or sets the ground speed of the player.
        /// </summary>
        public double GroundSpeed
        {
            get
            {
                return _gsp;
            }

            set
            {
                _gsp = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of gravity that affects the player.
        /// </summary>
        public double Gravity
        {
            get
            {
                return _gravity;
            }

            set
            {
                _gravity = value;
            }
        }

        /// <summary>
        /// Gets the current movement mode.
        /// </summary>
        public PlayerMovementMode MovementMode => _movementMode;

        /// <summary>
        /// Sets the horizontal control lock time.
        /// </summary>
        /// <param name="time">The time, in seconds.</param>
        public void SetHorizontalControlLock(double time)
        {
            _controlLockTime = time;
        }

        /// <summary>
        /// Moves the player braking into a state whereby it will no longer occur
        /// until more forwards accelleration is applied.
        /// </summary>
        public void FinishBraking()
        {
            if (_brakeState == BrakeState.Braking)
            {
                _brakeState = BrakeState.Finished;
            }
        }

        /// <summary>
        /// Unrolls the player.
        /// </summary>
        public void Unroll()
        {
            if (!_rolling)
            {
                return;
            }

            _rolling = false;
            _player.SetBoundingBox(Player.StandingBox);
        }

        /// <summary>
        /// Start the player rolling.
        /// </summary>
        public void Roll()
        {
            if (_falling || _rolling)
            {
                return;
            }

            if (Math.Abs(_gsp) <= 1.03125)
            {
                // too slow to roll
                return;
            }

            if (RollSound != 0)
            {
                _audioService.PlaySoundEffect(RollSound);
            }

            _rolling = true;
            _player.SetBoundingBox(Player.RollingBox);
        }

        /// <summary>
        /// Sets the speed of the player.
        /// </summary>
        /// <param name="xSpeed">If specified, sets the horizontal speed of the player.</param>
        /// <param name="ySpeed">If specified, sets the vertical speed of the player.</param>
        public void SetSpeed(double? xSpeed, double? ySpeed)
        {
            /* perform a wall check now in case our speed is about to adjust greatly
             * without this it's possible to see zip glitches when X speed changes drastically
             * in a single frame, for example if hitting a sideways set of spikes at high speed
             */
            CheckWall();

            if (ySpeed != null)
            {
                _falling = true;
                _ysp = ySpeed.Value;
            }

            if(xSpeed != null)
            {
                if (_falling)
                {
                    _xsp = xSpeed.Value;
                }
                else
                {
                    _gsp = xSpeed.Value;
                }
            }
        }

        /// <summary>
        /// Steps the movement of the parent entity by the controller logic.
        /// </summary>
        public override void Move()
        {
            if (_controlsEnabled)
            {
                _controlState = _inputService.CurrentState;
            }
            else
            {
                _controlState = ControllerState.None;
            }

            // may undo the ground speed, X pos etc.
            CheckWall();

            _balanceState = BalanceState.None;

            if (_falling)
            {
                // falling, so work out air speed and perform basic ground checks
                CalculateAirSpeed();
                CheckForGroundInAir();

                if (_groundAngle > 180)
                {
                    _groundAngle = Utils.Lerp(_groundAngle, 360.0, 150, _varService.DeltaTime);
                }
                else
                {
                    _groundAngle = Utils.Lerp(_groundAngle, 0.0, 150, _varService.DeltaTime);
                }
            }

            if (!_falling)
            {
                // horizontal control lock timeout
                _controlLockTime -= _varService.DeltaTime;
                if (_controlLockTime < 0.0)
                {
                    _controlLockTime = 0.0;
                }

                // too slow to roll?
                if (_rolling && Math.Abs(_gsp) < 0.5)
                {
                    Unroll();
                }

                // not falling, track ground, sort out angles/position etc.
                if (_movementMode == PlayerMovementMode.Floor)
                {
                    CheckForGroundFloor();
                }
                else if (_movementMode == PlayerMovementMode.RightWall)
                {
                    CheckForGroundRight();
                }
                else if (_movementMode == PlayerMovementMode.LeftWall)
                {
                    CheckForGroundLeft();
                }
                else if (_movementMode == PlayerMovementMode.Ceiling)
                {
                    CheckForGroundCeiling();
                }

                if (_falling)
                {
                    // now falling (just lost floor), so work out air speed and perform basic ground checks
                    CalculateAirSpeed();
                    CheckForGroundInAir();
                }
                else
                {
                    // calculate speed
                    if (!_rolling)
                    {
                        CalculateGroundSpeedRunning();
                    }
                    else
                    {
                        CalculateGroundSpeedRolling();
                    }

                    // translate ground speed to X/Y position offsets
                    CalcXYSpeed();

                    // if the player is scuttling about on the walls or ceiling, fall if too slow
                    if (_ysp <= 0.0)
                    {
                        if (_movementMode != PlayerMovementMode.Floor)
                        {
                            if (_controlLockTime <= 0.0)
                            {
                                if (Math.Abs(_gsp) < 2.5)
                                {
                                    _gsp = 0.0;
                                    _controlLockTime = 0.25;
                                    Fall();
                                }
                            }
                        }
                    }
                }
            }

            // check input for jump
            if (_allowJump)
            {
                if ((_controlState & ControllerState.Jump) > ControllerState.None)
                {
                    // only jump if the player has just pressed it
                    if ((_prevControlState & ControllerState.Jump) == ControllerState.None)
                    {
                        Jump();
                    }
                }
            }

            // check input for roll
            if ((_controlState & ControllerState.Down) > ControllerState.None)
            {
                Roll();
            }

            // impulse position
            base.Move();

            CheckBraking();
            SetMovementMode();

            // keep track of previous control state
            _prevControlState = _controlState;
        }

        /// <summary>
        /// Places the player into a falling state.
        /// </summary>
        private void Fall()
        {
            if (_falling)
            {
                return;
            }

            _movementMode = PlayerMovementMode.Floor;
            _falling = true;
        }

        /// <summary>
        /// Performs a jump.
        /// </summary>
        private void Jump()
        {
            if (_falling)
            {
                // cant jump whilst in the air
                return;
            }

            var position = _player.Position;
            var size = _player.Size;

            _jumping = true;

            double deltaX, deltaY;

            switch (_movementMode)
            {
                case PlayerMovementMode.Floor:
                    deltaX = 0.0;
                    deltaY = -((size.Y / 2) + 5.0);
                    break;
                case PlayerMovementMode.Ceiling:
                    deltaX = 0.0;
                    deltaY = ((size.Y / 2) + 5.0);
                    break;
                case PlayerMovementMode.RightWall:
                    deltaX = -((size.Y / 2) + 5.0);
                    deltaY = 0.0;
                    break;
                case PlayerMovementMode.LeftWall:
                    deltaX = ((size.Y / 2) + 5.0);
                    deltaY = 0.0;
                    break;
                default:
                    deltaX = 0.0;
                    deltaY = 0.0;
                    break;
            }

            var tq = new TraceQuery
            {
                Line = new Line(position.X, position.Y, position.X + deltaX, position.Y + deltaY),
                CollisionPath = _player.CollisionPath,
                Options = TraceLineOptions.IgnoreJumpThrough | TraceLineOptions.SolidOnly,
                Ignore = _player
            };
            var tr = _collisionService.TraceLine(tq);
            if (tr.Hit)
            {
                // relative ceiling too low, nowhere to jump
                return;
            }

            _movementMode = PlayerMovementMode.Floor;
            _falling = true;
            _rolling = true;
            _player.SetBoundingBox(Player.RollingBox);

            var rad = _groundAngle * (Math.PI / 180.0);
            _xsp -= JumpSpeed * -Math.Sin(rad);
            _ysp -= JumpSpeed * -Math.Cos(rad);

            if (_ysp < JumpSpeed)
            {
                _ysp = JumpSpeed;
            }

            if (JumpSound != 0)
            {
                _audioService.PlaySoundEffect(JumpSound);
            }
        }

        /// <summary>
        /// Checks whether the player should begin braking.
        /// </summary>
        private void CheckBraking()
        {
            if(_rolling)
            {
                _brakeState = BrakeState.None;
                return;
            }

            if (_brakeState == BrakeState.None)
            {
                if (_movementMode == PlayerMovementMode.Floor)
                {
                    if (!_falling && _turning && Math.Abs(_gsp) > 4.5)
                    {
                        _brakeState = BrakeState.Braking;

                        if (BrakeSound != 0)
                        {
                            _audioService.PlaySoundEffect(BrakeSound);
                        }
                    }
                }
            }
            else
            {
                if (Math.Abs(_gsp) < 0.5 || (_movementMode != PlayerMovementMode.Floor) || _falling || _rolling)
                {
                    _brakeState = BrakeState.None;
                }
            }
        }

        /// <summary>
        /// Calculates the speed of the player whilst rolling on the ground.
        /// </summary>
        private void CalculateGroundSpeedRolling()
        {
            var rad = _groundAngle * (Math.PI / 180.0);

            var sinVal = Math.Sin(rad);
            var sinSign = Math.Sign(sinVal);
            var slope = SlopeFactor;

            if (sinSign != 0)
            {
                bool downHill = Math.Sign(sinVal) != Math.Sign(_gsp);
                if(downHill)
                {
                    slope = 0.3125 * 60.0;
                }
                else
                {
                    slope = 0.078125 * 60.0;
                }
            }

            _gsp += Math.Round((slope * _varService.DeltaTime) * -Math.Sin(rad), 5);

            var controlsLocked = _controlLockTime > 0.0;

            if (!controlsLocked && (_controlState & ControllerState.Left) > ControllerState.None)
            {
                if (_gsp > 0.0D)
                {
                    _gsp -= RollDecelleration * _varService.DeltaTime;
                }
            }
            else if (!controlsLocked && (_controlState & ControllerState.Right) > ControllerState.None)
            {
                if (_gsp < 0.0D)
                {
                    _gsp += RollDecelleration * _varService.DeltaTime;
                }
            }

            double fdelta = (Friction / 2.0) * _varService.DeltaTime;
            _gsp -= Math.Min(Math.Abs(_gsp), fdelta) * Math.Sign(_gsp);

            if (_gsp > MaxRollSpeed)
            {
                _gsp = MaxRollSpeed;
            }
            else if (_gsp < -MaxRollSpeed)
            {
                _gsp = -MaxRollSpeed;
            }
        }

        /// <summary>
        /// Calculates the X and Y speed of the player from the ground speed.
        /// </summary>
        private void CalcXYSpeed()
        {
            var rad = _groundAngle * (Math.PI / 180.0);
            _xsp = _gsp * Math.Cos(rad);
            _ysp = _gsp * -Math.Sin(rad);
        }

        /// <summary>
        /// Calculates the speed of the player whilst running on the ground.
        /// </summary>
        private void CalculateGroundSpeedRunning()
        {
            var rad = _groundAngle * (Math.PI / 180.0);
            _gsp += Math.Round((SlopeFactor * _varService.DeltaTime) * -Math.Sin(rad), 5);

            var controlsLocked = _controlLockTime > 0.0;

            if (!controlsLocked && (_controlState & ControllerState.Left) > ControllerState.None)
            {
                _turning = _gsp > 0.0;

                if (_gsp > 0.0D)
                {
                    _gsp -= Decelleration * _varService.DeltaTime;
                }
                else if (_gsp > -MaxRunningSpeed)
                {
                    _gsp -= Accelleration * _varService.DeltaTime;
                }
            }
            else if (!controlsLocked && (_controlState & ControllerState.Right) > ControllerState.None)
            {
                _turning = _gsp < 0.0;

                if (_gsp < 0.0D)
                {
                    _gsp += Decelleration * _varService.DeltaTime;
                }
                else if (_gsp < MaxRunningSpeed)
                {
                    _gsp += Accelleration * _varService.DeltaTime;
                }
            }
            else
            {
                _turning = false;

                double friction = Friction;

                if (_rolling)
                {
                    friction /= 2.0;
                }

                double fdelta = friction * _varService.DeltaTime;
                _gsp -= Math.Min(Math.Abs(_gsp), fdelta) * Math.Sign(_gsp);
            }
        }

        /// <summary>
        /// Determines whether the player is balancing on an edge.
        /// </summary>
        /// <param name="trA">The result of sensor A.</param>
        /// <param name="trB">The result of sensor B.</param>
        private void DetectBalancing(TraceResult trA, TraceResult trB)
        {
            if (trA.Hit == trB.Hit)
            {
                return;
            }

            var flipped = _player.FlipHorizontally;
            var position = _player.Position;
            var size = _player.Size;

            TraceResult tr = trA.Hit ? trA : trB;
            var reversed = (trA.Hit && flipped) || (trB.Hit && !flipped);

            double edgeDiff;
            double objectWidth;

            if (tr.Entity == null)
            {
                var tile = tr.Tile;
                objectWidth = tile.Definition.Rect.Size.X;
                edgeDiff = Math.Abs(position.X - (tile.WorldPosition.X + objectWidth / 2.0));
            }
            else
            {
                var entity = tr.Entity;
                objectWidth = entity.Size.X;
                edgeDiff = Math.Abs(position.X - entity.Position.X);
            }

            var tq = new TraceQuery
            {
                Line = new Line(position, new Point(position.X, position.Y + (size.Y / 2.0))),
                CollisionPath = _player.CollisionPath,
                Ignore = _player,
                Options = TraceLineOptions.SolidOnly
            };

            var trMiddle = _collisionService.TraceLine(tq);
            if (!trMiddle.Hit)
            {
                if (reversed && (edgeDiff > objectWidth / 2))
                {
                    _balanceState = BalanceState.Backward;
                }
                else if (edgeDiff > (objectWidth / 2) + 6)
                {
                    _balanceState = BalanceState.ForwardVeryEdge;
                }
                else if (edgeDiff > objectWidth / 2)
                {
                    _balanceState = BalanceState.Forward;
                }
            }
        }

        /// <summary>
        /// Checks and performs tracking of the prescence of the ground when in floor mode.
        /// </summary>
        private void CheckForGroundFloor()
        {
            var position = _player.Position;
            var size = _player.Size;

            double groundHeight;
            double halfHeight = (size.Y / 2.0);
            double footLevel = position.Y + halfHeight;

            // length of A/B sensors
            const double sensorLength = 16.0;

            // A sensor
            var start = new Point(position.X - 9.0, position.Y);
            var end = new Point(position.X - 9.0, footLevel + sensorLength);
            var tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trA = _collisionService.TraceLine(tq);
            groundHeight = trA.Hit ? trA.ContactPoint.Y : Double.MaxValue;

            // B sensor
            start = new Point(position.X + 9.0, position.Y);
            end = new Point(position.X + 9.0, footLevel + sensorLength);
            tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trB = _collisionService.TraceLine(tq);
            groundHeight = Math.Min(groundHeight, trB.Hit ? trB.ContactPoint.Y : Double.MaxValue);

            if (!trA.Hit && !trB.Hit)
            {
                Fall();
                return;
            }

            _player.Position = new Point(position.X, groundHeight - halfHeight);
            CalculateGroundAngle(trA, trB, PlayerMovementMode.Floor);
            DetectBalancing(trA, trB);
        }

        /// <summary>
        /// Checks and performs tracking of the prescence of the ground when in right wall mode.
        /// </summary>
        private void CheckForGroundRight()
        {
            var position = _player.Position;
            var size = _player.Size;

            double groundHeight;
            double halfHeight = (size.Y / 2.0);
            double footLevel = position.X + halfHeight;

            // length of A/B sensors
            const double sensorLength = 16.0;

            // A sensor
            var start = new Point(position.X, position.Y + 9.0);
            var end = new Point(footLevel + sensorLength, position.Y + 9.0);
            var tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trA = _collisionService.TraceLine(tq);
            groundHeight = trA.Hit ? trA.ContactPoint.X : footLevel;

            // B sensor
            start = new Point(position.X, position.Y - 9.0);
            end = new Point(footLevel + sensorLength, position.Y - 9.0);
            tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trB = _collisionService.TraceLine(tq);
            groundHeight = Math.Min(groundHeight, trB.Hit ? trB.ContactPoint.X : footLevel);

            if (!trA.Hit && !trB.Hit)
            {
                Fall();
                return;
            }

            _player.Position = new Point(groundHeight - halfHeight, position.Y);
            CalculateGroundAngle(trA, trB, PlayerMovementMode.RightWall);
        }

        /// <summary>
        /// Checks and performs tracking of the prescence of the ground when in left wall mode.
        /// </summary>
        private void CheckForGroundLeft()
        {
            var position = _player.Position;
            var size = _player.Size;

            double groundHeight;
            double halfHeight = (size.Y / 2.0);
            double footLevel = position.X - halfHeight;

            // length of A/B sensors
            const double sensorLength = 16.0;

            // A sensor
            var start = new Point(position.X, position.Y - 9.0);
            var end = new Point(footLevel - sensorLength, position.Y - 9.0);
            var tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trA = _collisionService.TraceLine(tq);
            groundHeight = trA.Hit ? trA.ContactPoint.X : footLevel;

            // B sensor
            start = new Point(position.X, position.Y + 9.0);
            end = new Point(footLevel - sensorLength, position.Y + 9.0);
            tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trB = _collisionService.TraceLine(tq);
            groundHeight = Math.Max(groundHeight, trB.Hit ? trB.ContactPoint.X : footLevel);

            if (!trA.Hit && !trB.Hit)
            {
                Fall();
                return;
            }

            _player.Position = new Point(groundHeight + halfHeight, position.Y);
            CalculateGroundAngle(trA, trB, PlayerMovementMode.LeftWall);
        }

        /// <summary>
        /// Checks and performs tracking of the prescence of the ground when in ceiling mode.
        /// </summary>
        private void CheckForGroundCeiling()
        {
            var position = _player.Position;
            var size = _player.Size;

            double groundHeight;
            double halfHeight = (size.Y / 2.0);
            double footLevel = position.Y - halfHeight;

            // length of A/B sensors
            const double sensorLength = 16.0;

            // A sensor
            var start = new Point(position.X + 9.0, position.Y);
            var end = new Point(position.X + 9.0, footLevel - sensorLength);
            var tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trA = _collisionService.TraceLine(tq);
            groundHeight = trA.Hit ? trA.ContactPoint.Y : footLevel;

            // B sensor
            start = new Point(position.X - 9.0, position.Y);
            end = new Point(position.X - 9.0, footLevel - sensorLength);
            tq = new TraceQuery { Line = new Line(start, end), CollisionPath = _player.CollisionPath, Ignore = _player, Options = TraceLineOptions.SolidOnly };
            var trB = _collisionService.TraceLine(tq);
            groundHeight = Math.Max(groundHeight, trB.Hit ? trB.ContactPoint.Y : footLevel);

            if (!trA.Hit && !trB.Hit)
            {
                Fall();
                return;
            }

            _player.Position = new Point(position.X, groundHeight + halfHeight);
            CalculateGroundAngle(trA, trB, PlayerMovementMode.Ceiling);
        }

        /// <summary>
        /// Checks for the floor or ceiling whilst in the air.
        /// </summary>
        private void CheckForGroundInAir()
        {
            if (!_falling)
            {
                return;
            }

            var position = _player.Position;
            var size = _player.Size;

            double ceilingHeight, groundHeight;
            double halfHeight = (size.Y / 2.0);
            double footHeight = position.Y + halfHeight;

            // length of C/D sensors
            const double sensorLength = 16.0;

            // A sensor
            var start = new Point(position.X - 7.0, position.Y);
            var end = new Point(position.X - 7.0, footHeight + sensorLength);
            var tq = new TraceQuery
            {
                Line = new Line(start, end),
                CollisionPath = _player.CollisionPath,
                Ignore = _player,
                Options = TraceLineOptions.SolidOnly
            };
            var trA = _collisionService.TraceLine(tq);
            groundHeight = trA.Hit ? trA.ContactPoint.Y : Double.MaxValue;

            // B sensor
            start = new Point(position.X + 7.0, position.Y);
            end = new Point(position.X + 7.0, footHeight + sensorLength);
            tq = new TraceQuery
            {
                Line = new Line(start, end),
                CollisionPath = _player.CollisionPath,
                Ignore = _player,
                Options = TraceLineOptions.SolidOnly
            };
            var trB = _collisionService.TraceLine(tq);
            groundHeight = Math.Min(groundHeight, trB.Hit ? trB.ContactPoint.Y : Double.MaxValue);

            if (trA.Hit || trB.Hit)
            {
                if (_ysp > 0.0)
                {
                    if (groundHeight - position.Y < halfHeight)
                    {
                        // reaquired the ground

                        // get the ground angle
                        CalculateGroundAngle(trA, trB, PlayerMovementMode.Floor);
                        SetMovementMode();
                        _falling = false;
                        _jumping = false;

                        if ((_controlState & ControllerState.Down) == ControllerState.None)
                        {
                            Unroll();
                        }
                        else if ((_groundAngle != 0.0) || (Math.Abs(_xsp) > 1.03125))
                        {
                            if (RollSound != 0)
                            {
                                _audioService.PlaySoundEffect(RollSound);
                            }
                        }

                        CalculateLandingSpeed();
                    }
                }
            }

            // C sensor
            start = new Point(position.X - 7.0, position.Y);
            end = new Point(position.X - 7.0, position.Y - halfHeight - sensorLength);
            tq = new TraceQuery
            {
                Line = new Line(start, end),
                CollisionPath = _player.CollisionPath,
                Options = TraceLineOptions.IgnoreJumpThrough | TraceLineOptions.SolidOnly,
                Ignore = _player
            };
            var trC = _collisionService.TraceLine(tq);
            ceilingHeight = trC.ContactPoint.Y;

            // D sensor
            start = new Point(position.X + 7.0, position.Y);
            end = new Point(position.X + 7.0, position.Y - halfHeight - sensorLength);
            tq = new TraceQuery
            {
                Line = new Line(start, end),
                CollisionPath = _player.CollisionPath,
                Options = TraceLineOptions.IgnoreJumpThrough | TraceLineOptions.SolidOnly,
                Ignore = _player
            };
            var trD = _collisionService.TraceLine(tq);
            ceilingHeight = Math.Max(ceilingHeight, trD.ContactPoint.Y);

            if (trC.Hit || trD.Hit)
            {
                if (position.Y - ceilingHeight < halfHeight)
                {
                    _player.Position = new Point(position.X, ceilingHeight + halfHeight);
                    _ysp = Math.Max(_ysp, 0.0);
                }
            }
        }

        /// <summary>
        /// Calculates the ground speed at the point the player hits the floor.
        /// </summary>
        private void CalculateLandingSpeed()
        {
            var rad = _groundAngle * (Math.PI / 180.0);

            if (_ysp > 0.0)
            {
                // falling down
                if (
                    (_groundAngle >= 0.0D && _groundAngle <= 22.5) ||
                    (_groundAngle <= 360 && _groundAngle > 339.0))
                {
                    _gsp = _xsp;
                }
                else if (
                    (_groundAngle > 22.5 && _groundAngle <= 45.0) ||
                    (_groundAngle <= 339.0 && _groundAngle > 315))
                {
                    if (Math.Abs(_xsp) > _ysp)
                    {
                        _gsp = _xsp;
                    }
                    else
                    {
                        _gsp = _ysp * 0.5 * -Math.Sign(Math.Sin(rad));
                    }
                }
                else if (
                    (_groundAngle > 45 && _groundAngle <= 90.0) ||
                    (_groundAngle <= 315.0 && _groundAngle > 270))
                {
                    if (Math.Abs(_xsp) > _ysp)
                    {
                        _gsp = _xsp;
                    }
                    else
                    {
                        _gsp = _ysp * -Math.Sign(Math.Sin(rad));
                    }
                }
            }
            else
            {
                // falling "up" (jump/spring etc.)
            }
        }

        /// <summary>
        /// Calculates the ground angle from the contacted tiles.
        /// </summary>
        /// <param name="trA">The trace result from sensor A.</param>
        /// <param name="trB">The trace result from sensor B.</param>
        /// <param name="mode">The movement mode.</param>
        private void CalculateGroundAngle(TraceResult trA, TraceResult trB, PlayerMovementMode mode)
        {
            if (!trA.Hit && !trB.Hit)
            {
                TraceSource.TraceEvent(TraceEventType.Warning, 0, "Unable to calculate player angle!");
                return;
            }

            TraceResult tr;

            if (trA.Hit && !trB.Hit)
            {
                tr = trA;
            }
            else if (!trA.Hit && trB.Hit)
            {
                tr = trB;
            }
            else
            {
                if (mode == PlayerMovementMode.Ceiling)
                {
                    if (trA.ContactPoint.Y > trB.ContactPoint.Y)
                    {
                        tr = trA;
                    }
                    else
                    {
                        tr = trB;
                    }
                }
                else if (mode == PlayerMovementMode.RightWall)
                {
                    if (trA.ContactPoint.X < trB.ContactPoint.X)
                    {
                        tr = trA;
                    }
                    else
                    {
                        tr = trB;
                    }
                }
                else if (mode == PlayerMovementMode.LeftWall)
                {
                    if (trA.ContactPoint.X > trB.ContactPoint.X)
                    {
                        tr = trA;
                    }
                    else
                    {
                        tr = trB;
                    }
                }
                else
                {
                    if (trA.ContactPoint.Y < trB.ContactPoint.Y)
                    {
                        tr = trA;
                    }
                    else
                    {
                        tr = trB;
                    }
                }
            }

            if (tr.Entity != null)
            {
                _groundAngle = 0.0;
            }
            else
            {
                _groundAngle = tr.Tile.Definition.Angles[(int)mode];
            }

            _groundAngle = Utils.ClampAngle(_groundAngle);
        }

        /// <summary>
        /// Sets the ground movement mode based on current parameters.
        /// </summary>
        private void SetMovementMode()
        {
            if(_falling)
            {
                _movementMode = PlayerMovementMode.Floor;
                return;
            }

            var oldMode = _movementMode;

            if ((_groundAngle >= 45) && (_groundAngle < 135))
            {
                _movementMode = PlayerMovementMode.RightWall;
            }
            else if ((_groundAngle >= 135) && (_groundAngle < 225))
            {
                _movementMode = PlayerMovementMode.Ceiling;
            }
            else if ((_groundAngle >= 225) && (_groundAngle < 315))
            {
                _movementMode = PlayerMovementMode.LeftWall;
            }
            else
            {
                _movementMode = PlayerMovementMode.Floor;
            }

            if (oldMode != _movementMode)
            {
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, $"Switching to movement mode {_movementMode} from {oldMode}.");
            }
        }

        /// <summary>
        /// Calculates the speed of the player whilst in the air.
        /// </summary>
        private void CalculateAirSpeed()
        {
            var controlsLocked = _controlLockTime > 0.0;

            // early jump release
            bool jumpDownPrev = (_prevControlState & ControllerState.Jump) > ControllerState.None;
            if (_jumping && jumpDownPrev && ((_controlState & ControllerState.Jump) == ControllerState.None))
            {
                _jumping = false;

                if (_ysp < -4.0)
                {
                    _ysp = -4.0;
                }
            }

            if (!controlsLocked && (_controlState & ControllerState.Left) > ControllerState.None)
            {
                var ds = AirAccelleration * _varService.DeltaTime;

                if (_xsp - ds > -MaxRunningSpeed)
                {
                    _xsp -= ds;
                }
            }

            if (!controlsLocked && (_controlState & ControllerState.Right) > ControllerState.None)
            {
                var ds = AirAccelleration * _varService.DeltaTime;

                if (_xsp + ds < MaxRunningSpeed)
                {
                    _xsp += ds;
                }
            }

            if ((_ysp < 0.0 && _ysp > -4))
            {
                if (Math.Abs(_xsp) >= 0.125)
                {
                    _xsp *= 0.96875 * 60.0 * _varService.DeltaTime;
                }
            }

            // apply gravity
            _ysp += _gravity * _varService.DeltaTime;

            if (_ysp >= MaxFallSpeed)
            {
                _ysp = MaxFallSpeed;
            }
        }

        /// <summary>
        /// Performs a collision check against walls.
        /// </summary>
        private void CheckWall()
        {
            var pushingLeft = false;
            var pushingRight = false;
            var touchingLeft = false;
            var touchingRight = false;

            _pushing = false;

            if (_movementMode != PlayerMovementMode.Floor)
            {
                // can only push when on the floor
                return;
            }

            // determine check order by the direction of movement
            if (_xsp >= 0.0)
            {
                CheckWallRight(out touchingRight, out pushingRight);
                CheckWallLeft(out touchingLeft, out pushingLeft);
            }
            else
            {
                CheckWallLeft(out touchingLeft, out pushingLeft);
                CheckWallRight(out touchingRight, out pushingRight);
            }

            if (touchingLeft && touchingRight)
            {
                // touching both sides? have nowhere to go
                TraceSource.TraceEvent(TraceEventType.Warning, 0, $"Touching both sides - lodged in terrain/object?");
            }
            else if (pushingLeft || pushingRight)
            {
                // if the player is pushing, flag it and reset speed
                _pushing = true;
                _gsp = 0.0;
                _xsp = 0.0;
            }
        }

        /// <summary>
        /// Checks whether the player is touching/pushing a wall to the left.
        /// </summary>
        /// <param name="touching">set to <c>true</c> if the player is contacting the wall.</param>
        /// <param name="pushing">set to <c>true</c> is the player is actively pushing against the wall.</param>
        private void CheckWallLeft(out bool touching, out bool pushing)
        {
            var size = _player.Size;

            const int maxResolveIterations = 10;

            touching = false;
            pushing = false;

            var halfWidth = size.X / 2.0;
            double oldX;

            // don't perform iterative resolution if the player is falling
            int iterations = _falling ? 0 : maxResolveIterations;
            do
            {
                var position = _player.Position;
                oldX = _player.Position.X;
                var start = new Point(position.X, position.Y + 4.0);
                var end = new Point(position.X - halfWidth, position.Y + 4.0);
                var tq = new TraceQuery
                {
                    Line = new Line(start, end),
                    CollisionPath = _player.CollisionPath,
                    Options = TraceLineOptions.IgnoreJumpThrough | TraceLineOptions.SolidOnly,
                    Ignore = _player
                };
                var tr = _collisionService.TraceLine(tq);

                if (tr.Hit)
                {
                    touching = true;

                    if (position.X <= (tr.ContactPoint.X + halfWidth))
                    {
                        _player.Position = new Point(tr.ContactPoint.X + halfWidth, position.Y);

                        if (_xsp < 0.0)
                        {
                            pushing = true;
                        }
                    }
                }
            }
            while (--iterations > 0 && (oldX != _player.Position.X));
        }

        /// <summary>
        /// Checks whether the player is touching/pushing a wall to the right.
        /// </summary>
        /// <param name="touching">set to <c>true</c> if the player is contacting the wall.</param>
        /// <param name="pushing">set to <c>true</c> is the player is actively pushing agsint the wall.</param>
        private void CheckWallRight(out bool touching, out bool pushing)
        {
            var size = _player.Size;

            const int maxResolveIterations = 10;

            touching = false;
            pushing = false;

            var halfWidth = size.X / 2.0;
            double oldX;

            // don't perform iterative resolution if the player is falling
            int iterations = _falling ? 0 : maxResolveIterations;
            do
            {
                var position = _player.Position;
                oldX = position.X;
                var start = new Point(position.X, position.Y + 4.0);
                var end = new Point(position.X + halfWidth, position.Y + 4.0);
                var tq = new TraceQuery
                {
                    Line = new Line(start, end),
                    CollisionPath = _player.CollisionPath,
                    Options = TraceLineOptions.IgnoreJumpThrough | TraceLineOptions.SolidOnly,
                    Ignore = _player
                };
                var tr = _collisionService.TraceLine(tq);

                if (tr.Hit)
                {
                    touching = true;

                    if (position.X >= (tr.ContactPoint.X - halfWidth))
                    {
                        _player.Position = new Point(tr.ContactPoint.X - halfWidth, position.Y);

                        if (_xsp > 0.0)
                        {
                            pushing = true;
                        }
                    }
                }
            }
            while (--iterations > 0 && (oldX != _player.Position.X));
        }
    }

    /// <summary>
    /// Enumeration containing the various balancing states of the player.
    /// </summary>
    public enum BalanceState
    {
        /// <summary>
        /// Player is not in the balancing state.
        /// </summary>
        None,

        /// <summary>
        /// Player is balancing forwards.
        /// </summary>
        Forward,

        /// <summary>
        /// Player is balancing backwards.
        /// </summary>
        Backward,

        /// <summary>
        /// Player is balancing forwards on the very edge of the object/terrain.
        /// </summary>
        ForwardVeryEdge
    }

    /// <summary>
    /// Enumeration containing the various braking states of the player.
    /// </summary>
    public enum BrakeState
    {
        /// <summary>
        /// The player is not braking.
        /// </summary>
        None,

        /// <summary>
        /// The player is braking.
        /// </summary>
        Braking,

        /// <summary>
        /// The player has finished braking and won't brake again until they accellerate again.
        /// </summary>
        Finished
    }
}
