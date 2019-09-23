using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Activa las animaciones de los botones de los menús.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class MenuButton : MonoBehaviour
{
    // Menu which this button belongs. It assumes it's parent
	MenuButtonController menuButtonController;
    // Animator component of this button
	Animator animator;
    // AnimatorFunctions component of this button
    AnimatorFunctions animatorFunctions;
    // Index of this button inside its menu
	[SerializeField] int thisIndex;
    // If this button behaves like a tab
    [SerializeField] bool isTab = false;

    private void Start()
    {
        // Menu which this button belongs. It assumes it's parent
        menuButtonController = transform.parent.GetComponent<MenuButtonController>();
        // Animator component of this button
        animator = GetComponent<Animator>();
        // AnimatorFunctions component of this button
        animatorFunctions = GetComponent<AnimatorFunctions>();
    }

    void Update()
    {
        // If this button is being selected
		if(menuButtonController.index == thisIndex)
		{
            if (isTab && animator.GetBool("selected"))
                return;
			animator.SetBool ("selected", true);
            // Key enter or is just a tab
			if (Input.GetButtonDown ("Submit") || isTab) {
                if(!isTab)
				    animator.SetBool ("pressed", true);               
                // Execute action of this button
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


    
    /**
     * Acción a ejecutar cuando el ratón está sobre el botón.
     * @return void
     */
    public void OnEnterEvent()
    {
        // Enter event
        menuButtonController.index = thisIndex;
        animator.SetBool("selected", true);
   
    }


    /**
     * Acción a ejecutar cuando el ratón está sobre un tab.
     * @return void
     */
    public void OnEnterEventTab()
    {
        // Enter event tab
        menuButtonController.index = thisIndex;
        animator.SetBool("selected", true);
        animatorFunctions.Execute(menuButtonController.menuIndex, thisIndex);   
    }

    
    /**
     * Acción a ejecutar cuando el ratón hace click derecho sobre el botón.
     * @return void
     */
    public void Execute()
    {
        // Click event
        menuButtonController.index = thisIndex;       
        animator.SetBool("pressed", true);
        animatorFunctions.Execute(menuButtonController.menuIndex, thisIndex);      
    }
}
