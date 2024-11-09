using static Door;

[Title( "Halloween Door" )]
[Category( "Interaction" )]
[Icon( "door_front" )]
public sealed class HalloweenDoor : Component, IInteractable
{
	// Animation
	[Property] public Curve AnimationCurve { get; set; } = new Curve( new Curve.Frame( 0f, 0f ), new Curve.Frame( 1f, 1.0f ) );
	[Property] public float OpenTime { get; set; } = 0.5f;

	// Sounds
	[Property, Group( "Sound" )] public SoundEvent OpenSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent OpenFinishedSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent CloseSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent CloseFinishedSound { get; set; }
	[Property, Group( "Sound" )] public SoundEvent LockSound { get; set; }

	// Internal State
	[HostSync] public TimeSince LastUse { get; set; }
	[HostSync] public DoorState State { get; set; } = DoorState.Closed;
	[Sync] public bool Locked { get; set; } = true;
	private Transform StartTransform { get; set; }
	private Vector3 PivotPosition { get; set; }
	private float TargetAngle { get; set; } = -110.0f;
	public enum DoorState
	{
		Open,
		Opening,
		Closing,
		Closed
	}
	private DoorState DefaultState { get; set; } = DoorState.Closed;

	protected override void OnStart()
	{
		// Starting Position set to spawn position
		StartTransform = Transform.Local;
		PivotPosition = StartTransform.Position;
		DefaultState = State;
	}

	public void Interact( GameObject go )
	{
		if ( State == DoorState.Opening || State == DoorState.Closing )
			return;

		LastUse = 0.0f;

		if ( State == DoorState.Closed && !Locked )
		{
			State = DoorState.Opening;
			Sound.Play( OpenSound, WorldPosition );
			Locked = true;
		}
		else if ( State == DoorState.Open && !Locked )
		{
			State = DoorState.Closing;
			PlaySound( CloseSound );
		}
		else if ( Locked )
		{
			Sound.Play(LockSound, WorldPosition);
		}
	}

	[Broadcast]
	public void PlaySound( SoundEvent se )
	{
		Sound.Play( se, WorldPosition );
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return; // Skip for clients

		if ( State != DoorState.Opening && State != DoorState.Closing )
			return;

		var time = LastUse.Relative.Remap( 0.0f, OpenTime, 0.0f, 1.0f );
		var curve = AnimationCurve.Evaluate( time );

		if ( State == DoorState.Closing )
			curve = 1.0f - curve;

		Transform.Local = StartTransform.RotateAround( PivotPosition, Rotation.FromYaw( curve * TargetAngle ) );

		if ( time >= 1f )
		{
			OpenClose();
		}
	}

	[Broadcast] void OpenClose()
	{
		if ( State == DoorState.Opening )
		{
			State = DoorState.Open;
			Sound.Play( OpenFinishedSound, WorldPosition );
			Scene.GetComponentInChildren<ScareMaster>().StartTalking();
		}
		else
		{
			State = DoorState.Closed;
			Sound.Play( CloseFinishedSound, WorldPosition );
		}
	}
}
