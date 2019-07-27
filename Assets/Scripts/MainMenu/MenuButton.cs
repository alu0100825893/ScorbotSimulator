using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
	MenuButtonController menuButtonController;
	Animator animator;
	AnimatorFunctions animatorFunctions;
	[SerializeField] int thisIndex;
    [SerializeField] bool isTab = false;

    private void Start()
    {
        menuButtonController = transform.parent.GetComponent<MenuButtonController>();
        animator = GetComponent<Animator>();
        animatorFunctions = GetComponent<AnimatorFunctions>();
    }

    void Update()
    {
		if(menuButtonController.index == thisIndex)
		{
            if (isTab && animator.GetBool("selected"))
                return;
			animator.SetBool ("selected", true);
			if (Input.GetButtonDown ("Submit") || isTab) {
                if(!isTab)
				    animator.SetBool ("pressed", true);               
                animatorFunctions.Execute(menuButtonController.menuIndex , thisIndex);

            } else 
                if (animator.GetBool ("pressed")){
				    animator.SetBool ("pressed", false);
				    animatorFunctions.disableOnce = true;
			    }
		} else {
			animator.SetBool ("selected", false);
		}
    }


    // Enter event
    public void OnEnterEvent()
    {        
        menuButtonController.index = thisIndex;
        animator.SetBool("selected", true);
   
    }

    // Enter event tab
    public void OnEnterEventTab()
    {
        menuButtonController.index = thisIndex;
        animator.SetBool("selected", true);
        animatorFunctions.Execute(menuButtonController.menuIndex, thisIndex);   
    }

    // Click event
    public void Execute()
    {
        menuButtonController.index = thisIndex;       
        animator.SetBool("pressed", true);
        animatorFunctions.Execute(menuButtonController.menuIndex, thisIndex);      
    }
}
