using Sandbox;

public sealed class Funnymouth : Component, ITrickOrTreater, Component.ICollisionListener, IInteractable
{
	[Property] public string TreaterName { get; set; } = "funnymouth";
	[Property] public DialogueBubble Dialogue { get; set; }
	[Property] public SoundEvent KillSound { get; set; }
	public bool scary { get; set; } = true;
	public bool lethal { get; set; } = true;

	private bool serviced = true;


	public void Interact( GameObject go )
	{
		if (serviced) return;
		serviced = true;
		var ps = go.GetComponent<PlayerState>();

		if ( ps.CurrentItem.ItemName == "Meat" )
		{
			ps.ClearItem();
			Dialogue.ClearText();
			Dialogue.Speak( "i see ur handsome face" );
			Scene.GetComponentInChildren<ScareMaster>().Cleanup();
		}
		else if (ps.HasItem && !ps.HasAxe )
		{
			ps.ClearItem();
			Dialogue.ClearText();
			Dialogue.Speak( "it can b fun aagain youll see what" );
			FunnyHunt();
		}
	}

	private async void FunnyHunt()
	{
		await Task.Delay( 5000 );
		Dialogue.Enabled = false;

		var target = Scene.GetAllComponents<PlayerState>().FirstOrDefault();
		var targetpos = target.WorldPosition;

		target.WorldPosition += (Vector3.Up * 5f);
		target.GetComponent<PlayerController>().OnDeath();
		playkillsound();

		await Task.Delay( 5 );

		GameObject.WorldPosition = (targetpos);

		await Task.Delay( 2000 );

		var leaveTarget = Scene.Directory.FindByName( "TreaterSpawnPosition" ).First();

		GameObject.WorldPosition = leaveTarget.WorldPosition;
		Scene.GetComponentInChildren<ScareMaster>().Cleanup();
	}
	[Broadcast] void playkillsound()
	{
		Sound.Play( KillSound, WorldPosition );
	}
	public void SpeakRequest()
	{
		Dialogue.Enabled = true;
		Dialogue.Speak( "i like to lik the bluud" );
		serviced = false;
	}

}
