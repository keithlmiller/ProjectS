/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SaveData.cs"
 * 
 *	This script contains all the non-scene-specific data we wish to save.
 * 
 */

using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class SaveData
	{

		public MainData mainData;
		public List<PlayerData> playerData = new List<PlayerData>();
		public SaveData() { }

	}


	[System.Serializable]
	public struct MainData
	{

		public int currentPlayerID;
		public float timeScale;
		
		public string runtimeVariablesData;

		public string menuLockData;
		public string menuElementVisibilityData;
		public string menuJournalData;

		public int activeArrows;
		public int activeConversation;

		public int selectedInventoryID;

		public bool cursorIsOff;
		public bool inputIsOff;
		public bool interactionIsOff;
		public bool menuIsOff;
		public bool movementIsOff;
		public bool cameraIsOff;
		public bool triggerIsOff;
		public bool playerIsOff;

	}


	[System.Serializable]
	public struct PlayerData
	{

		public int playerID;
		public int currentScene;
		public int previousScene;
		
		public float playerLocX;
		public float playerLocY;
		public float playerLocZ;		
		public float playerRotY;
		
		public float playerWalkSpeed;
		public float playerRunSpeed;
		
		public string playerIdleAnim;
		public string playerWalkAnim;
		public string playerTalkAnim;
		public string playerRunAnim;

		public int playerWalkSound;
		public int playerRunSound;
		public int playerPortraitGraphic;
		public string playerSpeechLabel;

		public int playerTargetNode;
		public int playerPrevNode;
		public string playerPathData;
		public bool playerIsRunning;
		public bool playerLockedPath;

		public int playerActivePath;
		
		public bool playerUpLock;
		public bool playerDownLock;
		public bool playerLeftlock;
		public bool playerRightLock;
		public int playerRunLock;
		public bool playerInventoryLock;
		public bool playerIgnoreGravity;
		
		public bool playerLockDirection;
		public string playerSpriteDirection;
		public bool playerLockScale;
		public float playerSpriteScale;
		public bool playerLockSorting;
		public int playerSortingOrder;
		public string playerSortingLayer;
		
		public string inventoryData;

		public bool isHeadTurning;
		public float headTargetX;
		public float headTargetY;
		public float headTargetZ;

		public int gameCamera;
		public int lastNavCamera;
		public int lastNavCamera2;
		public float mainCameraLocX;
		public float mainCameraLocY;
		public float mainCameraLocZ;
		
		public float mainCameraRotX;
		public float mainCameraRotY;
		public float mainCameraRotZ;
		
		public bool isSplitScreen;
		public bool isTopLeftSplit;
		public bool splitIsVertical;
		public int splitCameraID;
		public float splitAmountMain;
		public float splitAmountOther;

	}

}