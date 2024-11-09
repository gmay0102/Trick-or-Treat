// ScareMaster.cs
using Sandbox;
using System.Threading.Tasks;
using static Sandbox.Citizen.CitizenAnimationHelper;

// Define the ITrickOrTreater interface with properties and methods required by ScareMaster
public interface ITrickOrTreater
{
	string TreaterName { get; }

	bool scary { get; set; }
	bool lethal { get; set; }
	void SpeakRequest();
}

// ScareMaster class implementation
public sealed class ScareMaster : Component
{
	[Sync] public GameObject CurrentTrickOrTreater { get; set; }
	[Property] private SoundEvent doorKnockSound { get; set; }
	[Property] public GameObject DoorSpawnPosition { get; set; }
	[Sync] public int TrickPoints { get; set; } = 0;
	[Sync] public int TreatPoints { get; set; } = 0;
	[Property] public List<GameObject> Treaters { get; set; }
	[Property] public GameObject Sans { get; set; }
	[Property] private SceneFile MainMenu { get; set; }
	private bool isTalking = false;
	public bool AxelPleased = false;

	protected override void OnEnabled()
	{
		if ( Connection.Local != Connection.Host ) return;
		Scene.PhysicsWorld.Gravity = Vector3.Down * 650f;
		FirstTime();
	}

	async void FirstTime()
	{
		await Task.Delay( 20000 );
		SelectTreater();
	}

	private int treaterCounter = 0;

	void SelectTreater()
	{
		if ( Treaters.Count == 0 )
		{
			CurrentTrickOrTreater = Sans;
			EnableTreater(CurrentTrickOrTreater);


			return;
		}
		// Ensure we loop only if there are treaters left in the list
		while ( Treaters.Count > 0 )
		{
			var randomIndex = Sandbox.Game.Random.Int( 0, Treaters.Count - 1 );
			CurrentTrickOrTreater = Treaters[randomIndex];
			var testDummy = CurrentTrickOrTreater.Clone();
			var tt = testDummy.Components.Get<ITrickOrTreater>();

			// Define criteria based on the current selection phase
			bool isAccepted = false;
			if ( treaterCounter < 5 )
			{
				isAccepted = !tt.lethal && !tt.scary;
			}
			else if ( treaterCounter < 10 )
			{
				isAccepted = !tt.lethal; // Scary is allowed, lethal is not
			}
			else
			{
				isAccepted = true; // Any type allowed
			}

			// Destroy testDummy now that it's no longer needed
			testDummy.Destroy();

			// If the selected treater meets the criteria, enable and remove them
			if ( isAccepted )
			{
				EnableTreater( CurrentTrickOrTreater );
				Treaters.RemoveAt( randomIndex );
				treaterCounter++; // Increment counter for each valid selection
				break; // Exit loop once a valid treater is found
			}
		}
	}

	// Broadcasted to everyone with randomIndex determined earlier
	[Broadcast]
	void EnableTreater( GameObject go )
	{
		if ( Connection.Local == Connection.Host )
		{
			var spawn = go.Clone( DoorSpawnPosition.WorldPosition );
			spawn.NetworkSpawn();
			CurrentTrickOrTreater = spawn;
		}

		// Unlocks Front Door, plays knock sound
		var hd = Scene.Components.GetInChildren<HalloweenDoor>();
		hd.Locked = false;
		Sound.Play( doorKnockSound, hd.WorldPosition );
		isTalking = false;
		StartKnocking();
	}

	private async void StartKnocking()
	{
		while ( !isTalking )
		{
			await Task.Delay( 8000 );
			if ( !isTalking )
			{				
				if (Connection.Local == Connection.Host)
					doorknocksound();
			}
		}
	}

	[Broadcast] void doorknocksound()
	{
		var hd = Scene.Components.GetInChildren<HalloweenDoor>();
		Sound.Play( doorKnockSound, hd.WorldPosition );
	}

	public async void StartTalking()
	{
		isTalking = true;
		var tt = CurrentTrickOrTreater.Components.Get<ITrickOrTreater>();

		await Task.Delay( 2500 );

		tt.SpeakRequest();
	}

	public void LoadMainMenu()
	{
		Scene.Load( MainMenu );
	}
	public async void Cleanup()
	{
		
		await Task.Delay( 3000 ); //wait 3 seconds

		// Host closes door for everybody
		if ( !IsProxy )
		{
			var hd = Scene.Components.GetInChildren<HalloweenDoor>();
			hd.Locked = false;
			hd.Interact( null );
			hd.Locked = true;
		}

		await Task.Delay( 2500 ); //wait 2.5 seconds

		if ( Scene.GetAll<PlayerState>().ToList().Count == 0 )
		{
			Sandbox.Services.Achievements.Unlock( "badending" );
			Scene.Load( MainMenu );
		}

		// Host destroys them and starts a fresh spawn
		if ( Connection.Local == Connection.Host )
		{
			CurrentTrickOrTreater?.Destroy();
			await Task.Delay( Sandbox.Game.Random.Int( 20000, 40000 ) );
			SelectTreater();
		}
	}
}
