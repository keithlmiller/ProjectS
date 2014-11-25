/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationMesh.cs"
 * 
 *	This script is used by the MeshCollider and PolygonCollider
 *  navigation methods to define the pathfinding area.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class NavigationMesh : MonoBehaviour
	{

		public List<PolygonCollider2D> polygonColliderHoles = new List<PolygonCollider2D>();
		public bool showInEditor = true;
		public bool disableRenderer = true;

		private SettingsManager settingsManager;
		private SceneSettings sceneSettings;


		private void Awake ()
		{
			if (disableRenderer)
			{
				Hide ();
			}

			settingsManager = AdvGame.GetReferences ().settingsManager;
			sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();

			ResetHoles ();
		}


		public void AddHole (PolygonCollider2D newHole)
		{
			foreach (PolygonCollider2D hole in polygonColliderHoles)
			{
				if (hole == newHole)
				{
					return;
				}
			}

			polygonColliderHoles.Add (newHole);
			ResetHoles ();

			if (GetComponent <RememberNavMesh2D>() == null)
			{
				Debug.LogWarning ("Changes to " + this.gameObject.name + "'s holes will not be saved because it has no RememberNavMesh2D script");
			}
		}


		public void RemoveHole (PolygonCollider2D oldHole)
		{
			foreach (PolygonCollider2D hole in polygonColliderHoles)
			{
				if (hole == oldHole)
				{
					polygonColliderHoles.Remove (hole);
					ResetHoles ();
					return;
				}
			}
		}


		private void ResetHoles ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				PolygonCollider2D poly = GetComponent <PolygonCollider2D>();
				poly.pathCount = 1;
			
				if (polygonColliderHoles.Count == 0)
				{
					return;
				}

				foreach (PolygonCollider2D hole in polygonColliderHoles)
				{
					if (hole != null)
					{
						poly.pathCount ++;
						
						List<Vector2> newPoints = new List<Vector2>();
						foreach (Vector2 holePoint in hole.points)
						{
							newPoints.Add (hole.transform.TransformPoint (holePoint) - transform.position);
						}
						
						poly.SetPath (poly.pathCount-1, newPoints.ToArray ());
						hole.gameObject.layer = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
						hole.isTrigger = true;
					}
				}
			}
		}
		
		
		public void TurnOn ()
		{
			if (sceneSettings == null)
			{
				sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			}
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (sceneSettings && (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider))
			{
				if (LayerMask.NameToLayer (settingsManager.navMeshLayer) == -1)
				{
					Debug.LogWarning ("Can't find layer " + settingsManager.navMeshLayer + " - please define it in the Tags Manager and list it in the Settings Manager.");
				}
				else if (settingsManager.navMeshLayer != "")
				{
					gameObject.layer = LayerMask.NameToLayer (settingsManager.navMeshLayer);
				}
				
				if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider && GetComponent <Collider>() == null)
				{
					Debug.LogWarning ("A Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
				else if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && GetComponent <Collider2D>() == null)
				{
					Debug.LogWarning ("A 2D Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
			}
			else if (sceneSettings)
			{
				Debug.LogWarning ("Cannot enable NavMesh " + this.name + " as this scene's Navigation Method is Unity Navigation.");
			}
			else
			{
				Debug.LogWarning ("Cannot enable NavMesh - no SceneSettings found.");
			}
		}
		
		
		public void TurnOff ()
		{
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			gameObject.layer = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
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


		protected void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		protected void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}


		public virtual void DrawGizmos ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), Color.white);
			}
		}


		private void OnDestroy ()
		{
			sceneSettings = null;
			settingsManager = null;
		}
		
	}

}