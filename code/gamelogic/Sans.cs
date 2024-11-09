using Sandbox;

public sealed class Sans : Component, ITrickOrTreater, Component.ICollisionListener
{
	[Property] public string TreaterName { get; set; } = "Sans";
	[Property] public DialogueBubble Dialogue { get; set; }
	[Property] private SoundBoxComponent SoundBox { get; set; }
	public bool scary { get; set; } = true;
	public bool lethal { get; set; } = true;


	public void SpeakRequest()
	{
		Dialogue.Enabled = true;
		Dialogue.Speak( "heya, how's your night been?" );
		SoundBox.StartSound();
		BeginGhostEvent();
	}

	private async void BeginGhostEvent()
	{
		await Task.Delay( 5000 );
		Dialogue.ClearText();
		var livecount = Scene.GetAll<PlayerState>().ToList().Count;
		Dialogue.Speak( "wow, only " + livecount + " of you left?" );

		await Task.Delay( 5000 );

		Dialogue.ClearText();
		Dialogue.Speak( "let's see how you did" );

		await Task.Delay( 5000 );

		Dialogue.ClearText();
		var scareMaster = Scene.GetComponentInChildren<ScareMaster>();
		var treat = scareMaster.TreatPoints;
		var trick = scareMaster.TrickPoints;
		Dialogue.Speak( treat + " treat points, and " + trick + " trick points..." );

		await Task.Delay( 5000 );

		Dialogue.ClearText();

		// Check if treat points are greater or players survived despite low treat points
		if ( treat > trick )
		{
			// Positive response
			Dialogue.Speak( "looks like you did well enough for the kids" );
			await Task.Delay( 5000 );
			Dialogue.ClearText();
			Dialogue.Speak( "now get on outta here, you scamps" );
			Sandbox.Services.Achievements.Unlock( "goodending" );
			await Task.Delay( 10000 );
			scareMaster.LoadMainMenu();
		}
		else
		{
			// Mocking response for surviving despite low treat points
			Dialogue.Speak( "well, you weren't exactly nice to the kids, but hey, somehow you're still here" );
			await Task.Delay( 8000 );
			Dialogue.ClearText();
			Dialogue.Speak( "guess you can call this a win... kinda" );
			Sandbox.Services.Achievements.Unlock( "goodending" );
			await Task.Delay( 10000 );
			scareMaster.LoadMainMenu();
		}

	}


}
