using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Ejecuta las acciones de todos los botones de los menús.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class AnimatorFunctions : MonoBehaviour
{
    // Parent of menus "Menu"
    GameObject menu;
    // Array of menus
    GameObject[] menus;
    // Controllers
    GameController gamecontroller;
    // The menu of this button
    MenuButtonController menuButtonController;
	public bool disableOnce;

    private void Start()
    {
        // Parent of menus "Menu"
        menu = transform.parent.GetComponent<MenuButtonController>().menu;
        // Array of menus
        menus = menu.transform.GetComponent<MenuActivation>().menus;
        // Controllers
        gamecontroller = transform.parent.GetComponent<MenuButtonController>().gamecontroller;
        // The menu of this button
        menuButtonController = transform.parent.GetComponent<MenuButtonController>();
    }

    void PlaySound(AudioClip whichSound){
		if(!disableOnce){
			menuButtonController.audioSource.PlayOneShot (whichSound);
		}else{
			disableOnce = false;
		}
	}

    /**
     * Ejecuta una acción de un elemento de un menú.
     * @param menuIndex Índice del array de menús
     * @param index ïndice de un botón
     * @return void
     */
    public void Execute(int menuIndex, int index)
    {       
        // Main menu
        if(menuIndex == MenuHelper.MAIN_MENU)
            switch(index)
            {
                case MenuHelper.START_MENU_ITEM:
                    StartSimulation();
                    break;
                case MenuHelper.SELECT_MENU_ITEM:
                    ChooseScorbot();
                    break;
                case MenuHelper.SETTINGS_MENU_ITEM:
                    Settings();
                    break;
                case MenuHelper.HELP_MENU_ITEM:
                    Help();
                    break;
                case MenuHelper.EXIT_MENU_ITEM:
                    Exit();
                    break;
                default: break;
            }

        // Selct menu
        if (menuIndex == MenuHelper.SELECT_MENU)
            switch (index)
            {
                case MenuHelper.SCORBOTERIX_MENU_ITEM:
                    SelectScorbotERIX();
                    break;
                case MenuHelper.SCORBOTERVPLUS_MENU_ITEM:
                    SelectScorbotERVPlus();
                    break;
                case MenuHelper.SELECT_BACK_MENU_ITEM:
                    MainMenu();
                    break;
                default: break;
            }

        // Settings menu
        if (menuIndex == MenuHelper.SETTINGS_MENU)
            switch (index)
            {           
                case MenuHelper.SETTINGS_BACK_MENU_ITEM:
                    MainMenu();
                    break;
                default: break;
            }

        // Help menu
        if (menuIndex == MenuHelper.HELP_MENU)
            switch (index)
            {
                case MenuHelper.CONTROLS_MENU_ITEM:
                    Controls();
                    break;
                case MenuHelper.COMMANDS_MENU_ITEM:
                    Commands();
                    break;
                case MenuHelper.ABOUT_MENU_ITEM:
                    About();
                    break;
                case MenuHelper.HELP_BACK_MENU_ITEM:
                    MainMenu();
                    break;
                default: break;
            }
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

    /**
     * Desactiva todos los menús y activa el control de la simulación.
     * @return void
     */
    public void HideMenu()
    {
        menu.SetActive(false);
        gamecontroller.GetComponent<CameraControl>().SetIsProcessing(true);
    }

    /**
     * Desactiva todos los menús.
     * @return void
     */
    private void SetAllFalse()
    {
        foreach (GameObject menu in menus)
            menu.SetActive(false);
    }

    /**
     * Desactiva todos los elementos de un menú.
     * @param menuIndex Índice de un menú
     * @return void
     */
    private void SetAllFalseMenuElements(int menuIndex)
    {
        foreach (GameObject menu in menus[menuIndex].GetComponent<MenuButtonController>().menuElements)
            menu.SetActive(false);
    }

    /**
     * Obtiene todos los elementos de un menú.
     * @param menuIndex Índice de un menú
     * @return GameObject[] Elementos de un menú
     */
    private GameObject[] GetMenuElements(int menuIndex)
    {
        return menus[menuIndex].GetComponent<MenuButtonController>().menuElements;
    }

    /**
     * Activa el menú "Select Scorbot".
     * @return void
     */
    private void ChooseScorbot()
    {
        SetAllFalse();
        menus[MenuHelper.SELECT_MENU].SetActive(true);          
    }

    /**
     * Activa el menú "Settings".
     * @return void
     */
    private void Settings()
    {
        SetAllFalse();
        menus[MenuHelper.SETTINGS_MENU].SetActive(true);
    }

    /**
     * Activa el menú "Help".
     * @return void
     */
    private void Help()
    {
        SetAllFalse();
        menus[MenuHelper.HELP_MENU].SetActive(true);
    }

    /**
     * Cierra el programa.
     * @return void
     */
    private void Exit()
    {
        Application.Quit();
    }

    /**
     * Oculta los todos los menús para mostrar la simulación.
     * @return void
     */
    private void StartSimulation()
    {
        HideMenu();
    }

    /**
     * Activa el Scorbot ER IX y pasa a la simulación.
     * @return void
     */
    private void SelectScorbotERIX()
    {
        gamecontroller.SetScorbot(ScorbotERIX.INDEX);
        HideMenu();
    }

    /**
     * Activa el Scorbot ER V+ y pasa a la simulación.
     * @return void
     */
    private void SelectScorbotERVPlus()
    {
        gamecontroller.SetScorbot(ScorbotERVPlus.INDEX);
        HideMenu();
    }

    /**
     * Activa "Controls" del menú "Help".
     * @return void
     */
    private void Controls()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.CONTROLS_MENU_ITEM].SetActive(true);       
    }

    /**
     * Activa "Commands" del menú "Help".
     * @return void
     */
    private void Commands()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.COMMANDS_MENU_ITEM].SetActive(true);
    }

    /**
     * Activa "About" del menú "Help".
     * @return void
     */
    private void About()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);    
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.ABOUT_MENU_ITEM].SetActive(true);
    }


}	
