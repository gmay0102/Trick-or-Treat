using Sandbox;

public class Shake : Component
{
	private Vector3 originalPosition;
	public float shakeAmount = 0.1f; // The base intensity of the shake

	[Property, Range( 0, 10, 1 )]
	public int intensityMultiplier { get; set; } = 1; // Multiplier for shake intensity

	protected override void OnEnabled()
	{
		// Store the original position when the shake starts
		originalPosition = WorldPosition;
	}

	protected override void OnFixedUpdate()
	{
		// Apply random offsets to the original position to create a shake effect
		float finalShakeAmount = shakeAmount * intensityMultiplier; // Multiply by the intensity

		Vector3 randomOffset = new Vector3(
			Sandbox.Game.Random.Float( -finalShakeAmount, finalShakeAmount ),
			Sandbox.Game.Random.Float( -finalShakeAmount, finalShakeAmount ),
			Sandbox.Game.Random.Float( -finalShakeAmount, finalShakeAmount )
		);

		// Set the new position with the random offset
		WorldPosition = originalPosition + randomOffset;
	}

	protected override void OnDisabled()
	{
		// Reset the position when the shake effect is disabled
		WorldPosition = originalPosition;
	}
}
