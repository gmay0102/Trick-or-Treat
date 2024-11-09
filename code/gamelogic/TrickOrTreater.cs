// TrickOrTreater.cs
using Sandbox;

[Title( "Trick or Treater" )]
[Icon( "child_care" )]
public sealed class TrickOrTreater : Component, IInteractable, ITrickOrTreater
{
	[Property] public string TreaterName { get; set; } = "";
	[Property, TextArea] public string Request { get; set; }

	[Property, TextArea] public string BestResponse { get; set; }
	[Property, TextArea] public string OkResponse { get; set; }
	[Property, TextArea] public string BadResponse { get; set; }
	[Property] public bool scary { get; set; } = false;
	[Property] public bool lethal { get; set; } = false;

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
		}
		else if ( OkCandies.Contains( ReceivedCandy.WorldModel ) )
		{
			Dialogue.Speak( OkResponse );
			sm.TreatPoints += 2;
			sm.TrickPoints += 2;
			Scene.GetComponentInChildren<Pmsound>().PlayGoodSound();
		}
		else
		{
			Dialogue.Speak( BadResponse );
			sm.TrickPoints += 4;
			Scene.GetComponentInChildren<Pmsound>().PlayBadSound();
		}

		sm.Cleanup();
	}
}
