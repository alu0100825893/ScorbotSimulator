using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es permitir el control de la cámara para hacer posible la navegación 
 * en la simulación mediante el ratón. Controles: Ver Menú principal->Help->Controls
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class CameraControl : MonoBehaviour {
    // Camera
    private Transform cam;
    
    // Camera variebles
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    // Sensibility variables
    private float rotationSensibity = 4f;
    private float zoomSensibity = 50f;    
    private float zoomSensibityReduction = 1f; // 0 to 1
    private float movementSensibity = 5f;
    private float movementSensibityReduction = 1f; // 0 to 1

    // Activate/deactivate
    private bool isProcessing = false;

    void Start () {
        // Camera
        cam = GetComponent<GameController>().cam;
        // Camera. Initial state
        yaw = cam.eulerAngles.y;
        pitch = cam.eulerAngles.x;
    }

    // Controls: See Main menu->Help->Controls
    void Update () {
        // Activate/deactivate
        if (!isProcessing)
            return;

        // Mouse right button
        // Camera rotation
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
            cam.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSensibityReduction * zoomSensibity));
        }

        // Mouse middle button
        // Camera movement
        if (Input.GetMouseButton(2))
        {
            cam.Translate(new Vector3(movementSensibityReduction * -movementSensibity * Input.GetAxis("Mouse X"),
                movementSensibityReduction * -movementSensibity * Input.GetAxis("Mouse Y"), 0f));
        }
    }

    /**
     * Modifica la velocidad de rotación de la camara. 
     * @param value Velocidad de rotación. Debe ser mayor que 0
     * @return void
     */
    public void SetRotation(float value)
    {
        rotationSensibity = value;  
    }

    /**
     * Modifica la velocidad de desplazamiento de la camara. Izquierda, derecha, arriba y abajo.
     * @param value Velocidad de desplazamiento. Debe ser mayor que 0
     * @return void
     */
    public void SetMovement(float value)
    {
        movementSensibity = value;
    }

    /**
     * Modifica la velocidad del zoom la camara. Adelante y atrás.
     * @param value Velocidad del zoom. Debe ser mayor que 0
     * @return void
     */
    public void SetZoom(float value)
    {
        zoomSensibity = value;
    }

    /**
     * Permite activar/desactivar el control de la cámara.
     * @param value Control de la camara activo. Activo = true
     * @return void
     */
    public void SetIsProcessing(bool value)
    {
        isProcessing = value;
    }

    /**
     * Multiplicador que modifica la velocidad de desplazamiento de la camara. Izquierda, derecha, arriba y abajo.
     * @param value Velocidad de desplazamiento. Debe estar entre 0-1. Velocidad normal: 1
     * @return void
     */
    public void SetMovementSensibilityReduction(float value)
    {
        movementSensibityReduction = value;
    }

    /**
     * Multiplicador que modifica la velocidad del zoom la camara. Adelante y atrás.
     * @param value Velocidad del zoom. Debe estar entre 0-1. Velocidad normal: 1
     * @return void
     */
    public void SetZoomSensibilityReduction(float value)
    {
        zoomSensibityReduction = value;
    }
}
