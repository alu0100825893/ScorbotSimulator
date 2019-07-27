using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelControl : MonoBehaviour {


    private ManualInputControl manualInputControl;

    private Animator manualControlPanel;
    private Animator commandsPanel;
    private Animator console;
    private Animator syncPanel;
    private Animator messageLogPanel;
    private Animator positionLogPanel;


    void Start () {
        manualInputControl = GetComponent<ManualInputControl>();
        
        manualControlPanel = GetComponent<GameController>().manualControlPanel.GetComponent<Animator>();
        commandsPanel = GetComponent<GameController>().commandsPanel.GetComponent<Animator>();
        console = GetComponent<GameController>().console.GetComponent<Animator>();
        syncPanel = GetComponent<GameController>().syncPanel.GetComponent<Animator>();
        messageLogPanel = GetComponent<GameController>().messageLogPanel.GetComponent<Animator>();
        positionLogPanel = GetComponent<GameController>().positionLogPanel.GetComponent<Animator>();
    }
	

	void Update () {
		
	}

    public void ShowHideManualControls()
    {
        PanelGroupHide(manualControlPanel);
        
        bool show = manualControlPanel.GetBool("show") ? false : true;        
        manualControlPanel.SetBool("show", show);
      
        manualInputControl.SetProcessing(show);
    }

    public void ShowHideCommands()
    {
        PanelGroupHide(commandsPanel);

        bool show = commandsPanel.GetBool("show") ? false : true;
        commandsPanel.SetBool("show", show);

    }

    public void ShowHideConsole()
    {
        bool show = console.GetBool("show") ? false : true;
        console.SetBool("show", show);  
    }

    public void ShowHideSync()
    {
        PanelGroupHide(syncPanel);

        bool show = syncPanel.GetBool("show") ? false : true;
        syncPanel.SetBool("show", show);
    }

    public void ShowHideMessageLog()
    {
        PanelGroupHide(messageLogPanel);

        bool show = messageLogPanel.GetBool("show") ? false : true;
        messageLogPanel.SetBool("show", show);
    }

    public void ShowHidePositionLog()
    {
        PanelGroupHide(positionLogPanel);

        bool show = positionLogPanel.GetBool("show") ? false : true;
        positionLogPanel.SetBool("show", show);
    }

    private void PanelGroupHide(Animator exceptThis)
    {       
        if (!exceptThis.Equals(manualControlPanel) )
        {
            manualControlPanel.SetBool("show", false);
        }
        if (!exceptThis.Equals(syncPanel))
        {
            syncPanel.SetBool("show", false);            
        }

        if (!exceptThis.Equals(commandsPanel))
        {
            commandsPanel.SetBool("show", false);
        }

        if (!exceptThis.Equals(messageLogPanel))
        {
            messageLogPanel.SetBool("show", false);
        }

        if (!exceptThis.Equals(positionLogPanel))
        {
            positionLogPanel.SetBool("show", false);
        }

    }
}
