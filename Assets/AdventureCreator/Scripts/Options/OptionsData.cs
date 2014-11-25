/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"OptionsData.cs"
 * 
 *	This script contains any variables we want to appear in our Options menu.
 * 
 */

[System.Serializable]
public class OptionsData
{
	
	public int language;
	public bool showSubtitles;
	
	public float sfxVolume;
	public float musicVolume;
	public float speechVolume;
	
	public string linkedVariables = "";
	
	
	public OptionsData ()
	{
		language = 0;
		showSubtitles = false;
		
		sfxVolume = 0.9f;
		musicVolume = 0.6f;
		speechVolume = 1f;
		
		linkedVariables = "";
	}
	
}