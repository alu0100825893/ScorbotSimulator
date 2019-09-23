using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es permitir controlar las articulaciones del Scorbot de forma individual
 * mediante el teclado, además del cierre y apertura de la pinza.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ManualInputControl : MonoBehaviour {

    // Keyboard keys list
    private List<KeyCode> keyCodes = new List<KeyCode>();
    // Scorbot
    private IK robot;
    // Constants
    private const float ROTATION_SENSIBILITY = 1f;

    // Activate/deactivate keyboard control. Active = true
    private bool processing = false;

    void Start () {
        // Scorbot
        robot = GetComponent<GameController>().robot;

        // Keyboard keys list
        keyCodes.Add(KeyCode.Q);
        keyCodes.Add(KeyCode.Alpha1);
        keyCodes.Add(KeyCode.W);
        keyCodes.Add(KeyCode.Alpha2);
        keyCodes.Add(KeyCode.E);
        keyCodes.Add(KeyCode.Alpha3);
        keyCodes.Add(KeyCode.R);
        keyCodes.Add(KeyCode.Alpha4);
        keyCodes.Add(KeyCode.T);
        keyCodes.Add(KeyCode.Alpha5);

        // Events
        GameController.ScorbotDel += SetScorbot;
    }
	
	void Update () {
        // Activate/deactivate keyboard control
        if (!processing)
            return;

        if (!robot)
            return;

        // Scorbot articulation rotation (See keyCodes at "Start()")
        int j = 0;
        for (int i = 0; i < robot.GetArticulations().Length; i++)
        {
            if (Input.GetKey(keyCodes[j]))
            {
                robot.GetArticulations()[i].Rotate(-ROTATION_SENSIBILITY);
            }
            j++;
            if (Input.GetKey(keyCodes[j]))
            {
                robot.GetArticulations()[i].Rotate(ROTATION_SENSIBILITY);
            }
            j++;
        }

        // Open/close Scorbot end efector
        if(Input.GetKey(KeyCode.Y))
        {
            robot.GetComponent<ScorbotModel>().Open();
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            robot.GetComponent<ScorbotModel>().Close();
        }
    }

    /**
     * Cambia el scorbot utilizado.
     * @param scorbot Nuevo Scorbot
     * @return void
     */
    private void SetScorbot(IK scorbot)
    {
        robot = scorbot;
    }

    /**
	 * Ejecuta una rotación de una articulación del Scorbot a través de un botón.
	 * @param btn Identificador del botón
	 * @return void
	 */
    public void ManualControlArticulation(int btn)
    {
        // buttons: Same order than keyCodes
        int j = 0;
        for (int i = 0; i < robot.GetArticulations().Length; i++)
        {
            if (keyCodes[btn] == keyCodes[j])
            {
                robot.GetArticulations()[i].Rotate(-ROTATION_SENSIBILITY);
            }
            j++;
            if (keyCodes[btn] == keyCodes[j])
            {
                robot.GetArticulations()[i].Rotate(ROTATION_SENSIBILITY);
            }
            j++;
        }
    }

    /**
	 * Si el control por teclado de las articulaciones del Scorbot está activado/deactivado.
	 * @return processing Control por teclado activado/desactivado. Activado = true
	 */
    public bool GetProcessing()
    {
        return processing;
    }

    /**
	 * Activa/desactiva el control por teclado de las articulaciones del Scorbot.
	 * @param processing Control por teclado activo, true
	 * @return void
	 */
    public void SetProcessing(bool processing)
    {
        this.processing = processing;
    }
}
