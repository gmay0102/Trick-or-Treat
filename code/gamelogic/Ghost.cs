using Sandbox;

public sealed class Ghost : Component, ITrickOrTreater, Component.ICollisionListener, IInteractable
{
	[Property] public string TreaterName { get; set; } = "Ghost";
	[Property] private NavMeshAgent NavMeshAgent { get; set; }
	[Property] public DialogueBubble Dialogue { get; set; }
	[Property] private SoundBoxComponent SoundBox { get; set; }
	[Property] private SoundEvent KillSound { get; set; }
	public bool scary { get; set; } = true;
	public bool lethal { get; set; } = true;

	[Sync] public int CandyReceived { get; set; } = 0;

	private bool hunting = false;
	private bool leaving = false;
	private bool appeased = false;
	[Sync] private bool eventstarted { get; set; } = false;
	private GameObject leaveTarget = null;
	private GameObject target = null;

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( hunting )
			NavMeshAgent.MoveTo( target.WorldPosition );

		if ( leaving )
			NavMeshAgent.MoveTo( leaveTarget.WorldPosition );
	}

	public void Interact( GameObject go )
	{
		if ( eventstarted )
		{
			var ps = go.GetComponent<PlayerState>();

			if ( ps.HasItem && !ps.HasAxe )
			{
				ps.ClearItem();
				CandyReceived++;
				UpdateCandy();

				// Check if candy goal is met
				if ( CandyReceived >= 10 )
				{
					SoundBox.StopSound();
					Dialogue.ClearText();
					Dialogue.Speak( "good job lt" );
					Scene.Components.GetInChildren<ScareMaster>().Cleanup();
					eventstarted = false;
					appeased = true;
					StopEvent();
				}
			}
		}
	}

	public void SpeakRequest()
	{
		Dialogue.Enabled = true;
		Dialogue.Speak( "oi mate, you're gonna want the whole task force for this" );
		BeginGhostEvent();
	}

	private async void BeginGhostEvent()
	{
		await Task.Delay( 10000 );
		Dialogue.ClearText();
		Dialogue.Speak( "you've got a minute to get me ten pieces of candy" );

		await Task.Delay( 4000 );
		Dialogue.ClearText();
		Dialogue.Speak( "your time starts now" );

		eventstarted = true;
		SoundBox.StartSound();
		await Task.Delay( 60000 ); // Full minute countdown
		eventstarted = false;
		SoundBox.StopSound();
		Dialogue.ClearText();
		Dialogue.Speak( "bloody hell, you're gonna get it now" );

		await Task.Delay( 4000 );
		Dialogue.Enabled = false;
		if ( !appeased ) Hunt();
	}

	private void UpdateCandy()
	{
		Dialogue.ClearText();
		Dialogue.Speak( CandyReceived.ToString() + " pieces" );
	}

	private void Hunt()
	{
		hunting = true;
		target = Scene.GetAllComponents<PlayerState>().FirstOrDefault().GameObject;
	}

	private void StopEvent()
	{
		eventstarted = false;
		hunting = false;
		leaving = true;
		leaveTarget = Scene.Directory.FindByName( "TreaterSpawnPosition" ).First();
	}

	public void OnCollisionStart( Collision other )
	{
		if ( other.Other.GameObject == target && hunting )
		{
			var pc = other.Other.GameObject.GetComponent<PlayerController>();
			if ( pc != null )
			{
				pc.OnDeath();
				playkillsound();
				StopEvent();
			}
		}

		if ( other.Other.GameObject.Tags.Has( "door" ) )
		{
			var door = other.Other.GameObject.GetComponent<Door>();
			door.Interact( GameObject );
		}

		if ( other.Other.GameObject.Tags.Has( "leavepoint" ) && leaving )
		{
			NavMeshAgent.Stop();
			NavMeshAgent.Acceleration = 0;
			NavMeshAgent.MaxSpeed = 0;
			NavMeshAgent.Velocity = 0;
			Scene.Components.GetInChildren<ScareMaster>().Cleanup();
		}
	}

	[Broadcast] void playkillsound()
	{
		Sound.Play( KillSound, LocalPosition );
	}
}
