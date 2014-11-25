/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberCollider.cs"
 * 
 *	This script is attached to Colliders in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberCollider : ConstantID
	{
		
		public AC_OnOff startState = AC_OnOff.On;
		
		
		public void Awake ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			if (settingsManager && GameIsPlaying ())
			{
				bool isOn = false;
				if (startState == AC_OnOff.On)
				{
					isOn = true;
				}

				if (GetComponent <Collider>())
				{
					GetComponent <Collider>().enabled = isOn;
				}

				else if (GetComponent <Collider2D>())
				{
					GetComponent <Collider2D>().enabled = isOn;
				}
			}
		}
		
		
		public ColliderData SaveData ()
		{
			ColliderData colliderData = new ColliderData ();

			colliderData.objectID = constantID;
			colliderData.isOn = false;

			if (GetComponent <Collider>())
			{
				colliderData.isOn = GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				colliderData.isOn = GetComponent <Collider2D>().enabled;
			}

			return (colliderData);
		}
		
		
		public void LoadData (ColliderData data)
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = data.isOn;
			}

			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = data.isOn;
			}
		}

	}


	[System.Serializable]
	public class ColliderData
	{
		public int objectID;
		public bool isOn;

		public ColliderData () { }
	}

}