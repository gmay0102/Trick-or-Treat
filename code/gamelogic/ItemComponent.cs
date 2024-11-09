using Sandbox;

public sealed class ItemComponent : Component, IInteractable
{
	[Property] public string ItemName { get; set; }
	[Property] public string ImageUrl { get; set; }
	[Property] public GameObject WorldModel {  get; set; }

	[Property] public bool DestroyOnPickup { get; set; } = false;
	[Property] public bool DestroyOnDrop { get; set; } = false;

	[Property] public string Attack1Tooltip { get; set; } = null;
	[Property] public string Attack2Tooltip { get; set; } = null;
	[Property] public string DropTooltip { get; set; } = null;

	[Property, Category( "Sound" )] SoundEvent PickUpSound { get; set; }

	public void Interact( GameObject go )
	{
		var ps = go.GetComponent<PlayerState>();
		var ih = go.Components.GetInChildren<HoldingHUD>();
		var tt = go.Components.GetInChildren<Tooltips>();

		if ( ps.HasItem )
			return;

		Sound.Play( PickUpSound, WorldPosition ); // plays pickup sound
		ps.CurrentItem = this;
		ps.HasItem = true;
		ih.SetItem( this );
		tt.SetItem( this );

		if ( this.ItemName == "Axe" )
		{
			var pc = go.GetComponent<PlayerController>();
			pc.HasAxe = true;
			ps.HasAxe = true;
		}

		if ( DestroyOnPickup )
			GameObject.Destroy();
	}
}

