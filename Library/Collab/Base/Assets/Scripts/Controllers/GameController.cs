using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * La clase GameController representa el controlador principal del simulador
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es
 * @version 1.0
 * @subject Simulador
 * @organization Universidad de La Laguna
 * @since 02-05-2019
 */



public delegate void OnlineDelegate(bool online);//
public delegate void HereFromDelegate(bool online);//

public class GameController : MonoBehaviour
{
    public static GameController gameController;
    public static event OnlineDelegate OnlineDel;//
    public static event HereFromDelegate HereFromDel;//
    // TODO: Visualization of positions
    // TODO:  unreachable point, test more, problems
    // TODO: CD algorithm DH , smooth robot movement with final result
    // target 22.02 91.58 -4.2 
    // TODO: AlgCDD. Problem when point is almost out of range, requires too many iterations so a point could be unreachable
    // TODO: XYZ manual control, it stops when not pressed
    // TODO: IK can lock it self when position is reachable (solution base 180 º, or home before kinematics or second try with home before)
    // DynamicPrecision Selected can be an arrow, fix this
    // PLANE:  pos 10 10   scale 2 2 2    pos 99.8 99.8    slace 20 20 20
    // TODO: Error?, record position, can't create several at a time (2 at most) because of size of selection list

    
    // done block manual input when writing
    // done target color change 
    // done coordinates in target for visualization
    // done speeds display    
    // done new millimeter planes with activation/desactivation
    // done menu principal.
    // done  read encoder transform into target
    // millimeter planes. fix other sides (invisible). fix size of target (dinamic) in close range
    // done teach load values by default
    // done? teach dont mantain p 0 and succeed (p 10?).  40 0 80
    // teachr. need sync when a point is moved. cant be const updated in real robot
    // invert sign in teach roll valueç
    // sync points

    // terminal, not set active, just move it out of screen 
    
    /* 
    * RotationAxisControl
    * MovementAxisControl
    * ScorbotIK
    * UIController
    */
    // --------------------------------- position with angles----------------------
    public Transform cam;


    private float axisSensibity = 4f;
    private float rotationSensibity = 4f;

    public Transform target;
    public Transform targetName;
    public Transform targetPrefab;
    public Transform targetNamePrefab;
    private Vector3 defaultPosition;

    public GameObject axisCamera; // Bug? Two cameras destroy selecting interaction. Temporal fix

    private bool rotateInAxis = false;

    public IK[] scorbots;
    public IK robot;
    public GameObject innerTarget;

    public TextMeshProUGUI output;
    public GameObject manualControlPanel;
    private bool activeManualControlPanel = false;

    public GameObject commandsPanel;
    private bool activecommandsPanel = false;

    public GameObject console;
    private bool activeConsole = false;
    public InputField targetNameInput;
    
    public Transform planeXY;
    public Transform planeXZ;
    public Transform planeYZ;


    // Controllers
    private ManualInputControl manualInputControl;
    private SelectionControl selectionControl;
    private TargetControl targetControl;
    private CommandControl commandControl;

    public Controller controller; // 

    public Transform canvas;

    public TextMeshProUGUI stateOutput;

    public Dropdown targetDropdown;
    private LineRenderer lineRenderer;
    public CatmullRomSpline spline = new CatmullRomSpline();
    private bool linear = false;

    public Dropdown commandsDropdown;
    public Dropdown position1Dropdown;
    public Dropdown position2Dropdown;

    private const string numberFormat = "0.00";
    public InputField XInput;
    public InputField YInput;
    public InputField ZInput;
    public InputField PInput;
    public InputField RInput;

    public Transform HereGroup;

    public GameObject menu;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI speedLText;

    private bool onlineMode = false;

    private void Awake()
    {
        if (gameController == null)
        {
            gameController = this;
            DontDestroyOnLoad(this);

        }
        else
        {
            if (gameController != this)
            {
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        GameController.OnlineDel += testExe;//
        
        lineRenderer = GetComponent<LineRenderer>();
        // Controllers
        manualInputControl = GetComponent<ManualInputControl>();
        selectionControl = GetComponent<SelectionControl>();
        targetControl = GetComponent<TargetControl>();
        commandControl = GetComponent<CommandControl>();
 

        //selectionControl.SetActiveAxis(target, false);
        axisCamera.SetActive(true);

        manualControlPanel.SetActive(false);
        console.SetActive(false);
        commandsPanel.SetActive(false);

        defaultPosition = target.position;
        target.GetComponent<ClampName>().textPanel.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
        // Initial target

        //Destroy(target.GetComponent<ClampName>().textPanel.gameObject);
        //Destroy(target.gameObject);
        //SetTarget(targetControl.GetTarget(0));
        //selectionControl.SetActiveAxis(targetControl.GetTarget(0), false);


        UpdateTargets(targetControl.GetNames());

        commandsDropdown.AddOptions(commandControl.GetNames());
        CommandsDropdown_IndexChanged(0);


    }

    // ---------------------------------- All controls ------------------------------
    void Update()
    {
        // Menu activation
        if (Input.GetKeyDown(KeyCode.Escape))
            menu.SetActive(true);
               

        // Robot values
        if (!robot)
            return;
        
        output.text = "Angles: \n";
        for (int artIndex = robot.GetArticulations().Length - 1; artIndex >= 0; artIndex--)
        {
            output.text += robot.GetArticulations()[artIndex].Angle() + "\n";
        }
        
        output.text += "Efector pos: \n" + robot.GetE().position + "\n";
        
        //robot.articulations[3].UpdateAngleAsGlobal(new Vector3(0f, 0f, 45f));
        /*
        output.text = "Efector pos: \n" + robot.GetE().position + "\n";
        
        output.text += robot.GetArticulations()[3].Angle() + "\n";
        output.text += robot.GetArticulations()[3].transform.rotation.eulerAngles + "\n";
        */
    }

    private void testExe(bool online)
    {
        Debug.Log("Test delegate Exe ");
    }

    /**
	 * Método que permite el control manual del Scorbot mediante teclado.
	 * @param btn Número de boton
	 * @return Nada
	 */
    public void ManualControlArticulation(int btn)
    {
        manualInputControl.ManualControlArticulation(btn);
    }

    public void Home()
    {
        commandControl.Home(robot);
        //OnlineDel();//
    }

    public void Open()
    {
        robot.GetComponent<GripScorbotERIX>().Open();
    }

    public void Close()
    {
        robot.GetComponent<GripScorbotERIX>().Close();
    }

    public void ShowHideManualControls()
    {
        if (activeManualControlPanel)
        {
            activeManualControlPanel = false;
            manualInputControl.SetProcessing(false);
        }
        else
        {
            activeManualControlPanel = true;
            manualInputControl.SetProcessing(true);
        }
        manualControlPanel.SetActive(activeManualControlPanel);
    }

    public void ShowHideCommands()
    {
        if (activecommandsPanel)
        {
            activecommandsPanel = false;            
        }
        else
        {
            activecommandsPanel = true;            
        }
        commandsPanel.SetActive(activecommandsPanel);
    }

    public void ShowHideConsole()
    {
        if (activeConsole)
            activeConsole = false;
        else
            activeConsole = true;
        console.SetActive(activeConsole);
    }

    public void RecordPosition()
    {
        Transform newTarget = Instantiate(targetPrefab).transform;

        // No points recorded
        if (targetControl.Count() == 0)
        {
            //ValidTarget(newTarget);
            if (targetNameInput.text.Equals("") || targetNameInput.text == null)
            {
                stateOutput.text = "Name required";
                Destroy(newTarget.gameObject);
                return;
            }


            Vector3 recordedPos = defaultPosition;
            newTarget.position = recordedPos;

            // Check if it's an unreachable point           
            if (!robot.TargetInRange(newTarget.transform))
            {
                Destroy(newTarget.gameObject);
                stateOutput.text = "Unreachable point";
                return;
            }

            stateOutput.text = "OK";

            // Add target
            Transform addedTarget = targetControl.Add(targetNameInput.text, recordedPos, robot.GetAnglesFromCopy());
            selectionControl.SetActiveAxis(addedTarget, true);
            selectionControl.SelectedObject(addedTarget.gameObject);
            SetTarget(addedTarget);

            UpdateTargets(targetControl.GetNames());
            targetNameInput.text = "";
            DrawTrayectory();
        }
        else // Already at least one point
        {
            GameObject prevSelectedObject = selectionControl.SearchContainTag("Target");

            if (prevSelectedObject != null)
            {
                prevSelectedObject = prevSelectedObject.transform.parent.gameObject;

                if (targetNameInput.text.Equals("") || targetNameInput.text == null)
                {
                    stateOutput.text = "Name required";
                    Destroy(newTarget.gameObject);
                    return;
                }


                Vector3 recordedPos = prevSelectedObject.transform.position;
                newTarget.position = recordedPos;

                // Check if it's an unreachable point           
                if (!robot.TargetInRange(prevSelectedObject.transform))
                {
                    Destroy(newTarget.gameObject);
                    stateOutput.text = "Unreachable point";
                    return;
                }

                stateOutput.text = "OK";

                // Add target
                Transform addedTarget = targetControl.Add(targetNameInput.text, recordedPos, robot.GetAnglesFromCopy());
                selectionControl.SetActiveAxis(addedTarget, false);
                selectionControl.SelectedObject(prevSelectedObject);

                UpdateTargets(targetControl.GetNames());

                targetNameInput.text = "";

                DrawTrayectory();
            }
        }

        Destroy(newTarget.gameObject);
    }

    // Finish this
    private bool ValidTarget(Transform newTarget) {
        
        if (targetNameInput.text.Equals("") || targetNameInput.text == null)
        {
            stateOutput.text = "Name required";
            Destroy(newTarget.gameObject);
            return false;
        }


        Vector3 recordedPos = defaultPosition;
        newTarget.position = recordedPos;

        // Check if it's an unreachable point           
        if (!robot.TargetInRange(newTarget.transform))
        {
            Destroy(newTarget.gameObject);
            stateOutput.text = "Unreachable point";
            return false;
        }

        stateOutput.text = "OK";
        
        return true;
        
    }

    public void RemoveTarget()
    {
        if (target)
            targetControl.Remove(target);
        UpdateTargets(targetControl.GetNames());
        if(targetControl.Count() >= 1) 
            SetTarget(targetControl.GetTarget(0));
    }

    public void TargetDropdown_IndexChanged(int index)
    {   
        SetTarget(targetControl.GetTarget(index));
    }

    // Update commands menu
    public void CommandsDropdown_IndexChanged(int index)
    {
        position1Dropdown.gameObject.SetActive(false);
        position2Dropdown.gameObject.SetActive(false);
        XInput.gameObject.SetActive(false);
        YInput.gameObject.SetActive(false);
        ZInput.gameObject.SetActive(false);
        PInput.gameObject.SetActive(false);
        RInput.gameObject.SetActive(false);

        XInput.text = "";
        YInput.text = "";
        ZInput.text = "";
        PInput.text = "";
        RInput.text = "";

        HereGroup.gameObject.SetActive(false);


        switch (index)
        {
            case (int)CommandHelper.move:
                position1Dropdown.gameObject.SetActive(true);                
                break;
            case (int)CommandHelper.movel:
                position1Dropdown.gameObject.SetActive(true);
                break;
            case (int)CommandHelper.movec:
                position1Dropdown.gameObject.SetActive(true);
                position2Dropdown.gameObject.SetActive(true);
                position2Dropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Between";
                break;
            case (int)CommandHelper.teach:
                position1Dropdown.gameObject.SetActive(true);
                XInput.gameObject.SetActive(true);                
                YInput.gameObject.SetActive(true);                
                ZInput.gameObject.SetActive(true);                
                PInput.gameObject.SetActive(true);                
                RInput.gameObject.SetActive(true);

                // Preview target values
                XInput.placeholder.GetComponent<Text>().text = "X:" + target.GetComponent<TargetModel>().GetPosmm().x;
                YInput.placeholder.GetComponent<Text>().text = "Y:" + target.GetComponent<TargetModel>().GetPosmm().z;
                ZInput.placeholder.GetComponent<Text>().text = "Z:" + target.GetComponent<TargetModel>().GetPosmm().y;
                PInput.placeholder.GetComponent<Text>().text = "P:" + target.GetComponent<TargetModel>().GetPitch().ToString(numberFormat);
                RInput.placeholder.GetComponent<Text>().text = "R:" + (-target.GetComponent<TargetModel>().GetRoll()).ToString(numberFormat);
                break;
            case (int)CommandHelper.here:
                position1Dropdown.gameObject.SetActive(true);
                HereGroup.gameObject.SetActive(true);
                break;
            case (int)CommandHelper.teachr:
                position1Dropdown.gameObject.SetActive(true);                
                position2Dropdown.gameObject.SetActive(true);
                position2Dropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "RelativeTo";

                XInput.gameObject.SetActive(true);
                YInput.gameObject.SetActive(true);
                ZInput.gameObject.SetActive(true);
                PInput.gameObject.SetActive(true);
                RInput.gameObject.SetActive(true);

                // Preview target values
                XInput.placeholder.GetComponent<Text>().text = "X:" + "0";
                YInput.placeholder.GetComponent<Text>().text = "Y:" + "0";
                ZInput.placeholder.GetComponent<Text>().text = "Z:" + "0";
                PInput.placeholder.GetComponent<Text>().text = "P:" + "0";
                RInput.placeholder.GetComponent<Text>().text = "R:" + "0";
                break;
            default:
                position1Dropdown.gameObject.SetActive(false);
                position2Dropdown.gameObject.SetActive(false);
                break;
        }

        DrawTrayectory();
    }

    // Execute selected command 
    public void Execute()
    {
        switch (commandsDropdown.value)
        {
            case (int)CommandHelper.move:
                if (targetControl.Count() <= 0)
                    return;
                commandControl.Move(robot, targetControl.GetTarget(position1Dropdown.value));
                break;
            case (int)CommandHelper.movel:
                if (targetControl.Count() <= 0)
                    return;
                commandControl.MoveL(robot, targetControl.GetTarget(position1Dropdown.value));
                break;
            case (int)CommandHelper.movec:
                if (targetControl.Count() <= 1)
                    return;
                commandControl.MoveC(robot, targetControl.GetTarget(position1Dropdown.value),
                    targetControl.GetTarget(position2Dropdown.value));
                break;
            case (int)CommandHelper.teach:
                if (targetControl.Count() <= 0)
                    return;
                // Just use targets values if no new values specified 
                if (XInput.text.Equals(""))
                    XInput.text = target.GetComponent<TargetModel>().GetPosmm().x.ToString();
                if (YInput.text.Equals(""))
                    YInput.text = target.GetComponent<TargetModel>().GetPosmm().z.ToString();
                if (ZInput.text.Equals(""))
                    ZInput.text = target.GetComponent<TargetModel>().GetPosmm().y.ToString();
                if (PInput.text.Equals(""))
                    PInput.text = target.GetComponent<TargetModel>().GetPitch().ToString(numberFormat);
                if (RInput.text.Equals(""))
                    RInput.text = (-target.GetComponent<TargetModel>().GetRoll()).ToString(numberFormat);

                commandControl.Teach(robot, targetControl.GetTarget(position1Dropdown.value),
                    new Vector3(float.Parse(XInput.text), float.Parse(YInput.text), float.Parse(ZInput.text)),
                    float.Parse(PInput.text), float.Parse(RInput.text));
                break;
            case (int)CommandHelper.here:
                if (targetControl.Count() <= 0)
                    return;
                commandControl.Here(robot, targetControl.GetTarget(position1Dropdown.value));
                break;
            case (int)CommandHelper.home:
                commandControl.Home(robot);
                break;
            case (int)CommandHelper.teachr:
                // Just use 0 values if no new values specified 
                if (XInput.text.Equals(""))
                    XInput.text = "0";
                if (YInput.text.Equals(""))
                    YInput.text = "0";
                if (ZInput.text.Equals(""))
                    ZInput.text = "0";
                if (PInput.text.Equals(""))
                    PInput.text = "0";
                if (RInput.text.Equals(""))
                    RInput.text = "0";
                commandControl.TeachR(robot, targetControl.GetTarget(position1Dropdown.value), targetControl.GetTarget(position2Dropdown.value),
                    new Vector3(float.Parse(XInput.text), float.Parse(YInput.text), float.Parse(ZInput.text)),
                    float.Parse(PInput.text), float.Parse(RInput.text));
                break;
            default:
                break;
        }
    }

    private void SetTarget(Transform t)
    {
        // Change targets color
        if (target)
            target.GetComponent<TargetModel>().SetCurrentTarget(false);
        t.GetComponent<TargetModel>().SetCurrentTarget(true);
        // Update target
        target = t;
        robot.SetD(t);

        // Update info in commands
        CommandsDropdown_IndexChanged(commandsDropdown.value);
    }

    // Refactor this
    public void DrawTrayectory()
    {
        
        if (!commandsPanel.activeSelf)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
        }

        if (commandsPanel.activeSelf && (commandsDropdown.value == (int)CommandHelper.move))
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
        }

        if (commandsPanel.activeSelf && (commandsDropdown.value == (int)CommandHelper.movel))
        {
            if (targetControl.Count() == 0)
                return;
            Vector3[] positions = new Vector3[2];

            positions[0] = robot.GetE().position;        
            positions[1] = targetControl.GetTarget(position1Dropdown.value).position;

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = 1f;
            lineRenderer.endWidth = 1f;
        }
             
        if (targetControl.Count() <= 1)
            return;

        if (commandsPanel.activeSelf && (commandsDropdown.value == (int)CommandHelper.movec))
        {     
            if (position1Dropdown.value == position2Dropdown.value)
                return;
                  
            Transform middlePoint = targetControl.GetTarget(position2Dropdown.value);
            Transform finalPoint = targetControl.GetTarget(position1Dropdown.value);

            List<Transform> list = new List<Transform>();
            list.Add(robot.GetE());
            list.Add(middlePoint);
            list.Add(finalPoint);

            Vector3[] positions = spline.PrepareSpline(list);          

            List<Vector3> result = spline.GetTrayectory(positions);

            lineRenderer.positionCount = result.Count;
            lineRenderer.SetPositions(result.ToArray());
            lineRenderer.startWidth = 1f;
            lineRenderer.endWidth = 1f;
        }

        if (commandsPanel.activeSelf && (commandsDropdown.value == (int)CommandHelper.teach))
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);

        }
    }

    public void SpeedInputEnd(string speed)
    {      
        robot.SetSpeed(int.Parse(speed));
        speedText.text = speed;
    }

    public void SpeedLInputEnd(string speed)
    {      
        robot.SetSpeedL(int.Parse(speed));
        speedLText.text = speed;
    }

    private void UpdateTargets(List<string> targetsNamesArray)
    {
        targetDropdown.ClearOptions();
        position1Dropdown.ClearOptions();
        position2Dropdown.ClearOptions();
        targetDropdown.AddOptions(targetsNamesArray);
        position1Dropdown.AddOptions(targetsNamesArray);
        position2Dropdown.AddOptions(targetsNamesArray);

                
    }

    public void SetScorbot(int index)
    {
        foreach (IK scorbot in scorbots)
            scorbot.gameObject.SetActive(false);

        if (index == ScorbotERIX.INDEX)
        {
            scorbots[ScorbotERIX.INDEX].gameObject.SetActive(true);
            robot = scorbots[ScorbotERIX.INDEX];
        }
        if (index == 1) {
            scorbots[1].gameObject.SetActive(true);
            robot = scorbots[1];
        }
    }

    // Mode Online - Offline
    public void OnlineOfflineSlider(float value)
    {
        if (value == 1f)
        {
            onlineMode = true;
            controller.Connection.GetComponent<SerialController>().Open_Port();
        }
        else
        { 
            onlineMode = false;            
        }
        OnlineDel(onlineMode);
    }

    public bool GetOnlineMode()
    {
        return onlineMode;
    }

    public void HereFromSimulation(bool fromSimulation)
    {   
        HereFromDel(fromSimulation);
    }

    public void AAAAAAAAAAAaTargets()
    {
        if (GetOnlineMode())
        {
            foreach (string t in targetControl.GetNames())
                commandControl.SyncTargets(robot, targetControl.GetTarget(t));
        }
    }
}