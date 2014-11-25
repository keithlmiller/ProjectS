using UnityEngine;
using System.Collections;

public class CameraDad : MonoBehaviour {
	
	public Transform[] pivots;
	public int currPivot = 0;
	
	// Use this for initialization
	void Start () {
		NextPivot();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) NextPivot();
	}
	
	void NextPivot(){
		currPivot+=1;
		if(currPivot>=pivots.Length) currPivot = 0;
		transform.position = pivots[currPivot].position;
	}
}