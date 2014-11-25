/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberHotspot.cs"
 * 
 *	This script is attached to hotspot objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberHotspot : ConstantID
	{

		public AC_OnOff startState = AC_OnOff.On;


		public void Awake ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

			if (settingsManager && GameIsPlaying ())
			{
				if (startState == AC_OnOff.On)
				{
					this.gameObject.layer = LayerMask.NameToLayer (settingsManager.hotspotLayer);
				}
				else
				{
					this.gameObject.layer = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
				}
			}
		}


		public HotspotData SaveData ()
		{
			HotspotData hotspotData = new HotspotData ();
			hotspotData.objectID = constantID;
			
			if (gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				hotspotData.isOn = true;
			}
			else
			{
				hotspotData.isOn = false;
			}
			
			if (GetComponent <Hotspot>())
			{
				hotspotData.buttonStates = ButtonStatesToString (GetComponent <Hotspot>());
			}
			
			return (hotspotData);
		}


		public void LoadData (HotspotData data)
		{
			if (data.isOn)
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
			}
			
			if (GetComponent <Hotspot>())
			{
				StringToButtonStates (GetComponent <Hotspot>(), data.buttonStates);
			}
		}


		private void StringToButtonStates (Hotspot hotspot, string stateString)
		{
			if (stateString.Length == 0)
			{
				return;
			}
			
			string[] typesArray = stateString.Split ("|"[0]);
			
			if (AdvGame.GetReferences ().settingsManager == null || AdvGame.GetReferences ().settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Look interactions
				if (hotspot.provideLookInteraction && hotspot.lookButton != null)
				{
					hotspot.lookButton.isDisabled = SetButtonDisabledValue (typesArray [1]);
				}
			}

			if (hotspot.provideUseInteraction)
			{
				string[] usesArray = typesArray[0].Split (","[0]);
				
				for (int i=0; i<usesArray.Length; i++)
				{
					if (hotspot.useButtons.Count < i+1)
					{
						break;
					}
					
					hotspot.useButtons[i].isDisabled = SetButtonDisabledValue (usesArray [i]);
				}
			}

			// Inventory interactions
			if (hotspot.provideUseInteraction)
			{
				string[] invArray = typesArray[typesArray.Length - 1].Split (","[0]);
				
				for (int i=0; i<invArray.Length; i++)
				{
					if (hotspot.invButtons.Count < i+1)
					{
						break;
					}
					
					hotspot.invButtons[i].isDisabled = SetButtonDisabledValue (invArray [i]);
				}
			}
		}
		
		
		private string ButtonStatesToString (Hotspot hotspot)
		{
			System.Text.StringBuilder stateString = new System.Text.StringBuilder ();
			
			if (AdvGame.GetReferences ().settingsManager == null || AdvGame.GetReferences ().settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Single-use and Look interaction
				if (hotspot.provideLookInteraction)
				{
					stateString.Append (GetButtonDisabledValue (hotspot.lookButton) + "|");
				}
				else
				{
					stateString.Append ("0|");
				}
			}

			// Multi-use interactions
			if (hotspot.provideUseInteraction)
			{
				foreach (AC.Button button in hotspot.useButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.useButtons.IndexOf (button) < hotspot.useButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
				
			stateString.Append ("|");

			// Inventory interactions
			if (hotspot.provideInvInteraction)
			{
				foreach (AC.Button button in hotspot.invButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.invButtons.IndexOf (button) < hotspot.invButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
			
			return stateString.ToString ();
		}


		private string GetButtonDisabledValue (AC.Button button)
		{
			if (button != null && !button.isDisabled)
			{
				return ("1");
			}
			
			return ("0");
		}
		
		
		private bool SetButtonDisabledValue (string text)
		{
			if (text == "1")
			{
				return false;
			}
			
			return true;
		}

	}


	[System.Serializable]
	public class HotspotData
	{
		public int objectID;
		public bool isOn;
		public string buttonStates;
		
		public HotspotData () { }
	}

}