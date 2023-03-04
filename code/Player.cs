using Sandbox;

partial class FpsPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	[Net, Predicted]
	public bool ThirdPersonCamera { get; set; }
	
	public FpsPlayer()
	{
	}

	public FpsPlayer( IClient cl ) : this()
	{
	}

	public override void Respawn()
	{
		Controller = new WalkController();

		this.ClearWaterLevel();
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		base.Respawn();
	}


	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		var controller = GetActiveController();
		SimulateAnimation( controller );


		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Released( InputButton.Jump ) )
		{
			timeSinceJumpReleased = 0;
		}

		if ( InputDirection.y != 0 || InputDirection.x != 0f )
		{
			timeSinceJumpReleased = 1;
		}
	}

	void SimulateAnimation( PawnController controller )
	{
		if ( controller == null )
			return;

		var turnSpeed = 0.02f;

		Rotation rotation;

		rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle );

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = ( Game.IsClient && Client.IsValid() ) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();

		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.AimBodyWeight = 0.5f;

	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}

	public override void FrameSimulate( IClient cl )
	{
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.Position = EyePosition;
		Camera.FirstPersonViewer = this;
		Camera.Main.SetViewModelCamera( 90f );
	}
}
