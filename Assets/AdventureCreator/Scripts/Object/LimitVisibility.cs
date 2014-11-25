/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LimitVisibility.cs"
 * 
 *	Attach this script to a GameObject to limit it's visibility
 *	to a specific GameCamera in your scene.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class LimitVisibility : MonoBehaviour
	{

		public _Camera limitToCamera;
		public bool affectChildren = false;

		private _Camera activeCamera;
		private bool isVisible = false;


		private void Start ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;
			
			if (activeCamera == limitToCamera)
			{
				SetVisibility (true);
			}
			else if (activeCamera != limitToCamera)
			{
				SetVisibility (false);
			}
		}


		private void Update ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;

			if (activeCamera == limitToCamera && !isVisible)
			{
				SetVisibility (true);
			}
			else if (activeCamera != limitToCamera && isVisible)
			{
				SetVisibility (false);
			}
		}


		private void SetVisibility (bool state)
		{
			if (this.renderer)
			{
				this.renderer.enabled = state;
			}
			else if (this.gameObject.GetComponent <SpriteRenderer>())
			{
				this.gameObject.GetComponent <SpriteRenderer>().enabled = state;
			}

			if (affectChildren)
			{
				SpriteRenderer[] children;
				children = GetComponentsInChildren <SpriteRenderer>();
				foreach (SpriteRenderer child in children)
				{
					child.enabled = state;
				}
			}

			isVisible = state;
		}

	}

}