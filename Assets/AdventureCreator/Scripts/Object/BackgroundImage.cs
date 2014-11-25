/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"BackgroundImage.cs"
 * 
 *	The BackgroundImage prefab is used to store a GUITexture for use in background images for 2.5D games.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class BackgroundImage : MonoBehaviour
	{
		
		private float shakeDecay;
		private float shakeIntensity;
		private Rect originalPixelInset;


		public void TurnOn ()
		{
			if (LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.backgroundImageLayer) == -1)
			{
				Debug.LogWarning ("No '" + AdvGame.GetReferences ().settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.backgroundImageLayer);
			}
			
			if (GetComponent <GUITexture>())
			{
				GetComponent <GUITexture>().enabled = true;
			}
			else
			{
				Debug.LogWarning (this.name + " has no GUITexture component");
			}
		}
		
		
		public void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
			
			if (GetComponent <GUITexture>())
			{
				GetComponent <GUITexture>().enabled = false;
			}
			else
			{
				Debug.LogWarning (this.name + " has no GUITexture component");
			}
		}
		
		
		public void Shake (float _shakeDecay)
		{
			if (shakeIntensity > 0f)
			{
				this.GetComponent <GUITexture>().pixelInset = originalPixelInset;
			}
			
			originalPixelInset = this.GetComponent <GUITexture>().pixelInset;
			
			shakeDecay = _shakeDecay;
			shakeIntensity = shakeDecay * 150f;
		}
		
		
		private void FixedUpdate ()
		{
			if (this.GetComponent <GUITexture>())
			{
				if (shakeIntensity > 0f)
				{
					float _size = Random.Range (0, shakeIntensity) * 0.2f;
					
					this.GetComponent <GUITexture>().pixelInset = new Rect
					(
						originalPixelInset.x - Random.Range (0, shakeIntensity) * 0.1f,
						originalPixelInset.y - Random.Range (0, shakeIntensity) * 0.1f,
						originalPixelInset.width + _size,
						originalPixelInset.height + _size
					);
					
					shakeIntensity -= shakeDecay;
				}
				
				else if (shakeIntensity < 0f)
				{
					shakeIntensity = 0f;
					this.GetComponent <GUITexture>().pixelInset = originalPixelInset;
				}
			}
		}
		
	}

}