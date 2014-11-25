/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuDialogList.cs"
 * 
 *	This MenuElement lists the available options of the active conversation,
 *	and runs them when clicked on.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{
	
	public class MenuDialogList : MenuElement
	{
		
		public TextEffects textEffects;
		public ConversationDisplayType displayType = ConversationDisplayType.TextOnly;
		public Texture2D testIcon = null;
		public TextAnchor anchor;
		public bool fixedOption;
		public int optionToShow;
		public int maxSlots = 10;
		
		private int numOptions = 0;
		private string[] labels;
		private Texture2D[] icons;
		
		
		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			fixedOption = false;
			displayType = ConversationDisplayType.TextOnly;
			testIcon = null;
			optionToShow = 1;
			numSlots = 0;
			SetSize (new Vector2 (20f, 5f));
			maxSlots = 10;
			anchor = TextAnchor.MiddleLeft;
			textEffects = TextEffects.None;
			
			base.Declare ();
		}
		
		
		public override MenuElement DuplicateSelf ()
		{
			MenuDialogList newElement = CreateInstance <MenuDialogList>();
			newElement.Declare ();
			newElement.CopyDialogList (this);
			return newElement;
		}
		
		
		public void CopyDialogList (MenuDialogList _element)
		{
			textEffects = _element.textEffects;
			displayType = _element.displayType;
			testIcon = _element.testIcon;
			anchor = _element.anchor;
			labels = _element.labels;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			maxSlots = _element.maxSlots;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			fixedOption = EditorGUILayout.Toggle ("Fixed option number?", fixedOption);
			if (fixedOption)
			{
				numSlots = 1;
				slotSpacing = 0f;
				optionToShow = EditorGUILayout.IntSlider ("Option to display:", optionToShow, 1, 10);
			}
			else
			{
				numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, maxSlots);
				maxSlots = EditorGUILayout.IntSlider ("Maximum no. of slots:", maxSlots, 1, 10);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			}
			displayType = (ConversationDisplayType) EditorGUILayout.EnumPopup ("Display type:", displayType);
			if (displayType == ConversationDisplayType.IconOnly)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Test icon:", GUILayout.Width (145f));
				testIcon = (Texture2D) EditorGUILayout.ObjectField (testIcon, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}
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
			string labelText = "";
			
			if (fixedOption)
			{
				_slot = 0;
				
				if (!Application.isPlaying)
				{
					labelText = "Dialogue option " + optionToShow.ToString ();
				}
			}
			else
			{
				if (!Application.isPlaying)
				{
					labelText = "Dialogue option " + _slot.ToString ();
				}
			}
			
			if (Application.isPlaying)
			{
				labelText = labels [_slot];
			}
			
			if (displayType == ConversationDisplayType.TextOnly)
			{
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labelText, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labelText, _style);
				}
			}
			else
			{
				if (Application.isPlaying && icons[_slot] != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), icons[_slot], ScaleMode.StretchToFill, true, 0f);
				}
				else if (testIcon != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), testIcon, ScaleMode.StretchToFill, true, 0f);
				}
				
				GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), "", _style);
			}
		}
		
		
		public override void RecalculateSize ()
		{
			if (Application.isPlaying && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
			{
				PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
				
				if (playerInput && playerInput.activeConversation)
				{
					numOptions = playerInput.activeConversation.GetCount ();
					
					if (fixedOption)
					{
						if (numOptions < optionToShow)
						{
							numSlots = 0;
						}
						else
						{
							numSlots = 1;
							labels = new string [numSlots];
							labels[0] = playerInput.activeConversation.GetOptionName (optionToShow - 1);
							
							icons = new Texture2D [numSlots];
							icons[0] = playerInput.activeConversation.GetOptionIcon (optionToShow - 1);
						}
					}
					else
					{
						numSlots = numOptions;
						
						labels = new string [numSlots];
						icons = new Texture2D [numSlots];
						for (int i=0; i<numSlots; i++)
						{
							labels[i] = playerInput.activeConversation.GetOptionName (i + offset);
							icons[i] = playerInput.activeConversation.GetOptionIcon (i + offset);
						}
						
						if (numSlots > maxSlots)
						{
							numSlots = maxSlots;
						}
						
						LimitOffset (numOptions);
					}
				}
				else
				{
					numSlots = 0;
				}
			}
			else if (fixedOption)
			{
				numSlots = 1;
				offset = 0;
				labels = new string [numSlots];
				icons = new Texture2D [numSlots];
			}
			
			base.RecalculateSize ();
		}
		
		
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, numOptions, amount);
			}
		}
		
		
		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if ((maxSlots + offset) >= numOptions)
				{
					return false;
				}
			}
			return true;
		}
		
		
		public override string GetLabel (int slot)
		{
			if (labels.Length > slot)
			{
				return labels[slot];
			}
			
			return "";
		}
		
		
		public void RunOption (int slot)
		{
			PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
			
			if (playerInput && playerInput.activeConversation)
			{
				if (fixedOption)
				{
					playerInput.activeConversation.RunOption (optionToShow - 1);
				}
				else
				{
					playerInput.activeConversation.RunOption (slot + offset);
				}
			}
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == ConversationDisplayType.IconOnly)
			{
				AutoSize (new GUIContent (testIcon));
			}
			else
			{
				AutoSize (new GUIContent ("Dialogue option 0"));
			}
			
		}
		
	}
	
}
