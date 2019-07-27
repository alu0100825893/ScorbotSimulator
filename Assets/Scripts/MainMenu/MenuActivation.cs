using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActivation : MonoBehaviour {

    public GameObject[] menus;
    

    private void OnDisable()
    {
        MainMenu();
    }

    public void MainMenu()
    {
        foreach (GameObject menu in menus)
            menu.SetActive(false);
        menus[MenuHelper.MAIN_MENU].SetActive(true);
    }
}
