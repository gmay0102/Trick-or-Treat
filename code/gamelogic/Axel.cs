// TrickOrTreater.cs
using Sandbox;

[Title( "Axel" )]
[Icon( "child_care" )]
public sealed class Axel : Component, IInteractable, ITrickOrTreater
{
	[Property] public string TreaterName { get; set; } = "";
	[Property, TextArea] public string Request { get; set; }

	[Property, TextArea] public string BestResponse { get; set; }
	[Property, TextArea] public string BadResponse { get; set; }
	[Property] public bool scary { get; set; } = false;
	[Property] public bool lethal { get; set; } = false;
	[Property] private GameObject AxePrefab { get; set; }

	[Property] public List<GameObject> BestCandies { get; set; }
	[Property] public List<GameObject> OkCandies { get; set; }

	[Property] public DialogueBubble Dialogue { get; set; }
	[Property] public SoundEvent Denied { get; set; }

	private ItemComponent ReceivedCandy { get; set; } = null;
	private bool Serviced = true;

	// Enables dialogue box, speaks their request.
	public void SpeakRequest()
	{
		Dialogue.Enabled = true;
		Dialogue.Speak( Request );
		Serviced = false;
	}

	public void Interact( GameObject go )
	{
		var ps = go.GetComponent<PlayerState>();



		if ( ps.HasItem && !ps.HasAxe && !Serviced )
		{
			ReceivedCandy = ps.CurrentItem;
			ps.ClearItem();
			Serviced = true;
			if ( !IsProxy )
				Scene.GetComponentInChildren<Chat>().AddSystemText( $"{Connection.Local.DisplayName} sees {go.Network.Owner.DisplayName}'s item being taken." );
		}
		else
		{
			Sound.Play( Denied, WorldPosition );
			return;
		}

		Dialogue.ClearText();
		var sm = Scene.Components.GetInChildren<ScareMaster>();

		if ( BestCandies.Contains( ReceivedCandy.WorldModel ) )
		{
			Dialogue.Speak( BestResponse );
			sm.TreatPoints += 3;
			Scene.GetComponentInChildren<Pmsound>().PlayGreatSound();
			GiveAxe();
			Scene.GetComponentInChildren<ScareMaster>().AxelPleased = true;
		}
		else
		{
			Dialogue.Speak( BadResponse );
			sm.TreatPoints += 3;
			Scene.GetComponentInChildren<Pmsound>().PlayBadSound();
		}

		sm.Cleanup();
	}

	private void GiveAxe()
	{
		if ( IsProxy ) return;

		var sneed = AxePrefab.Clone( WorldPosition );
		sneed.NetworkSpawn();
		var physics = sneed.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
		physics.Velocity = (WorldRotation.Forward.WithY(90) * 40f + Vector3.Up * 10f);
		
	}
}
