using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Scritp para el padre de todos los menus "Menu". Activa el menú principal (para la próxima vez) 
 * cuando se desactiva "Menu".
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class MenuActivation : MonoBehaviour {
    // Array of menus
    public GameObject[] menus;
    
    private void OnDisable()
    {
        MainMenu();
    }

    /**
     * Desactiva todos los menús y activa el menú principal.
     * @return void
     */
    public void MainMenu()
    {
        foreach (GameObject menu in menus)
            menu.SetActive(false);
        menus[MenuHelper.MAIN_MENU].SetActive(true);
    }
}
