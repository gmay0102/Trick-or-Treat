using Sandbox;

public sealed class Lightswitch : Component, IInteractable
{
	[Property] List<PointLight> lights { get; set; }
	[Property] SoundEvent SwitchSound { get; set; }
	[HostSync] bool LightToggle { get; set; } = false;

	public void Interact( GameObject go )
	{
		Sound.Play( SwitchSound, WorldPosition );
		foreach ( var light in lights )
			light.Enabled = LightToggle;

		LightToggle = !LightToggle;
	}
}
