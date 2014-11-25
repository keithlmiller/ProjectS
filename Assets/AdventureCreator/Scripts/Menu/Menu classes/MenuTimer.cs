/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuTimer.cs"
 * 
 *	This MenuElement can be used in conjunction with MenuDialogList to create
 *	timed conversations, "Walking Dead"-style.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuTimer : MenuElement
	{
		
		public Texture2D timerTexture;
		
		
		public override void Declare ()
		{
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			SetSize (new Vector2 (20f, 5f));
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuTimer newElement = CreateInstance <MenuTimer>();
			newElement.Declare ();
			newElement.CopyTimer (this);
			return newElement;
		}
		
		
		public void CopyTimer (MenuTimer _element)
		{
			timerTexture = _element.timerTexture;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Timer texture:", GUILayout.Width (145f));
					timerTexture = (Texture2D) EditorGUILayout.ObjectField (timerTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			EndGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (timerTexture)
			{
				if (Application.isPlaying)
				{
					if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
					{
						PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
					
						if (playerInput.activeConversation && playerInput.activeConversation.isTimed)
						{
							Rect timerRect = relativeRect;
							timerRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize().x * playerInput.activeConversation.GetTimeRemaining ();
							GUI.DrawTexture (ZoomRect (timerRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
						}
					}
				}
				else
				{
					GUI.DrawTexture (ZoomRect (relativeRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
				}
			}
			
			base.Display (_style, _slot, zoom, isActive);
		}

	}

}