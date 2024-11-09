// TrickOrTreater.cs
using Sandbox;

[Title( "KinitoPet" )]
[Icon( "child_care" )]
public sealed class KinitoPet : Component, ITrickOrTreater, Component.ICollisionListener
{
	[Property] public string TreaterName { get; set; } = "";
	[Property] SoundEvent IsGaisleInThere { get; set; }
	[Property] SoundEvent IsNovaInThere { get; set; }
	[Property] SoundEvent IWantToPlayTag { get; set; }
	[Property] SoundEvent StartRunning { get; set; }
	[Property] SoundEvent ThisIsFun {  get; set; }
	[Property] SoundEvent YouGotAway { get; set; }
	[Property] SoundEvent YayIGotHim { get; set; }
	[Property] SoundBoxComponent soundbox {  get; set; }
	[Property] public DialogueBubble Dialogue { get; set; }

	[Property] private NavMeshAgent NavMeshAgent { get; set; }

	[Property] private GameObject leaveTarget {  get; set; }

	public bool scary { get; set; } = true;
	public bool lethal { get; set; } = true;

	[Sync] private bool hunting { get; set; } = false;
	private bool leaving = false;
	private GameObject target = null;
	private string hunttarget = "";
	private bool checkedfornova = false;

	[Property] public bool CanKill = true;


	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( hunting )
			NavMeshAgent.MoveTo( target.WorldPosition );

		if ( leaving )
			NavMeshAgent.MoveTo( leaveTarget.WorldPosition );
	}

	public void SpeakRequest()
	{		
		Dialogue.Enabled = true;
		Dialogue.Speak( "Is my best friend Gaisle in here?" );
		Sound.Play( IsGaisleInThere, WorldPosition );
		hunttarget = "Bald";
		HuntCycle();
	}

	private void NovaRequest()
	{
		Dialogue.Speak( "Is my best friend Nova in here?" );
		Sound.Play( IsNovaInThere, WorldPosition );
		hunttarget = "nvoa";
		HuntCycle();
		checkedfornova = true;
	}

	private async void HuntCycle()
	{
		await Task.Delay( 3000 );

		Dialogue.ClearText();
		Dialogue.Speak( "I want to play tag with him!" );
		Sound.Play(IWantToPlayTag, WorldPosition );

		await Task.Delay( 3000 );

		foreach (var c in Scene.GetAllComponents<PlayerState>())
		{
			if ( c.Network.Owner.DisplayName == hunttarget )
				target = c.GameObject;
			else
				continue;
		}

		Dialogue.ClearText();
		if ( target == null && !checkedfornova )
		{
			NovaRequest();
			return;
		}
		else if ( target == null && checkedfornova)
		{
			Dialogue.Speak( "Wow, they're both dead? Too bad!" );
			Scene.Components.GetInChildren<ScareMaster>().Cleanup();
			return;
		}
		else
		{
			Dialogue.Speak( "Start running!" );
			Sound.Play( StartRunning, WorldPosition );
		}

		await Task.Delay( 1500 );

		Dialogue.Enabled = false;
		hunting = true;
		soundbox.StartSound();

		await Task.Delay( 10000 );

		if ( hunting && !leaving  )
		{
			Dialogue.Enabled = true;
			Dialogue.ClearText();
			Dialogue.Speak( "This is so much fun, I love playing games with you!" );
			Sound.Play( ThisIsFun, WorldPosition );
		}

		await Task.Delay( 15000 );
			
		if ( hunting && !leaving )
		{
			hunting = false;
			soundbox.StopSound();
			NavMeshAgent.Stop();
			Dialogue.Enabled = true;
			Dialogue.ClearText();
			Dialogue.Speak( "Aww, you got away! Well, that was fun." );
			Sound.Play( YouGotAway, WorldPosition );
		}

		Leave();
	}

	public void OnCollisionStart ( Collision other )
	{
		if (other.Other.GameObject == target && hunting)
		{
			var pc = other.Other.GameObject.GetComponent<PlayerController>();
			if ( pc != null )
			{
				pc.OnDeath();
				hunting = false;
				Sound.StopAll( 0.1f );

				NavMeshAgent.Stop();
				Dialogue.Enabled = true;
				Dialogue.ClearText();
				Dialogue.Speak( "Yay, I got him! Don't worry, he'll be safe where I sent him." );
				Sound.Play(YayIGotHim, WorldPosition );
				Leave();
			}
		}

		if ( other.Other.GameObject.Tags.Has( "door" ) )
		{
			var door = other.Other.GameObject.GetComponent<Door>();
			door.Interact( GameObject );
		}
			

		if ( other.Other.GameObject.Tags.Has( "leavepoint" ) && leaving )
			Scene.Components.GetInChildren<ScareMaster>().Cleanup();
	}

	private async void Leave()
	{
		await Task.Delay( 2000 );
		leaveTarget = Scene.Directory.FindByName( "TreaterSpawnPosition" ).First();
		leaving = true;
	}
}
