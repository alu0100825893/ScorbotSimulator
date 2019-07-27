using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
    GameObject menu;
    GameObject[] menus;
    GameController gamecontroller;
    MenuButtonController menuButtonController;
	public bool disableOnce;

    private void Start()
    {
        menu = transform.parent.GetComponent<MenuButtonController>().menu;
        menus = menu.transform.GetComponent<MenuActivation>().menus;
        gamecontroller = transform.parent.GetComponent<MenuButtonController>().gamecontroller;
        menuButtonController = transform.parent.GetComponent<MenuButtonController>();
    }

    void PlaySound(AudioClip whichSound){
		if(!disableOnce){
			menuButtonController.audioSource.PlayOneShot (whichSound);
		}else{
			disableOnce = false;
		}
	}
   

    public void Execute(int menuIndex, int index)
    {       
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

        if (menuIndex == MenuHelper.SETTINGS_MENU)
            switch (index)
            {           
                case MenuHelper.SETTINGS_BACK_MENU_ITEM:
                    MainMenu();
                    break;
                default: break;
            }

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

    public void MainMenu()
    {
        foreach (GameObject menu in menus)
            menu.SetActive(false);
        menus[MenuHelper.MAIN_MENU].SetActive(true);
    }

    public void HideMenu()
    {
        menu.SetActive(false);
        gamecontroller.GetComponent<CameraControl>().SetIsProcessing(true);
    }

    private void SetAllFalse()
    {
        foreach (GameObject menu in menus)
            menu.SetActive(false);
    }

    private void SetAllFalseMenuElements(int menuIndex)
    {
        foreach (GameObject menu in menus[menuIndex].GetComponent<MenuButtonController>().menuElements)
            menu.SetActive(false);
    }

    private GameObject[] GetMenuElements(int menuIndex)
    {
        return menus[menuIndex].GetComponent<MenuButtonController>().menuElements;
    }

    private void ChooseScorbot()
    {
        SetAllFalse();
        menus[MenuHelper.SELECT_MENU].SetActive(true);          
    }

    private void Settings()
    {
        SetAllFalse();
        menus[MenuHelper.SETTINGS_MENU].SetActive(true);
    }

    private void Help()
    {
        SetAllFalse();
        menus[MenuHelper.HELP_MENU].SetActive(true);
    }

    private void Exit()
    {
        Application.Quit();
    }

    private void StartSimulation()
    {
        HideMenu();
    }

    private void SelectScorbotERIX()
    {
        gamecontroller.SetScorbot(ScorbotERIX.INDEX);
        HideMenu();
    }

    private void SelectScorbotERVPlus()
    {
        gamecontroller.SetScorbot(ScorbotERVPlus.INDEX);
        HideMenu();
    }

    private void Controls()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.CONTROLS_MENU_ITEM].SetActive(true);       
    }
    private void Commands()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.COMMANDS_MENU_ITEM].SetActive(true);
    }
    private void About()
    {
        SetAllFalseMenuElements(MenuHelper.HELP_MENU);    
        GetMenuElements(MenuHelper.HELP_MENU)[MenuHelper.ABOUT_MENU_ITEM].SetActive(true);
    }


}	
