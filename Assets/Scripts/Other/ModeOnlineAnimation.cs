using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Maneja las animacions del modo online-offline.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ModeOnlineAnimation : MonoBehaviour {
    // Component animator
    private Animator animator;
   
    void Start () {
        // Component animator
        animator = GetComponent<Animator>();
	}

    /**
     * Activa la animación del modo online u offline.
     * @param value El valor 1 activa la animacion del modo online
     * @return void
     */
    public void SetOnlineMode(float value)
    {
        if (value == 1f)
        {
            animator.SetBool("on", true);
        }
        else
        {
            animator.SetBool("on", false);
        }        
    }
}
