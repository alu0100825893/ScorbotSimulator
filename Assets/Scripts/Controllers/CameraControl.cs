using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es permitir el control de la cámara para hacer posible la navegación 
 * en la simulación mediante el ratón.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class CameraControl : MonoBehaviour {

    private Transform cam;
    
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float rotationSensibity = 4f;
    private float zoomSensivity = 50f;    
    private float movementSensibity = 5f;

    private bool isProcessing = false;

    void Start () {
        cam = GetComponent<GameController>().cam;
        yaw = cam.eulerAngles.y;
        pitch = cam.eulerAngles.x;
    }
	

	void Update () {

        if (!isProcessing)
            return;

        // Mouse right button
        // Camera rotation, 
        if (Input.GetMouseButton(1))
        {
            yaw += rotationSensibity * Input.GetAxis("Mouse X");
            pitch -= rotationSensibity * Input.GetAxis("Mouse Y");

            cam.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Mouse scroll wheel
        // Camera zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            cam.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSensivity));
        }

        // Mouse middle button
        // Camera movement
        if (Input.GetMouseButton(2))
        {
            cam.Translate(new Vector3(-movementSensibity * Input.GetAxis("Mouse X"), -movementSensibity * Input.GetAxis("Mouse Y"), 0f));
        }

    }

    public void SetRotation(float value)
    {
        rotationSensibity = value;  
    }

    public void SetMovement(float value)
    {
        movementSensibity = value;
    }

    public void SetZoom(float value)
    {
        zoomSensivity = value;
    }

    public void SetIsProcessing(bool value)
    {
        isProcessing = value;
    }
}
