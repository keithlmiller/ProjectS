/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"FirstPersonCamera.cs"
 * 
 *	An optional script that allows First Person control.
 *	This is attached to a camera which is a child of the player.
 *	It must be tagged as "FirstPersonCamera" to work.
 *	Only one First Person Camera should ever exist in the scene at runtime.
 *	Only the yaw is affected here: the pitch is determined by the player parent object.
 *
 *	Headbobbing code adapted from Mr. Animator's code: http://wiki.unity3d.com/index.php/Headbobber
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class FirstPersonCamera : _Camera
{
	
	public float rotationY = 0f;
	public Vector2 sensitivity = new Vector2 (15f, 15f);
	
	public float minY = -60F;
	public float maxY = 60F;
	
	public bool allowMouseWheelZooming = false;
	public float minimumZoom = 13f;
	public float maximumZoom = 65f;
	
	public bool headBob = true;
	public float bobbingSpeed = 0.18f;
	public float bobbingAmount = 0.2f;
	
	private float bobTimer = 0f;
	private float height = 0f;
	private float deltaHeight = 0f;

	private PlayerInput playerInput;
	private StateHandler stateHandler;


	private void OnLevelWasLoaded ()
	{
		Awake ();
	}


	protected override void Awake ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
		{
			playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		}

		height = transform.localPosition.y;
	}


	private void Start ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}
	}


	new public void ResetTarget ()
	{}
	
	
	private void Update ()
	{
		if (playerInput && headBob)
		{
			deltaHeight = 0f;
			
			if ((playerInput.moveKeys.x == 0f && playerInput.moveKeys.y == 0f) || AdvGame.GetReferences ().settingsManager.IsFirstPersonDragRotation ())
			{ 
			   bobTimer = 0f;
			} 
			else
			{
				float waveSlice = Mathf.Sin (bobTimer);
				
				if (playerInput.isRunning)
				{
					bobTimer = bobTimer + (2f * bobbingSpeed);
				}
				else
				{
					bobTimer = bobTimer + bobbingSpeed;
				}
				
				if (bobTimer > Mathf.PI * 2)
				{
					bobTimer = bobTimer - (2f * Mathf.PI);
				}
				
				float totalAxes = Mathf.Abs (playerInput.moveKeys.x) + Mathf.Abs (playerInput.moveKeys.y);
				totalAxes = Mathf.Clamp (totalAxes, 0f, 1f);
				
				deltaHeight = totalAxes * waveSlice * bobbingAmount;
			}
			
			transform.localPosition = new Vector3 (transform.localPosition.x, height + deltaHeight, transform.localPosition.z);
		}
		
		if (allowMouseWheelZooming && GetComponent <Camera>() && stateHandler && stateHandler.gameState == AC.GameState.Normal)
		{
			try
			{
				if (Input.GetAxis("Mouse ScrollWheel") > 0)
				{
					GetComponent <Camera>().fieldOfView = Mathf.Max (GetComponent <Camera>().fieldOfView - 3, minimumZoom);
				 
				}
				if (Input.GetAxis("Mouse ScrollWheel") < 0)
				{
					GetComponent <Camera>().fieldOfView = Mathf.Min (GetComponent <Camera>().fieldOfView + 3, maximumZoom);
				}
			}
			catch
			{ }
		}
	}
	
	
	private void FixedUpdate ()
	{
		rotationY = Mathf.Clamp (rotationY, minY, maxY);
		transform.localEulerAngles = new Vector3 (rotationY, 0, 0);
	}
	
	
	public void SetRotationY (float tilt)
	{
		rotationY = tilt;
	}

}
