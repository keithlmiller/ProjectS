/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeVariables.cs"
 * 
 *	This script creates a local copy of the VariableManager's Global vars.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class RuntimeVariables : MonoBehaviour
	{
		
		public List<GVar> globalVars = new List<GVar>();
		
		
		public void Awake ()
		{
			// Transfer the vars set in VariablesManager to self on runtime
			UpdateSelf ();
			LinkAllValues ();
		}
		
		
		private void UpdateSelf ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
			{
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;

				globalVars.Clear ();
				foreach (GVar assetVar in variablesManager.vars)
				{
					globalVars.Add (new GVar (assetVar));
				}

				// Options Variables
				if (GetComponent <Options>() && GetComponent <Options>().optionsData != null && GetComponent <Options>().optionsData.linkedVariables != "")
				{
					SaveSystem.AssignVariables (GetComponent <Options>().optionsData.linkedVariables);
				}
			}

		}


		private void LinkAllValues ()
		{
			foreach (GVar var in globalVars)
			{
				if (var.link == VarLink.PlaymakerGlobalVariable)
				{
					if (var.updateLinkOnStart)
					{
						var.Download ();
					}
					else
					{
						var.Upload ();
					}
				}
			}
		}


		public static List<GVar> GetAllVars ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
			{
				return GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>().globalVars;
			}
			return null;
		}


		public static void UploadAll ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
			{
				foreach (GVar var in GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>().globalVars)
				{
					var.Upload ();
				}
			}
		}


		public static void DownloadAll ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
			{
				foreach (GVar var in GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>().globalVars)
				{
					var.Download ();
				}
			}
		}


		public static GVar GetVariable (int _id)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
			{
				foreach (GVar _var in GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>().globalVars)
				{
					if (_var.id == _id)
					{
						return _var;
					}
				}
			}

			return null;
		}


		public static int GetIntegerValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).val;
		}


		public static bool GetBooleanValue (int _id)
		{
			if (RuntimeVariables.GetVariable (_id).val == 1)
			{
				return true;
			}
			return false;
		}


		public static string GetStringValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).textVal;
		}


		public static float GetFloatValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).floatVal;
		}


		public static void SetIntegerValue (int _id, int _value)
		{
			RuntimeVariables.GetVariable (_id).val = _value;
		}
		
		
		public static void SetBooleanValue (int _id, bool _value)
		{
			if (_value)
			{
				RuntimeVariables.GetVariable (_id).val = 1;
			}
			else
			{
				RuntimeVariables.GetVariable (_id).val = 0;
			}
		}
		
		
		public static void SetStringValue (int _id, string _value)
		{
			RuntimeVariables.GetVariable (_id).textVal = _value;
		}
		
		
		public static void SetFloatValue (int _id, float _value)
		{
			RuntimeVariables.GetVariable (_id).floatVal = _value;
		}
		
	}

}