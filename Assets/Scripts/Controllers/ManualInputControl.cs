using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualInputControl : MonoBehaviour {

    private List<KeyCode> keyCodes = new List<KeyCode>();
    private IK robot;

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
                robot.GetArticulations()[i].Rotate(-1f);
            }
            j++;
            if (Input.GetKey(keyCodes[j]))
            {
                robot.GetArticulations()[i].Rotate(1f);
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
                robot.GetArticulations()[i].Rotate(-1f);
            }
            j++;
            if (keyCodes[btn] == keyCodes[j])
            {
                robot.GetArticulations()[i].Rotate(1f);
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
