/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberNavMesh2D.cs"
 * 
 *	This script is attached to NavMesh2D prefabs
 *	who have their "holes" changed during gameplay.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberNavMesh2D : ConstantID
	{
		
		public NavMesh2DData SaveData ()
		{
			NavMesh2DData navMesh2DData = new NavMesh2DData ();
			
			navMesh2DData.objectID = constantID;
			
			if (GetComponent <NavigationMesh>())
			{
				NavigationMesh navMesh = GetComponent <NavigationMesh>();
				navMesh2DData.linkedIDs = new List<int>();

				for (int i=0; i<navMesh.polygonColliderHoles.Count; i++)
				{
					if (navMesh.polygonColliderHoles[i].GetComponent <ConstantID>())
					{
						navMesh2DData.linkedIDs.Add (navMesh.polygonColliderHoles[i].GetComponent <ConstantID>().constantID);
					}
					else
					{
						Debug.LogWarning ("Cannot save " + this.gameObject.name + "'s holes because " + navMesh.polygonColliderHoles[i].gameObject.name + " has no Constant ID!");
					}
				}
			}
			
			return navMesh2DData;
		}
		
		
		public void LoadData (NavMesh2DData data)
		{
			if (GetComponent <NavigationMesh>())
			{
				NavigationMesh navMesh = GetComponent <NavigationMesh>();
				navMesh.polygonColliderHoles.Clear ();
				
				for (int i=0; i<data.linkedIDs.Count; i++)
				{
					PolygonCollider2D polyHole = Serializer.returnComponent <PolygonCollider2D> (data.linkedIDs[i]);
					if (polyHole != null)
					{
						navMesh.AddHole (polyHole);
					}
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class NavMesh2DData
	{
		
		public int objectID;
		
		public List<int> linkedIDs = new List<int>();
		
		public NavMesh2DData () { }
		
	}
	
}