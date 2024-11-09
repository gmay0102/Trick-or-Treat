
[Title ( "Door" )]
[Category ( "Interaction" )]
[Icon ( "door_front" )]

//Door wholesale stolen from HC1

public sealed class Door : Component, IInteractable
{
	/// <summary>
	/// Animation curve to use, X is the time between 0-1 and Y is how much the door is open to its target angle from 0-1.
	/// </summary>
	[Property] public Curve AnimationCurve { get; set; } = new Curve( new Curve.Frame( 0f, 0f ), new Curve.Frame( 1f, 1.0f ) );
	[Property, Group( "Sound" )] public SoundEvent OpenSound { get; set; }

	[Property, Group( "Sound" )] public SoundEvent OpenFinishedSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent CloseSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent CloseFinishedSound { get; set; }
	/// <summary>
	/// Optional pivot point, origin used if not specified.
	/// </summary>
	[Property] public GameObject Pivot {  get; set; }
	/// <summary>
	/// How far the door rotates.
	/// </summary>
	[Property, Range( 0.0f, 90.0f )] public float TargetAngle { get; set; } = 90.0f;
	/// <summary>
	/// How long in seconds it takes for door to open.
	/// </summary>
	[Property] public float OpenTime { get; set; } = 0.5f;
	/// <summary>
	/// Open away from person who uses this door?
	/// </summary>
	[Property] public bool OpenAwayFromPlayer { get; set; } = true;

	public enum DoorState
	{
		Open,
		Opening,
		Closing,
		Closed
	}

	private Transform StartTransform {  get; set; }
	private Vector3 PivotPosition { get; set; }
	private bool ReverseDirection { get; set; }
	[HostSync] public TimeSince LastUse {  get; set; }
	[HostSync] public DoorState State { get; set; } = DoorState.Closed;

	private DoorState DefaultState { get; set; } = DoorState.Closed;

	protected override void OnStart()
	{
		// Starting Position set to spawn position
		StartTransform = Transform.Local;
		// Do we have a pivot? If so set pivotpos to the set gameobject
		PivotPosition = Pivot is not null ? Pivot.WorldPosition : StartTransform.Position;
		// We're starting in the state defined
		DefaultState = State;
	}
	public void Interact( GameObject go )
	{
		//Don't let us do anything if the door's busy opening or closing
		if ( State == DoorState.Opening || State == DoorState.Closing )
			return;

		LastUse = 0.0f;

		if (State == DoorState.Closed)
		{
			State = DoorState.Opening;
			if ( OpenSound != null )
				Sound.Play( OpenSound, WorldPosition );

			if ( OpenAwayFromPlayer )
			{
				var doorToPlayer = (go.WorldPosition - PivotPosition).Normal;
				var doorForward = -Transform.Local.Rotation.Forward;

				ReverseDirection = Vector3.Dot( doorToPlayer, doorForward ) > 0;
			}
		}
		else if ( State == DoorState.Open )
		{
			State = DoorState.Closing;
			if ( CloseSound != null )
				Sound.Play( CloseSound, WorldPosition );
		}
	}

	protected override void OnFixedUpdate()
	{
		//If we're not opening or closing, get outta here!
		if ( State != DoorState.Opening && State != DoorState.Closing )
			return;

		var time = LastUse.Relative.Remap( 0.0f, OpenTime, 0.0f, 1.0f );
		var curve = AnimationCurve.Evaluate( time );

		if ( State == DoorState.Closing ) 
			curve = 1.0f - curve;

		var targetAngle = TargetAngle;

		if ( ReverseDirection ) 
			targetAngle *= -1.0f;

		Transform.Local = StartTransform.RotateAround( PivotPosition, Rotation.FromYaw( curve * targetAngle ) );

		if ( time < 1f) 
			return;

		if ( State == DoorState.Opening )
		{
			State = DoorState.Open;
			if ( !IsProxy )
				PlaySound( OpenFinishedSound );
		}
		else
		{
			State = DoorState.Closed;
			if ( !IsProxy )
				PlaySound( CloseFinishedSound );
		}
	}

	[Broadcast]
	public void PlaySound( SoundEvent se )
	{
		Sound.Play( se, WorldPosition );
	}
}
