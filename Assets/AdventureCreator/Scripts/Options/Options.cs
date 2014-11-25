/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Options.cs"
 * 
 *	This script provides a runtime instance of OptionsData,
 *	and has functions for saving and loading this data
 *	into the PlayerPrefs
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Options : MonoBehaviour
	{
		
		public OptionsData optionsData;
		
		private string ppKey = "Options";

		
		private void Awake ()
		{
			optionsData = new OptionsData();
			LoadPrefs ();

			if (optionsData.language == 0 && AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager && AdvGame.GetReferences ().speechManager.ignoreOriginalText && AdvGame.GetReferences ().speechManager.languages.Count > 1)
			{
				// Ignore original language
				optionsData.language = 1;
				SavePrefs ();
			}

			OnLevelWasLoaded ();
		}
		
		
		public void SavePrefs ()
		{
			// Linked Variables
			RuntimeVariables.DownloadAll ();
			optionsData.linkedVariables = SaveSystem.CreateVariablesData (GetComponent <RuntimeVariables>().globalVars, true, VariableLocation.Global);
			
			string optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
			
			Debug.Log ("PlayerPrefs saved.");
		}
		
		
		private void LoadPrefs ()
		{
			if (PlayerPrefs.HasKey (ppKey))
			{
				string optionsBinary = PlayerPrefs.GetString (ppKey);
				optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);
				
				Debug.Log ("PlayerPrefs loaded.");
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			SetVolume (SoundType.Music);
			SetVolume (SoundType.SFX);	
		}
		
		
		public void SetVolume (SoundType _soundType)
		{
			Sound[] soundObs = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound soundOb in soundObs)
			{
				if (soundOb.soundType == _soundType)
				{
					soundOb.AfterLoading ();
				}
			}
		}


		public static void SetLanguage (int i)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>() && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData != null)
			{
				Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
				options.optionsData.language = i;
				options.SavePrefs ();
			}
			else
			{
				Debug.LogWarning ("Could not find Options data!");
			}
		}


		public static string GetLanguageName ()
		{
			return AdvGame.GetReferences ().speechManager.languages [GetLanguage ()];
		}
		
		
		public static int GetLanguage ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>() && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData != null)
			{
				return (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData.language);
			}
			else
			{
				return 0;
			}
		}
		
	}

}