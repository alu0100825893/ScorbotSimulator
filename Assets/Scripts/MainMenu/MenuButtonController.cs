using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Permite que los botónes de un menú se puedan navegar mediante las flechas del teclado.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class MenuButtonController : MonoBehaviour {

    // Start index of buttons of this menu. Always 0
    public int index;
	[SerializeField] bool keyDown;
    // Number of buttons of this menu
	[SerializeField] int maxIndex;
	public AudioSource audioSource;

    // Menu index, same as the array inside "Menu"
    public int menuIndex;
    // Menus parent. "Menu"
    public GameObject menu;
    // Controllers
    public GameController gamecontroller;

    // Elements of this menu. Only "Help" menu need setup 3: Controls, Commands, About
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
