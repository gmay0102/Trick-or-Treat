using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using Sandbox.Tasks;
using System.Threading.Tasks;
using System;


interface IInteractable
{
	public void Interact( GameObject go );
}

public sealed class PlayerState : Component
{
	[Property] public PlayerController Controller;
	[Property] public ItemComponent CurrentItem { get; set; }
	[Property] public HoldingHUD itemHUD;
	[Property] public Tooltips tooltips;
	[Property] SoundEvent FailedUseSound { get; set; }
	[Property] SoundEvent DropSound { get; set; }
	[Property, Sync] public GameObject ImpactEffect { get; set; }
	[Sync] public bool HasItem { get; set; } = false;
	[Sync] public bool HasAxe { get; set; } = false;

	private GameObject currentHighlightedObject { get; set; }

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		Highlight();

		if ( Input.Pressed( "Use" ) )
			TryUse();

		if ( Input.Pressed( "drop" ) )
			DropItem();

		if ( HasAxe && Input.Pressed( "attack1" ) )
			_ = SwingAsync();
	}

	
	private DateTime lastSwingTime = DateTime.MinValue; // Store the last swing time

	private async Task SwingAsync()
	{
		if ( DateTime.Now < lastSwingTime.AddSeconds( 1 ) )
			return;

		// Update last swing time to now
		lastSwingTime = DateTime.Now;

		Playsound(DropSound);
		var animationHelper = Controller.GetComponentInChildren<CitizenAnimationHelper>();
		Animate( "b_attack" );

		await Task.Delay( 230 );

		AxeHit();
	}

	[Broadcast]
	private void Animate( string Animation )
	{
		var animationHelper = Controller.GetComponentInChildren<CitizenAnimationHelper>();
		animationHelper.Target.Set( Animation, true );
	}

	private void AxeHit()
	{
		var lookDir = Controller.LookAngleAngles.ToRotation();
		var eyePos = Scene.Camera.WorldPosition;

		var tr = Scene.Trace.WithoutTags( "player", "ignore" ).Ray( eyePos, eyePos + lookDir.Forward * 90 )
			.Size( 5f )
			.Run();

		if ( tr.Hit )
			AxeDust( tr );	
		
		if ( tr.Hit && tr.GameObject.Tags.Has("tendon"))
		{
			var td = tr.GameObject.Components.Get<Tendon>();
			td.Gib();
		}
	}
	[Broadcast]
	private void AxeDust( SceneTraceResult tr )
	{
		// Sound
		if (!IsProxy)
			tr.Surface.PlayCollisionSound( tr.HitPosition );

		// Particles
		if ( ImpactEffect.IsValid() )
		{
			var sneed = ImpactEffect.Clone( new Transform( tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt( tr.Normal ) ) );
			sneed.NetworkSpawn();
		}
	}
	private void TryUse()
	{
		var lookDir = Controller.LookAngleAngles.ToRotation();
		var eyePos = Scene.Camera.WorldPosition;

		var tr = Scene.Trace.WithoutTags( "player", "ignore", "leavepoint" ).Ray( eyePos, eyePos + lookDir.Forward * 110 )
			.Size( 5f )
			.Run();

		if ( tr.Hit && tr.GameObject.Tags.Has("interact"))
		{
			var interactObj = tr.GameObject.Components.Get<IInteractable>();
			if ( interactObj != null )
				InteractRPC( tr.GameObject );
		}
		else
		{
			Playsound( FailedUseSound );
			return;
		}
	}

	public void ClearItem()
	{
		CurrentItem = null;
		itemHUD.SetItem( null );
		tooltips.SetItem( null );
		HasItem = false;
	}

	[Broadcast] private void InteractRPC( GameObject go )
	{
		var target = go.GetComponent<IInteractable>();
		target.Interact( GameObject );
	}

	public void DropItem()
	{
		if ( CurrentItem != null )
		{
			//fun code that fuckin FLINGS whatever we're holding forward
			Playsound(DropSound);
			if (CurrentItem.DestroyOnDrop) //destroyondrop means it's expendable, so let's have fun and fling it
			{
				var projectile = CurrentItem.WorldModel.Clone(WorldPosition + Vector3.Up * 60f + Controller.LookAngleAngles.Forward * 2f);
				projectile.NetworkSpawn();
				projectile.LocalRotation = (Controller.LookAngleAngles);
				var physics = projectile.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
				physics.Velocity = (Controller.LookAngleAngles.Forward * 500f + Vector3.Up * 50f);
			}
			else
			{
				var projectile = CurrentItem.WorldModel.Clone( WorldPosition + Vector3.Up * 60f + Controller.LookAngleAngles.Forward * 2f );
				projectile.NetworkSpawn();
				projectile.LocalRotation = (Controller.LookAngleAngles);
				var physics = projectile.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
				physics.Velocity = (Controller.LookAngleAngles.Forward * 150f + Vector3.Up * 50f);
			}

			if ( CurrentItem.ItemName == "Axe" ) //let em know we don't have the axe anymore
			{
				Controller.HasAxe = false;
				HasAxe = false;
			}

			//boring code to clear out our UI and get us ready to pick items up again
			CurrentItem = null;
			itemHUD.SetItem( null );
			tooltips.SetItem( null );
			HasItem = false;
		}
	}


	[Broadcast]
	private void Playsound( SoundEvent go)
	{ Sound.Play( go, LocalPosition + Vector3.Up * 50f + Controller.LookAngleAngles.Forward * 5f); }

	private void Highlight()
	{
		var lookDir = Controller.LookAngleAngles.ToRotation();
		var eyePos = Scene.Camera.WorldPosition;

		var tr = Scene.Trace.WithoutTags( "player", "ignore", "leavepoint" ).Ray( eyePos, eyePos + lookDir.Forward * 110 )
			.Size( 5f )
			.Run();

		if ( tr.Hit && tr.Component.Tags.Has( "highlight" ) )
		{
			if ( currentHighlightedObject != tr.GameObject )
			{
				ClearCurrentHighlight();
				HighlightGameObject( tr.GameObject );
			}
		}
		else
		{
			ClearCurrentHighlight();
		}
	}

	private void HighlightGameObject( GameObject go )
	{
		var outline = go.Components.GetOrCreate<HighlightOutline>();
		if ( outline != null )
		{
			outline.Enabled = true;
			currentHighlightedObject = go;
		}
	}

	private void ClearCurrentHighlight()
	{
		if ( currentHighlightedObject != null )
		{
			var outline = currentHighlightedObject.Components.GetOrCreate<HighlightOutline>();

			if ( outline != null )
				outline.Enabled = false;

			currentHighlightedObject = null;
		}
	}

}
