/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"FollowSortingMap.cs"
 * 
 *	This script causes any attached Sprite Renderer
 *	to change according to the scene's Sorting Map.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class FollowSortingMap : MonoBehaviour
	{
		
		public bool lockSorting = false;
		public bool affectChildren = true;
		public bool followSortingMap = false;
		public bool offsetOriginal = false;
		
		private float originalDepth = 0f;
		private enum DepthAxis { Y, Z };
		private DepthAxis depthAxis = DepthAxis.Y;
		
		private List<int> offsets = new List<int>();
		private int sortingOrder = 0;
		private string sortingLayer = "";
		private SortingMap sortingMap;
		private Vector2 oldPosition;
		
		private SettingsManager settingsManager;


		private void Awake ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager.IsInLoadingScene ())
			{
				return;
			}

			SetOriginalDepth ();
		}
		
		
		private void Start ()
		{
			if (settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			oldPosition = transform.position;
			UpdateSortingMap ();
			SetOriginalOffsets ();
			
			StartCoroutine ("_Update");
		}
		
		
		private IEnumerator _Update ()
		{
			while (Application.isPlaying)
			{
				UpdateRenderers ();
				yield return new WaitForFixedUpdate ();
			}
		}
		
		
		private void SetOriginalOffsets ()
		{
			offsets = new List<int>();
			
			if (offsetOriginal && renderer)
			{
				if (affectChildren)
				{
					Renderer[] renderers = GetComponentsInChildren <Renderer>();
					foreach (Renderer _renderer in renderers)
					{
						offsets.Add (_renderer.sortingOrder);
					}
				}
				else
				{
					offsets.Add (renderer.sortingOrder);
				}
			}
		}
		
		
		public string GetOrder ()
		{
			if (sortingMap == null)
			{
				return "";
			}
			
			if (sortingMap.mapType == SortingMapType.OrderInLayer)
			{
				return sortingOrder.ToString ();
			}
			else
			{
				return sortingLayer;
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (AdvGame.GetReferences ().settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			UpdateSortingMap ();
			SetOriginalOffsets ();//
			StopCoroutine ("_Update");
			StartCoroutine ("_Update");
		}
		
		
		private void SetOriginalDepth ()
		{
			if (settingsManager == null)
			{
				return;
			}
			
			if (settingsManager.IsTopDown ())
			{
				depthAxis = DepthAxis.Y;
				originalDepth = transform.position.y;
			}
			else
			{
				depthAxis = DepthAxis.Z;
				originalDepth = transform.position.z;
			}
		}
		
		
		public void SetDepth (float depth)
		{
			if (depthAxis == DepthAxis.Y)
			{
				transform.position = new Vector3 (transform.position.x, originalDepth + depth, transform.position.z);
				oldPosition = new Vector2 (transform.position.x, transform.position.z);
			}
			else
			{
				transform.position = new Vector3 (transform.position.x, transform.position.y, originalDepth + depth);
				oldPosition = new Vector2 (transform.position.x, transform.position.y);
			}
		}
		
		
		public void UpdateSortingMap ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>() && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap != null)
			{
				sortingMap = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap;
				SetOriginalDepth ();
				sortingMap.UpdateSimilarFollowers (this);
			}
			else
			{
				Debug.Log ("Cannot find sorting map to follow!");
			}
		}
		
		
		private void UpdateRenderers ()
		{
			if (lockSorting || !followSortingMap || sortingMap == null || renderer == null)
			{
				return;
			}
			
			// Check if moving in camera plane
			if ((depthAxis == DepthAxis.Y && oldPosition != new Vector2 (transform.position.x, transform.position.z)) ||
			    (depthAxis == DepthAxis.Z && oldPosition != new Vector2 (transform.position.x, transform.position.y)))
			{
				sortingMap.UpdateSimilarFollowers (this);
			}
			
			if (depthAxis == DepthAxis.Y)
			{
				oldPosition = new Vector2 (transform.position.x, transform.position.z);
			}
			else
			{
				oldPosition = new Vector2 (transform.position.x, transform.position.y);
			}
			
			if (sortingMap.sortingAreas.Count > 0)
			{
				// Set initial value as below the last line
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					sortingOrder = sortingMap.sortingAreas [sortingMap.sortingAreas.Count-1].order;
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					sortingLayer = sortingMap.sortingAreas [sortingMap.sortingAreas.Count-1].layer;
				}
				
				for (int i=0; i<sortingMap.sortingAreas.Count; i++)
				{
					// Determine angle between SortingMap's normal and relative position - if <90, must be "behind" the plane
					if (Vector3.Angle (sortingMap.transform.forward, sortingMap.GetAreaPosition (i) - transform.position) < 90f)
					{
						if (sortingMap.mapType == SortingMapType.OrderInLayer)
						{
							sortingOrder = sortingMap.sortingAreas [i].order;
						}
						else if (sortingMap.mapType == SortingMapType.SortingLayer)
						{
							sortingLayer = sortingMap.sortingAreas [i].layer;
						}
						break;
					}
				}
			}
			
			if (!affectChildren)
			{
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					renderer.sortingOrder = sortingOrder;
					
					if (offsetOriginal && offsets.Count > 0)
					{
						renderer.sortingOrder += offsets[0];
					}
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					renderer.sortingLayerName = sortingLayer;
				}
				
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			for (int i=0; i<renderers.Length; i++)
			{
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					renderers[i].sortingOrder = sortingOrder;
					
					if (offsetOriginal)
					{
						renderers[i].sortingOrder += offsets[i];
					}
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					renderers[i].sortingLayerName = sortingLayer;
				}
			}
		}
		
		
		public void LockSortingOrder (int order)
		{
			if (renderer == null) return;
			
			lockSorting = true;
			
			if (!affectChildren)
			{
				renderer.sortingOrder = order;
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			foreach (Renderer _renderer in renderers)
			{
				_renderer.sortingOrder = order;
			}
		}
		
		
		public void LockSortingLayer (string layer)
		{
			if (renderer == null) return;
			
			lockSorting = true;
			
			if (!affectChildren)
			{
				renderer.sortingLayerName = layer;
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			foreach (Renderer _renderer in renderers)
			{
				_renderer.sortingLayerName = layer;
			}
		}
		
		
		public float GetLocalScale ()
		{
			if (followSortingMap && sortingMap != null && sortingMap.affectScale)
			{
				return (sortingMap.GetScale (transform.position) / 100f);
			}
			
			return 0f;
		}
		
		
		public float GetLocalSpeed ()
		{
			if (followSortingMap && sortingMap != null && sortingMap.affectScale && sortingMap.affectSpeed)
			{
				return (sortingMap.GetScale (transform.position) / 100f);
			}
			
			return 1f;
		}
		
	}
	
}



