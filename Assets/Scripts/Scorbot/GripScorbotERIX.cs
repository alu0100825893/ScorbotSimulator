using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Permite abrir y cerrar la pinza del Scorbot ER IX.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class GripScorbotERIX : MonoBehaviour {
    // Grip parts
    public Transform Grip1;
    public Transform Grip2;
    // Constants
    private const float DISTANCE = 1.75f;
    private const float DURATION = 1f;
    // If grip open
    private bool open = true;
    // If its closing or opening
    private bool isProcessing = false;

    /**
     * Abre la pinza.
     * @return void
     */
    public void Open()
    {
        if (!open && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(OpenCoroutine());
        }
    }

    /**
     * Cierra la pinza.
     * @return void
     */
    public void Close()
    {
        if (open && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(CloseCoroutine());
        }
    }

    /**
     * Inicia el proceso de abrir la pinza.
     * @return IEnumerator Proceso en segundo plano
     */
    private IEnumerator OpenCoroutine()
    {
        isProcessing = true;

        Vector3 finalPos = Grip1.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1, finalPos));
        finalPos = Grip2.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2, finalPos));
        // Wait
        yield return move1;
        yield return move2;
        // After waiting
        open = true;
        isProcessing = false;
    }

    /**
     * Inicia el proceso de cerrar la pinza.
     * @return IEnumerator Proceso en segundo plano
     */
    private IEnumerator CloseCoroutine()
    {
        isProcessing = true;
      
        Vector3 finalPos = Grip1.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1, finalPos));
        finalPos = Grip2.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2, finalPos));
        // Wait
        yield return move1;
        yield return move2;
        // After waiting
        open = false;
        isProcessing = false;
    }

    /**
     * Inicia el proceso de mover una pieza hacia una posición.
     * @param from Pieza
     * @param toPos Posición
     * @return IEnumerator Proceso en segundo plano
     */
    private IEnumerator MoveCoroutine(Transform from, Vector3 toPos)
    {        
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / DURATION)
        {
            from.localPosition = Vector3.Lerp(from.localPosition, toPos, t);
            yield return null;
        }        
    }
}
