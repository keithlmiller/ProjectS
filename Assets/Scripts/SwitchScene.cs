using UnityEngine;
using System.Collections;

public class SwitchScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown("x"))
		{
			Application.LoadLevel("DiningRoom2");
		}
	}
}
