/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCycle.cs"
 * 
 *	This MenuElement is like a label, only it's text cycles through an array when clicked on.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuCycle : MenuElement
	{
		
		public string label = "Element";
		public TextEffects textEffects;
		public TextAnchor anchor;
		public int selected;
		public List<string> optionsArray = new List<string>();
		public AC_CycleType cycleType;
		public int varID;
		
		
		public override void Declare ()
		{
			label = "Cycle";
			selected = 0;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			textEffects = TextEffects.None;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			cycleType = AC_CycleType.CustomScript;
			varID = 0;
			optionsArray = new List<string>();
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuCycle newElement = CreateInstance <MenuCycle>();
			newElement.Declare ();
			newElement.CopyCycle (this);
			return newElement;
		}
		
		
		public void CopyCycle (MenuCycle _element)
		{
			label = _element.label;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			selected = _element.selected;
			optionsArray = _element.optionsArray;
			cycleType = _element.cycleType;
			varID = _element.varID;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Label text:", label);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			cycleType = (AC_CycleType) EditorGUILayout.EnumPopup ("Cycle type:", cycleType);
			if (cycleType == AC_CycleType.CustomScript || cycleType == AC_CycleType.Variable)
			{
				int numOptions = optionsArray.Count;
				numOptions = EditorGUILayout.IntField ("Number of choices:", optionsArray.Count);
				if (numOptions < 0)
				{
					numOptions = 0;
				}
				
				if (numOptions < optionsArray.Count)
				{
					optionsArray.RemoveRange (numOptions, optionsArray.Count - numOptions);
				}
				else if (numOptions > optionsArray.Count)
				{
					if(numOptions > optionsArray.Capacity)
					{
						optionsArray.Capacity = numOptions;
					}
					for (int i=optionsArray.Count; i<numOptions; i++)
					{
						optionsArray.Add ("");
					}
				}
				
				for (int i=0; i<optionsArray.Count; i++)
				{
					optionsArray [i] = EditorGUILayout.TextField ("Choice #" + i.ToString () + ":", optionsArray [i]);
				}
				
				if (cycleType == AC_CycleType.CustomScript)
				{
					if (optionsArray.Count > 0)
					{
						selected = EditorGUILayout.IntField ("Default option #:", selected);
					}
					ShowClipHelp ();
				}
				else if (cycleType == AC_CycleType.Variable)
				{
					varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
				}
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			string toggleText = TranslateLabel (label) + " : ";
			
			if (Application.isPlaying)
			{
				if (optionsArray.Count > selected && selected > -1)
				{
					toggleText += optionsArray [selected];
				}
				else
				{
					Debug.Log ("Could not gather options options for MenuCycle " + label);
					selected = 0;
				}
			}
			else if (optionsArray.Count > 0)
			{
				if (selected >= 0 && selected < optionsArray.Count)
				{
					toggleText += optionsArray [selected];
				}
				else
				{
					toggleText += optionsArray [0];
				}
			}
			else
			{
				toggleText += "Default option";	
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), toggleText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), toggleText, _style);
			}
		}
		
		
		public override string GetLabel (int slot)
		{
			if (optionsArray.Count > selected && selected > -1)
			{
				return TranslateLabel (label) + " : " + optionsArray [selected];
			}
			
			return TranslateLabel (label);
		}
		
		
		public void Cycle ()
		{
			selected ++;
			if (selected > optionsArray.Count-1)
			{
				selected = 0;
			}
			
			if (cycleType == AC_CycleType.Language)
			{
				if (selected == 0 && AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager && AdvGame.GetReferences ().speechManager.ignoreOriginalText && AdvGame.GetReferences ().speechManager.languages.Count > 1)
				{
					// Ignore original text by skipping to first language
					selected = 1;
				}
				Options.SetLanguage (selected);
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Integer)
					{
						var.val = selected;
						var.Upload ();
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			if (Application.isPlaying)
			{
				if (cycleType == AC_CycleType.Language)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager && GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
					{	
						SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
						Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
						optionsArray = speechManager.languages;
						if (options.optionsData != null)
						{
							selected = options.optionsData.language;
						}
					}
				}
				else if (cycleType == AC_CycleType.Variable)
				{
					if (varID >= 0)
					{
						if (RuntimeVariables.GetVariable (varID) == null || RuntimeVariables.GetVariable (varID).type != VariableType.Integer)
						{
							Debug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not an Integer.");
						}
						else if (optionsArray.Count > 0)
						{
							selected = Mathf.Clamp (RuntimeVariables.GetIntegerValue (varID), 0, optionsArray.Count - 1);
						}
						else
						{
							selected = 0;
						}
					}
				}
			}
			
			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label) + " : Default option"));
		}
		
	}
	
}