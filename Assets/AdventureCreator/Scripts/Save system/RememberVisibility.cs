/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberVisibility.cs"
 * 
 *	This script is attached to scene objects
 *	whose renderer.enabled state we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberVisibility : ConstantID
	{
		
		public AC_OnOff startState = AC_OnOff.On;
		public bool affectChildren = false;

		
		public void Awake ()
		{
			if (GameIsPlaying ())
			{
				bool state = false;
				if (startState == AC_OnOff.On)
				{
					state = true;
				}

				if (renderer)
				{
					renderer.enabled = state;
				}

				if (affectChildren)
				{
					foreach (Transform child in transform)
					{
						if (child.gameObject.renderer)
						{
							child.gameObject.renderer.enabled = state;
						}
					}
				}
			}
		}


		public VisibilityData SaveData ()
		{
			VisibilityData visibilityData = new VisibilityData ();
			visibilityData.objectID = constantID;
			
			if (renderer)
			{
				visibilityData.isOn = renderer.enabled;
			}
			else if (affectChildren)
			{
				foreach (Transform child in transform)
				{
					if (child.gameObject.renderer)
					{
						visibilityData.isOn = child.gameObject.renderer.enabled;
						break;
					}
				}
			}
			
			return (visibilityData);
		}


		public void LoadData (VisibilityData data)
		{
			if (renderer)
			{
				renderer.enabled = data.isOn;
			}

			if (affectChildren)
			{
				foreach (Transform child in transform)
				{
					if (child.gameObject.renderer)
					{
						child.gameObject.renderer.enabled = data.isOn;
					}
				}
			}
		}
		
	}


	[System.Serializable]
	public class VisibilityData
	{
		public int objectID;
		public bool isOn;
		
		public VisibilityData () { }
	}

}