/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MainCamera.cs"
 * 
 *	This is attached to the Main Camera, and must be tagged as "MainCamera" to work.
 *	Only one Main Camera should ever exist in the scene.
 *
 *	Shake code adapted from Mike Jasper's code: http://www.mikedoesweb.com/2012/camera-shake-in-unity/
 *
 *  Aspect-rattio code adapated from Eric Haines' code: http://wiki.unity3d.com/index.php?title=AspectRatioEnforcer
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class MainCamera : MonoBehaviour
	{
		
		public Texture2D fadeTexture;
		public _Camera attachedCamera;

		public _Camera lastNavCamera;
		public _Camera lastNavCamera2;
		public bool isSmoothChanging;

		private bool isCrossfading;
		private Texture2D crossfadeTexture;
		
		private bool cursorAffectsRotation;
		
		public Vector2 perspectiveOffset = new Vector2 (0f, 0f);
		private Vector2 startPerspectiveOffset = new Vector2 (0f, 0f);

		private float timeToFade = 0f;
		private int drawDepth = -1000;
		private float alpha = 0f; 
		private FadeType fadeType;
		private float fadeStartTime;
		
		private MoveMethod moveMethod;
		private float changeTime;
		
		private	Vector3 startPosition;
		private	Quaternion startRotation;
		private float startFOV;
		private float startOrtho;
		private	float startTime;
		
		public Transform lookAtTransform;
		private Vector2 lookAtAmount;
		private float LookAtZ;
		private Vector3 lookAtTarget;
		
		private SettingsManager settingsManager;
		private StateHandler stateHandler;
		private PlayerInput playerInput;
		
		private float shakeDecay;
		private bool shakeMove;
		private float shakeIntensity;
		private Vector3 shakePosition;
		private Vector3 shakeRotation;

		// Aspect ratio
		private Camera borderCam;
		public static MainCamera mainCam;
		public float borderWidth;
		public MenuOrientation borderOrientation;

		// Split-screen
		public bool isSplitScreen;
		public bool isTopLeftSplit;
		public MenuOrientation splitOrientation;
		public _Camera splitCamera;
		public float splitAmountMain = 0.49f;
		public float splitAmountOther = 0.49f;

		private Camera _camera;


		private void Awake()
		{
			if (GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();
			}

			if (this.transform.parent && this.transform.parent.name != "_Cameras")
			{
				if (GameObject.Find ("_Cameras"))
				{
					this.transform.parent = GameObject.Find ("_Cameras").transform;
				}
				else
				{
					this.transform.parent = null;
				}
			}
			
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
			{
				playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
			}
			
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (!camera)
			{
				Debug.LogError ("The MainCamera script requires a Camera component.");
				return;
			}
			
			if (settingsManager.forceAspectRatio)
			{
				#if !UNITY_IPHONE
				settingsManager.landscapeModeOnly = false;
				#endif
				if (SetAspectRatio ())
				{
					CreateBorderCamera ();
				}
				SetCameraRect ();
			}
			
		}	

		
		private void Start()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
		
			if (lookAtTransform)
			{
				lookAtTransform.localPosition = new Vector3 (0f, 0f, 10f);
				LookAtZ = lookAtTransform.localPosition.z;
				LookAtCentre ();
			}
		}


		public void Shake (float _shakeDecay, bool _shakeMove)
		{
			shakePosition = Vector3.zero;
			shakeRotation = Vector3.zero;
			
			shakeMove = _shakeMove;
			shakeDecay = _shakeDecay;
			shakeIntensity = shakeDecay * 150f;
		}
		
		
		public bool IsShaking ()
		{
			if (shakeIntensity > 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void StopShaking ()
		{
			shakeIntensity = 0f;
			shakePosition = Vector3.zero;
			shakeRotation = Vector3.zero;
		}
		
		
		private void FixedUpdate ()
		{
			if (stateHandler && stateHandler.cameraIsOff)
			{
				return;
			}

			if (shakeIntensity > 0f)
			{
				if (shakeMove)
				{
					shakePosition = Random.insideUnitSphere * shakeIntensity * 0.5f;
				}
				
				shakeRotation = new Vector3
				(
					Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
					Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
					Random.Range (-shakeIntensity, shakeIntensity) * 0.2f
				);
				
				shakeIntensity -= shakeDecay;
			}
			else if (shakeIntensity < 0f)
			{
				StopShaking ();
			}
		}
		
		
		public void PrepareForBackground ()
		{
			if (_camera == null && GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();
			}
			if (settingsManager == null && AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			_camera.clearFlags = CameraClearFlags.Depth;

			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				_camera.cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}
		
		
		private void RemoveBackground ()
		{
			if (_camera == null && GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();
			}
			if (settingsManager == null && AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			_camera.clearFlags = CameraClearFlags.Skybox;
			
			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				_camera.cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}

		
		public void SetFirstPerson ()
		{
			if (settingsManager)
			{
				if (settingsManager.movementMethod == MovementMethod.FirstPerson || settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					GameObject FPCam = GameObject.FindWithTag (Tags.firstPersonCamera);
					if (FPCam != null)
					{
						if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
						{
							if (FPCam.GetComponent <_Camera>() == null)
							{
								FPCam.AddComponent (typeof (_Camera));
							}
							if (FPCam.GetComponent <AudioListener>())
							{
								FPCam.GetComponent <AudioListener>().enabled = false;
							}
						}

						if (FPCam.GetComponent <_Camera>())
						{
							FPCam.GetComponent <_Camera>()._camera = FPCam.GetComponent <Camera>();
							SetGameCamera (FPCam.GetComponent <_Camera>());
							SnapToAttached ();
						}
					}
				}
			}

			if (attachedCamera)
			{
				if (lastNavCamera != attachedCamera)
				{
					lastNavCamera2 = lastNavCamera;
				}

				lastNavCamera = attachedCamera;
			}
		}


		public void DrawCameraFade ()
		{
			if (timeToFade > 0f)
			{
				alpha = (Time.time - fadeStartTime) / timeToFade;

				if (fadeType == FadeType.fadeIn)
				{
					alpha = 1f - alpha;
				}

				alpha = Mathf.Clamp01 (alpha);

				if (Time.time > (fadeStartTime + timeToFade))
				{
					if (fadeType == FadeType.fadeIn)
					{
						alpha = 0f;
					}
					else
					{
						alpha = 1f;
					}

					timeToFade = 0f;
					StopCrossfade ();
				}
			}

			if (alpha > 0f)
			{
				Color tempColor = GUI.color;
				tempColor.a = alpha;
				GUI.color = tempColor;
				GUI.depth = drawDepth;

				if (isCrossfading)
				{
					GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), crossfadeTexture);
				}
				else
				{
					GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeTexture);
				}
			}
		}
		
		
		public void ResetProjection ()
		{
			if (_camera)
			{
				perspectiveOffset = Vector2.zero;
				_camera.projectionMatrix = AdvGame.SetVanishingPoint (_camera, perspectiveOffset);
				_camera.ResetProjectionMatrix ();
			}
		}


		public void ResetMoving ()
		{
			isSmoothChanging = false;
			startTime = 0f;
			changeTime = 0f;
		}

		
		private void LateUpdate ()
		{
			if (stateHandler != null && stateHandler.cameraIsOff)
			{
				return;
			}

			if (settingsManager && settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (stateHandler != null && stateHandler.gameState == GameState.Normal)
			{
				SetFirstPerson ();
			}

			if (attachedCamera && (!(attachedCamera is GameCamera25D)))
			{
				if (!isSmoothChanging)
				{
					transform.rotation = attachedCamera.transform.rotation;
					transform.position = attachedCamera.transform.position;

					if (attachedCamera is GameCamera2D)
					{
						GameCamera2D cam2D = (GameCamera2D) attachedCamera;
						perspectiveOffset = cam2D.perspectiveOffset;
						if (!_camera.orthographic)
						{
							_camera.projectionMatrix = AdvGame.SetVanishingPoint (_camera, perspectiveOffset);
						}
						else
						{
							_camera.orthographicSize = attachedCamera._camera.orthographicSize;
						}
					}
					
					else
					{
						_camera.fieldOfView = attachedCamera._camera.fieldOfView;
						if (cursorAffectsRotation)
						{
							SetlookAtTransformation ();
							transform.LookAt (lookAtTransform, attachedCamera.transform.up);
						}
					}
				}
				else
				{
					// Move from one GameCamera to another
					if (Time.time < startTime + changeTime)
					{
						if (attachedCamera is GameCamera2D)
						{
							GameCamera2D cam2D = (GameCamera2D) attachedCamera;
							
							perspectiveOffset.x = Mathf.Lerp (startPerspectiveOffset.x, cam2D.perspectiveOffset.x, AdvGame.Interpolate (startTime, changeTime, moveMethod));
							perspectiveOffset.y = Mathf.Lerp (startPerspectiveOffset.y, cam2D.perspectiveOffset.y, AdvGame.Interpolate (startTime, changeTime, moveMethod));

							_camera.ResetProjectionMatrix ();
						}
						
						if (moveMethod == MoveMethod.Curved)
						{
							// Don't slerp y position as this will create a "bump" effect
							Vector3 newPosition = Vector3.Slerp (startPosition, attachedCamera.transform.position, AdvGame.Interpolate (startTime, changeTime, moveMethod));
							newPosition.y = Mathf.Lerp (startPosition.y, attachedCamera.transform.position.y, AdvGame.Interpolate (startTime, changeTime, moveMethod));
							transform.position = newPosition;
							
							transform.rotation = Quaternion.Slerp (startRotation, attachedCamera.transform.rotation, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						}
						else
						{
							transform.position = Vector3.Lerp (startPosition, attachedCamera.transform.position, AdvGame.Interpolate (startTime, changeTime, moveMethod)); 
							transform.rotation = Quaternion.Lerp (startRotation, attachedCamera.transform.rotation, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						}

						_camera.fieldOfView = Mathf.Lerp (startFOV, attachedCamera._camera.fieldOfView, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						_camera.orthographicSize = Mathf.Lerp (startOrtho, attachedCamera._camera.orthographicSize, AdvGame.Interpolate (startTime, changeTime, moveMethod));

						if (attachedCamera is GameCamera2D && !_camera.orthographic)
						{
							_camera.projectionMatrix = AdvGame.SetVanishingPoint (_camera, perspectiveOffset);
						}
					}
					else
					{
						LookAtCentre ();
						isSmoothChanging = false;
					}
				}
				
				if (cursorAffectsRotation)
				{
					lookAtTransform.localPosition = Vector3.Lerp (lookAtTransform.localPosition, lookAtTarget, Time.deltaTime * 3f);	
				}
			}
			
			else if (attachedCamera && (attachedCamera is GameCamera25D))
			{
				transform.position = attachedCamera.transform.position;
				transform.rotation = attachedCamera.transform.rotation;
			}
			
			transform.position += shakePosition;
			transform.localEulerAngles += shakeRotation;
			
		}

		
		private void LookAtCentre ()
		{
			if (lookAtTransform)
			{
				lookAtTarget = new Vector3 (0, 0, LookAtZ);
			}
		}
		

		private void SetlookAtTransformation ()
		{
			if (stateHandler.gameState == GameState.Normal)
			{
				Vector2 mousePosition = playerInput.GetMousePosition ();
				Vector2 mouseOffset = new Vector2 (mousePosition.x / (Screen.width / 2) - 1, mousePosition.y / (Screen.height / 2) - 1);
				float distFromCentre = mouseOffset.magnitude;
		
				if (distFromCentre < 1.4f)
				{
					lookAtTarget = new Vector3 (mouseOffset.x * lookAtAmount.x, mouseOffset.y * lookAtAmount.y, LookAtZ);
				}
			}
		}
		
		
		public void SnapToAttached ()
		{
			if (attachedCamera && attachedCamera._camera)
			{
				LookAtCentre ();
				isSmoothChanging = false;
				
				_camera.isOrthoGraphic = attachedCamera._camera.isOrthoGraphic;
				_camera.fieldOfView = attachedCamera._camera.fieldOfView;
				_camera.orthographicSize = attachedCamera._camera.orthographicSize;
				transform.position = attachedCamera.transform.position;
				transform.rotation = attachedCamera.transform.rotation;
				
				if (attachedCamera is GameCamera2D)
				{
					GameCamera2D cam2D = (GameCamera2D) attachedCamera;
					perspectiveOffset = cam2D.perspectiveOffset;
				}
				else
				{
					perspectiveOffset = new Vector2 (0f, 0f);
				}
			}
		}


		public void Crossfade (float _changeTime, _Camera _linkedCamera)
		{
			object[] parms = new object[2] { _changeTime, _linkedCamera};
			StartCoroutine ("StartCrossfade", parms);
		}


		public void StopCrossfade ()
		{
			StopCoroutine ("StartCrossfade");
			if (isCrossfading)
			{
				isCrossfading = false;
				alpha = 0f;
			}
			crossfadeTexture = null;
			DestroyObject (crossfadeTexture);
		}


		private IEnumerator StartCrossfade (object[] parms)
		{
			float _changeTime = (float) parms[0];
			_Camera _linkedCamera = (_Camera) parms[1];

			yield return new WaitForEndOfFrame ();

			crossfadeTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
			crossfadeTexture.ReadPixels (new Rect (0f, 0f, Screen.width, Screen.height), 0, 0, false);
			crossfadeTexture.Apply ();

			isSmoothChanging = false;
			isCrossfading = true;
			SetGameCamera (_linkedCamera);
			FadeOut (0f);
			FadeIn (_changeTime);
			SnapToAttached ();
		}
		
		
		public void SmoothChange (float _changeTime, MoveMethod method)
		{
			LookAtCentre ();
			moveMethod = method;
			isSmoothChanging = true;
			StopCrossfade ();
			
			startTime = Time.time;
			changeTime = _changeTime;
			
			startPosition = transform.position;
			startRotation = transform.rotation;
			startFOV = _camera.fieldOfView;
			startOrtho = _camera.orthographicSize;
			
			startPerspectiveOffset = perspectiveOffset;
		}
		
		
		public void SetGameCamera (_Camera newCamera)
		{
			if (newCamera == null)
			{
				return;
			}

			if (attachedCamera != null && attachedCamera is GameCamera25D)
			{
				if (newCamera is GameCamera25D)
				{ }
				else
				{
					RemoveBackground ();
				}
			}

			_camera.ResetProjectionMatrix ();
			attachedCamera = newCamera;
			attachedCamera.SetCameraComponent ();
			
			if (attachedCamera && attachedCamera._camera)
			{
				_camera.farClipPlane = attachedCamera._camera.farClipPlane;
				_camera.nearClipPlane = attachedCamera._camera.nearClipPlane;
				_camera.orthographic = attachedCamera._camera.orthographic;
			}
			
			// Set LookAt
			if (attachedCamera is GameCamera)
			{
				GameCamera gameCam = (GameCamera) attachedCamera;
				cursorAffectsRotation = gameCam.followCursor;
				lookAtAmount = gameCam.cursorInfluence;
			}
			else if (attachedCamera is GameCameraAnimated)
			{
				GameCameraAnimated gameCam = (GameCameraAnimated) attachedCamera;
				if (gameCam.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
				{
					cursorAffectsRotation = gameCam.followCursor;
					lookAtAmount = gameCam.cursorInfluence;
				}
				else
				{
					cursorAffectsRotation = false;
				}
			}
			else
			{
				cursorAffectsRotation = false;
			}
			
			// Set background
			if (attachedCamera is GameCamera25D)
			{
				GameCamera25D cam25D = (GameCamera25D) attachedCamera;
				cam25D.SetActiveBackground ();
			}
			
			// TransparencySortMode
			if (attachedCamera is GameCamera2D)
			{
				_camera.transparencySortMode = TransparencySortMode.Orthographic;
			}
			else if (attachedCamera)
			{
				if (attachedCamera._camera.orthographic)
				{
					_camera.transparencySortMode = TransparencySortMode.Orthographic;
				}
				else
				{
					_camera.transparencySortMode = TransparencySortMode.Perspective;
				}
			}
		}
		
		
		public void FadeIn (float _timeToFade)
		{
			if (_timeToFade > 0f)
			{
				timeToFade = _timeToFade;
				alpha = 1f;
				fadeType = FadeType.fadeIn;
				fadeStartTime = Time.time;
			}
			else
			{
				alpha = 0f;
				timeToFade = 0f;
			}
		}

		
		public void FadeOut (float _timeToFade)
		{
			if (_timeToFade > 0f)
			{
				alpha = Mathf.Clamp01 (alpha);
				timeToFade = _timeToFade;
				fadeType = FadeType.fadeOut;
				fadeStartTime = Time.time - (alpha * timeToFade);
			}
			else
			{
				alpha = 1f;
				timeToFade = 0f;
			}
		}
		
		
		public bool isFading ()
		{
			if (fadeType == FadeType.fadeOut && alpha < 1f)
			{
				return true;
			}
			else if (fadeType == FadeType.fadeIn && alpha > 0f)
			{
				return true;
			}
			return false;
		}

		
		public void OnDeserializing ()
		{
			FadeIn (0.5f);
		}
		
		
		public Vector3 PositionRelativeToCamera (Vector3 _position)
		{
			return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
		}
		
		
		public Vector3 RightVector ()
		{
			return (transform.right);
		}
		
		
		public Vector3 ForwardVector ()
		{
			Vector3 camForward;
			
			camForward = transform.forward;
			camForward.y = 0;
			
			return (camForward);
		}


		private bool SetAspectRatio ()
		{
			float currentAspectRatio = 0f;
			
			if (Screen.orientation == ScreenOrientation.LandscapeRight || Screen.orientation == ScreenOrientation.LandscapeLeft)
			{
				currentAspectRatio = (float) Screen.width / Screen.height;
			}
			else
			{
				if (Screen.height  > Screen.width && settingsManager.landscapeModeOnly)
				{
					currentAspectRatio = (float) Screen.height / Screen.width;
				}
				else
				{
					currentAspectRatio = (float) Screen.width / Screen.height;
				}
			}
			
			// If the current aspect ratio is already approximately equal to the desired aspect ratio, use a full-screen Rect (in case it was set to something else previously)
			if ((int) (currentAspectRatio * 100) / 100f == (int) (settingsManager.wantedAspectRatio * 100) / 100f)
			{
				borderWidth = 0f;
				borderOrientation = MenuOrientation.Horizontal;

				if (borderCam) 
				{
					Destroy (borderCam.gameObject);
				}
				return false;
			}
			
			// Pillarbox
			if (currentAspectRatio > settingsManager.wantedAspectRatio)
			{
				borderWidth = 1f - settingsManager.wantedAspectRatio / currentAspectRatio;
				borderWidth /= 2f;
				borderOrientation = MenuOrientation.Vertical;
			}
			// Letterbox
			else
			{
				borderWidth = 1f - currentAspectRatio / settingsManager.wantedAspectRatio;
				borderWidth /= 2f;
				borderOrientation = MenuOrientation.Horizontal;
			}


			return true;
		}


		public Rect GetSplitScreenRect (bool isMainCamera)
		{
			bool _isTopLeftSplit = isTopLeftSplit;
			float split = splitAmountMain;

			if (!isMainCamera)
			{
				_isTopLeftSplit = !isTopLeftSplit;
				split = splitAmountOther;
			}

			// Pillarbox
			if (borderOrientation == MenuOrientation.Vertical)
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (!_isTopLeftSplit)
					{
						return new Rect (borderWidth, 0f, 1f - (2*borderWidth), split);
					}
					else
					{
						return new Rect (borderWidth, 1f - split, 1f - (2*borderWidth), split);
					}
				}
				else
				{
					if (_isTopLeftSplit)
					{
						return new Rect (borderWidth, 0f, split - borderWidth, 1f);
					}
					else
					{
						return new Rect (1f - split, 0f, split - borderWidth, 1f);
					}
				}
			}
			// Letterbox
			else
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (_isTopLeftSplit)
					{
						return new Rect (0f, 1f - split, 1f, split - borderWidth);
					}
					else
					{
						return new Rect (0f, borderWidth, 1f, split - borderWidth);
					}
				}
				else
				{
					if (_isTopLeftSplit)
					{
						return new Rect (0f, borderWidth, split, 1f - (2*borderWidth));
					}
					else
					{
						return new Rect (1f - split, borderWidth, split, 1f - (2*borderWidth));
					}
				}
			}
		}


		private void SetCameraRect ()
		{
			CreateBorderCamera ();

			if (isSplitScreen)
			{
				_camera.rect = GetSplitScreenRect (true);
			}
			else
			{
				if (borderOrientation == MenuOrientation.Vertical)
				{
					_camera.rect = new Rect (borderWidth, 0f, 1f - (2*borderWidth), 1f);
				}
				else if (borderOrientation == MenuOrientation.Horizontal)
				{
					_camera.rect = new Rect (0f, borderWidth, 1f, 1f - (2*borderWidth));
				}
			}
		}


		private void CreateBorderCamera ()
		{
			if (!borderCam)
			{
				// Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
				borderCam = new GameObject ("BorderCamera", typeof (Camera)).camera;
				borderCam.transform.parent = this.transform;
				borderCam.depth = int.MinValue;
				borderCam.clearFlags = CameraClearFlags.SolidColor;
				borderCam.backgroundColor = Color.black;
				borderCam.cullingMask = 0;
			}
		}
		

		public Vector2 LimitMouseToAspect (Vector2 mousePosition)
		{
			if (settingsManager == null || !settingsManager.forceAspectRatio)
			{
				return mousePosition;
			}

			if (borderOrientation == MenuOrientation.Horizontal)
			{
				// Letterbox
				int yOffset = (int) (Screen.height * borderWidth);
				
				if (mousePosition.y < yOffset)
				{
					mousePosition.y = yOffset;
				}
				else if (mousePosition.y > (Screen.height - yOffset))
				{
					mousePosition.y = Screen.height - yOffset;
				}
			}
			else
			{
				// Pillarbox
				int xOffset = (int) (Screen.width * borderWidth);

				if (mousePosition.x < xOffset)
				{
					mousePosition.x = xOffset;
				}
				else if (mousePosition.x > (Screen.width - xOffset))
				{
					mousePosition.x = Screen.width - xOffset;
				}
			}

			return mousePosition;
		}


		public static Rect _LimitMenuToAspect (Rect rect)
		{
			if (mainCam == null)
			{
				mainCam = KickStarter.mainCamera;
			}

			if (mainCam != null)
			{
				return mainCam.LimitMenuToAspect (rect);
			}

			return rect;
		}


		public Rect LimitMenuToAspect (Rect rect)
		{
			if (settingsManager == null || !settingsManager.forceAspectRatio)
			{
				return rect;
			}

			if (borderOrientation == MenuOrientation.Horizontal)
			{
				// Letterbox
				int yOffset = (int) (Screen.height * borderWidth);
				
				if (rect.y < yOffset)
				{
					rect.y = yOffset;

					if (rect.height > (Screen.height - yOffset - yOffset))
					{
						rect.height = Screen.height - yOffset - yOffset;
					}
				}
				else if (rect.y + rect.height > (Screen.height - yOffset))
				{
					rect.y = Screen.height - yOffset - rect.height;
				}
			}
			else
			{
				// Pillarbox
				int xOffset = (int) (Screen.width * borderWidth);
				
				if (rect.x < xOffset)
				{
					rect.x = xOffset;

					if (rect.width > (Screen.width - xOffset - xOffset))
					{
						rect.width = Screen.width - xOffset - xOffset;
					}
				}
				else if (rect.x + rect.width > (Screen.width - xOffset))
				{
					rect.x = Screen.width - xOffset - rect.width;
				}
			}
			
			return rect;
		}


		public void RemoveSplitScreen ()
		{
			isSplitScreen = false;
			SetCameraRect ();

			if (splitCamera)
			{
				splitCamera.RemoveSplitScreen ();
				splitCamera = null;
			}
		}


		public void SetSplitScreen (_Camera _camera1, _Camera _camera2, MenuOrientation _splitOrientation, bool _isTopLeft, float splitAmountMain, float splitAmountOther)
		{
			splitCamera = _camera2;
			isSplitScreen = true;
			splitOrientation = _splitOrientation;
			isTopLeftSplit = _isTopLeft;
			
			SetGameCamera (_camera1);
			SnapToAttached ();

			StartSplitScreen (splitAmountMain, splitAmountOther);
		}


		public void StartSplitScreen (float _splitAmountMain, float _splitAmountOther)
		{
			splitAmountMain = _splitAmountMain;
			splitAmountOther = _splitAmountOther;

			splitCamera.SetSplitScreen ();
			SetCameraRect ();
		}


		private void OnDestroy ()
		{
			crossfadeTexture = null;
			settingsManager = null;
			stateHandler = null;
			playerInput = null;
		}
		
	}

}