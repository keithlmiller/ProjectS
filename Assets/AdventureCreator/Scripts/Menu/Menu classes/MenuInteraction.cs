/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInteraction.cs"
 * 
 *	This MenuElement displays a cursor icon inside a menu.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuInteraction : MenuElement
	{
		
		public enum AC_DisplayType { IconOnly, TextOnly, IconAndText };
		public AC_DisplayType displayType = AC_DisplayType.IconOnly;
		public TextAnchor anchor;
		public TextEffects textEffects;
		public int iconID;

		private CursorIcon icon;
		private string label = "";
		private CursorManager cursorManager;

		
		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (5f, 5f));
			iconID = -1;
			textEffects = TextEffects.None;
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuInteraction newElement = CreateInstance <MenuInteraction>();
			newElement.Declare ();
			newElement.CopyInteraction (this);
			return newElement;
		}
		
		
		public void CopyInteraction (MenuInteraction _element)
		{
			displayType = _element.displayType;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			iconID = _element.iconID;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				displayType = (AC_DisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);
				GetCursorGUI ();
			
				if (displayType != AC_DisplayType.IconOnly)
				{
					anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
					textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				}
			EditorGUILayout.EndVertical ();

			base.ShowGUI ();
		}
		
		
		private void GetCursorGUI ()
		{
			if (AdvGame.GetReferences ().cursorManager)
			{
				CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;

				if (cursorManager.cursorIcons.Count > 0)
				{
					int iconInt = cursorManager.GetIntFromID (iconID);
					iconInt = EditorGUILayout.Popup ("Cursor:", iconInt, cursorManager.GetLabelsArray (iconInt));
					iconID = cursorManager.cursorIcons [iconInt].id;
				}
				else
				{
					iconID = -1;
				}
			}
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

			if (displayType != AC_DisplayType.IconOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), label, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (relativeRect, zoom), label, _style);
				}
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), "", _style);
			}

			if (displayType != AC_DisplayType.TextOnly && icon != null)
			{
				icon.DrawAsInteraction (ZoomRect (relativeRect, zoom), isActive);
			}
		}


		public void MatchInteractions (InvItem item)
		{
			bool match = false;
			foreach (InvInteraction interaction in item.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					match = true;
					break;
				}
			}
			
			isVisible = match;
		}
		
		
		public void MatchInteractions (List<AC.Button> buttons)
		{
			bool match = false;
			
			foreach (AC.Button button in buttons)
			{
				if (button.iconID == iconID && !button.isDisabled)
				{
					match = true;
					break;
				}
			}
			
			isVisible = match;
		}


		public void MatchUseInteraction (AC.Button button)
		{
			if (button.iconID == iconID)
			{
				isVisible = true;
			}
		}


		public void MatchLookInteraction (int lookIconID)
		{
			if (lookIconID == iconID)
			{
				isVisible = true;
			}
		}

		
		public override void RecalculateSize ()
		{
			if (AdvGame.GetReferences ().cursorManager)
			{
				CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
				CursorIcon _icon = cursorManager.GetCursorIconFromID (iconID);
				if (_icon != null)
				{
					icon = _icon;
					label = _icon.label;
					icon.Reset ();
				}
			}

			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == AC_DisplayType.IconOnly && icon != null && icon.texture != null)
			{
				GUIContent content = new GUIContent (icon.texture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (label);
				AutoSize (content);
			}
		}
		
	}

}