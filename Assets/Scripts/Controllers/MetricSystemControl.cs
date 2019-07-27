using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricSystemControl : MonoBehaviour {

    private Transform planeXY;
    private Transform planeXZ;
    private Transform planeYZ;
    private Transform cam;
    private const float SHORT_DISTANCE = 30f;

    private SelectionControl selectionControl;

    void Start () {
        planeXY = GetComponent<GameController>().planeXY;
        planeXZ = GetComponent<GameController>().planeXZ;
        planeYZ = GetComponent<GameController>().planeYZ;
        cam = GetComponent<GameController>().cam;
        selectionControl = GetComponent<SelectionControl>();
    }
	
	void Update () {

        // Selected object is not a target, nothing to do
        GameObject selectedObject = selectionControl.SearchContainTag("Target");        
        if (!selectedObject)
            return;

        // Move plane in order to contain the target and check distance to improve the pression's plane
        DynamicPrecision(selectedObject.transform.root);
    }

    private void DynamicPrecision(Transform selectedObject)
    {                  
        
        Vector3 posSelected = selectedObject.position;

        if (planeXY.gameObject.activeSelf)
        {            
            Vector3 pos = new Vector3(planeXY.position.x, planeXY.position.y, posSelected.z);
            UpdatePlane(selectedObject, planeXY, pos);
        }
        if (planeXZ.gameObject.activeSelf)
        { 
            Vector3 pos = new Vector3(planeXZ.position.x, posSelected.y, planeXZ.position.z);
            UpdatePlane(selectedObject, planeXZ, pos);
        }
        if (planeYZ.gameObject.activeSelf)
        {          
            Vector3 pos = new Vector3(posSelected.x, planeYZ.position.y, planeYZ.position.z);
            UpdatePlane(selectedObject, planeYZ, pos);
        }
    }

    private void UpdatePlane(Transform selectedObject, Transform plane, Vector3 pos)
    {
               
        float distance = Vector3.Distance(cam.transform.position, selectedObject.position);

        plane.position = pos;
        if (distance <= SHORT_DISTANCE) // Camera far from target
        {
            plane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(10f, 10f); // 10 mm each square    
        }
        else // Camera close to target
        {
            plane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f); // 10 cm each square
        }
    }
}
