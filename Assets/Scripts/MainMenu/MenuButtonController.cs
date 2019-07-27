using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour {


	public int index;
	[SerializeField] bool keyDown;
	[SerializeField] int maxIndex;
	public AudioSource audioSource;

    public int menuIndex;

    public GameObject menu;
    public GameController gamecontroller;
    public GameObject[] menuElements;

    void Start () {
		audioSource = GetComponent<AudioSource>();
	}
	

	void Update () {
		if (Input.GetAxis ("Vertical") != 0 || (Input.GetAxis("Horizontal") != 0)) {
			if (!keyDown){
				if (Input.GetAxis ("Vertical") < 0 || (Input.GetAxis("Horizontal") > 0)) { // Down Left
					if(index < maxIndex) {
						index++;
					}else{
						index = 0;
					}
				} else 
                    if (Input.GetAxis ("Vertical") > 0 || (Input.GetAxis("Horizontal") < 0)) { // Up Right
					    if (index > 0){
						    index --; 
					    } else {
						    index = maxIndex;
					    }
				    }
				keyDown = true;
			}
		} else {
			keyDown = false;
		}
	}

}
