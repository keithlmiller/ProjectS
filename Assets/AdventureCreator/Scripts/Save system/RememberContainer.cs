/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberContainer.cs"
 * 
 *	This script is attached to container objects in the scene
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberContainer : ConstantID
	{
		
		public ContainerData SaveData ()
		{
			ContainerData containerData = new ContainerData();
			containerData.objectID = constantID;
			
			if (GetComponent <Container>())
			{
				Container container = GetComponent <Container>();
				containerData.linkedIDs = new List<int>();
				containerData.counts = new List<int>();
				containerData.IDs = new List<int>();

				for (int i=0; i<container.items.Count; i++)
				{
					containerData.linkedIDs.Add (container.items[i].linkedID);
					containerData.counts.Add (container.items[i].count);
					containerData.IDs.Add (container.items[i].id);
				}
			}
			
			return (containerData);
		}
		
		
		public void LoadData (ContainerData data)
		{
			if (GetComponent <Container>())
			{
				Container container = GetComponent <Container>();
				container.items.Clear ();

				for (int i=0; i<data.IDs.Count; i++)
				{
					ContainerItem newItem = new ContainerItem (data.linkedIDs[i], data.counts[i], data.IDs[i]);
					container.items.Add (newItem);
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class ContainerData
	{
		public int objectID;
		public List<int> linkedIDs = new List<int>();
		public List<int> counts = new List<int>();
		public List<int> IDs = new List<int>();

		public ContainerData () { }
	}
	
}