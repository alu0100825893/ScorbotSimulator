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

    private List<KeyCode> keyCodes = new List<KeyCode>();
    private IK robot;
    private const float ROTATION_SENSIBILITY = 1f;

    private bool processing = false;

    void Start () {
        robot = GetComponent<GameController>().robot;

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
    }
	
	void Update () {

        if (!processing)
            return;

        // Manual rotation (See keyCodes at "Start()")
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

        if(Input.GetKey(KeyCode.Y))
        {
            robot.GetComponent<GripScorbotERIX>().Open();
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            robot.GetComponent<GripScorbotERIX>().Close();
        }
    }

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

    public bool GetProcessing()
    {
        return processing;
    }

    public void SetProcessing(bool processing)
    {
        this.processing = processing;
    }
}
