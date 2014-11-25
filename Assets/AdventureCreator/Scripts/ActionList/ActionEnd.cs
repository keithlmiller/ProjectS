/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCheckMultiple.cs"
 * 
 *	This is a container for "end" Action data.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class ActionEnd
	{

		public ResultAction resultAction;
		public int skipAction;
		public Action skipActionActual;
		public Cutscene linkedCutscene;
		public ActionListAsset linkedAsset;


		public ActionEnd ()
		{
			resultAction = ResultAction.Continue;
			skipAction = -1;
			skipActionActual = null;
			linkedCutscene = null;
			linkedAsset = null;
		}

	}

}