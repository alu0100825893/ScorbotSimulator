using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Permite abrir y cerrar la pinza del Scorbot ER V Plus.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class GripScorbotERVPlus : MonoBehaviour {

    // Grip parts
    public Transform Grip1_1;
    public Transform Grip1_2;
    public Transform Grip2_1;
    public Transform Grip2_2;
    // Constants
    private const float DISTANCE = 2.95f;
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

        Vector3 finalPos = Grip1_1.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1_1, finalPos));
        finalPos = Grip2_1.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2_1, finalPos));

        finalPos = Grip1_2.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move3 = StartCoroutine(MoveCoroutine(Grip1_2, finalPos));
        finalPos = Grip2_2.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move4 = StartCoroutine(MoveCoroutine(Grip2_2, finalPos));
        // Wait
        yield return move1;
        yield return move2;
        yield return move3;
        yield return move4;
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

        Vector3 finalPos = Grip1_1.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1_1, finalPos));
        finalPos = Grip2_1.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2_1, finalPos));

        finalPos = Grip1_2.localPosition + new Vector3(0f, 0f, DISTANCE);
        Coroutine move3 = StartCoroutine(MoveCoroutine(Grip1_2, finalPos));
        finalPos = Grip2_2.localPosition + new Vector3(0f, 0f, -DISTANCE);
        Coroutine move4 = StartCoroutine(MoveCoroutine(Grip2_2, finalPos));

        // Wait
        yield return move1;
        yield return move2;
        yield return move3;
        yield return move4;
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
