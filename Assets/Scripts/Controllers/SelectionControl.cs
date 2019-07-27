using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SelectionControl : MonoBehaviour {

    private GameController gameController;
    private StateMessageControl stateMessageControl;

    public GameObject selectedObject;   
    public List<GameObject> selectedList;
    private int limitSelectedList = 3;

    public bool moveInAxis = false;

    private float axisSensibity = 4f;
    private float rotationSensibity = 4f;
    private GameObject innerTarget;
    private Transform cam;
    private IK robot;

    void Start () {
        innerTarget = GetComponent<GameController>().innerTarget;
        cam = GetComponent<GameController>().cam;
        robot = GetComponent<GameController>().robot;
        gameController = GetComponent<GameController>();
        stateMessageControl = GetComponent<StateMessageControl>();
    }
	
	void Update () {
        // Move in axis      
        // TODO: Move with mouse paralelly 
        if (moveInAxis)
        {
            MoveOrRotate();

        }

        // Mouse left button (pressed)
        // Select object
        if (Input.GetMouseButtonDown(0))
        {
            Select();
        }
        if (Input.GetMouseButtonUp(0))
        {
            moveInAxis = false;
        }
    }

    private void Select()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            //Debug.Log("Mouse: " + hitInfo.collider.name);

            GameObject hitObject = hitInfo.transform.gameObject;

            hitObject = SearchTagged(hitObject);

            SelectedObject(hitObject);

        }
        else
        {
            ClearSelection();
        }
    }

    public void SelectedObject(GameObject obj)
    {

        if (selectedObject != null)
        {
            // Selected axis
            //Debug.Log("HERE" + obj.transform.tag + " " + obj);
            if (obj.tag.Contains("Axis") || obj.tag.Contains("Rotation"))
            {
                moveInAxis = true;
                selectedObject = obj;
                return;
            }



            // Selected object
            if (obj == selectedObject)
            {
                ClearSelection();
                return;
            }

        }
        ClearSelection();

        selectedObject = obj;
        AddSelectedList(selectedObject);

        if (selectedObject != null)
        {
            //
            if (obj.tag.Contains("InnerTarget"))
            {
                moveInAxis = true;
                selectedObject = innerTarget;
                //return;
            }
            //if (obj.tag.Contains("Axis"))
            SetActiveAxis(selectedObject.transform.root, true);            

            if (obj.tag.Contains("Rotation"))
                SetActiveRotation(selectedObject.transform.parent, true);
            else
                SetActiveRotation(selectedObject.transform, true);
        }
    }

    private void ClearSelection()
    {
        moveInAxis = false;

        if (selectedObject != null)
        {
            //if (selectedObject.tag.Contains("Axis"))
            SetActiveAxis(selectedObject.transform.parent, false);
            if (selectedObject.tag.Contains("Rotation"))
                SetActiveRotation(selectedObject.transform.parent, false);
            else
                SetActiveRotation(selectedObject.transform, false);
        }
        //prevSelectedObject = selectedObject;
       
        AddSelectedList(selectedObject);

        selectedObject = null;

    }

    private void AddSelectedList(GameObject selectedObject)
    {
       
        if (selectedObject == null)
            return;

        
        if (selectedList.Count >= limitSelectedList)
            selectedList.RemoveAt(0);

        selectedList.Add(selectedObject);

    }

    private void MoveOrRotate()
    {   
        Transform parent = selectedObject.transform.parent.transform;
 
        if (selectedObject.tag.Contains("Axis"))
        {
            Vector3 startPos = parent.position;

            if (selectedObject.tag.Contains("X"))
            {     
                float angle = Vector3.Angle(cam.right, parent.right);
                
                if (angle < 90f)
                    parent.Translate(new Vector3(axisSensibity * Input.GetAxis("Mouse X"), 0f, 0f));
                else
                    parent.Translate(new Vector3(-axisSensibity * Input.GetAxis("Mouse X"), 0f, 0f));

            }

            if (selectedObject.tag.Contains("Y"))
            {
                parent.Translate(new Vector3(0f, axisSensibity * Input.GetAxis("Mouse Y"), 0f));
            }

            if (selectedObject.tag.Contains("Z"))
            {
                float angle = Vector3.Angle(cam.right, parent.forward);
                if (angle < 90f)
                    parent.Translate(new Vector3(0f, 0f, axisSensibity * Input.GetAxis("Mouse X")));
                else
                    parent.Translate(new Vector3(0f, 0f, -axisSensibity * Input.GetAxis("Mouse X")));
            }

            // Target moved. Invalid angles
            if(selectedObject.tag.Contains("Target") && (!startPos.Equals(parent.position)))
            {
                parent.GetComponent<TargetModel>().SetSync(false);
                stateMessageControl.UpdatePositionLog();
                //parent.GetComponent<TargetModel>().SetAngles(null);
                // Check if it's an unreachable point           
                if (robot.TargetInRange(parent))
                {
                    // Load data to target
                    parent.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
                    parent.GetComponent<TargetModel>().SetValid(true);
                }
                else
                {
                    parent.GetComponent<TargetModel>().SetValid(false);
                }
                
            }

            gameController.DrawTrayectory();
        }

        if (selectedObject.tag.Contains("Rotation"))
        {
            Articulation art = parent.GetComponent<Articulation>();
            if (art != null)
            {
                if (art.GetPlane().Equals("xz"))
                    parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis("Mouse X"));
                if (art.GetPlane().Equals("xy"))
                {
                    //float angle = Vector3.SignedAngle(cam.transform.forward, parent.forward, parent.up);
                    //if (angle < 0f)
                    float angle = Vector3.Angle(cam.forward, parent.forward);

                    if (angle < 90f)
                        parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis("Mouse X"));
                    else
                        parent.GetComponent<Articulation>().Rotate(rotationSensibity * Input.GetAxis("Mouse X"));

                }
                if (art.GetPlane().Equals("yz"))
                {               
                    float angle = Vector3.Angle(cam.forward, parent.right);
                    if (angle < 90f)
                        parent.GetComponent<Articulation>().Rotate(-rotationSensibity * Input.GetAxis("Mouse X"));
                    else
                        parent.GetComponent<Articulation>().Rotate(rotationSensibity * Input.GetAxis("Mouse X"));
                }
            }
        }

        if (selectedObject.tag.Contains("InnerAxis"))
        {
            robot.CCDAlg(innerTarget.transform);

        }
        else
            innerTarget.transform.parent.transform.position = robot.GetE().position;
    }



    public void SetActiveAxis(Transform obj, bool active)
    {
        if (obj == null) // Because error when null in ClearSelection
            return;
        //
        if(obj.GetComponent<ClampName>())
            obj.GetComponent<ClampName>().SetSelected(active);

        for (int i = 0; i < obj.childCount; i++)
        {
            if (obj.GetChild(i).tag.Contains("Axis"))
            {
                obj.GetChild(i).gameObject.SetActive(active);
            }
        }
    }

    private void SetActiveRotation(Transform obj, bool active)
    {

        for (int i = 0; i < obj.childCount; i++)
        {
            if (obj.GetChild(i).tag.Contains("Rotation"))
            {
                obj.GetChild(i).gameObject.SetActive(active);
            }
        }
    }

    private GameObject SearchTagged(GameObject hitObject)
    {
        int i = 0;
        while (i < 3)
        {

            if (hitObject && hitObject.CompareTag("Untagged") && hitObject.transform.parent)
                hitObject = hitObject.transform.parent.gameObject;
            else
                break;
        }
        return hitObject;
    }

    public GameObject SearchContainTag(string tag)
    {
        GameObject gameObject = null;
        int count = selectedList.Count;
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

    public void SetAxis(float value)
    {
        axisSensibity = value;
    }

    public void SetRotation(float value)
    {
        rotationSensibity = value;
    }
}