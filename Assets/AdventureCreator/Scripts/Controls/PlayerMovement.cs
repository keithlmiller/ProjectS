/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMovement.cs"
 * 
 *	This script analyses the variables in PlayerInput, and moves the character
 *	based on the control style, defined in the SettingsManager.
 *	To move the Player during cutscenes, a PlayerPath object must be defined.
 *	This Path will dynamically change based on where the Player must travel to.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class PlayerMovement : MonoBehaviour
	{

		private FirstPersonCamera firstPersonCamera;

		private StateHandler stateHandler;
		private PlayerInput playerInput;
		private PlayerMenus playerMenus;
		private PlayerCursor playerCursor;
		private PlayerInteraction playerInteraction;
		private SettingsManager settingsManager;
		private SceneSettings sceneSettings;
		private NavigationManager navigationManager;
		private RuntimeInventory runtimeInventory;
		
		
		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			playerInput = this.GetComponent <PlayerInput>();
			playerInteraction = this.GetComponent <PlayerInteraction>();
			playerCursor = this.GetComponent <PlayerCursor>();
			sceneSettings = this.GetComponent <SceneSettings>();
			navigationManager = this.GetComponent <NavigationManager>();
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.firstPersonCamera) && GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>())
			{
				firstPersonCamera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>();
			}

			playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			runtimeInventory = playerMenus.GetComponent <RuntimeInventory>();
		}


		public void UpdatePlayerMovement ()
		{
			if (settingsManager && KickStarter.player && playerInput && playerInteraction)
			{
				if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UFPSControlPlayer ();
					return;
				}

				if (playerInput.activeArrows == null)
				{
					if (playerInput.mouseState == MouseState.SingleClick && !playerInput.interactionMenuIsOn && !playerInput.mouseOverMenu && !playerInteraction.IsMouseOverHotspot ())
					{
						if (playerInput.hotspotMovingTo != null)
						{
							StopMovingToHotspot ();
						}

						playerInteraction.DisableHotspot (false);
					}

					if (playerInput.hotspotMovingTo != null && settingsManager.movementMethod != MovementMethod.PointAndClick && playerInput.moveKeys != Vector2.zero)
					{
						StopMovingToHotspot ();
					}

					if (settingsManager.movementMethod == MovementMethod.Direct)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							DragPlayer (true);
						}
						else
						{
							if (KickStarter.player.GetPath () == null || !KickStarter.player.lockedPath)
							{
								// Normal gameplay
								DirectControlPlayer (true);
							}
							else
							{
								// Move along pre-determined path
								DirectControlPlayerPath ();
							}
						}
					}

					else if (settingsManager.movementMethod == MovementMethod.Drag)
					{
						DragPlayer (true);
					}

					else if (settingsManager.movementMethod == MovementMethod.StraightToCursor)
					{
						MoveStraightToCursor ();
					}
					
					else if (settingsManager.movementMethod == MovementMethod.PointAndClick)
					{
						PointControlPlayer ();
					}
					
					else if (settingsManager.movementMethod == MovementMethod.FirstPerson)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							FirstPersonControlPlayer ();

							if (settingsManager.dragAffects == DragAffects.Movement)
							{
								DragPlayer (false);
							}
							else
							{
								DragPlayerLook ();
							}
						}
						else
						{
							FirstPersonControlPlayer ();
							DirectControlPlayer (false);
						}
					}
				}
			}
		}


		// Straight to cursor functions

		private void MoveStraightToCursor ()
		{
			if (playerInput.isDownLocked && playerInput.isUpLocked && playerInput.isLeftLocked && playerInput.isRightLocked)
			{
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				return;
			}

			if (playerInput.dragState == DragState.None)
			{
				playerInput.dragSpeed = 0f;
				
				if (KickStarter.player.charState == CharState.Move && KickStarter.player.GetPath () == null)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}

			if (playerInput.mouseState == MouseState.SingleClick && settingsManager.singleTapStraight)
			{
				Vector3 clickPoint = ClickPoint (playerInput.GetMousePosition ());
				Vector3 moveDirection = clickPoint - KickStarter.player.transform.position;
				
				if (clickPoint != Vector3.zero)
				{
					if (moveDirection.magnitude < settingsManager.destinationAccuracy / 2f)
					{
						if (KickStarter.player.charState == CharState.Move)
						{
							KickStarter.player.charState = CharState.Decelerate;
						}
					}
					
					else if (moveDirection.magnitude > settingsManager.destinationAccuracy)
					{
						if (settingsManager.IsUnity2D ())
						{
							moveDirection = new Vector3 (moveDirection.x, 0f, moveDirection.y);
						}
						
						bool run = false;
						if (moveDirection.magnitude > settingsManager.dragRunThreshold)
						{
							run = true;
						}
						
						if (playerInput.runLock == PlayerMoveLock.AlwaysRun)
						{
							run = true;
						}
						else if (playerInput.runLock == PlayerMoveLock.AlwaysWalk)
						{
							run = false;
						}

						List<Vector3> pointArray = new List<Vector3>();
						pointArray.Add (clickPoint);
						KickStarter.player.MoveAlongPoints (pointArray.ToArray (), run);
					}
				}
			}

			else if (playerInput.dragState == DragState.Player && (!settingsManager.singleTapStraight || playerInput.CanClick ()))
			{
				Vector3 clickPoint = ClickPoint (playerInput.GetMousePosition ());
				Vector3 moveDirection = clickPoint - KickStarter.player.transform.position;

				if (clickPoint != Vector3.zero)
				{
					if (moveDirection.magnitude < settingsManager.destinationAccuracy / 2f)
					{
						if (KickStarter.player.charState == CharState.Move)
						{
							KickStarter.player.charState = CharState.Decelerate;
						}

						if (KickStarter.player.activePath)
						{
							KickStarter.player.EndPath ();
						}
					}

					else if (moveDirection.magnitude > settingsManager.destinationAccuracy)
					{
						if (settingsManager.IsUnity2D ())
						{
							moveDirection = new Vector3 (moveDirection.x, 0f, moveDirection.y);
						}

						bool run = false;
						if (moveDirection.magnitude > settingsManager.dragRunThreshold)
						{
							run = true;
						}

						if (playerInput.runLock == PlayerMoveLock.AlwaysRun)
						{
							run = true;
						}
						else if (playerInput.runLock == PlayerMoveLock.AlwaysWalk)
						{
							run = false;
						}

						KickStarter.player.isRunning = run;
						KickStarter.player.charState = CharState.Move;
						
						KickStarter.player.SetLookDirection (moveDirection, false);
						KickStarter.player.SetMoveDirectionAsForward ();

						if (KickStarter.player.activePath)
						{
							KickStarter.player.EndPath ();
						}
					}
				}
				else
				{
					if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.charState = CharState.Decelerate;
					}

					if (KickStarter.player.activePath)
					{
						KickStarter.player.EndPath ();
					}
				}
			}
		}


		public Vector3 ClickPoint (Vector2 mousePosition)
		{
			if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				RaycastHit2D hit;
				if (Camera.main.isOrthoGraphic)
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 pos = mousePosition;
					pos.z = KickStarter.player.transform.position.z - Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
				}
				
				if (hit.collider != null)
				{
					return hit.point;
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (mousePosition);
				RaycastHit hit = new RaycastHit();
				
				if (settingsManager && sceneSettings && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength))
				{
					return hit.point;
				}
			}
			
			return Vector3.zero;
		}
		
		
		// Drag functions

		private void DragPlayer (bool doRotation)
		{
			if (playerInput.dragState == DragState.None)
			{
				playerInput.dragSpeed = 0f;
				
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			
			if (playerInput.dragState == DragState.Player)
			{
				Vector3 moveDirectionInput = Vector3.zero;
				
				if (settingsManager.IsTopDown ())
				{
					moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
				}
				else
				{
					moveDirectionInput = (playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
				}
				
				if (playerInput.dragSpeed > settingsManager.dragWalkThreshold * 10f)
				{
					KickStarter.player.isRunning = playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
				
					if (doRotation)
					{
						KickStarter.player.SetLookDirection (moveDirectionInput, false);
						KickStarter.player.SetMoveDirectionAsForward ();
					}
					else
					{
						if (playerInput.GetDragVector ().y < 0f)
						{
							KickStarter.player.SetMoveDirectionAsForward ();
						}
						else
						{
							KickStarter.player.SetMoveDirectionAsBackward ();
						}
					}
				}
				else
				{
					if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.charState = CharState.Decelerate;
					}
				}
			}
		}
		
		
		// Direct-control functions
		
		private void DirectControlPlayer (bool doRotation)
		{
			if (settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
			{
				if (playerInput.moveKeys != Vector2.zero)
				{
					Vector3 moveDirectionInput = Vector3.zero;

					if (settingsManager.IsTopDown ())
					{
						moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
					}
					else
					{
						moveDirectionInput = (playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
					}
			
					KickStarter.player.isRunning = playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
					
					if (doRotation)
					{
						KickStarter.player.SetLookDirection (moveDirectionInput, false);
						KickStarter.player.SetMoveDirectionAsForward ();
					}
					else
					{
						KickStarter.player.SetMoveDirection (moveDirectionInput);
					}
				}
				else if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			
			else if (settingsManager.directMovementType == DirectMovementType.TankControls)
			{
				if (playerInput.moveKeys.x < -0.5f)
				{
					KickStarter.player.TankTurnLeft ();
				}
				else if (playerInput.moveKeys.x > 0.5f)
				{
					KickStarter.player.TankTurnRight ();
				}
				else
				{
					KickStarter.player.StopTurning ();
				}
				
				if (playerInput.moveKeys.y > 0f)
				{
					KickStarter.player.isRunning = playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsForward ();
				}
				else if (playerInput.moveKeys.y < 0f)
				{
					KickStarter.player.isRunning = playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsBackward ();
				}
				else if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
					KickStarter.player.SetMoveDirectionAsForward ();
				}
			}
		}


		private void UFPSControlPlayer ()
		{
			if (playerInput.moveKeys != Vector2.zero)
			{
				KickStarter.player.isRunning = playerInput.isRunning;
				KickStarter.player.charState = CharState.Move;
			}
			else if (KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.charState = CharState.Decelerate;
			}
		}
		
		
		private void DirectControlPlayerPath ()
		{
			if (playerInput.moveKeys != Vector2.zero)
			{
				Vector3 moveDirectionInput = Vector3.zero;

				if (settingsManager.IsTopDown ())
				{
					moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
				}
				else
				{
					moveDirectionInput = (playerInput.moveKeys.y * KickStarter.mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * KickStarter.mainCamera.RightVector ());
				}

				if (Vector3.Dot (moveDirectionInput, KickStarter.player.GetMoveDirection ()) > 0f)
				{
					// Move along path, because movement keys are in the path's forward direction
					KickStarter.player.isRunning = playerInput.isRunning;
					KickStarter.player.charState = CharState.Move;
				}
			}
			else
			{
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
		}
		
		
		// Point/click functions
		
		private void PointControlPlayer ()
		{
			if (playerInput.IsCursorLocked ())
			{
				return;
			}

			if (playerInput.isDownLocked && playerInput.isUpLocked && playerInput.isLeftLocked && playerInput.isRightLocked)
			{
				if (KickStarter.player.GetPath () == null && KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				return;
			}

			if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.DoubleClick) && !playerInput.interactionMenuIsOn && !playerInput.mouseOverMenu && !playerInteraction.IsMouseOverHotspot () && playerCursor && playerCursor.GetSelectedCursor () < 0)
			{
				if (settingsManager.doubleClickMovement && playerInput.mouseState == MouseState.SingleClick)
				{
					return;
				}

				if (runtimeInventory.selectedItem != null && !settingsManager.canMoveWhenActive)
				{
					return;
				}

				bool doubleClick = false;
				if (playerInput.mouseState == MouseState.DoubleClick && !settingsManager.doubleClickMovement)
				{
					doubleClick = true;
				}

				if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && playerMenus != null)
				{
					playerMenus.SetInteractionMenus (false);
				}
				if (!RaycastNavMesh (playerInput.GetMousePosition (), doubleClick))
				{
					// Move Ray down screen until we hit something
					Vector3 simulatedMouse = playerInput.GetMousePosition ();
	
					if (((int) Screen.height * settingsManager.walkableClickRange) > 1)
					{
						for (int i=1; i<Screen.height * settingsManager.walkableClickRange; i+=4)
						{
							// Up
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y - i), doubleClick))
							{
								break;
							}
							// Down
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y + i), doubleClick))
							{
								break;
							}
							// Left
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y), doubleClick))
							{
								break;
							}
							// Right
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y), doubleClick))
							{
								break;
							}
							// UpLeft
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y - i), doubleClick))
							{
								break;
							}
							// UpRight
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y - i), doubleClick))
							{
								break;
							}
							// DownLeft
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x - i, simulatedMouse.y + i), doubleClick))
							{
								break;
							}
							// DownRight
							if (RaycastNavMesh (new Vector2 (simulatedMouse.x + i, simulatedMouse.y + i), doubleClick))
							{
								break;
							}
						}
					}
				}
			}
			else if (KickStarter.player.GetPath () == null && KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.charState = CharState.Decelerate;
			}
		}


		private bool ProcessHit (Vector3 hitPoint, GameObject hitObject, bool run)
		{
			if (hitObject.layer != LayerMask.NameToLayer (settingsManager.navMeshLayer))
			{
				return false;
			}
			
			if (!run)
			{
				ShowClick (hitPoint);
			}
			
			if (playerInput.runLock == PlayerMoveLock.AlwaysRun)
			{
				run = true;
			}
			else if (playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				run = false;
			}

			Vector3[] pointArray;
			pointArray = navigationManager.navigationEngine.GetPointsArray (KickStarter.player.transform.position, hitPoint);
			KickStarter.player.MoveAlongPoints (pointArray, run);

			return true;
		}


		private bool RaycastNavMesh (Vector3 mousePosition, bool run)
		{
			if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				RaycastHit2D hit;
				if (Camera.main.isOrthoGraphic)
				{
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 pos = mousePosition;
					pos.z = KickStarter.player.transform.position.z - Camera.main.transform.position.z;
					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
				}

				if (hit.collider != null)
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (mousePosition);
				RaycastHit hit = new RaycastHit();
				
				if (settingsManager && sceneSettings && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength))
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			
			return false;
		}


		private void ShowClick (Vector3 clickPoint)
		{
			if (settingsManager && settingsManager.clickPrefab)
			{
				Destroy (GameObject.Find (settingsManager.clickPrefab.name + "(Clone)"));
				Instantiate (settingsManager.clickPrefab, clickPoint, Quaternion.identity);
			}
		}

		
		// First-person functions
		
		private void FirstPersonControlPlayer ()
		{
			if (firstPersonCamera)
			{
				Vector2 freeAim = playerInput.freeAim;
				if (freeAim.magnitude > settingsManager.dragWalkThreshold / 10f)
				{
					freeAim.Normalize ();
					freeAim *= settingsManager.dragWalkThreshold / 10f;
				}

				float rotationX = KickStarter.player.transform.localEulerAngles.y + freeAim.x * firstPersonCamera.sensitivity.x;
				firstPersonCamera.rotationY -= freeAim.y * firstPersonCamera.sensitivity.y;
				KickStarter.player.transform.localEulerAngles = new Vector3 (0, rotationX, 0);
			}
			else
			{
				Debug.LogWarning ("Could not find first person camera");
			}
		}


		private void DragPlayerLook ()
		{
			if (playerInput.isDownLocked && playerInput.isUpLocked && playerInput.isLeftLocked && playerInput.isRightLocked)
			{
				return;
			}

			if (playerInput.mouseState == MouseState.Normal)
			{
				return;
			}
			
			else if (!playerInput.mouseOverMenu && !playerInput.interactionMenuIsOn && (playerInput.mouseState == MouseState.RightClick || !playerInteraction.IsMouseOverHotspot ()))
			{
				if (playerInput.mouseState == MouseState.SingleClick)
				{
					playerInteraction.DisableHotspot (false);
				}
			}
		}


		private void StopMovingToHotspot ()
		{
			playerInput.hotspotMovingTo = null;
			KickStarter.player.EndPath ();
			KickStarter.player.ClearHeadTurnTarget (HeadFacing.Hotspot, false);
			playerInteraction.StopInteraction ();
		}
		
		
		private void OnDestroy ()
		{
			firstPersonCamera = null;
			playerInput = null;
			settingsManager = null;
			sceneSettings = null;
			playerMenus = null;
			navigationManager = null;
		}
		
	}

}