/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ConstantID.cs"
 * 
 *	This script is used by the Serialization classes to store a permanent ID
 *	of the gameObject (like InstanceID, only retained after reloading the project).
 *	To save a reference to an arbitrary object in a scene, this script must be attached to it.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[ExecuteInEditMode]
	public class ConstantID : MonoBehaviour
	{

		public int constantID;
		public bool retainInPrefab = false;
		public AutoManual autoManual = AutoManual.Automatic;

		#if UNITY_EDITOR
		private bool isNewInstance = true;
		#endif

		protected bool GameIsPlaying ()
		{
			#if UNITY_EDITOR
			
			if (!Application.isPlaying)
			{
				return false;
			}

			#endif

			return true;
		}


		#if UNITY_EDITOR

		public int AssignInitialValue ()
		{
			if (gameObject.activeInHierarchy)
			{
				retainInPrefab = false;
				SetNewID ();
			}
			else
			{
				retainInPrefab = true;
				SetNewID_Prefab ();
			}
			return constantID;
		}

		
		protected void Update ()
		{
			if (gameObject.activeInHierarchy)
			{
				if (constantID == 0)
				{
					SetNewID ();
				}
				
				if (isNewInstance)
				{
					isNewInstance = false;
					CheckForDuplicateIDs ();
				}
			}
		}


		public void SetNewID_Prefab ()
		{
			SetNewID ();
			isNewInstance = false;
		}
		

		private void SetNewID ()
		{
			// Share ID if another ID script already exists on object
			ConstantID[] idScripts = GetComponents <ConstantID>();
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript != this)
				{
					constantID = idScript.constantID;
					EditorUtility.SetDirty (this);
					return;
				}
			}

			constantID = GetInstanceID ();
			if (constantID < 0)
			{
				constantID *= -1;
			}
			EditorUtility.SetDirty (this);
			Debug.Log ("Set new ID for " + this.name + " : " + constantID);
		}
		
		
		private void CheckForDuplicateIDs ()
		{
			ConstantID[] idScripts = FindObjectsOfType (typeof (ConstantID)) as ConstantID[];
			
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript.constantID == constantID && idScript.gameObject != this.gameObject && constantID != 0)
				{
					Debug.Log ("Duplicate ID found: " + idScript.gameObject.name + " and " + this.name + " : " + constantID);
					SetNewID ();
					break;
				}
			}
		}
		
		#endif
		
	}

}