using Sandbox;

public sealed class SelfDestructComponent : Component
{
	[Property] public float Seconds { get; set; }

	TimeUntil timeUntilDie;
	bool timerStarted = false;

	protected override void OnUpdate()
	{
		if ( GameObject.IsProxy )
			return;

		// Only start the timer once when Seconds is set
		if ( !timerStarted )
		{
			timeUntilDie = Seconds;
			timerStarted = true;
		}

		if ( timeUntilDie <= 0.0f )
		{
			GameObject.Destroy();
		}
	}
}
