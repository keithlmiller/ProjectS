/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Hotspot.cs"
 * 
 *	This script handles all the possible
 *	interactions on both hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace AC
{
	
	public class Hotspot : MonoBehaviour
	{
		
		public bool showInEditor = true;
		public ActionListSource interactionSource;
		
		public string hotspotName;
		public Highlight highlight;
		public bool playUseAnim;
		public Marker walkToMarker;
		public int lineID = -1;
		
		public bool provideUseInteraction;
		public Button useButton = new Button(); // This is now deprecated, and replaced by useButtons
		
		public List<Button> useButtons = new List<Button>();
		public bool oneClick = false;
		
		public bool provideLookInteraction;
		public Button lookButton = new Button();
		
		public bool provideInvInteraction;
		public List<Button> invButtons = new List<Button>();
		
		private float iconAlpha = 0;
		
		public Collider _collider;
		public Collider2D _collider2D;
		
		public bool drawGizmos = true;
		
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		
		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().cursorManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
				cursorManager = AdvGame.GetReferences ().cursorManager;
				
			}
			
			if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				UpgradeSelf ();
			}
			
			if (GetComponent <Collider>())
			{
				_collider = GetComponent <Collider>();
			}
			else if (GetComponent <Collider2D>())
			{
				_collider2D = GetComponent <Collider2D>();
			}
		}
		
		
		public bool UpgradeSelf ()
		{
			if (useButton.IsButtonModified ())
			{
				Button newUseButton = new Button ();
				newUseButton.CopyButton (useButton);
				useButtons.Add (newUseButton);
				useButton = new Button ();
				provideUseInteraction = true;

				if (Application.isPlaying)
				{
					Debug.Log ("Hotspot '" + gameObject.name + "' has been temporarily upgraded - please view it's Inspector when the game ends and save the scene.");
				}
				else
				{
					Debug.Log ("Upgraded Hotspot '" + gameObject.name + "', please save the scene.");
				}

				return true;
			}
			return false;
		}
		
		
		public void DrawHotspotIcon ()
		{
			if (IsOn () && Camera.main.WorldToScreenPoint (transform.position).z > 0f)
			{
				iconAlpha = 1f;
				
				if (settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenHighlighting)
				{
					if (highlight)
					{
						iconAlpha = highlight.GetHighlightAlpha ();
					}
					else
					{
						Debug.LogWarning ("Cannot display correct Hotspot Icon alpha on " + name + " because it has no associated Highlight object.");
					}
				}
			}
			else
			{
				iconAlpha = 0f;
			}
			
			if (iconAlpha > 0f)
			{
				Color c = GUI.color;
				Color tempColor = c;
				c.a = iconAlpha;
				GUI.color = c;
				
				if (settingsManager.hotspotIcon == HotspotIcon.UseIcon)
				{
					CursorIconBase icon = GetMainIcon ();
					if (icon != null)
					{
						icon.Draw (GetIconScreenPosition ());
					}
				}
				else if (settingsManager.hotspotIconTexture != null)
				{
					GUI.DrawTexture (AdvGame.GUIBox (GetIconScreenPosition (), settingsManager.hotspotIconSize), settingsManager.hotspotIconTexture, ScaleMode.ScaleToFit, true, 0f);
				}
				
				GUI.color = tempColor;
			}
		}
		
		
		public Button GetFirstUseButton ()
		{
			foreach (Button button in useButtons)
			{
				if (button != null && !button.isDisabled)
				{
					return button;
				}
			}
			return null;
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerMask.NameToLayer (settingsManager.hotspotLayer);
		}
		
		
		private void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
		}
		
		
		public bool IsOn ()
		{
			if (gameObject.layer == LayerMask.NameToLayer (settingsManager.deactivatedLayer))
			{
				return false;
			}
			
			return true;
		}
		
		
		public void Select ()
		{
			if (highlight)
			{
				highlight.HighlightOn ();
			}
		}
		
		
		public void Deselect ()
		{
			if (highlight)
			{
				highlight.HighlightOff ();
			}
		}
		
		
		public bool IsSingleInteraction ()
		{
			if (oneClick && provideUseInteraction && useButtons != null && GetFirstUseButton () != null)// && (invButtons == null || invButtons.Count == 0))
			{
				return true;
			}
			return false;
		}
		
		
		public void DeselectInstant ()
		{
			if (highlight)
			{
				highlight.HighlightOffInstant ();
			}
		}
		
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			if (this.GetComponent <AC.Char>() == null && drawGizmos)
			{
				if (GetComponent <PolygonCollider2D>())
				{
					AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), new Color (1f, 1f, 0f, 0.6f));
				}
				else
				{
					AdvGame.DrawCubeCollider (transform, new Color (1f, 1f, 0f, 0.6f));
				}
			}
		}
		
		
		private Vector2 GetIconScreenPosition ()
		{
			Vector3 screenPosition = Camera.main.WorldToScreenPoint (GetIconPosition ());
			return new Vector3 (screenPosition.x, screenPosition.y);
		}
		
		
		public Vector3 GetIconPosition ()
		{
			Vector3 worldPoint = transform.position;
			
			if (_collider != null)
			{
				if (_collider is BoxCollider)
				{
					BoxCollider boxCollider = (BoxCollider) _collider;
					worldPoint += boxCollider.center;
				}
				else if (_collider is CapsuleCollider)
				{
					CapsuleCollider capsuleCollider = (CapsuleCollider) _collider;
					worldPoint += capsuleCollider.center;
				}
			}
			else if (_collider2D != null)
			{
				if (_collider2D is BoxCollider2D)
				{
					BoxCollider2D boxCollider = (BoxCollider2D) _collider2D;
					worldPoint += new Vector3 (boxCollider.center.x, boxCollider.center.y * transform.localScale.y, 0f);
				}
			}
			
			return worldPoint;
		}
		
		
		private CursorIconBase GetMainIcon ()
		{
			if (cursorManager == null)
			{
				return null;
			}
			
			if (provideUseInteraction && useButton != null && useButton.iconID >= 0 && !useButton.isDisabled)
			{
				return cursorManager.GetCursorIconFromID (useButton.iconID);
			}
			
			if (provideLookInteraction && lookButton != null && lookButton.iconID >= 0 && !lookButton.isDisabled)
			{
				return cursorManager.GetCursorIconFromID (lookButton.iconID);
			}
			
			if (provideUseInteraction && useButtons != null && useButtons.Count > 0 && !useButtons[0].isDisabled)
			{
				return cursorManager.GetCursorIconFromID (useButtons[0].iconID);
			}
			
			return null;
		}
		
		
		public bool HasContextUse ()
		{
			if ((oneClick || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive) && provideUseInteraction && useButtons != null && GetFirstUseButton () != null)
			{
				return true;
			}
			
			return false;
		}
		
		
		public bool HasContextLook ()
		{
			if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
			{
				return true;
			}
			
			return false;
		}


		public int GetNextInteraction (int i, int numInvInteractions)
		{
			if (i < useButtons.Count)
			{
				i ++;
				while (i < useButtons.Count && useButtons [i].isDisabled)
				{
					i++;
				}

				if (i >= useButtons.Count + numInvInteractions)
				{
					return 0;
				}
				else
				{
					return i;
				}
			}
			else if (i == useButtons.Count - 1 + numInvInteractions)
			{
				return 0;
			}

			return (i+1);
		}


		public int GetPreviousInteraction (int i, int numInvInteractions)
		{
			if (i > useButtons.Count && numInvInteractions > 0)
			{
				return (i-1);
			}
			else if (i == 0)
			{
				return GetNumInteractions (numInvInteractions) - 1;
			}
			else if (i <= useButtons.Count)
			{
				i --;
				while (i > 0 && useButtons [i].isDisabled)
				{
					i --;
				}

				if (i < 0)
				{
					return GetNumInteractions (numInvInteractions) - 1;
				}
				else
				{
					return i;
				}
			}

			return (i-1);
		}


		private int GetNumInteractions (int numInvInteractions)
		{
			int num = 0;
			foreach (Button _button in useButtons)
			{
				if (!_button.isDisabled)
				{
					num ++;
				}
			}
			return (num + numInvInteractions);
		}

		
		private void OnDestroy ()
		{
			settingsManager = null;
			cursorManager = null;
		}
		
	}
	
}