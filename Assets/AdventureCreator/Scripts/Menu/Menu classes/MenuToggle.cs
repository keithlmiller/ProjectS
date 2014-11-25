/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuToggle.cs"
 * 
 *	This MenuElement toggles between On and Off when clicked on.
 *	It can be used for changing boolean options.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuToggle : MenuElement
	{
		
		public string label;
		public bool isOn;
		public TextEffects textEffects;
		public TextAnchor anchor;
		public int varID;
		public AC_ToggleType toggleType;
		public bool appendState = true;
		public Texture2D onTexture = null;
		public Texture2D offTexture = null;
		
		
		public override void Declare ()
		{
			label = "Toggle";
			isOn = false;
			isVisible = true;
			isClickable = true;
			toggleType = AC_ToggleType.CustomScript;
			numSlots = 1;
			varID = 0;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			appendState = true;
			onTexture = null;
			offTexture = null;
			textEffects = TextEffects.None;
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuToggle newElement = CreateInstance <MenuToggle>();
			newElement.Declare ();
			newElement.CopyToggle (this);
			return newElement;
		}
		
		
		public void CopyToggle (MenuToggle _element)
		{
			label = _element.label;
			isOn = _element.isOn;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			toggleType = _element.toggleType;
			varID = _element.varID;
			appendState = _element.appendState;
			onTexture = _element.onTexture;
			offTexture = _element.offTexture;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Label text:", label);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			appendState = EditorGUILayout.Toggle ("Append state to label?", appendState);
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("'On' texture:", GUILayout.Width (145f));
			onTexture = (Texture2D) EditorGUILayout.ObjectField (onTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("'Off' texture:", GUILayout.Width (145f));
			offTexture = (Texture2D) EditorGUILayout.ObjectField (offTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
			EditorGUILayout.EndHorizontal ();
			
			toggleType = (AC_ToggleType) EditorGUILayout.EnumPopup ("Toggle type:", toggleType);
			if (toggleType == AC_ToggleType.CustomScript)
			{
				isOn = EditorGUILayout.Toggle ("On by default?", isOn);
				ShowClipHelp ();
			}
			else if (toggleType == AC_ToggleType.Variable)
			{
				varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
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
			
			Rect rect = ZoomRect (relativeRect, zoom);
			if (isOn && onTexture != null)
			{
				GUI.DrawTexture (rect, onTexture, ScaleMode.StretchToFill, true, 0f);
			}
			else if (!isOn && offTexture != null)
			{
				GUI.DrawTexture (rect, offTexture, ScaleMode.StretchToFill, true, 0f);
			}
			
			string toggleText = TranslateLabel (label);
			if (appendState)
			{
				if (isOn)
				{
					toggleText += " : On";
				}
				else
				{
					toggleText += " : Off";
				}
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (rect, toggleText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (rect, toggleText, _style);
			}
		}
		
		
		public override string GetLabel (int slot)
		{
			if (isOn)
			{
				return TranslateLabel (label) + " : " + "On";
			}
			
			return TranslateLabel (label) + " : " + "Off";
		}
		
		
		public void Toggle ()
		{
			if (isOn)
			{
				isOn = false;
			}
			else
			{
				isOn = true;
			}
			
			if (toggleType == AC_ToggleType.Subtitles)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
				{
					Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
					
					options.optionsData.showSubtitles = isOn;
					options.SavePrefs ();
				}
			}
			else if (toggleType == AC_ToggleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Boolean)
					{
						if (isOn)
						{
							var.val = 1;
						}
						else
						{
							var.val = 0;
						}
						var.Upload ();
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			if (Application.isPlaying)
			{
				if (toggleType == AC_ToggleType.Subtitles)
				{	
					if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>() && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData != null)
					{
						isOn = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData.showSubtitles;
					}
				}
				else if (toggleType == AC_ToggleType.Variable)
				{
					if (varID >= 0)
					{
						if (RuntimeVariables.GetVariable (varID).type != VariableType.Boolean)
						{
							Debug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not a Boolean.");
						}
						else
						{
							isOn = RuntimeVariables.GetBooleanValue (varID);;
						}
					}
				}
			}
			
			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			if (appendState)
			{
				AutoSize (new GUIContent (TranslateLabel (label) + " : Off"));
			}
			else
			{
				AutoSize (new GUIContent (TranslateLabel (label)));
			}
		}
		
	}
	
}