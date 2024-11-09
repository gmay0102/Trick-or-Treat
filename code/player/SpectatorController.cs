using System;
using System.Diagnostics;
using System.Threading.Channels;
using Sandbox;
using Sandbox.Citizen;

[Title( "Spectator Controller" )]
[Category( "Player" )]
[Icon( "directions_walk" )]

// This entire player controller is ripped from SmileCorp's Movement Base. All credit goes to them.

public sealed class SpectatorController : Component
{

	[Property, ToggleGroup( "UseCustomFOV", Label = "Use Custom Field Of View" )] private bool UseCustomFOV { get; set; } = true;
	[Property, ToggleGroup( "UseCustomFOV" ), Title( "Field Of View" ), Range( 60f, 120f )] public float CustomFOV { get; set; } = 90f;

	[Property, Group( "Movement Properties" ), Description( "CS2 Default: 250f" )] public float MoveSpeed { get; set; } = 250f;


	// Internal objects
	private CameraComponent Camera;

	private Vector2 SmoothLookAngle = Vector2.Zero; // => localLookAngle.LerpTo(LookAngle, Time.Delta / 0.1f);
	public Angles LookAngleAngles => new Angles( LookAngle.x, LookAngle.y, 0 );


	// Synced internal vars
	[Sync] public Vector2 LookAngle { get; set; } = Vector2.Zero;



	protected override void OnAwake()
	{
		Scene.FixedUpdateFrequency = 64;
		Camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		
	}

	protected override void OnUpdate()
	{

		if ( Camera == null ) return;

		SmoothLookAngle = SmoothLookAngle.LerpTo( LookAngle, Time.Delta / 0.035f );

		if ( IsProxy )
			return;

		LookAngle += new Vector2( (Input.MouseDelta.y), -(Input.MouseDelta.x) ) * Preferences.Sensitivity * 0.022f;
		LookAngle = LookAngle.WithX( LookAngle.x.Clamp( -89f, 89f ) );

		var angles = LookAngleAngles;


		Camera.WorldPosition = GameObject.WorldPosition + new Vector3( 0, 0 * 0.89f * WorldScale.z );
		Camera.WorldRotation = angles.ToRotation();

		if ( UseCustomFOV )
		{
			Camera.FieldOfView = CustomFOV;
		}
		else
		{
			Camera.FieldOfView = Preferences.FieldOfView;
		}
		Vector3 movement = Input.AnalogMove;

		WorldRotation = LookAngleAngles;

		if ( !movement.IsNearlyZero() )
		{
			WorldPosition += WorldRotation * movement.Normal * Time.Delta * MoveSpeed;
		}
	}


}
