using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripScorbotERIX : MonoBehaviour {
    
    public Transform Grip1;
    public Transform Grip2;

    private float distance = 1.75f;
    private float duration = 1f;
    private bool open = true;
    private bool isProcessing = false;

    public void Open()
    {
        if (!open && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(OpenCoroutine());
        }
    }

    public void Close()
    {
        if (open && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(CloseCoroutine());
        }
    }

    private IEnumerator OpenCoroutine()
    {
        isProcessing = true;

        Vector3 finalPos = Grip1.localPosition + new Vector3(0f, 0f, -distance);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1, finalPos));
        finalPos = Grip2.localPosition + new Vector3(0f, 0f, distance);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2, finalPos));

        yield return move1;
        yield return move2;
        open = true;
        isProcessing = false;
    }

    private IEnumerator CloseCoroutine()
    {
        isProcessing = true;
      
        Vector3 finalPos = Grip1.localPosition + new Vector3(0f, 0f, distance);
        Coroutine move1 = StartCoroutine(MoveCoroutine(Grip1, finalPos));
        finalPos = Grip2.localPosition + new Vector3(0f, 0f, -distance);
        Coroutine move2 = StartCoroutine(MoveCoroutine(Grip2, finalPos));

        yield return move1;
        yield return move2;   
        open = false;
        isProcessing = false;
    }

    private IEnumerator MoveCoroutine(Transform from, Vector3 toPos)
    {        
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            from.localPosition = Vector3.Lerp(from.localPosition, toPos, t);
            yield return null;
        }        
    }
}
