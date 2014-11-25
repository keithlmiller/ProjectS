/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberName.cs"
 * 
 *	This script is attached to gameObjects in the scene
 *	with a name we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberName : ConstantID
	{

		public NameData SaveData ()
		{
			NameData nameData = new NameData();
			nameData.objectID = constantID;
			nameData.newName = gameObject.name;

			return (nameData);
		}
		
		
		public void LoadData (NameData data)
		{
			gameObject.name = data.newName;
		}

	}


	[System.Serializable]
	public class NameData
	{
		public int objectID;
		public string newName;
		
		public NameData () { }
	}

}