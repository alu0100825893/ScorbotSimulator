using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


/**
 * Permite que los botones se puedan mantener pulsados para ejecutar una acción continuamente. Se utiiza para
 * el control de las articulaciones de Scorbots mediante la ventana "Manual".
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // If pressed
    public bool buttonPressed;
    // Action
    public UnityEvent method;

    /**
     * Evento cuando el ratón mantiene click primario sobre el botón.
     * @param eventData Evento
     * @return void
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    /**
     * Evento cuando el ratón deja de mantiener click primario sobre el botón.
     * @param eventData Evento
     * @return void
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
    }
	
	void Update () {
        // Execute action when button pressed
        if (buttonPressed)
        {         
            method.Invoke();
        }
            
	}
}
