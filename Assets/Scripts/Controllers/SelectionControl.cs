using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es seleccionar elementos de la simulación a través de un
 * historial de elementos seleccionados. Entre estos elementos están las articulaciones del Scorbot,
 * su pinza y las posiciones creadas. 
 * A su vez, al seleccionar un elemento pueden aparecer axis o flechas curvadas con las que se pueden
 * interactuar para aplicar un desplazamiento o rotación, respectivamente.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class SelectionControl : MonoBehaviour {

    // Controllers
    private GameController gameController;
    private StateMessageControl stateMessageControl;

    // Selected object
    public GameObject selectedObject; 
    // List of selected objects
    public List<GameObject> selectedList;
    private int limitSelectedList = 3;
    // Activate/deactivate movement around axis x,y,z or rotation around articulation planes
    public bool moveInAxis = false;

    // Sensibility variables
    private float axisSensibity = 4f;
    private float axisSensibityReduction = 1f;
    private float rotationSensibity = 4f;
    // Scorbot inner target
    private GameObject innerTarget;
    // Camera
    private Transform cam;
    // Scorbot
    private IK robot;

    // Constants. Tags: Important object in the main scena are tagged. Tags are used to differenciate objects.
    private const int MOUSE_LEFT = 0;
    private const string MOUSE_X = "Mouse X";
    private const string MOUSE_Y = "Mouse Y";
    private const string AXIS_TAG = "Axis";
    private const string ROTATION_TAG = "Rotation";
    private const string X_TAG = "X";    
    private const string Y_TAG = "Y";
    private const string Z_TAG = "Z";    
    private const string TARGET_TAG = "Target";
    private const string INNERAXIS_TAG = "InnerAxis";    
    private const string INNERTARGET_TAG = "InnerTarget";
    private const string UNTAGGED_TAG = "Untagged";

    void Start () {
        // Scorbot inner target
        innerTarget = GetComponent<GameController>().innerTarget;
        // Camera
        cam = GetComponent<GameController>().cam;
        // Scorbot
        robot = GetComponent<GameController>().robot;
        // Controllers
        gameController = GetComponent<GameController>();
        stateMessageControl = GetComponent<StateMessageControl>();
    }
	
	void Update () {
        // Move in axis or rotate in articulation planes         
        if (moveInAxis)
        {
            MoveOrRotate();
        }

        // Mouse left button (pressed)
        // Select object
        if (Input.GetMouseButtonDown(MOUSE_LEFT))
        {
            Select();
        }
        // Mouse left button (no pressed)
        if (Input.GetMouseButtonUp(MOUSE_LEFT))
        {
            moveInAxis = false;
        }   
    }

    /**
     * Proyecta un rayo para saber si hay un objeto apuntado por el puntero del ratón. Si hay algo
     * se selecciona el mismo.
     * @return void
     */
    private void Select()
    {
        // Create ray 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        // Execute ray to hit something
        if (Physics.Raycast(ray, out hitInfo))
        {
            // Get hit object
            GameObject hitObject = hitInfo.transform.gameObject;
            // Search tagged object
            hitObject = SearchTagged(hitObject);
            // Select tagged object
            SelectedObject(hitObject);

        }
        else // Ray doesn't hit anything, select nothing
        {
            ClearSelection();
        }
    }

    /**
	 * Selecciona un objeto y activa sus ejes o ejes de rotación. Si el objeto ya estaba seleccionado
     * se deselecciona al mismo.
	 * @param obj Objeto a seleccionar
	 * @return void
	 */
    public void SelectedObject(GameObject obj)
    {

        if (selectedObject != null)
        {
            // Selected object is Axis or Rotation axis        
            if (obj.tag.Contains(AXIS_TAG) || obj.tag.Contains(ROTATION_TAG))
            {                
                // Activate movement or rotacion
                moveInAxis = true;
                selectedObject = obj;
                return;
            }
                       
            // Selected object is an object already selected
            if (obj == selectedObject)
            {
                ClearSelection();
                return;
            }

        }
        ClearSelection();
        // Add object as selected object
        selectedObject = obj;
        AddSelectedList(selectedObject);

        if (selectedObject != null)
        {
            // Selected object is InnerTarget from Scorbot
            if (obj.tag.Contains(INNERTARGET_TAG))
            {
                moveInAxis = true;
                selectedObject = innerTarget;           
            }
            // Activate object axis
            SetActiveAxis(selectedObject.transform.root, true);            
            // Activate object rotation axis
            if (obj.tag.Contains(ROTATION_TAG))
                SetActiveRotation(selectedObject.transform.parent, true);
            else
                SetActiveRotation(selectedObject.transform, true);
        }
    }

    /**
	 * Desactiva los ejes y ejes de rotación del objeto seleccionado actualmente y también
     * deselecciona al mismo.
	 * @return void
	 */
    private void ClearSelection()
    {
        moveInAxis = false;

        if (selectedObject != null)
        {    
            // Deactivate object axis
            SetActiveAxis(selectedObject.transform.parent, false);
            // Deactivate object rotation axis
            if (selectedObject.tag.Contains(ROTATION_TAG))
                SetActiveRotation(selectedObject.transform.parent, false);
            else
                SetActiveRotation(selectedObject.transform, false);
        }
           
        AddSelectedList(selectedObject);
        selectedObject = null;
    }

    /**
	 * Añade a la lista de objetos seleccionados un nuevo objeto. La lista tiene un límite que si se supera,
     * se elimina el objeto más antiguo y se añade el nuevo objeto.
     * @param selectedObject Objecto seleccionado
	 * @return void
	 */
    private void AddSelectedList(GameObject selectedObject)
    {
       // Do nothing of object is null
        if (selectedObject == null)
            return;
       // Remove oldest selected object from list if list is full 
        if (selectedList.Count >= limitSelectedList)
            selectedList.RemoveAt(0);
 
        selectedList.Add(selectedObject);
    }

    /**
	 * Mueve o rota un objeto dependiento del objeto seleccionado. Si es un eje se produce un movimiento
     * con ayuda de los cambios del ratón, si es un eje de rotación se aplica una rotación con ayuda 
     * de los cambios del ratón.
	 * @return void
	 */
    private void MoveOrRotate()
    {   
        // Parent from selected object. An object can contain axis and a selectable object,
        // these components are contained in a parent object, so to move everything we just move
        // the parent since its children will automatically follow.
        Transform parent = selectedObject.transform.parent.transform;
 
        // Selected object is an Axis
        if (selectedObject.tag.Contains(AXIS_TAG))
        {
            Vector3 startPos = parent.position;
            // Selected object is an Axis X 
            if (selectedObject.tag.Contains(X_TAG))
            {     
                // Angle between camera and object
                float angle = Vector3.Angle(cam.right, parent.right);
                // Move object around axis x, angle determines forward or backward
                if (angle < 90f)
                    parent.Translate(new Vector3(axisSensibityReduction * axisSensibity * 
                        Input.GetAxis(MOUSE_X), 0f, 0f));
                else
                    parent.Translate(new Vector3(axisSensibityReduction * -axisSensibity * 
                        Input.GetAxis(MOUSE_X), 0f, 0f));
            }
            // Selected object is an Axis Y
            if (selectedObject.tag.Contains(Y_TAG))
            {
                // Move object around axis y
                parent.Translate(new Vector3(0f, axisSensibityReduction * axisSensibity * 
                    Input.GetAxis(MOUSE_Y), 0f));
            }
            // Selected object is an Axis Z
            if (selectedObject.tag.Contains(Z_TAG))
            {
                // Angle between camera and object
                float angle = Vector3.Angle(cam.right, parent.forward);
                // Move object around axis z, angle determines forward or backward
                if (angle < 90f)
                    parent.Translate(new Vector3(0f, 0f, axisSensibityReduction * axisSensibity * 
                        Input.GetAxis(MOUSE_X)));
                else
                    parent.Translate(new Vector3(0f, 0f, axisSensibityReduction  * -axisSensibity * 
                        Input.GetAxis(MOUSE_X)));
            }

            // Target (a position) moved. Invalid angles
            if(selectedObject.tag.Contains(TARGET_TAG) && (!startPos.Equals(parent.position)))
            {
                // If it is not a being used by another position (relative), this position not sync anymore
                
                if (parent.GetComponent<TargetModel>().GetRelativeTo() == null)
                {
                    Transform relativePosition = parent.GetComponent<TargetModel>().GetRelativeFrom();
                    parent.GetComponent<TargetModel>().SetSync(false);
                    // if this position is being used by another position (relative), that position is not sync anymore
                    ;
                    if (parent.GetComponent<TargetModel>().GetRelativeFrom())
                    {
                        relativePosition.GetComponent<TargetModel>().SetSync(false);
                        // Updating relative position
                        relativePosition.GetComponent<TargetModel>().UpdateRelativePosition();
                    
                        // Update angles data                    
                        if (robot.TargetInRange(relativePosition))
                        {
                            // Reachable. Load data to target
                            relativePosition.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
                            relativePosition.GetComponent<TargetModel>().SetValid(true);
                        }
                        else // Unreachable
                        {
                            relativePosition.GetComponent<TargetModel>().SetValid(false);
                        }
                    }                    

                    // Check if it's an unreachable point           
                    if (robot.TargetInRange(parent))
                    {
                        // Reachable. Load data to target
                        parent.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
                        parent.GetComponent<TargetModel>().SetValid(true);
                    }
                    else // Unreachable
                    {
                        parent.GetComponent<TargetModel>().SetValid(false);
                    }

                    stateMessageControl.UpdatePositionLog();
                }
                
                else // Moved a relative position, revert position
                {              
                    parent.GetComponent<TargetModel>().UpdateRelativePosition();
                }

                
            }
            // Update trayectory in case a position is moved
            gameController.DrawTrayectory();
        }

        // Selected object is a rotation axis
        if (selectedObject.tag.Contains(ROTATION_TAG))
        {
            // Scorbot articulation
            Articulation art = parent.GetComponent<Articulation>();
            // If selected object parent is an articulation
            if (art != null)
            {
                // If articulation plane is xz, apply rotation
                if (art.GetPlane().Equals(PlaneHelper.XZ))
                    parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis(MOUSE_X));

                // If articulation plane is xy, apply rotation
                if (art.GetPlane().Equals(PlaneHelper.XY))
                {
                    // Angle between camera and object
                    float angle = Vector3.Angle(cam.forward, parent.forward);
                    // Rotate object around plane xy, angle determines forward or backward
                    if (angle < 90f)
                        parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis(MOUSE_X));
                    else
                        parent.GetComponent<Articulation>().Rotate(rotationSensibity * Input.GetAxis(MOUSE_X));

                }

                // If articulation plane is yz, apply rotation
                if (art.GetPlane().Equals(PlaneHelper.YZ))
                {
                    // Angle between camera and object
                    float angle = Vector3.Angle(cam.forward, parent.right);
                    // Rotate object around plane yz, angle determines forward or backward
                    if (angle < 90f)
                        parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis(MOUSE_X));
                    else
                        parent.GetComponent<Articulation>().Rotate(rotationSensibity * Input.GetAxis(MOUSE_X));
                }
            }
        }

        // If selected object is Scorbot InnerAxis (InnerTarget contains InnerAxis)
        if (selectedObject.tag.Contains(INNERAXIS_TAG))
        {
            // Move Scorbot to InnerTarget position
            robot.CCDAlg(innerTarget.transform);
        }
        else // Update InnerTarget position to Scorbot end effector position when is not selected
            innerTarget.transform.parent.transform.position = robot.GetE().position;
    }
    
    /**
	 * Activa todos los ejes del objeto y si es una posición su nombre y coordenadas.
     * @param obj Objeto con ejes
     * @param active Activar/desactivar ejes
	 * @return void
	 */
    public void SetActiveAxis(Transform obj, bool active)
    {
        // If object of null, do nothing
        if (obj == null) 
            return;
        // If object is a position show its name and coordinates
        if(obj.GetComponent<ClampName>())
            obj.GetComponent<ClampName>().SetSelected(active);
        // Activate all axis in object
        for (int i = 0; i < obj.childCount; i++)
        {
            if (obj.GetChild(i).tag.Contains(AXIS_TAG))
            {
                obj.GetChild(i).gameObject.SetActive(active);
            }
        }
    }

    /**
	 * Activa todos los ejes de rotación del objeto.
     * @param obj Objeto con ejes de rotación
     * @param active Activar/desactivar ejes de rotación
	 * @return void
	 */
    private void SetActiveRotation(Transform obj, bool active)
    {
        // Activate all ratacion axis in object
        for (int i = 0; i < obj.childCount; i++)
        {
            if (obj.GetChild(i).tag.Contains(ROTATION_TAG))
            {
                obj.GetChild(i).gameObject.SetActive(active);
            }

        }
    }

    /**
	 * Busca un objeto con etiqueta a partir de otro objeto. Si el objeto tiene etiqueta inicialmente
     * se devuelve el mismo objeto, sino se devuelve uno de sus padres, el primero con etiqueta.
     * @param hitObject Objeto
	 * @return GameObject Objeto con etiqueta
	 */
    private GameObject SearchTagged(GameObject hitObject)
    {
        int i = 0;
        // Search tagget object from hitObject, if not found check parents (up to 3 levels)
        while (i < 3)
        {
            // If object not tagged and it has not a parent
            if (hitObject && hitObject.CompareTag(UNTAGGED_TAG) && hitObject.transform.parent)
                hitObject = hitObject.transform.parent.gameObject;
            else // Stop of object is tagged or it does not have a parent
                break;
        }
        return hitObject;
    }

    /**
	 * Busca un objeto con una etiqueta especifica. Se usa la lista de objetos seleccionados, empezando
     * por el más reciente.
     * @param tag Etiqueta a buscar
	 * @return GameObject Objeto con etiqueta especificada
	 */
    public GameObject SearchContainTag(string tag)
    {
        GameObject gameObject = null;
        int count = selectedList.Count;
        // Search object with specific tag in selectedList (starts with most recent object)
        for(int i = count-1; i >= 0; i--)
        {
            if (selectedList[i] && selectedList[i].tag.Contains(tag))
            {
                gameObject = selectedList[i];
                break;
            }
        }
        return gameObject;
    }

    /**
	 * Modifica la sensibilidad de los ejes x,y,z para mover un objeto.
     * @param value Sensibilidad de los ejes. Debe ser mayor que 0
	 * @return void
	 */
    public void SetAxis(float value)
    {
        axisSensibity = value;
    }

    /**
	 * Modifica la sensibilidad de la rotación los ejes para rotar un objeto.
     * @param value Sensibilidad de la rotación los ejes. Debe ser mayor que 0
	 * @return void
	 */
    public void SetRotation(float value)
    {
        rotationSensibity = value;
    }

    /**
	 * Modifica el multiplicador de la sensibilidad de los ejes para mover un objeto.
     * @param value Multiplicador de la sensibilidad de los ejes. Debe estar entre 0-1
	 * @return void
	 */
    public void SetAxisSensibilityReduction(float value)
    {
        axisSensibityReduction = value;
    }
}