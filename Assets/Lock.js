#pragma strict

 // controlScript.js
 
 var currentCombination : int = 0;
 private var correctCombination : int = 1234;
 var buttonClickProgress : int;
 
 function OnGUI() {
 if(GUI.Button(Rect (0,0, 50, 50), "0")){ ButtonWasClicked(0); }
 if(GUI.Button(Rect (50,0, 50, 50), "1")){ ButtonWasClicked(1); }
 if(GUI.Button(Rect (100,0, 50, 50), "2")){ ButtonWasClicked(2); }
 if(GUI.Button(Rect (0,50, 50, 50), "3")){ ButtonWasClicked(3); }
 if(GUI.Button(Rect (50,50, 50, 50), "4")){ ButtonWasClicked(4); }
 if(GUI.Button(Rect (100,50, 50, 50), "5")){ ButtonWasClicked(5); }
 if(GUI.Button(Rect (0,100, 50, 50), "6")){ ButtonWasClicked(6); }
 if(GUI.Button(Rect (50,100, 50, 50), "7")){ ButtonWasClicked(7); }
 if(GUI.Button(Rect (100,100, 50, 50), "8")){ ButtonWasClicked(8); }
 if(GUI.Button(Rect (0,150, 50, 50), "9")){ ButtonWasClicked(9); }
 }
 
 function ButtonWasClicked (buttonNmb : int) {
 
 currentCombination += buttonNmb;
 buttonClickProgress++;
 
 if(buttonClickProgress < 4){
     currentCombination *= 10;
 }
 else{
     if(currentCombination == correctCombination){
         Debug.Log("You Opened the Combination lock!");
         buttonClickProgress = 0;
         currentCombination = 0;
     }
     else{
         Debug.Log("Wrong Combination Code, reseting...");
         buttonClickProgress = 0;
         currentCombination = 0;
     }
 }
 }