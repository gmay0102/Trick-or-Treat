using Sandbox;

public sealed class Radio : Component, IInteractable
{
	[Property] SoundPointComponent soundPointComponent { get; set; }
	[Property] SoundEvent track { get; set; }
	[Property] SoundEvent radioSwitch { get; set; }

	[HostSync] public bool IsPlaying { get; set; } = false;

	public void Interact( GameObject go )
	{
		Sound.Play( radioSwitch, WorldPosition );
		if ( IsPlaying )
		{
			soundPointComponent.SoundEvent = null;
			soundPointComponent.StopSound();
			IsPlaying = false;
		}
		else
		{
			soundPointComponent.SoundEvent = track;
			soundPointComponent.StartSound();
			IsPlaying = true;
		}
	}
}
