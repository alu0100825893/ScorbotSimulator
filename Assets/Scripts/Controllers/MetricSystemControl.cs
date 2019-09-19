using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es proporcionar un sistema de medición a la hora de mover
 * una posición mediante sus axis a través de la visualización de planos. Hay tres planos (“xy”, “xz”, “yz”) 
 * que son activables/desactivables mediante casillas check y también están cuadriculadas. Cada cuadrícula es 
 * de 100 milímetros de lado, aunque si se está lo suficientemente cerca, los planos se subdividen en 
 * cuadrículas de 10 milímetros de lado. También se ajusta la sensibilidad de la camara, los ejes de movimiento del
 * objetivo seleccionado y el tamaño del objetivo.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class MetricSystemControl : MonoBehaviour
{
    // Planes. These planes have their own parent (empty object) which have the initial values pos and rotation,
    // so the planes should have pos 0 0 0, rotation 0 0 0 scale 1 1 1
    private Transform planeXY;
    private Vector3 posPlaneXY; // Needed because of accumulation error in axis Y
    private Transform planeXZ;
    private Transform planeYZ;

    // Camera
    private Transform cam;

    // Constants
    private const float SHORT_DISTANCE = 30f;
    private const float SMALL_SCALE = 0.15f;
    private const float NORMAL_SCALE = 1f;
    private const float TEXTURE_10MM = 10f;
    private const float TEXTURE_10CM = 1f;
    private const float AXIS_SENSIBILITY_REDUCTION = 0.1f;    
    private const float ZOOM_SENSIBILITY_REDUCTION = 0.1f;
    private const float MOVEMENT_SENSIBILITY_REDUCTION = 0.1f;

    private const float SENSIBILITY_REDUCTION_NORMAL = 1f;

    // Controllers
    private SelectionControl selectionControl;
    private CameraControl cameraControl;

    void Start () {
        // Planes
        planeXY = GetComponent<GameController>().planeXY;
        planeXZ = GetComponent<GameController>().planeXZ;
        planeYZ = GetComponent<GameController>().planeYZ;

        // Camera
        cam = GetComponent<GameController>().cam;

        //Controllers
        selectionControl = GetComponent<SelectionControl>();
        cameraControl = GetComponent<CameraControl>();

        // Needed because of accumulation error in axis Y
        posPlaneXY = planeXY.position;
    }
	
	void Update () {

        // Selected object is not a target, nothing to do
        GameObject selectedObject = selectionControl.SearchContainTag("Target");        
        if (!selectedObject)
            return;

        // Update planes in order to improve precision
        DynamicPrecision(selectedObject.transform.root);
    }

    /**
	 * Mantiene tres planos actualizados en base a un objetivo para mejorar la precisión del movimiento de ejes
     * del objeto actuando como objetivo.
	 * @param selectedObject Objetivo
	 * @return void
	 */
    private void DynamicPrecision(Transform selectedObject)
    {
        // Object position
        Vector3 posSelected = selectedObject.position;

        // If planeXY active
        if (planeXY.gameObject.activeSelf)
        {            
            // Plane position with object inside
            Vector3 pos = new Vector3(posPlaneXY.x, posPlaneXY.y, posSelected.z);
            UpdatePlane(selectedObject, planeXY, pos);

            // Flip plane when the camera is on the other side (one side is transparent)
            
            if (cam.position.z > planeXY.position.z)            
                planeXY.localRotation = Quaternion.Euler(0f, 0f, 180f);            
            else            
                planeXY.localRotation = Quaternion.Euler(Vector3.zero);
            
        }

        // If planeXZ active
        if (planeXZ.gameObject.activeSelf)
        {
            // Plane position with object inside
            Vector3 pos = new Vector3(planeXZ.position.x, posSelected.y, planeXZ.position.z);
            UpdatePlane(selectedObject, planeXZ, pos);

            // Flip plane when the camera is on the other side (one side is transparent)
            if (cam.position.y < planeXZ.position.y)
                planeXZ.localRotation = Quaternion.Euler(180f, 0f, 0f);
            else
                planeXZ.localRotation = Quaternion.Euler(Vector3.zero);
        }

        // If planeYZ active
        if (planeYZ.gameObject.activeSelf)
        {
            // Plane position with object inside
            Vector3 pos = new Vector3(posSelected.x, planeYZ.position.y, planeYZ.position.z);
            UpdatePlane(selectedObject, planeYZ, pos);

            // Flip plane when the camera is on the other side (one side is transparent)
            if (cam.position.x < planeYZ.position.x)            
                planeYZ.localRotation = Quaternion.Euler(0f, 0f, 180f);             
            else            
                planeYZ.localRotation = Quaternion.Euler(Vector3.zero);            
        }
    }

    /**
	 * Mueve un plano para que contenga el objetivo. Ajusta las texturas, sensibilidad de la cámara y
     * el movimiento de ejes del objetivo según la cámara este lejos o cerca del objetivo.
	 * @param selectedObject Objetivo que tiene que estar contenido en el plano
     * @param plane Plano a modificar
     * @param pos Posición del plano que contiene al objetivo
	 * @return void
	 */
    private void UpdatePlane(Transform selectedObject, Transform plane, Vector3 pos)
    {
        // Distance between camera and object
        float distance = Vector3.Distance(cam.transform.position, selectedObject.position);

        // Update plane position
        plane.position = pos;

        if (distance <= SHORT_DISTANCE) // Camera close to target
        {
            // 10 mm each square
            plane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(TEXTURE_10MM, TEXTURE_10MM);  
            // Adjust object size
            selectedObject.localScale = new Vector3(SMALL_SCALE, SMALL_SCALE, SMALL_SCALE);
            //Adjust sensibility of camera and axis movement
            selectionControl.SetAxisSensibilityReduction(AXIS_SENSIBILITY_REDUCTION);
            cameraControl.SetMovementSensibilityReduction(MOVEMENT_SENSIBILITY_REDUCTION);
            cameraControl.SetZoomSensibilityReduction(ZOOM_SENSIBILITY_REDUCTION);
        }
        else // Camera far from target
        {
            // 10 cm each square
            plane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(TEXTURE_10CM, TEXTURE_10CM);
            // Adjust object's size
            selectedObject.localScale = new Vector3(NORMAL_SCALE, NORMAL_SCALE, NORMAL_SCALE);
            //Adjust sensibility of camera and axis movement
            selectionControl.SetAxisSensibilityReduction(SENSIBILITY_REDUCTION_NORMAL);
            cameraControl.SetMovementSensibilityReduction(SENSIBILITY_REDUCTION_NORMAL);
            cameraControl.SetZoomSensibilityReduction(SENSIBILITY_REDUCTION_NORMAL);
        }
    }
}