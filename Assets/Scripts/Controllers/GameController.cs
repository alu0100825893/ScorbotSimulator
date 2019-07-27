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




public delegate void HereFromDelegate(bool fromSimulation);//

public class GameController : MonoBehaviour
{
    public static GameController gameController;   
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


    // millimeter planes. fix other sides (invisible). fix size of target (dinamic) in close range
    // done? teach dont mantain p 0 and succeed (p 10?).  40 0 80
    // teachr. need sync when a point is moved. cant be const updated in real robot
    // sync points Test this
    // teach
    // shiftc Test this
    // SYNC // What if a position is unrechable???
    // LOGS Bloks
    // Commands redundant
    // HERE ERROR
    // Message online offline, validation
    // CONSOLE PIVOT  0 1   panel: DOWN STRETCH Terminal:EXPAND
    // Fix console animation
    /*
     * 
     * Canvas canvas = FindObjectOfType<Canvas>();
            float h = canvas.GetComponent<RectTransform>().rect.height;
            y = h - y;
            */


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

    // Panels
    public GameObject manualControlPanel;
    public GameObject commandsPanel;
    public GameObject console;
    public GameObject syncPanel;
    public GameObject messageLogPanel;
    public GameObject positionLogPanel;
     
    public InputField targetNameInput;
    public InputField DefpNameInput;

    public Transform planeXY;
    public Transform planeXZ;
    public Transform planeYZ;


    // Controllers
    private ManualInputControl manualInputControl;
    private SelectionControl selectionControl;
    private TargetControl targetControl;
    private CommandControl commandControl;
    private PanelControl panelControl;
    private StateMessageControl stateMessageControl;
    private CameraControl cameraControl;

    public Controller controller; // 

    public Transform canvas;

    public TextMeshProUGUI stateOutput;
    public TextMeshProUGUI messageLog;
    public TextMeshProUGUI positionLog;
    public TextMeshProUGUI positionSyncLog; 
    public TextMeshProUGUI positionCountLog; 
       

    public Dropdown targetDropdown;
    private LineRenderer lineRenderer;
    public CatmullRomSpline spline = new CatmullRomSpline();
    private bool linear = false;

    public Dropdown commandsDropdown;
    public Dropdown position1Dropdown;
    public Dropdown position2Dropdown;
    public Dropdown syncTargetDropdown;
    public Dropdown byXYZPRDropdown;
    public Dropdown portsDropdown;

    public const string NUMBER_FORMAT = "0.00";
    public InputField XInput;
    public InputField YInput;
    public InputField ZInput;
    public InputField PInput;
    public InputField RInput;
    public InputField byXYZPRInput;
    
    public Transform hereGroup;
    public Transform syncGroup;


    public GameObject menu;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI speedLText;

    private bool onlineMode = false;
    private bool syncFromSimulationToScorbot = false;

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
                
        lineRenderer = GetComponent<LineRenderer>();
        // Controllers
        manualInputControl = GetComponent<ManualInputControl>();
        selectionControl = GetComponent<SelectionControl>();
        targetControl = GetComponent<TargetControl>();
        commandControl = GetComponent<CommandControl>();
        panelControl = GetComponent<PanelControl>();
        stateMessageControl = GetComponent<StateMessageControl>();
        cameraControl = GetComponent<CameraControl>();

        cameraControl.SetIsProcessing(true);//
        //selectionControl.SetActiveAxis(target, false);
        axisCamera.SetActive(true);

        
        defaultPosition = target.position;
        target.GetComponent<ClampName>().textPanel.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
        SetTarget(null);
        // Initial target

        //Destroy(target.GetComponent<ClampName>().textPanel.gameObject);
        //Destroy(target.gameObject);
        //SetTarget(targetControl.GetTarget(0));
        //selectionControl.SetActiveAxis(targetControl.GetTarget(0), false);


        UpdateTargets(targetControl.GetNames());

        commandsDropdown.AddOptions(commandControl.GetNames());
        CommandsDropdown_IndexChanged(0);

        byXYZPRDropdown.AddOptions(new List<string>() { "X", "Y", "Z", "P", "R"});

        // Ports list
        //Debug.Log(controller.List_Ports().Length);
        portsDropdown.AddOptions(new List<string>(controller.List_Ports()));

        //waitPanel.SetActive(false);
    }

    // ---------------------------------- All controls ------------------------------
    void Update()
    {
        // Menu activation
        if (menu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menu.SetActive(false);
                cameraControl.SetIsProcessing(true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                MainMenu();
            }
        }

        // Robot values
        if (!robot)
            return;
        /*
        output.text = "Angles: \n";
        for (int artIndex = robot.GetArticulations().Length - 1; artIndex >= 0; artIndex--)
        {
            output.text += robot.GetArticulations()[artIndex].Angle() + "\n";
        }
        output.text += "Efector pos: \n" + robot.GetE().position + "\n";
        */

        Vector3 pos = new Vector3(robot.GetE().position.x, robot.GetE().position.z, robot.GetE().position.y);
        pos = pos * 10f;
        output.text = "Efector pos: \n" + "X: " + 
            pos.x.ToString(NUMBER_FORMAT) + "\nY: " + pos.y.ToString(NUMBER_FORMAT) + "\nZ: " + pos.z.ToString(NUMBER_FORMAT) + "\n";

        //robot.articulations[3].UpdateAngleAsGlobal(new Vector3(0f, 0f, 45f));
        /*
        output.text = "Efector pos: \n" + robot.GetE().position + "\n";
        
        output.text += robot.GetArticulations()[3].Angle() + "\n";
        output.text += robot.GetArticulations()[3].transform.rotation.eulerAngles + "\n";
        */
        DrawTrayectory();
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
        stateMessageControl.NewBlock();
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
        panelControl.ShowHideManualControls();
    }

    public void ShowHideCommands()
    {
        panelControl.ShowHideCommands();        
    }

    public void ShowHideConsole()
    {
        panelControl.ShowHideConsole();
    }

    public void ShowHideSync()
    {
        panelControl.ShowHideSync();
    }

    public void ShowHideMessageLog()
    {
        stateMessageControl.UpdateMessageLog();
        panelControl.ShowHideMessageLog();
    }

    public void ShowHidePositionLog()
    {
        panelControl.ShowHidePositionLog();
    }

    public void RecordPosition(bool fromCommand)
    {
        InputField targetNameInput = this.targetNameInput;
        if (fromCommand)
            targetNameInput = this.DefpNameInput;
        stateMessageControl.NewBlock();
        // No positions recorded
        if (targetControl.Count() == 0)
        {
            CreateDefaultTarget(targetNameInput);
        }
        else // Already at least one point
        {
            GameObject prevSelectedObject = selectionControl.SearchContainTag("Target");

            if (prevSelectedObject != null)
            {
                prevSelectedObject = prevSelectedObject.transform.parent.gameObject;

                // Validate target
                Vector3 newPos = prevSelectedObject.transform.position;
                if (!ValidTarget(targetNameInput, prevSelectedObject.transform, newPos))
                {                  
                    return;
                }
                
                // Add target
                Transform addedTarget = targetControl.Add(targetNameInput.text, newPos, robot.GetAnglesFromCopy());
                addedTarget.GetComponent<TargetModel>().SetValid(true);
                selectionControl.SetActiveAxis(addedTarget, false);
                selectionControl.SelectedObject(prevSelectedObject);

                stateMessageControl.WriteMessage("Done. Recorded position \"" + targetNameInput.text + "\"", true);

                UpdateTargets(targetControl.GetNames());
                targetNameInput.text = "";
                //DrawTrayectory();
            }
            else
                CreateDefaultTarget(targetNameInput);
        }

        stateMessageControl.UpdatePositionLog();
    }

    private void CreateDefaultTarget(InputField targetNameInput)
    {
        Transform newTarget = Instantiate(targetPrefab).transform;

        // Validate target
        Vector3 newPos = defaultPosition;
        if (!ValidTarget(targetNameInput ,newTarget, newPos))
        {
            Destroy(newTarget.gameObject);      
            return;
        }

        // Add target
        Transform addedTarget = targetControl.Add(targetNameInput.text, newPos, robot.GetAnglesFromCopy());
        addedTarget.GetComponent<TargetModel>().SetValid(true);
        selectionControl.SetActiveAxis(addedTarget, false);
        //selectionControl.SelectedObject(addedTarget.gameObject);
        SetTarget(addedTarget);

        stateMessageControl.WriteMessage("Done. Recorded position \"" + targetNameInput.text + "\"", true);

        UpdateTargets(targetControl.GetNames());
        targetNameInput.text = "";
        //DrawTrayectory();
        Destroy(newTarget.gameObject);
    }
    
    private bool ValidTarget(InputField targetNameInput, Transform newTarget, Vector3 newPos) {

        // Check if it's a valid name  
        if (targetNameInput.text.Equals("") || targetNameInput.text == null)
        {      
            stateMessageControl.WriteMessage("Error. Name required", false);            
            return false;
        }
        if (!targetControl.ValidNameLength(targetNameInput.text))
        {
            stateMessageControl.WriteMessage("Error. Name too long \"" + targetNameInput.text + "\"", false);
            return false;
        }
        if (!targetControl.ValidName(targetNameInput.text))
        {
            stateMessageControl.WriteMessage("Error. Name already in use \"" + targetNameInput.text + "\"", false);            
            return false;
        }
                
        newTarget.position = newPos;

        // Check if it's an unreachable position           
        if (!robot.TargetInRange(newTarget.transform))
        {              
            stateMessageControl.WriteMessage("Error. Unreachable position \"" + targetNameInput.text + "\"", false);
            return false;
        }
               
        return true;        
    }

    public void RemoveTarget()
    {
        stateMessageControl.NewBlock();
        if (!target)
        {
            stateMessageControl.WriteMessage("Done. Nothing to delete", true);
            return;
        }

        string name = target.GetComponent<TargetModel>().GetName();
        // Delete target
        targetControl.Remove(target);
        // Update targets
        UpdateTargets(targetControl.GetNames());
        if(targetControl.Count() >= 1) 
            SetTarget(targetControl.GetTarget(0));

        stateMessageControl.WriteMessage("Done. Deleted \"" + name + "\"", true);
        stateMessageControl.UpdatePositionLog();
    }

    // Target change = change selected object?
    public void TargetDropdown_IndexChanged(int index)
    {  
        
        SetTarget(targetControl.GetTarget(index));
        //selectionControl.SetActiveAxis(targetControl.GetTarget(index), true);
        //selectionControl.SelectedObject(targetControl.GetTarget(index).gameObject);
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
        byXYZPRDropdown.gameObject.SetActive(false);
        byXYZPRInput.gameObject.SetActive(false);


        XInput.text = "";
        YInput.text = "";
        ZInput.text = "";
        PInput.text = "";
        RInput.text = "";
        byXYZPRInput.text = "";

        DefpNameInput.gameObject.SetActive(false);
        hereGroup.gameObject.SetActive(false);
        
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
                PInput.placeholder.GetComponent<Text>().text = "P:" + target.GetComponent<TargetModel>().GetPitch().ToString(NUMBER_FORMAT);
                RInput.placeholder.GetComponent<Text>().text = "R:" + target.GetComponent<TargetModel>().GetRoll().ToString(NUMBER_FORMAT);
                break;
            case (int)CommandHelper.here:
                position1Dropdown.gameObject.SetActive(true);
                hereGroup.gameObject.SetActive(true);
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
            case (int)CommandHelper.shiftc:
                position1Dropdown.gameObject.SetActive(true);
                byXYZPRDropdown.gameObject.SetActive(true);
                byXYZPRInput.gameObject.SetActive(true);
                break; 
            case (int)CommandHelper.defp:
                DefpNameInput.gameObject.SetActive(true);
                break;
            default:
                position1Dropdown.gameObject.SetActive(false);
                position2Dropdown.gameObject.SetActive(false);
                break;
        }

        //DrawTrayectory();
    }

    // Execute selected command
    public void Execute()
    {
        stateMessageControl.NewBlock();

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
                if (targetControl.Count() <= 1 || (position1Dropdown.value == position2Dropdown.value))
                {
                    stateMessageControl.WriteMessage("Error. MOVEC Needs 2 different positions", false);
                    return;
                }
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
                    PInput.text = target.GetComponent<TargetModel>().GetPitch().ToString(NUMBER_FORMAT);
                if (RInput.text.Equals(""))
                    RInput.text = target.GetComponent<TargetModel>().GetRoll().ToString(NUMBER_FORMAT);

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
            case (int)CommandHelper.shiftc:
                if (byXYZPRInput.text.Equals(""))
                    return;

                commandControl.Shiftc(robot, targetControl.GetTarget(position1Dropdown.value), byXYZPRDropdown.value, float.Parse(byXYZPRInput.text));
                break;
            case (int)CommandHelper.con:
                commandControl.CON();
                break;
            case (int)CommandHelper.defp:
                RecordPosition(true);
                break;
            default:
                break;
        }
    }

    private void SetTarget(Transform t)
    {
        if (t)
        {
            // Change targets color
            if (target)
                target.GetComponent<TargetModel>().SetCurrentTarget(false);
            t.GetComponent<TargetModel>().SetCurrentTarget(true);
        }
        // Update target
        target = t;
        robot.SetD(t);

        // Update info in commands
        CommandsDropdown_IndexChanged(commandsDropdown.value);
    }

    public void DrawTrayectory()
    {
        // Don't draw if commands panel is not visible
        if (!commandsPanel.transform.GetComponent<Animator>().GetBool("show"))
        {
            LineRenderEmpty();
            return;
        }

        switch(commandsDropdown.value)
        {
            case (int)CommandHelper.move:
                LineRenderEmpty();
                break;
            case (int)CommandHelper.movel:
                if (targetControl.Count() == 0)
                    return;

                Vector3[] positions = new Vector3[2];

                positions[0] = robot.GetE().position;
                positions[1] = targetControl.GetTarget(position1Dropdown.value).position;

                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);
                lineRenderer.startWidth = 1f;
                lineRenderer.endWidth = 1f;
                break;
            case (int)CommandHelper.movec:
                if (targetControl.Count() <= 1)
                    return;

                if (position1Dropdown.value == position2Dropdown.value)
                    return;

                Transform middlePoint = targetControl.GetTarget(position2Dropdown.value);
                Transform finalPoint = targetControl.GetTarget(position1Dropdown.value);

                List<Transform> list = new List<Transform>();
                list.Add(robot.GetE());
                list.Add(middlePoint);
                list.Add(finalPoint);

                Vector3[] positionsSpline = spline.PrepareSpline(list);

                List<Vector3> result = spline.GetTrayectory(positionsSpline);

                lineRenderer.positionCount = result.Count;
                lineRenderer.SetPositions(result.ToArray());
                lineRenderer.startWidth = 1f;
                lineRenderer.endWidth = 1f;
                break;
            case (int)CommandHelper.teach:
                LineRenderEmpty();
                break;
        }
        
    }

    private void LineRenderEmpty()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.SetPositions(new Vector3[0]);
    }

    public void SpeedInputEnd(string speed)
    {
        commandControl.Speed(robot, int.Parse(speed));
        //robot.SetSpeed(int.Parse(speed));
        speedText.text = speed;
    }

    public void SpeedLInputEnd(string speed)
    {
        commandControl.SpeedL(robot, int.Parse(speed));
        //robot.SetSpeedL(int.Parse(speed));
        speedLText.text = speed;
    }

    private void UpdateTargets(List<string> targetsNamesArray)
    {
        targetDropdown.ClearOptions();
        position1Dropdown.ClearOptions();
        position2Dropdown.ClearOptions();
        syncTargetDropdown.ClearOptions();

        targetDropdown.AddOptions(targetsNamesArray);
        position1Dropdown.AddOptions(targetsNamesArray);
        position2Dropdown.AddOptions(targetsNamesArray);
        syncTargetDropdown.AddOptions(targetsNamesArray);
                        
    }

    public void SetScorbot(int index)
    {
        foreach (IK scorbot in scorbots)
            scorbot.gameObject.SetActive(false);

        switch(index)
        {
            case ScorbotERIX.INDEX:
                scorbots[ScorbotERIX.INDEX].gameObject.SetActive(true);
                robot = scorbots[ScorbotERIX.INDEX];
                break;
            case ScorbotERVPlus.INDEX:
                scorbots[ScorbotERVPlus.INDEX].gameObject.SetActive(true);
                robot = scorbots[ScorbotERVPlus.INDEX];
                break;
        }
    }

    // Mode Online - Offline
    public void OnlineOfflineSlider(float value)
    {
        onlineMode = value == 1f ? true : false;          
        controller.Online_Offline(onlineMode);
    }

    public bool GetOnlineMode()
    {
        return onlineMode;
    }

    public void HereFromSimulation(bool fromSimulation)
    {   
        HereFromDel(fromSimulation);
    }

    public void SyncFromSimulationToScorbot(bool fromSimulation)
    {
        syncFromSimulationToScorbot = fromSimulation;
    }
    
    public void SyncAllTargets()
    {
        stateMessageControl.NewBlock();

        List<string> namesError = new List<string>();
        if (!GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online ALLSYNC You are offline", false);
            return;
        }

        //waitPanel.SetActive(true);
        // Only online
        if (syncFromSimulationToScorbot)
        {
            // Defp?            
            foreach (string t in targetControl.GetNames())
            {
                if (!commandControl.SyncSimulationToScorbot(robot, targetControl.GetTarget(t)))
                    namesError.Add(t);
            }
        }
        else
        {
            foreach (string t in targetControl.GetNames())
            {
                if (!commandControl.SyncScorbotToSimulation(robot, targetControl.GetTarget(t)))
                    namesError.Add(t);
            }
        }
        
        if(namesError.Count >= 1)
            stateMessageControl.WriteMessage("Error. Online ALLSYNC Positions with error: " + namesError.Count, false);
        else
            stateMessageControl.WriteMessage("Done. Online ALLSYNC", true);

        //waitPanel.SetActive(false);
      
        
    }

    public void SyncTarget()
    {
        stateMessageControl.NewBlock();

        if (!GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online SYNC You are offline \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }

        //waitPanel.SetActive(true);
        // Only online
        if (syncFromSimulationToScorbot)
        {           
            commandControl.SyncSimulationToScorbot(robot, targetControl.GetTarget(syncTargetDropdown.value));
        }
        else
        {
            commandControl.SyncScorbotToSimulation(robot, targetControl.GetTarget(syncTargetDropdown.value));
        }
     
        //waitPanel.SetActive(false);
    }

    public void MainMenu()
    {
        menu.SetActive(true);
        cameraControl.SetIsProcessing(false);
    }

    public void CON()
    {
        stateMessageControl.NewBlock();
        if (!GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online CON You are offline", false);
            return;
        }
        commandControl.CON();
    }

}