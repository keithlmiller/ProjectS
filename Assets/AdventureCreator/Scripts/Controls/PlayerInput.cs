/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInput.cs"
 * 
 *	This script records all input and processes it for other scripts.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class PlayerInput : MonoBehaviour
	{
		[HideInInspector] public MouseState mouseState = MouseState.Normal;
		[HideInInspector] public DragState dragState = DragState.None;
		[HideInInspector] public Hotspot hotspotMovingTo;
		[HideInInspector] public bool mouseOverMenu = false;
		[HideInInspector] public bool interactionMenuIsOn = false;
		
		[HideInInspector] public Vector2 moveKeys = new Vector2 (0f, 0f);
		[HideInInspector] public bool isRunning = false;
		[HideInInspector] public float timeScale = 1f;
		
		[HideInInspector] public bool isUpLocked = false;
		[HideInInspector] public bool isDownLocked = false;
		[HideInInspector] public bool isLeftLocked = false;
		[HideInInspector] public bool isRightLocked = false;
		[HideInInspector] public PlayerMoveLock runLock = PlayerMoveLock.Free;
		
		[HideInInspector] public int selected_option;
		
		public float clickDelay = 0.3f;
		public float doubleClickDelay = 1f;
		private float clickTime = 0f;
		private float doubleClickTime = 0;
		[HideInInspector] public MenuDrag activeDragElement;
		[HideInInspector] public bool hasUnclickedSinceClick = false;
		
		// Menu input override
		[HideInInspector] public string menuButtonInput;
		[HideInInspector] public float menuButtonValue;
		[HideInInspector] public SimulateInputType menuInput;
		
		// Controller movement
		private Vector2 xboxCursor;
		public float cursorMoveSpeed = 4f;
		private Vector2 mousePosition;
		private bool scrollingLocked = false;
		
		// Touch-Screen movement
		private Vector2 dragStartPosition = Vector2.zero;
		[HideInInspector] public float dragSpeed = 0f;
		private Vector2 dragVector;
		private float touchTime = 0f;
		private float touchThreshold = 0.2f;

		// 1st person movement
		[HideInInspector] public Vector2 freeAim;
		[HideInInspector] public bool cursorIsLocked = false;
		private bool toggleRun = false;

		// Draggable
		private bool canDragMoveable = false;
		private float cameraInfluence = 100000f;
		private DragBase dragObject = null;
		private Vector2 lastMousePosition;
		private Vector3 lastCameraPosition;
		private Vector3 dragForce;
		private Vector2 deltaDragMouse;
		
		[HideInInspector] public Conversation activeConversation = null;
		[HideInInspector] public ArrowPrompt activeArrows = null;
		[HideInInspector] public Container activeContainer = null;
		
		private PlayerInteraction playerInteraction;
		private ActionListManager actionListManager;
		private StateHandler stateHandler;
		private RuntimeInventory runtimeInventory;
		private SettingsManager settingsManager;

		
		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
				cursorIsLocked = settingsManager.lockCursorOnStart;
			}
			
			actionListManager = this.GetComponent <ActionListManager>();
			playerInteraction = this.GetComponent <PlayerInteraction>();
						
			ResetClick ();
			
			xboxCursor.x = Screen.width / 2;
			xboxCursor.y = Screen.height / 2;
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine))
			{
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
				{
					stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				}
				
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
				}
			}
			
			if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetCameraState (cursorIsLocked);
			}

			if (settingsManager.offsetTouchCursor)
			{
				mousePosition = xboxCursor;
			}
		}
		
		
		public void UpdateInput ()
		{
			if (clickTime > 0f)
			{
				clickTime -= 0.1f;
			}
			if (clickTime < 0f)
			{
				clickTime = 0f;
			}
			
			if (doubleClickTime > 0f)
			{
				doubleClickTime -= 0.1f;
			}
			if (doubleClickTime < 0f)
			{
				doubleClickTime = 0f;
			}
			
			if (stateHandler && settingsManager)
			{
				try
				{
					if (InputGetButtonDown ("ToggleCursor") && stateHandler.gameState == GameState.Normal)
					{
						ToggleCursor ();
					}
				}
				catch
				{
					cursorIsLocked = false;
				}
				
				if (stateHandler.gameState == GameState.Cutscene && InputGetButtonDown ("EndCutscene"))
				{
					actionListManager.EndCutscene ();
				}
				
				#if UNITY_EDITOR
				if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard || settingsManager.inputMethod == InputMethod.TouchScreen)
				#else
					if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
				#endif
				{
					// Cursor position
					if (cursorIsLocked && stateHandler.gameState == GameState.Normal)
					{
						mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
						freeAim = new Vector2 (InputGetAxis ("CursorHorizontal"), InputGetAxis ("CursorVertical"));
					}
					else if (!cursorIsLocked || stateHandler.gameState == AC.GameState.Paused || stateHandler.gameState == AC.GameState.DialogOptions)
					{
						mousePosition = InputMousePosition ();
						freeAim = Vector2.zero;
					}
					
					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					
					if (InputGetMouseButtonDown (0))
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();
								
								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (InputGetMouseButtonDown (1))
					{
						mouseState = MouseState.RightClick;
					}
					else if (InputGetMouseButton (0))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick ())
						{
							mouseState = MouseState.LetGo;
						}
						else
						{
							mouseState = MouseState.Normal;
						}
					}
					
					if (settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						if (dragState == DragState.Player)
						{
							if (settingsManager.IsFirstPersonDragMovement ())
							{
								freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, 0f);
							}
							else
							{
								freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, -dragVector.y * settingsManager.freeAimTouchSpeed);
							}
						}
						else
						{
							freeAim = Vector2.zero; //
						}
					}
				}
				else if (settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					int touchCount = Input.touchCount;

					// Cursor position
					if (touchCount > 0)
					{
						if (settingsManager.offsetTouchCursor)
						{
							if (touchTime > touchThreshold)
							{
								Touch t = Input.GetTouch (0);
								if (t.phase == TouchPhase.Moved && touchCount == 1)
								{
									if (stateHandler.gameState == GameState.Paused)
									{
										mousePosition += t.deltaPosition * 1.7f;
									}
									else
									{
										mousePosition += t.deltaPosition * Time.deltaTime / t.deltaTime;
									}

									if (mousePosition.x < 0f)
									{
										mousePosition.x = 0f;
									}
									else if (mousePosition.x > Screen.width)
									{
										mousePosition.x = Screen.width;
									}
									if (mousePosition.y < 0f)
									{
										mousePosition.y = 0f;
									}
									else if (mousePosition.y > Screen.height)
									{
										mousePosition.y = Screen.height;
									}
								}
							}
						}
						else
						{
							mousePosition = Input.GetTouch (0).position;
						}
					}

					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}

					if (touchTime > 0f && touchTime < touchThreshold)
						dragStartPosition = GetInvertedMouse ();

					if ((touchCount == 1 && stateHandler.gameState == GameState.Cutscene && Input.GetTouch (0).phase == TouchPhase.Began)
					    || (touchCount == 1 && !settingsManager.offsetTouchCursor && Input.GetTouch (0).phase == TouchPhase.Began)
					    || touchTime == -1f)
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();

								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (touchCount == 2 && Input.GetTouch (1).phase == TouchPhase.Began)
					{
						mouseState = MouseState.RightClick;
					}
					else if (touchCount == 1 && (Input.GetTouch (0).phase == TouchPhase.Stationary || Input.GetTouch (0).phase == TouchPhase.Moved))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick ())
						{
							mouseState = MouseState.LetGo;
						}
						else
						{
							mouseState = MouseState.Normal;
						}
					}

					if (settingsManager.offsetTouchCursor)
					{
						if (touchCount > 0)
						{
							if (stateHandler.gameState == GameState.Paused)
							{
								touchTime += 0.02f;
							}
							else
							{
								touchTime += Time.deltaTime;
							}
						}
						else
						{
							if (touchTime > 0f && touchTime < touchThreshold)
							{
								touchTime = -1f;
							}
							else
							{
								touchTime = 0f;
							}
						}
					}

					if (dragState == DragState.Player)
					{
						if (settingsManager.IsFirstPersonDragMovement ())
						{
							freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, 0f);
						}
						else
						{
							freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, -dragVector.y * settingsManager.freeAimTouchSpeed);
						}
					}
					else
					{
						freeAim = Vector2.zero; //
					}
				}
				else if (settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					// Cursor position
					if (cursorIsLocked && stateHandler.gameState == GameState.Normal)
					{
						mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
						freeAim = new Vector2 (InputGetAxis ("CursorHorizontal") * 50f, InputGetAxis ("CursorVertical") * 50f);
					}
					else
					{
						xboxCursor.x += InputGetAxis ("CursorHorizontal") * cursorMoveSpeed * Screen.width;
						xboxCursor.y += InputGetAxis ("CursorVertical") * cursorMoveSpeed * Screen.height;
						
						xboxCursor.x = Mathf.Clamp (xboxCursor.x, 0f, Screen.width);
						xboxCursor.y = Mathf.Clamp (xboxCursor.y, 0f, Screen.height);
						
						mousePosition = xboxCursor;
						freeAim = Vector2.zero;
					}
					
					// Cursor state
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					
					if (InputGetButtonDown ("InteractionA"))
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick ())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick ();
							}
							else if (CanClick ())
							{
								dragStartPosition = GetInvertedMouse ();
								
								mouseState = MouseState.SingleClick;
								ResetClick ();
								ResetDoubleClick ();
							}
						}
					}
					else if (InputGetButtonDown ("InteractionB"))
					{
						mouseState = MouseState.RightClick;
					}
					else if (InputGetButton ("InteractionA"))
					{
						mouseState = MouseState.HeldDown;
						SetDragState ();
					}
					else
					{
						mouseState = MouseState.Normal;
					}
					
					// Menu option changing
					if (stateHandler.gameState == GameState.DialogOptions || stateHandler.gameState == GameState.Paused)
					{
						if (!scrollingLocked)
						{
							if (InputGetAxisRaw ("Vertical") > 0.1 || InputGetAxisRaw ("Horizontal") < -0.1)
							{
								// Up / Left
								scrollingLocked = true;
								selected_option --;
							}
							else if (InputGetAxisRaw ("Vertical") < -0.1 || InputGetAxisRaw ("Horizontal") > 0.1)
							{
								// Down / Right
								scrollingLocked = true;
								selected_option ++;
							}
						}
						else if (InputGetAxisRaw ("Vertical") < 0.05 && InputGetAxisRaw ("Vertical") > -0.05 && InputGetAxisRaw ("Horizontal") < 0.05 && InputGetAxisRaw ("Horizontal") > -0.05)
						{
							scrollingLocked = false;
						}
					}
				}
				
				if (hotspotMovingTo != null)
				{
					freeAim = Vector2.zero;
				}

				if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot && interactionMenuIsOn)
				{
					try
					{
						if (InputGetButtonDown ("CycleInteractionsRight"))
						{
							playerInteraction.SetNextInteraction ();
						}
						else if (InputGetButtonDown ("CycleInteractionsLeft"))
						{
							playerInteraction.SetPreviousInteraction ();
						}
						else if (InputGetAxis ("CycleInteractions") > 0.1f)
						{
							playerInteraction.SetNextInteraction ();
						}
						else if (InputGetAxis ("CycleInteractions") < -0.1f)
						{
							playerInteraction.SetPreviousInteraction ();
						}
					}
					catch {}
				}

				mousePosition = KickStarter.mainCamera.LimitMouseToAspect (mousePosition);

				if (mouseState == MouseState.Normal && !hasUnclickedSinceClick)
				{
					hasUnclickedSinceClick = true;
				}

				if (mouseState == MouseState.Normal)
				{
					canDragMoveable = true;
				}

				UpdateDrag ();
				
				if (dragState != DragState.None)
				{
					dragVector = GetInvertedMouse () - dragStartPosition;
					dragSpeed = dragVector.magnitude;
				}
				else
				{
					dragSpeed = 0f;
				}
			}
		}
		
		
		public Vector2 GetMousePosition ()
		{
			return mousePosition;
		}
		
		
		public Vector2 GetInvertedMouse ()
		{
			return new Vector2 (GetMousePosition ().x, Screen.height - GetMousePosition ().y);
		}
		
		
		public bool IsCursorReadable ()
		{
			if (settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				if (mouseState == MouseState.Normal)
				{
					if (runtimeInventory.selectedItem != null && settingsManager.inventoryDragDrop)
					{
						return true;
					}

					return settingsManager.offsetTouchCursor;
				}
			}
			return true;
		}
		
		
		public void DrawDragLine ()
		{
			if (dragState == DragState.Player && settingsManager.movementMethod != MovementMethod.StraightToCursor && settingsManager.drawDragLine)
			{
				Vector2 pointA = dragStartPosition;
				Vector2 pointB = GetInvertedMouse ();
				
				if (pointB.x >= 0f)
				{
					DrawStraightLine.Draw (pointA, pointB, settingsManager.dragLineColor, settingsManager.dragLineWidth, true);
				}
			}
			
			if (activeDragElement != null)
			{
				if (mouseState == MouseState.HeldDown)
				{
					if (!activeDragElement.DoDrag (GetDragVector ()))
					{
						activeDragElement = null;
					}
				}
				else if (mouseState == MouseState.Normal)
				{
					if (activeDragElement.CheckStop (GetInvertedMouse ()))
					{
						activeDragElement = null;
					}
				}
			}
		}
		
		
		public void UpdateDirectInput ()
		{
			if (settingsManager != null)
			{
				if (activeArrows != null)
				{
					if (activeArrows.arrowPromptType == ArrowPromptType.KeyOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick)
					{
						Vector2 normalizedVector = new Vector2 (InputGetAxis ("Horizontal"), InputGetAxis ("Vertical"));
						if (settingsManager.inputMethod == InputMethod.TouchScreen && dragState == DragState.ScreenArrows)
						{
							normalizedVector = GetDragVector () / settingsManager.dragRunThreshold / settingsManager.dragWalkThreshold;
						}
						
						if (normalizedVector.x > 0.95f)
						{
							activeArrows.DoRight ();
						}
						else if (normalizedVector.x < -0.95f)
						{
							activeArrows.DoLeft ();
						}
						else if (normalizedVector.y < -0.95f)
						{
							activeArrows.DoDown();
						}
						else if (normalizedVector.y > 0.95f)
						{
							activeArrows.DoUp ();
						}
					}
					
					if (activeArrows != null && (activeArrows.arrowPromptType == ArrowPromptType.ClickOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick))
					{
						// Arrow Prompt is displayed: respond to mouse clicks
						Vector2 invertedMouse = GetInvertedMouse ();
						if (mouseState == MouseState.SingleClick)
						{
							if (activeArrows.upArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoUp ();
							}
							
							else if (activeArrows.downArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoDown ();
							}
							
							else if (activeArrows.leftArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoLeft ();
							}
							
							else if (activeArrows.rightArrow.rect.Contains (invertedMouse))
							{
								activeArrows.DoRight ();
							}
						}
					}
				}
				
				if (activeArrows == null && settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					float h = 0f;
					float v = 0f;
					bool run;
					
					if (settingsManager.inputMethod == InputMethod.TouchScreen || settingsManager.movementMethod == MovementMethod.Drag)
					{
						if (dragState != DragState.None)
						{
							h = dragVector.x;
							v = -dragVector.y;
						}
					}
					else
					{
						h = InputGetAxis ("Horizontal");
						v = InputGetAxis ("Vertical");
					}
					try
					{
						if (InputGetButtonDown ("Jump") && stateHandler.gameState == GameState.Normal)
						{
							KickStarter.player.Jump ();
						}
					}
					catch
					{}
					
					if ((isUpLocked && v > 0f) || (isDownLocked && v < 0f))
					{
						v = 0f;
					}
					
					if ((isLeftLocked && h > 0f) || (isRightLocked && h < 0f))
					{
						h = 0f;
					}
					
					if (runLock == PlayerMoveLock.Free)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen || settingsManager.movementMethod == MovementMethod.Drag)
						{
							if (dragStartPosition != Vector2.zero && dragSpeed > settingsManager.dragRunThreshold * 10f)
							{
								run = true;
							}
							else
							{
								run = false;
							}
						}
						else
						{
							try
							{
								run = InputGetButton ("Run");
							}
							catch
							{
								run = false;
							}
							
							try
							{
								if (InputGetButtonDown ("ToggleRun"))
								{
									toggleRun = !toggleRun;
								}
							}
							catch
							{
								toggleRun = false;
							}
						}
					}
					else if (runLock == PlayerMoveLock.AlwaysWalk)
					{
						run = false;
					}
					else
					{
						run = true;
					}
					
					if (settingsManager.inputMethod != InputMethod.TouchScreen && (settingsManager.movementMethod == MovementMethod.FirstPerson || settingsManager.movementMethod == MovementMethod.Direct) && runLock == PlayerMoveLock.Free && toggleRun)
					{
						isRunning = !run;
					}
					else
					{
						isRunning = run;
					}
					
					moveKeys = new Vector2 (h, v);
				}
				
				if (InputGetButtonDown ("FlashHotspots"))
				{
					FlashHotspots ();
				}
			}
		}
		
		
		private void FlashHotspots ()
		{
			Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot.IsOn () && hotspot.highlight && hotspot != playerInteraction.GetActiveHotspot ())
				{
					hotspot.highlight.Flash ();
				}
			}
		}
		
		
		public void RemoveActiveArrows ()
		{
			if (activeArrows)
			{
				activeArrows.TurnOff ();
			}
		}
		
		
		public void ResetClick ()
		{
			clickTime = clickDelay;
			hasUnclickedSinceClick = false;
		}
		
		
		private void ResetDoubleClick ()
		{
			doubleClickTime = doubleClickDelay;
		}
		
		
		public bool CanClick ()
		{
			if (clickTime == 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		private bool CanDoubleClick ()
		{
			if (doubleClickTime > 0f && clickTime == 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void SimulateInput (SimulateInputType input, string axis, float value)
		{
			if (axis != "")
			{
				menuInput = input;
				menuButtonInput = axis;
				
				if (input == SimulateInputType.Button)
				{
					menuButtonValue = 1f;
				}
				else
				{
					menuButtonValue = value;
				}
				
				CancelInvoke ();
				Invoke ("StopSimulatingInput", 0.1f);
			}
		}
		
		
		public bool IsCursorLocked ()
		{
			return cursorIsLocked;
		}
		
		
		private void StopSimulatingInput ()
		{
			menuButtonInput = "";
		}


		private float InputGetAxisRaw (string axis)
		{
			try
			{
				if (settingsManager.useOuya)
				{
					if (OuyaIntegration.GetAxisRaw (axis) != 0f)
					{
						return OuyaIntegration.GetAxisRaw (axis);
					}
				}

				else if (Input.GetAxisRaw (axis) != 0f)
				{
					return Input.GetAxisRaw (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}
		
		
		private float InputGetAxis (string axis)
		{
			try
			{
				if (settingsManager.useOuya)
				{
					if (OuyaIntegration.GetAxis (axis) != 0f)
					{
						return OuyaIntegration.GetAxis (axis);
					}
				}

				else if (Input.GetAxis (axis) != 0f)
				{
					return Input.GetAxis (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}


		private bool InputGetMouseButton (int button)
		{
			if (settingsManager.useOuya)
			{
				return OuyaIntegration.GetMouseButton (button);
			}
			return Input.GetMouseButton (button);
		}


		private Vector2 InputMousePosition ()
		{
			if (settingsManager.useOuya)
			{
				return OuyaIntegration.mousePosition;
			}
			return Input.mousePosition;
		}


		private bool InputGetMouseButtonDown (int button)
		{
			if (settingsManager.useOuya)
			{
				return OuyaIntegration.GetMouseButtonDown (button);
			}
			return Input.GetMouseButtonDown (button);
		}


		private bool InputGetButton (string axis)
		{
			try
			{
				if (settingsManager.useOuya)
				{
					if (OuyaIntegration.GetButton (axis))
					{
						return true;
					}
				}

				else if (Input.GetButton (axis))
				{
					return true;
				}
			}
			catch {}
			return false;
		}
		
		
		public bool InputGetButtonDown (string axis)
		{
			try
			{
				if (settingsManager.useOuya)
				{
					if (OuyaIntegration.GetButtonDown (axis))
					{
						return true;
					}
				}

				else if (Input.GetButtonDown (axis))
				{
					return true;
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Button)
			{
				if (menuButtonValue > 0f)
				{
					ResetClick ();
					StopSimulatingInput ();	
					return true;
				}
				
				StopSimulatingInput ();
			}
			
			return false;
		}
		
		
		private void SetDragState ()
		{
			if (runtimeInventory.selectedItem != null && settingsManager.inventoryDragDrop && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.Paused))
			{
				dragState = DragState.Inventory;
			}
			else if (activeDragElement != null && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.Paused))
			{
				dragState = DragState.Menu;
			}
			else if (activeArrows != null)
			{
				dragState = DragState.ScreenArrows;
			}
			else if (dragObject != null)
			{
				dragState = DragState.Moveable;
			}
			else if (KickStarter.mainCamera.attachedCamera && KickStarter.mainCamera.attachedCamera.isDragControlled)
			{
				if (!GetComponent <PlayerInteraction>().IsMouseOverHotspot ())
				{
					dragState = DragState._Camera;
					if (deltaDragMouse.magnitude * Time.deltaTime <= 1f && (GetInvertedMouse () - dragStartPosition).magnitude < 10f)
					{
						dragState = DragState.None;
					}
				}
			}
			else if ((settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.StraightToCursor || (settingsManager.movementMethod != MovementMethod.PointAndClick && settingsManager.inputMethod == InputMethod.TouchScreen))
			         && stateHandler.gameState == GameState.Normal)
			{
				if (!mouseOverMenu && !interactionMenuIsOn)
				{
					if (GetComponent <PlayerInteraction>().IsMouseOverHotspot ())
					{}
					else
					{
						dragState = DragState.Player;
					}
				}
			}
			else
			{
				dragState = DragState.None;
			}
		}


		private void UpdateDrag ()
		{
			if (dragState != DragState.None)
			{
				// Calculate change in mouse position
				if (freeAim.magnitude != 0f)
				{
					deltaDragMouse = freeAim * 500f / Time.deltaTime;
				}
				else
				{
					deltaDragMouse = ((Vector2) mousePosition - lastMousePosition) / Time.deltaTime;
				}
			}

			if (dragObject && stateHandler.gameState != GameState.Normal)
			{
				dragObject.LetGo ();
				dragObject = null;
			}

			if (mouseState == MouseState.HeldDown && dragState == DragState.None)
			{
				Grab ();
			}
			else if (dragState == DragState.Moveable)
			{
				if (dragObject)
				{
					if (dragObject.isHeld && dragObject.IsOnScreen () && dragObject.IsCloseToCamera (settingsManager.moveableRaycastLength))
					{
						Drag ();
					}
					else
					{
						dragObject.LetGo ();
						dragObject = null;
					}
				}
			}
			else if (dragObject)
			{
				dragObject.LetGo ();
				dragObject = null;
			}

			if (dragState != DragState.None)
			{
				lastMousePosition = mousePosition;
			}
		}


		private void Grab ()
		{
			if (dragObject)
			{
				dragObject.LetGo ();
				dragObject = null;
			}
			else if (canDragMoveable)
			{
				canDragMoveable = false;

				RaycastHit hit = new RaycastHit ();
				Ray ray = Camera.main.ScreenPointToRay (mousePosition); 
				
				if (Physics.Raycast (ray, out hit, settingsManager.moveableRaycastLength))
				{
					if (hit.transform.GetComponent <DragBase>())
					{
						dragObject = hit.transform.GetComponent <DragBase>();
						dragObject.Grab (hit.point);
						lastMousePosition = mousePosition;
						lastCameraPosition = KickStarter.mainCamera.transform.position;
					}
				}
			}
		}
		
		
		private void Drag ()
		{
			// Convert to a 3D force
			if (dragObject.invertInput)
			{
				dragForce = (-KickStarter.mainCamera.transform.right * deltaDragMouse.x) + (-KickStarter.mainCamera.transform.up * deltaDragMouse.y);
			}
			else
			{
				dragForce = (KickStarter.mainCamera.transform.right * deltaDragMouse.x) + (KickStarter.mainCamera.transform.up * deltaDragMouse.y);
			}
			
			// Scale force with distance to camera, to lessen effects when close
			float distanceToCamera = (KickStarter.mainCamera.transform.position - dragObject.transform.position).magnitude;
			
			// Incoporate camera movement
			Vector3 deltaCamera = KickStarter.mainCamera.transform.position - lastCameraPosition;
			dragForce += deltaCamera * cameraInfluence;
			
			dragObject.ApplyDragForce (dragForce, mousePosition, distanceToCamera);
			
			lastCameraPosition = KickStarter.mainCamera.transform.position;
		}
		
		
		public Vector2 GetDragVector ()
		{
			if (dragState == AC.DragState._Camera)
			{
				return deltaDragMouse;
			}
			return dragVector;
		}
		
		
		public void SetUpLock (bool state)
		{
			isUpLocked = state;
			
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetMovementState (state);
			}
		}
		
		
		public bool ActiveArrowsDisablingHotspots ()
		{
			if (activeArrows != null && activeArrows.disableHotspots)
			{
				return true;
			}
			return false;
		}


		public void StartRotatingObject ()
		{
			if (cursorIsLocked)
			{
				ToggleCursor ();
			}
		}


		private void ToggleCursor ()
		{
			if (!cursorIsLocked)
			{
				cursorIsLocked = true;
			}
			else
			{
				cursorIsLocked = false;
			}
			
			if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetCameraState (cursorIsLocked);
			}
		}
		
		
		private void OnDestroy ()
		{
			stateHandler = null;
			runtimeInventory = null;
			settingsManager = null;
			actionListManager = null;
			playerInteraction = null;
		}

	}
	
}

