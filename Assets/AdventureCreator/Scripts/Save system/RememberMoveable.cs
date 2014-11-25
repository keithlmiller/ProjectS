/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberTransform.cs"
 * 
 *	This script, when attached to Moveable objects in the scene,
 *	will record appropriate positional data
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class RememberMoveable : ConstantID
	{

		public AC_OnOff startState = AC_OnOff.On;
		
		
		public void Awake ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			if (settingsManager && GameIsPlaying ())
			{
				if (GetComponent <DragBase>())
				{
					if (startState == AC_OnOff.On)
					{
						GetComponent <DragBase>().TurnOn ();
					}
					else
					{
						GetComponent <DragBase>().TurnOff ();
					}
				}

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

		
		public MoveableData SaveData ()
		{
			MoveableData moveableData = new MoveableData ();
			
			moveableData.objectID = constantID;

			if (gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				moveableData.isOn = true;
			}
			else
			{
				moveableData.isOn = false;
			}

			if (GetComponent <Moveable_Drag>())
			{
				Moveable_Drag moveable_Drag = GetComponent <Moveable_Drag>();
				moveableData.trackValue = moveable_Drag.trackValue;
				moveableData.revolutions = moveable_Drag.revolutions;
			}
			
			moveableData.LocX = transform.position.x;
			moveableData.LocY = transform.position.y;
			moveableData.LocZ = transform.position.z;
			
			moveableData.RotX = transform.eulerAngles.x;
			moveableData.RotY = transform.eulerAngles.y;
			moveableData.RotZ = transform.eulerAngles.z;
			
			moveableData.ScaleX = transform.localScale.x;
			moveableData.ScaleY = transform.localScale.y;
			moveableData.ScaleZ = transform.localScale.z;
			
			return moveableData;
		}
		
		
		public void LoadData (MoveableData data)
		{
			if (GetComponent <DragBase>())
			{
				if (data.isOn)
				{
					GetComponent <DragBase>().TurnOn ();
				}
				else
				{
					GetComponent <DragBase>().TurnOff ();
				}
			}

			if (data.isOn)
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
			}

			transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
			transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
			transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);

			if (GetComponent <Moveable_Drag>())
			{
				Moveable_Drag moveable_Drag = GetComponent <Moveable_Drag>();
				moveable_Drag.isHeld = false;
				if (moveable_Drag.dragMode == DragMode.LockToTrack && moveable_Drag.track != null)
				{
					moveable_Drag.trackValue = data.trackValue;
					moveable_Drag.revolutions = data.revolutions;
					moveable_Drag.StopAutoMove ();
					moveable_Drag.track.SetPositionAlong (data.trackValue, moveable_Drag);
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class MoveableData
	{
		
		public int objectID;

		public bool isOn;

		public float trackValue;
		public int revolutions;

		public float LocX;
		public float LocY;
		public float LocZ;
		
		public float RotX;
		public float RotY;
		public float RotZ;
		
		public float ScaleX;
		public float ScaleY;
		public float ScaleZ;
		
		public MoveableData () { }
		
	}
	
}