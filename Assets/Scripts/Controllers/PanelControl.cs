using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es gestionar las ventanas de la interfaz gráfica mediante
 * animaciones para ponerlas visibles/no visibles.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class PanelControl : MonoBehaviour {
    // Controllers
    private ManualInputControl manualInputControl;
    // Panels
    private Animator manualControlPanel;
    private Animator commandsPanel;
    private Animator console;
    private Animator syncPanel;
    private Animator messageLogPanel;
    private Animator positionLogPanel;
    
    void Start () {
        // Controllers
        manualInputControl = GetComponent<ManualInputControl>();
        // Panels
        manualControlPanel = GetComponent<GameController>().manualControlPanel.GetComponent<Animator>();
        commandsPanel = GetComponent<GameController>().commandsPanel.GetComponent<Animator>();
        console = GetComponent<GameController>().console.GetComponent<Animator>();
        syncPanel = GetComponent<GameController>().syncPanel.GetComponent<Animator>();
        messageLogPanel = GetComponent<GameController>().messageLogPanel.GetComponent<Animator>();
        positionLogPanel = GetComponent<GameController>().positionLogPanel.GetComponent<Animator>();
    }

    /**
	 * Muestra/oculta la ventana "Manual".
	 * @return void
	 */
    public void ShowHideManualControls()
    {
        // Hide all windows except one
        PanelGroupHide(manualControlPanel);
        // Execute animation
        bool show = manualControlPanel.GetBool("show") ? false : true;        
        manualControlPanel.SetBool("show", show);
        // Active manual input control if window "Manual" is visible
        manualInputControl.SetProcessing(show);
    }

    /**
	 * Muestra/oculta la ventana "Commands".
	 * @return void
	 */
    public void ShowHideCommands()
    {
        // Hide all windows except one
        PanelGroupHide(commandsPanel);
        // Execute animation
        bool show = commandsPanel.GetBool("show") ? false : true;
        commandsPanel.SetBool("show", show);

    }

    /**
	 * Muestra/oculta la ventana "Console".
	 * @return void
	 */
    public void ShowHideConsole()
    {
        // Execute animation
        bool show = console.GetBool("show") ? false : true;
        console.SetBool("show", show);  
    }

    /**
	 * Muestra/oculta la ventana "Sync".
	 * @return void
	 */
    public void ShowHideSync()
    {
        // Hide all windows except one
        PanelGroupHide(syncPanel);
        // Execute animation
        bool show = syncPanel.GetBool("show") ? false : true;
        syncPanel.SetBool("show", show);
    }

    /**
	 * Muestra/oculta la ventana "Log".
	 * @return void
	 */
    public void ShowHideMessageLog()
    {
        // Hide all windows except one
        PanelGroupHide(messageLogPanel);
        // Execute animation
        bool show = messageLogPanel.GetBool("show") ? false : true;
        messageLogPanel.SetBool("show", show);
    }

    /**
	 * Muestra/oculta la ventana "Log".
	 * @return void
	 */
    public void ShowHidePositionLog()
    {
        // Hide all windows except one
        PanelGroupHide(positionLogPanel);
        // Execute animation
        bool show = positionLogPanel.GetBool("show") ? false : true;
        positionLogPanel.SetBool("show", show);
    }

    /**
	 * Oculta todas las ventanas exceptuando 1.
     * @param exceptThis Ventana que no se va a ocultar
	 * @return void
	 */
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
