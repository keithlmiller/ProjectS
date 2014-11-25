/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavMeshSegment.cs"
 * 
 *	This script is used for the NavMeshSegment prefab, which defines
 *	the area to be baked by the Unity Navigation window.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class NavMeshSegment : MonoBehaviour
{
	
	private SceneSettings sceneSettings;
	
	
	private void Awake ()
	{
		Hide ();

		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			if (sceneSettings.navigationMethod == AC_NavigationMethod.UnityNavigation)
			{
				if (LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.navMeshLayer) == -1)
				{
					Debug.LogWarning ("No 'NavMesh' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.navMeshLayer);
				}
			}
		}
	}
	
	
	public void Hide ()
	{
		if (this.GetComponent <MeshRenderer>())
		{
			this.GetComponent <MeshRenderer>().enabled = false;
		}
	}
	
	
	public void Show ()
	{
		if (this.GetComponent <MeshRenderer>() && this.GetComponent <MeshFilter>() && this.GetComponent <MeshCollider>() && this.GetComponent <MeshCollider>().sharedMesh)
		{
			this.GetComponent <MeshFilter>().mesh = this.GetComponent <MeshCollider>().sharedMesh;
			this.GetComponent <MeshRenderer>().enabled = true;
			this.GetComponent <MeshRenderer>().castShadows = false;
			this.GetComponent <MeshRenderer>().receiveShadows = false;
		}
	}
	
}
