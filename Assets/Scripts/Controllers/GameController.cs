using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Event. Command "Here" mode        
public delegate void HereFromDelegate(bool fromSimulation);
// Event. Change Scorbot  
public delegate void ScorbotDelegate(IK robot);
/**
 * El controlador principal de la simulación es el GameController. Su función principal es la de inicializar 
 * la simulación y ejecutar los demás controladores para que realicen tareas específicas. 
 * 
 * Entre sus funciones, también está la creación y eliminación de las posiciones, el manejo de los elementos
 * de la interfaz gráfica que utiliza cada comando y la propia ejecución de los comandos. Además, se encarga
 * del dibujado de trayectorias para “movel” y “movec” con ayuda del algoritmo de spline de Catmull-Rom.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class GameController : MonoBehaviour
{
    public static GameController gameController;
    // Event. Command "Here" mode      
    public static event HereFromDelegate HereFromDel;
    // Event. Change Scorbot      
    public static event ScorbotDelegate ScorbotDel;//

    // Camera
    public Transform cam;

    // Position test and prefab (for duplication)
    public Transform target;
    public Transform targetName;
    public Transform targetPrefab;
    public Transform targetNamePrefab;
    private Vector3 defaultPosition;

    // Second camera. Wolrd axises
    public GameObject axisCamera; 

    // Scorbots
    public IK[] scorbots;
    public IK robot;
    public GameObject innerTarget;
    public static int indexRobot = 0;

    // Scorbot efector
    public TextMeshProUGUI output;

    // Panels
    public GameObject manualControlPanel;
    public GameObject commandsPanel;
    public GameObject console;
    public GameObject syncPanel;
    public GameObject messageLogPanel;
    public GameObject positionLogPanel;
     
    // Data input
    public InputField targetNameInput;
    public InputField DefpNameInput;

    // Metric system. Planes
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
    private BackUpFileControl backupFileControl;
    private EffectorFileControl effectorFileControl;

    // Terminal
    public Controller controller;

    // Canvas. User interface
    public Transform canvas;

    // Output text
    public TextMeshProUGUI stateOutput;
    public TextMeshProUGUI messageLog;
    public TextMeshProUGUI positionLog;
    public TextMeshProUGUI positionSyncLog; 
    public TextMeshProUGUI positionCountLog;
    public TextMeshProUGUI backupFileOutput;

    // Online text
    public TextMeshProUGUI onlineText;
    // Delete. Dropdown
    public Dropdown targetDropdown;

    // Drawing tool
    private LineRenderer lineRenderer;

    // Catmull-Rom Spline
    public CatmullRomSpline spline = new CatmullRomSpline();

    // Dropdown list
    public Dropdown commandsDropdown;
    public Dropdown position1Dropdown;
    public Dropdown position2Dropdown;
    public Dropdown syncTargetDropdown;
    public Dropdown byXYZPRDropdown;
    public Dropdown portsDropdown;
    public Dropdown scorbotVersionDropdown;

    public const string NUMBER_FORMAT = "0.00";

    // Data input
    public InputField XInput;
    public InputField YInput;
    public InputField ZInput;
    public InputField PInput;
    public InputField RInput;
    public InputField byXYZPRInput;
    
    // Check box
    public Transform hereGroup;
    public Transform syncGroup;
    
    // Main menu
    public GameObject menu;

    // Speed text
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI speedLText;

    private bool onlineMode = false;
    // Mode command "Here"
    private bool syncFromSimulationToScorbot = false;

    private bool loadedEffector = false;

    private void Awake()
    {
        // GameController can only have one instance
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

        // Active all scorbots for initialization
        foreach (IK scorbot in scorbots)
        {            
            scorbot.gameObject.SetActive(true);            
        }
    }

    void Start()
    {
        // Drawing tool
        lineRenderer = GetComponent<LineRenderer>();
        // Controllers
        manualInputControl = GetComponent<ManualInputControl>();
        selectionControl = GetComponent<SelectionControl>();
        targetControl = GetComponent<TargetControl>();
        commandControl = GetComponent<CommandControl>();
        panelControl = GetComponent<PanelControl>();
        stateMessageControl = GetComponent<StateMessageControl>();
        cameraControl = GetComponent<CameraControl>();
        backupFileControl = GetComponent<BackUpFileControl>();
        effectorFileControl = GetComponent<EffectorFileControl>();

        // Initial config
        cameraControl.SetIsProcessing(true);//
        axisCamera.SetActive(true);
                
        defaultPosition = target.position;
        target.GetComponent<ClampName>().textPanel.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
        SetTarget(null);               
        UpdateTargets(targetControl.GetNames());

        // Get commands names
        commandsDropdown.AddOptions(commandControl.GetNames());
        CommandsDropdown_IndexChanged(0);

        byXYZPRDropdown.AddOptions(new List<string>() { "X", "Y", "Z", "P", "R"});

        // Ports list 
        portsDropdown.AddOptions(new List<string>(controller.List_Ports()));

        // Scorbot ER IX version list 
        scorbotVersionDropdown.AddOptions(new List<string>() { "Original" });

    }
        
    void Update()
    {
        // After initialization, only once
        if (!loadedEffector)
        {            
            // New end effector values
            effectorFileControl.Load(scorbots);
            loadedEffector = true;
            // Only one scorbot should be active
            foreach (IK scorbot in scorbots)
            {
                if (scorbot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERIX.INDEX)
                    scorbot.gameObject.SetActive(true);
                else
                    scorbot.gameObject.SetActive(false);
            }      
        }

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
                MainMenu();        
        }

        // Scorbot values
        if (!robot || !robot.GetE())
            return;
        // Scorbot end effector
        Vector3 pos = new Vector3(robot.GetE().position.x, robot.GetE().position.z, robot.GetE().position.y);
        pos = pos * 10f;
        output.text = "Efector pos: \n" + "X: " + 
            pos.x.ToString(NUMBER_FORMAT) + "\nY: " + pos.y.ToString(NUMBER_FORMAT) + "\nZ: " + pos.z.ToString(NUMBER_FORMAT) + "\n";
        
        DrawTrayectory();
    }

    /**
	 * Permite el control manual del Scorbot mediante botones de la interfaz gráfica.
	 * @param btn Número de boton
	 * @return void
	 */
    public void ManualControlArticulation(int btn)
    {
        manualInputControl.ManualControlArticulation(btn);
    }

    /**
     * Ejecuta el comando "Home".
     * @return void
     */
    public void Home()
    {
        stateMessageControl.NewBlock();
        commandControl.Home(robot);
    }

    /**
     * Abre la pinza del Scorbot.
     * @return void
     */
    public void Open()
    {
        robot.GetComponent<ScorbotModel>().Open();
    }

    /**
     * Cierra la pinza del Scorbot.
     * @return void
     */
    public void Close()
    {
        robot.GetComponent<ScorbotModel>().Close();
    }

    /**
     * Activa desactiva el panel "Manual"
     * @return void
     */
    public void ShowHideManualControls()
    {
        panelControl.ShowHideManualControls();
    }

    /**
     * Activa desactiva el panel "Commands"
     * @return void
     */
    public void ShowHideCommands()
    {
        panelControl.ShowHideCommands();        
    }

    /**
     * Activa desactiva el panel "Console"
     * @return void
     */
    public void ShowHideConsole()
    {
        panelControl.ShowHideConsole();
    }

    /**
     * Activa desactiva el panel "Sync"
     * @return void
     */
    public void ShowHideSync()
    {
        panelControl.ShowHideSync();
    }

    /**
     * Activa desactiva el panel "Log"
     * @return void
     */
    public void ShowHideMessageLog()
    {
        stateMessageControl.UpdateMessageLog();
        panelControl.ShowHideMessageLog();
    }

    /**
     * Activa desactiva el panel "Positions(0/0)"
     * @return void
     */
    public void ShowHidePositionLog()
    {
        panelControl.ShowHidePositionLog();
    }

    /**
     * Crea una posición (objeto) y la válida. El nombre se obtiene del campo la interfaz gráfica.
     * @param fromCommand Si es para el comando "defp".
     * @return Transform
     */
    public void RecordPosition(bool fromCommand)
    {
        Transform addedTarget = null;

        InputField targetNameInput = this.targetNameInput;
        if (fromCommand)
            targetNameInput = this.DefpNameInput;
        stateMessageControl.NewBlock();
        // No positions recorded
        if (targetControl.Count() == 0)
        {
            CreateDefaultTarget(targetNameInput.text);
        }
        else // Already at least one point
        {
            GameObject prevSelectedObject = selectionControl.SearchContainTag("Target");

            if (prevSelectedObject != null)
            {
                prevSelectedObject = prevSelectedObject.transform.parent.gameObject;

                // Validate target
                Vector3 newPos = prevSelectedObject.transform.position;
                if (!ValidTarget(targetNameInput.text, prevSelectedObject.transform, newPos))
                {                  
                    return;
                }
                
                // Add target
                addedTarget = targetControl.Add(targetNameInput.text, newPos, robot.GetAnglesFromCopy());
                addedTarget.GetComponent<TargetModel>().SetValid(true);
                selectionControl.SetActiveAxis(addedTarget, false);
                selectionControl.SelectedObject(prevSelectedObject);

                stateMessageControl.WriteMessage("Done. Recorded position \"" + targetNameInput.text + "\"", true);

                UpdateTargets(targetControl.GetNames());
                targetNameInput.text = "";            
            }
            else
                CreateDefaultTarget(targetNameInput.text);
        }

        stateMessageControl.UpdatePositionLog();       
    }

    /**
     * Crea una posición (objeto) nueva en una posición por defecto. Debe ser válida.
     * @param targetName Nombre de la posición
     * @return Transform
     */
    public void CreateDefaultTarget(string targetName)
    {
        Transform newTarget = Instantiate(targetPrefab).transform;

        // Validate target
        Vector3 newPos = defaultPosition;
        if (!ValidTarget(targetName ,newTarget, newPos))
        {
            Destroy(newTarget.gameObject);      
            return;
        }

        // Add target
        Transform addedTarget = targetControl.Add(targetName, newPos, robot.GetAnglesFromCopy());
        addedTarget.GetComponent<TargetModel>().SetValid(true);
        selectionControl.SetActiveAxis(addedTarget, false);
       
        SetTarget(addedTarget);

        stateMessageControl.WriteMessage("Done. Recorded position \"" + targetName + "\"", true);

        UpdateTargets(targetControl.GetNames());
        targetName = "";
      
        Destroy(newTarget.gameObject);      
    }

    /**
     * Comprueba si una posición (objeto) tiene un nombre válido y está en el alcance del Scorbot.
     * @param targetName Nombre de la posición
     * @param newTarget Posición (objeto)
     * @param newPos Coordenadas
     * @return bool Válido
     */
    private bool ValidTarget(string targetName, Transform newTarget, Vector3 newPos) {

        // Check if it's a valid name  
        if (targetName.Equals("") || targetName == null)
        {      
            stateMessageControl.WriteMessage("Error. Name required", false);            
            return false;
        }
        if (!targetControl.ValidNameLength(targetName))
        {
            stateMessageControl.WriteMessage("Error. Name too long \"" + targetName + "\"", false);
            return false;
        }
        if (!targetControl.ValidName(targetName))
        {
            stateMessageControl.WriteMessage("Error. Name already in use \"" + targetName + "\"", false);            
            return false;
        }
                
        newTarget.position = newPos;

        // Check if it's an unreachable position           
        if (!robot.TargetInRange(newTarget.transform))
        {              
            stateMessageControl.WriteMessage("Error. Unreachable position \"" + targetName + "\"", false);
            return false;
        }
               
        return true;        
    }

    /**
     * Elimina la posición (objeto) seleccionada en la lista de posiciones.
     * @return void
     */
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

    /**
     * Actualiza una posición (objeto) como el objetivo actual (color violeta) especificando su índice en 
     * una lista de posiciones.
     * @param index
     * @return void
     */
    public void TargetDropdown_IndexChanged(int index)
    {          
        SetTarget(targetControl.GetTarget(index));   
    }
    
    /**
     * Activa los elementos gráficos de un comando especificado por su índice.
     * @param index Índice del comando
     * @return void
     */
    public void CommandsDropdown_IndexChanged(int index)
    {
        // Update commands menu

        // Reset everything
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

        XInput.readOnly = false;
        YInput.readOnly = false;
        ZInput.readOnly = false;
        PInput.readOnly = false;
        RInput.readOnly = false;

        // Activate selected command elements
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
            case (int)CommandHelper.listpv:
                position1Dropdown.gameObject.SetActive(true);
                XInput.gameObject.SetActive(true);
                YInput.gameObject.SetActive(true);
                ZInput.gameObject.SetActive(true);
                PInput.gameObject.SetActive(true);
                RInput.gameObject.SetActive(true);

                XInput.readOnly = true;
                YInput.readOnly = true;
                ZInput.readOnly = true;
                PInput.readOnly = true;
                RInput.readOnly = true;

                // Preview target values
                XInput.placeholder.GetComponent<Text>().text = "X:" + target.GetComponent<TargetModel>().GetPositionInScorbot().x;
                YInput.placeholder.GetComponent<Text>().text = "Y:" + target.GetComponent<TargetModel>().GetPositionInScorbot().y;
                ZInput.placeholder.GetComponent<Text>().text = "Z:" + target.GetComponent<TargetModel>().GetPositionInScorbot().z;
                PInput.placeholder.GetComponent<Text>().text = "P:" + target.GetComponent<TargetModel>().GetPitch().ToString(NUMBER_FORMAT);
                RInput.placeholder.GetComponent<Text>().text = "R:" + target.GetComponent<TargetModel>().GetRoll().ToString(NUMBER_FORMAT);
                break;
            case (int)CommandHelper.teach:
                position1Dropdown.gameObject.SetActive(true);
                XInput.gameObject.SetActive(true);                
                YInput.gameObject.SetActive(true);                
                ZInput.gameObject.SetActive(true);                
                PInput.gameObject.SetActive(true);                
                RInput.gameObject.SetActive(true);

                // Preview target values
                XInput.placeholder.GetComponent<Text>().text = "X:" + target.GetComponent<TargetModel>().GetPositionInScorbot().x;
                YInput.placeholder.GetComponent<Text>().text = "Y:" + target.GetComponent<TargetModel>().GetPositionInScorbot().y;
                ZInput.placeholder.GetComponent<Text>().text = "Z:" + target.GetComponent<TargetModel>().GetPositionInScorbot().z;
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
    }
    
    /**
     * Ejecuta el comando selectionado actualmente en la lista de comandos.
     * @return void
     */
    public void Execute()
    {
        stateMessageControl.NewBlock();

        // Backup file in case a command fails in online
        Save();

        // Execute selected command
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
                    XInput.text = target.GetComponent<TargetModel>().GetPositionInScorbot().x.ToString();
                if (YInput.text.Equals(""))
                    YInput.text = target.GetComponent<TargetModel>().GetPositionInScorbot().y.ToString();
                if (ZInput.text.Equals(""))
                    ZInput.text = target.GetComponent<TargetModel>().GetPositionInScorbot().z.ToString();
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
                if (targetControl.Count() <= 1 || (position1Dropdown.value == position2Dropdown.value))
                {
                    stateMessageControl.WriteMessage("Error. TEACHR Needs 2 different positions", false);
                    return;
                }
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
                // Defp. Last position added
                Transform added = targetControl.GetTarget(targetControl.Count() - 1);
                              
                if (GetOnlineMode())
                {
                    commandControl.SyncSimulationToScorbot(robot, added);
                }
                
                break;
            default:
                break;
        }
        // Backup file 
        Save();
    }

    /**
     * Actualiza una posición (objeto) como el objetivo actual (color violeta).
     * @param t Posición (objeto)
     * @return void
     */
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

    /**
     * Dibuja las trayectorias de los comandos "movel" y "movec".
     * @return void
     */
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
            case (int)CommandHelper.movel: // Draw. Command "movel"
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
            case (int)CommandHelper.movec: // Draw. Command "movec"
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

    /**
     * Borra cualquier trayectoria dibujada.
     * @return void
     */
    private void LineRenderEmpty()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.SetPositions(new Vector3[0]);
    }

    /**
     *Ejecuta el comando "speed" del Scorbot.
     * @param speed Velocidad
     * @return void
     */
    public void SpeedInputEnd(string speed)
    {
        commandControl.Speed(robot, int.Parse(speed));     
        speedText.text = speed;
    }

    /**
     *Ejecuta el comando "speedl" del Scorbot.
     * @param speed Velocidad
     * @return void
     */
    public void SpeedLInputEnd(string speed)
    {
        commandControl.SpeedL(robot, int.Parse(speed));  
        speedLText.text = speed;
    }

    /**
     * Actualiza las lista de los nombres de las posiciones.
     * @param targetsNamesArray Lista de los nombre de las posiciones
     * @return void
     */
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

    /**
     * Activa el Scorbot especificado por su índice.
     * @param index Índice del Scorbot
     * @return void
     */
    public void SetScorbot(int index)
    {
        // Hide all Scorbots
        foreach (IK scorbot in scorbots)
            scorbot.gameObject.SetActive(false);
        // Activate selected Scorbot
        switch(index)
        {
            case ScorbotERIX.INDEX:
                scorbots[ScorbotERIX.INDEX].gameObject.SetActive(true);
                robot = scorbots[ScorbotERIX.INDEX];
                GameController.indexRobot = ScorbotERIX.INDEX;
                break;
            case ScorbotERVPlus.INDEX:
                scorbots[ScorbotERVPlus.INDEX].gameObject.SetActive(true);
                robot = scorbots[ScorbotERVPlus.INDEX];
                GameController.indexRobot = ScorbotERVPlus.INDEX;
                break;
            case ScorbotERIXV2.INDEX:
                scorbots[ScorbotERIXV2.INDEX].gameObject.SetActive(true);
                robot = scorbots[ScorbotERIXV2.INDEX];
                GameController.indexRobot = ScorbotERIXV2.INDEX;
                break;
        }

        ScorbotDel(robot);
    }
        
    /**
     * Controla los cambios en la barra dezlizadora para cambiar entre el modo online y offline.
     * @param value Valor. Debe ser 0 o 1
     * @return void
     */
    public void OnlineOfflineSlider(float value)
    {
        bool previousOnlineMode = onlineMode;
        // Mode Online - Offline
        onlineMode = value == 1f ? true : false;

        onlineText.text = onlineMode? "Online" : "Offline";
                
        bool done = controller.Online_Offline(onlineMode);
        if (done)
        {
            if(!previousOnlineMode && onlineMode)
                stateMessageControl.WriteMessage("Done. Online ONLINE", done);
            else
                stateMessageControl.WriteMessage("Done. Online OFFLINE", done);
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online ONLINE", done);
            onlineMode = false;
            onlineText.text = onlineMode ? "Online" : "Offline";
        }
    }

    /**
     * Obtiene si el modo online está activo
     * @return void
     */
    public bool GetOnlineMode()
    {
        return onlineMode;
    }

    /**
     * Modifica si el modo "From real Scorbot" está activo. En otro caso se usará el modo "From simulation".
     * @param fromSimulation Modo "From simulation" activo
     * @return  void
     */
    public void HereFromSimulation(bool fromSimulation)
    {   
        HereFromDel(fromSimulation);
    }

    /**
     * Modifica si el modo "From simulation to Scorbot" del panel "Sync" está activo. En otro caso se usará el modo 
     * "From Scorbot to simulation".
     * @param fromSimulation Modo "From simulation to Scorbot" activo
     * @return void
     */
    public void SyncFromSimulationToScorbot(bool fromSimulation)
    {
        syncFromSimulationToScorbot = fromSimulation;
    }

    /**
     * Sincroniza todas las posiciones entre el simulador y el Scorbot real. Dependiendo del modo se hace de uno a otro o
     * viceversa.
     * @return void
     */
    public void SyncAllTargets()
    {
        stateMessageControl.NewBlock();

        List<string> namesError = new List<string>();
        if (!GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online ALLSYNC You are offline", false);
            return;
        }

        // Only online mode
        // If a position fails, it continues with others
        if (syncFromSimulationToScorbot)
        {                    
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
    }

    /**
     * Sincroniza una posición entre el simulador y el Scorbot real. Dependiendo del modo se hace de uno a otro o
     * viceversa.
     * @return void
     */
    public void SyncTarget()
    {
        stateMessageControl.NewBlock();

        if (!GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online SYNC You are offline \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }
              
        // Only online mode
        if (syncFromSimulationToScorbot)
        {           
            commandControl.SyncSimulationToScorbot(robot, targetControl.GetTarget(syncTargetDropdown.value));
        }
        else
        {
            commandControl.SyncScorbotToSimulation(robot, targetControl.GetTarget(syncTargetDropdown.value));
        }     
    }

    /**
     * Activa el menú principal.
     * @return void
     */
    public void MainMenu()
    {
        menu.SetActive(true);
        cameraControl.SetIsProcessing(false);
    }

    /**
     * Ejecuta el comando "Con".
     * @return void
     */
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

    /**
     * Guarda los datos de las posiciones actuales en el fichero "backup.txt"
     * @return void
     */
    public void Save()
    {
        backupFileControl.Save();
    }

    /**
     * Carga los datos de las posiciones desde el fichero "backup.txt"
     * @return void
     */
    public void Load()
    {
        backupFileControl.Load();
    }

}