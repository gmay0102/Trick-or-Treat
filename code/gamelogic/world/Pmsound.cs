using Sandbox;

public sealed class Pmsound : Component
{
	[Property] SoundEvent GreatSound = null;
	[Property] SoundEvent GoodSound = null;
	[Property] SoundEvent BadSound = null;



	public void PlayGreatSound()
	{
		Sound.Play( GreatSound, WorldPosition );
	}

	public void PlayGoodSound()
	{
		Sound.Play( GoodSound, WorldPosition );
	}

	public void PlayBadSound()
	{
		Sound.Play( BadSound, WorldPosition );
	}
}
