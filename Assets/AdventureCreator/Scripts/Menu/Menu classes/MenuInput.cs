/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInput.cs"
 * 
 *	This MenuElement acts like a label, whose text can be changed with keyboard input.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuInput : MenuElement
	{
		
		public string label = "Element";
		public TextAnchor anchor;
		public TextEffects textEffects;
		public AC_InputType inputType;
		public int characterLimit = 10;
		public string linkedButton = "";
		public bool allowSpaces = false;

		private bool isSelected = false;

		
		public override void Declare ()
		{
			label = "Input";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			inputType = AC_InputType.AlphaNumeric;
			characterLimit = 10;
			linkedButton = "";
			textEffects = TextEffects.None;
			allowSpaces = false;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuInput newElement = CreateInstance <MenuInput>();
			newElement.Declare ();
			newElement.CopyInput (this);
			return newElement;
		}
		
		
		public void CopyInput (MenuInput _element)
		{
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			inputType = _element.inputType;
			characterLimit = _element.characterLimit;
			linkedButton = _element.linkedButton;
			allowSpaces = _element.allowSpaces;

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Default text:", label);
			inputType = (AC_InputType) EditorGUILayout.EnumPopup ("Input type:", inputType);
			if (inputType == AC_InputType.AlphaNumeric)
			{
				allowSpaces = EditorGUILayout.Toggle ("Allow spaces?", allowSpaces);
			}
			characterLimit = EditorGUILayout.IntField ("Character limit:", characterLimit);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			linkedButton = EditorGUILayout.TextField ("'Enter' key's linked Button:", linkedButton);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			string text = label;//TranslateLabel (label);
			if (isSelected || isActive)
			{
				if (Options.GetLanguageName () == "Arabic")
				{
					text = "|" + text;
				}
				else
				{
					text += "|";
				}
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), text, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), text, _style);
			}
		}


		public override string GetLabel (int slot)
		{
			return TranslateLabel (label);
		}


		public void CheckForInput (string input, bool shift, string menuName)
		{
			bool rightToLeft = false;
			if (Options.GetLanguageName () == "Arabic")
			{
				rightToLeft = true;
			}

			isSelected = true;
			if (input == "Backspace")
			{
				if (label.Length > 1)
				{
					if (rightToLeft)
					{
						label = label.Substring (1, label.Length - 1);
					}
					else
					{
						label = label.Substring (0, label.Length - 1);
					}
				}
				else if (label.Length == 1)
				{
					label = "";
				}
			}
			else if (input == "KeypadEnter" || input == "Return" || input == "Enter")
			{
				if (linkedButton != "" && menuName != "")
				{
					PlayerMenus.SimulateClick (menuName, PlayerMenus.GetElementWithName (menuName, linkedButton), 1);
				}
			}
			else if ((inputType == AC_InputType.AlphaNumeric && (input.Length == 1 || input.Contains ("Alpha"))) ||
			         (inputType == AC_InputType.NumbericOnly && input.Contains ("Alpha")) ||
			         (inputType == AC_InputType.AlphaNumeric && allowSpaces && input == "Space"))
			{
				input = input.Replace ("Alpha", "");
				input = input.Replace ("Space", " ");
				if (shift)
				{
					input = input.ToUpper ();
				}
				else
				{
					input = input.ToLower ();
				}

				if (characterLimit == 1)
				{
					label = input;
				}
				else if (label.Length < characterLimit)
				{
					if (rightToLeft)
					{
						label = input + label;
					}
					else
					{
						label += input;
					}
				}
			}
		}


		public override void RecalculateSize ()
		{
			Deselect ();
			base.RecalculateSize ();
		}


		public void Deselect ()
		{
			isSelected = false;
		}

		
		protected override void AutoSize ()
		{
			GUIContent content = new GUIContent (TranslateLabel (label));
			AutoSize (content);
		}
		
	}

}