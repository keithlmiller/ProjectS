using UnityEngine;
using System.Collections;

public class CameraDad : MonoBehaviour {
	
	public Transform[] pivots;
	public int currPivot = 0;
	public Texture2D overlayImage;
	public Rect overlay;

	
	// Use this for initialization
	void Start () {
		NextPivot();
		overlay = new Rect (0, 0, Screen.width, Screen.height);
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

	void onGui() {
		if(Input.GetKeyDown(KeyCode.Space))
		GUI.DrawTexture (overlay, overlayImage);
		}
}	