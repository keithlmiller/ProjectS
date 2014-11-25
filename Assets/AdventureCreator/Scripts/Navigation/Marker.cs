/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Marker.cs"
 * 
 *	This script allows a simple way of teleporting
 *	characters and objects around the scene.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Marker : MonoBehaviour
	{

		protected void Awake ()
		{
			if (this.renderer)
			{
				this.renderer.enabled = false;
			}
			
			if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsUnity2D ())
			{
				transform.RotateAround (transform.position, Vector3.right, 90f);
				transform.RotateAround (transform.position, transform.right, -90f);
			}
		}
		
	}

}