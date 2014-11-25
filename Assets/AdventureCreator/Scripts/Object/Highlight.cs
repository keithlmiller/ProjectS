/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Highlight.cs"
 * 
 *	This script is attached to any gameObject that glows
 *	when a cursor is placed over it's associated interaction
 *	object.  These are not always the same object.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Highlight : MonoBehaviour
	{

		public bool brightenMaterials = true;

		private float maxHighlight = 2f;
		private float highlight = 1f;
		private int direction = 1;
		private float fadeStartTime;
		private float fadeTime = 0.3f;

		private HighlightState highlightState = HighlightState.None;

		private List<Color> originalColors = new List<Color>();


		public float GetHighlightIntensity ()
		{
			return (highlight - 1f) / maxHighlight;
		}
		
		
		private void Awake ()
		{
			// Go through own materials
			if (renderer)
			{
				foreach (Material material in renderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}
			
			// Go through any child materials
			Component[] children;
			children = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in children)
			{
				foreach (Material material in childRenderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}

			if (guiTexture)
			{
				originalColors.Add (guiTexture.color);
			}
		}
		
		
		private void FixedUpdate ()
		{
			if (highlightState != HighlightState.None)
			{	
				if (direction == 1)
				{
					// Add highlight
					highlight = Mathf.Lerp (1f, maxHighlight, AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear));

					if (highlight >= maxHighlight)
					{
						highlight = maxHighlight;
						
						if (highlightState == HighlightState.Flash || highlightState == HighlightState.Pulse)
						{
							direction = -1;
							fadeStartTime = Time.time;
						}
						else
						{
							highlightState = HighlightState.None;
						}
					}
				}
				else
				{
					// Remove highlight
					highlight = Mathf.Lerp (maxHighlight, 1f, AdvGame.Interpolate (fadeStartTime, fadeTime, AC.MoveMethod.Linear));

					if (highlight <= 1f)
					{
						highlight = 1f;

						if (highlightState == HighlightState.Pulse)
						{
							direction = 1;
							fadeStartTime = Time.time;
						}
						else
						{
							highlightState = HighlightState.None;
						}
					}
				}

				if (brightenMaterials)
				{
					UpdateMaterials ();
				}
			}
		}
		
		
		public void HighlightOn ()
		{
			highlightState = HighlightState.Normal;
			direction = 1;
			fadeStartTime = Time.time;
			
			if (highlight > 1f)
			{
				fadeStartTime -= (highlight - 1f) / (maxHighlight - 1f) * fadeTime;
			}
			else
			{
				highlight = 1f;
			}
		}


		public void HighlightOnInstant ()
		{
			highlightState = HighlightState.None;
			highlight = maxHighlight;
			
			UpdateMaterials ();
		}
		
		
		public void HighlightOff ()
		{
			highlightState = HighlightState.Normal;
			direction = -1;
			fadeStartTime = Time.time;
			
			if (highlight < maxHighlight)
			{
				fadeStartTime -= (maxHighlight - highlight) / (maxHighlight - 1) * fadeTime;
			}
			else
			{
				highlight = maxHighlight;
			}
		}
		
		
		public void Flash ()
		{
			if (highlightState != HighlightState.Flash && (highlightState == HighlightState.None || direction == -1))
			{
				highlightState = HighlightState.Flash;
				highlight = 1f;
				direction = 1;
				fadeStartTime = Time.time;
			}
		}


		public float GetFlashTime ()
		{
			return fadeTime * 2f;
		}


		public float GetFadeTime ()
		{
			return fadeTime;
		}


		public void Pulse ()
		{
			highlightState = HighlightState.Pulse;
			highlight = 1f;
			direction = 1;
			fadeStartTime = Time.time;
		}


		public float GetHighlightAlpha ()
		{
			return (highlight - 1f);
		}
		
		
		public void HighlightOffInstant ()
		{
			highlightState = HighlightState.None;
			highlight = 1f;

			UpdateMaterials ();
		}


		private void UpdateMaterials ()
		{
			int i = 0;
			float alpha;

			// Go through own materials
			if (renderer)
			{
				foreach (Material material in renderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						alpha = material.color.a;
						Color newColor = originalColors[i] * highlight;
						newColor.a = alpha;
						material.color = newColor;
						i++;
					}
				}
			}
			
			// Go through materials
			Component[] children;
			children = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in children)
			{
				foreach (Material material in childRenderer.materials)
				{
					if (originalColors.Count <= i)
					{
						break;
					}
					
					if (material.HasProperty ("_Color"))
					{
						alpha = material.color.a;
						Color newColor = originalColors[i] * highlight;
						newColor.a = alpha;
						material.color = newColor;
						i++;
					}
				}
			}
			
			if (guiTexture)
			{
				alpha = Mathf.Lerp (0.2f, 1f, highlight - 1f); // highlight is between 1 and 2
				Color newColor = originalColors[i];
				newColor.a = alpha;
				guiTexture.color = newColor;
			}
		}

	}

}