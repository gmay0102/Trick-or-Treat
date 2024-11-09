using Sandbox;

public sealed class Tendon : Component
{
	[Property] SoundEvent Gibbed {  get; set; }

	[Broadcast]
	public void Gib()
	{
		Sound.Play( Gibbed, WorldPosition );
		GameObject.Destroy();
	}
}
