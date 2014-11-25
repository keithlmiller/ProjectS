using UnityEngine;
using System.Collections;

public class ChairSounds : MonoBehaviour {

public AudioClip Hit;

	

	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.name == "Spencer")
		{
			audio.PlayOneShot(Hit);
		}
	}
}
