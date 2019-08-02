using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeOnlineAnimation : MonoBehaviour {

    private Animator animator;
   
    void Start () {
        animator = GetComponent<Animator>();
	}


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
