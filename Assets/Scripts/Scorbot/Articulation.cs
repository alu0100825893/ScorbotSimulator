using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class Articulation : MonoBehaviour {

    // Scorbot articulation angle. It has 3 values, only one represents the angle
    private Vector3 angle;
    // Angle limit
    private List<float> limit;
    // Articulation plane
    private string plane;

    // Encoder counts limit
    public int lowerLimitCount;
    public int upperLimitCount;
    // Home encoder counts
    public int homeCount;
    // If encoder counts limit max < encoder counts limit min, it needs a transformation so that max > min
    private bool countsAreTransformed = false;
    // Degrees max
    public float degrees;
    // Multiplier. Encoder counts -> degrees
    private float countToDegrees;
    // Multiplier. Degrees -> encoder counts
    private float degreesToCount;
    // Unity offset (initial angle)
    public float offset;

    // Speed limit
    private float minSpeed;
    private float maxSpeed;
    // "Speed". Percentage
    private int percSpeed = 50; // %
    // Angles/second
    private float currentSpeed = 0f; 

    public Articulation()
    {
        // Scorbot articulation angle
        angle = new Vector3(0f, 0f, 0f);
        // Angle limit
        limit = new List<float>();
        limit.Add(0f);
        limit.Add(0f);
        // Articulation plane
        plane = PlaneHelper.XY;
    }

    /**
     * Obtiene las coordenadas del centro de la articulación en el contexto de Unity (no Scorbot real).
     * @return Vector3 Coordenadas
     */
     
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /**
     * Obtiene el ángulo Vector3 de la articulación.
     * @return Vector3 Ángulo
     */
    public Vector3 GetAngle()
    {
        return angle;
    }
            
    /**
     * Obtiene el tiempo (segundos) que tarda la articulación en rotar al ángulo especificado.
     * @finalAngle Ángulo final
     * @return float Tiempo en segundos
     */
    public float GetTimeToRotate(Vector3 finalAngle)
    {
        // Seconds
        float angleToRotate = Angle(finalAngle) - Angle();
        return Mathf.Abs(angleToRotate / currentSpeed);
    }

    /**
     * Obtiene el valor del ángulo de la articulación a partir del Vector3 del ángulo de la articulación. Este
     * Vector3 tiene tres valores de los cuales solo uno representa el ángulo de la articulación, pero con el
     * objetivo de realizar operaciones de los ángulos se ha mantenido el Vector3.
     * @return float Ángulo
     */
    public float Angle()
    {
        float aux = 0f;
        switch (plane)
        {
            case PlaneHelper.XY:
                aux = angle.z;
                break;
            case PlaneHelper.XZ:
                aux = angle.y;
                break;
            case PlaneHelper.YZ:
                aux = angle.x;
                break;
            default:
                aux = 0.0f;
                break;
        }
        return aux;
    }

    /**
     * Obtiene el valor del ángulo de la articulación a partir del Vector3 de un ángulo. Este
     * Vector3 tiene tres valores de los cuales solo uno representa el ángulo de la articulación, pero con el
     * objetivo de realizar operaciones de los ángulos se ha mantenido el Vector3.
     * @param angle Ángulo 
     * @return float Ángulo
     */
    public float Angle(Vector3 angle)
    {
        float aux = 0f;
        switch (plane)
        {
            case PlaneHelper.XY:
                aux = angle.z;
                break;
            case PlaneHelper.XZ:
                aux = angle.y;
                break;
            case PlaneHelper.YZ:
                aux = angle.x;
                break;
            default:
                aux = 0.0f;
                break;
        }
        return aux;
    }

    /**
     * Obtiene el plano sobre el que rota la articulación.
     * @return string Plano 
     */
    public string GetPlane()
    {
        return plane;
    }

  
    /**
     * Rota la articulación una cantidad de grados específica a partir del ángulo actual, o la rotación se hace
     * hasta alcanzar el ángulo específicado (absoluta). En ambos casos se el ángulo se ajusta a los límites 
     * del ángulo.
     * @param beta Ángulo a rotar
     * @param absolute Si es ángulo absoluto (final). En otro caso se considera el ángulo actual
     * @return void
     */
    public void Rotate(float beta, bool absolute = false)
    {       
        // Apply limit
        if(absolute)
            beta = ApplyLimit(beta, 0f);
        else // Relative to actual angle
            beta = ApplyLimit(beta, Angle());

        // Apply new angle
        if (absolute)
        {
            angle = BuiltAngle(beta);        
            transform.localRotation = Quaternion.Euler(angle);
        }
        else // Relative to actual angle
        {
            angle += BuiltAngle(beta);
            transform.localRotation = Quaternion.Euler(angle);
        }      
    }

    // Degrees
    /**
     * Obtiene el ángulo ajustado a los límites de la articulación del Scorbot.
     * @param angle Ángulo
     * @param currentAngle Ángulo actual de la articulación
     * @return float Ángulo
     */
    private float ApplyLimit(float angle, float currentAngle)
    {
        float newAngle = angle;
        // Min limit
        if (angle + currentAngle > limit[0])
            newAngle = limit[0] - currentAngle;
        // Max limit
        if (angle + currentAngle < limit[1])
            newAngle = limit[1] - currentAngle;
        return newAngle;
    }

    /**
     * Obtiene el ángulo Vector3 de la articulación a partir del valor del ángulo de la articulación. La forma en
     * Vector3 se usa para realizar operaciones.
     * @param beta Ángulo
     * @return Vector3 Ángulo
     */
    public Vector3 BuiltAngle(float beta)
    {
        Vector3 angle = Vector3.zero;
        switch (plane)
        {
            case PlaneHelper.XY:
                angle = new Vector3(0f, 0f, beta);
                break;
            case PlaneHelper.XZ:
                angle = new Vector3(0f, beta, 0f);
                break;
            case PlaneHelper.YZ:
                angle = new Vector3(beta, 0f, 0f);
                break;
            default:
                break;
        }
        return angle;
    }


    /**
     * Modifica el ángulo de la articulación a un nuevo ñangulo y lo rota (instantaneo).
     * @param newAngle Ángulo
     * @return void
     */
    public void SetAngle(Vector3 newAngle)
    {
        angle = newAngle;
        transform.localRotation = Quaternion.Euler(angle);
    }

    /**
     * Establece el limite de los ángulos de la articulación.
     * @param max Ángulo máximo
     * @param min Ángulo mínimo
     * @return void
     */
    private void SetLimit(float max, float min)
    {
        limit.Clear();        
        {
            limit.Add(max);
            limit.Add(min);
        }
    }

    /**
     * 
     * @param minCount Conteos de encoder mínimos
     * @param maxCount Conteos de encoder máximos
     * @param homeC Conteos de encoder en las posición HOME del Scorbot
     * @param degrees Grados máximos de la articulación
     * @param offset Desface para Unity
     * @return void
     */
    public void SetLimit(int minCount, int maxCount, int homeC, float degrees, float offset = 0f)
    {
        // Special case: minCount > maxCount, it needs a tranformation
        if (minCount > maxCount) {
            countsAreTransformed = true;
            lowerLimitCount = maxCount;
            upperLimitCount = minCount;
            homeCount = upperLimitCount + (lowerLimitCount - homeC);            
        }
        else { // minCount < maxCount
            lowerLimitCount = minCount;
            upperLimitCount = maxCount;
            homeCount = homeC;
        }
        // Max degrees
        this.degrees = degrees;
        // Mutipliers
        degreesToCount = (Mathf.Abs(maxCount) + Mathf.Abs(minCount)) / degrees;
        countToDegrees = 1 / degreesToCount;
        // Unity offset
        this.offset = offset;
         // Angle limit
        SetLimit(degrees + offset, offset);             
    }

    /**
     * Modifica el plano en el que rota la articulación.
     * @param newPlane Plano
     * @return void
     */
    public void SetPlane(string newPlane)
    {
        plane = newPlane;
    }

    /**
     * Modifica el límite de la velocidad de rotación de la articulación.
     * @param min
     * @param max
     * @return void
     */
    public void SetSpeed(float min, float max)
    {
        minSpeed = min;
        maxSpeed = max;        
        UpdateCurrentSpeed();
    }

    /**
     * Modifica el porcentaje de la velocidad que va a utilizar la articulación al rotar.
     * @param perc Porcentage de velocidad. 1-100%
     * @return void
     */
    public void SetPercSpeed(int perc)
    {
        percSpeed = perc;
        // Limits: 1-100
        if (perc > 100)
            perc = 100;
        if (perc < 1)
            perc = 1;

        UpdateCurrentSpeed();
    }

    /**
     * Obtiene el porcentaje de la velocidad que está usando la articulación al rotar.
     * @return int Porcentaje
     */
    public int GetSpeed()
    {
        return percSpeed;
    }

    /**
     * Calcula la velocidad de rotación de la articulación con su porcentaje (grados/segundos).
     * @return void
     */
    private void UpdateCurrentSpeed()
    {
        switch(percSpeed)
        {
            case 1:
                currentSpeed = minSpeed;
                break;
            case 100:
                currentSpeed = maxSpeed;
                break;
            default:
                currentSpeed = minSpeed + ((maxSpeed - minSpeed) * percSpeed / 100f);
                break;
        }
    }

    /**
     * Mueve el Scorbot a su posición HOME en el tiempo especificado (simulación).
     * @param duration Duración (segundos)
     * @return void
     */
    public void GoHome(float duration)
    {
        StartCoroutine(MoveCoroutine(GetAngleHome(), duration));        
    }

    /**
     * Obtiene el ángulo de la articulación en la posición HOME en forma de Vector3.
     * @return Vector3 Ángulo
     */
    public Vector3 GetAngleHome()
    {
        float hDegrees = 0f;
        if (homeCount <= 0)
            hDegrees = (Mathf.Abs(homeCount) + Mathf.Abs(lowerLimitCount)) * countToDegrees;
        else
            hDegrees = (homeCount - lowerLimitCount) * countToDegrees;

        return BuiltAngle(hDegrees + offset);
    }

    /**
     * Inicia la rotación en la articulación para que se complete en el tiempo especificado.
     * @param finalAngle Ángulo final
     * @param duration Duración (segundos)
     * @return IEnumerator Proceso en segundo plano
     */
    public IEnumerator MoveCoroutine(Vector3 finalAngle, float duration)
    {     
        Vector3 startAngle = GetAngle();
        Vector3 endAngle = finalAngle;

        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {    
            Quaternion newRotation = Quaternion.Euler(LerpAngleNoShortest(startAngle, endAngle, t));
            transform.localRotation = newRotation;
            // Wait until nnext frame
            yield return null; 
        }
     
        SetAngle(endAngle);

        // Sometimes t skips 1.0, correcting that    
        transform.localRotation = Quaternion.Euler(LerpAngleNoShortest(startAngle, endAngle, 1.0f));
    }

    /**
     * Obtiene el ángulo del momento t al interpolar entre un ángulo inicial y otro final.
     * @param startAngle Ángulo inicial
     * @param endAngle Ángulo final
     * @param t Momento. Debe estar entre 0-1
     * @return Vector3 Ángulo
     */
    private Vector3 LerpAngleNoShortest(Vector3 startAngle, Vector3 endAngle, float t)
    {
        // Interpolates between angles even if it's not the shortest path
        float x = Mathf.Lerp(startAngle.x, endAngle.x, t);
        float y = Mathf.Lerp(startAngle.y, endAngle.y, t);
        float z = Mathf.Lerp(startAngle.z, endAngle.z, t);
        return new Vector3(x, y, z);
    }

    /**
     * Modifica el ángulo de la articulación para que se mantenga constante globalmente. El ángulo global es 
     * el ángulo que la articulación forma con el plano horizontal del centro de la articulación. Mientras que
     * el ángulo local es el ángulo que se forma con respecto a la articulación anterior.?
     * @param angle
     * @return void
     */
    public void UpdateAngleAsGlobal(Vector3 angle)
    {        
        float globalAngle = GetGlobalAngle();

        // Global rotation to absolute local rotation 
        float newAngle = Angle() - globalAngle + Angle(angle);
        Rotate(newAngle, true);
    }

    /**
     * Obtiene el ángulo global (0:360 grados) a partir del ángulo (local) de la articulación (0:180 -180:0 grados).?
     * @return float Ángulo (0:360 grados)
     */
    public float GetGlobalAngle()
    {
        // Global rotation to local. Full global rotation is 0:360, but full local rotation is 0:180 and -180:0
        float globalAngle = Angle(transform.rotation.eulerAngles);
        if (globalAngle > 180f)
        {
            globalAngle = globalAngle - 360f;
        }
        return globalAngle;
    }
       
    /**
     * Obtiene el ángulo de la articulación a partir de los conteos de encoder. Los conteos se ajustan a los 
     * limites del encoder para la articulación.
     * @param count Counteos de encoder
     * @return Vector3 Ángulo
     */
    public Vector3 CountToAngle(int count)
    {
        // Degrees
        // Checking count limits
        count = ApplyCountLimits(count, lowerLimitCount, upperLimitCount);

        // Special case: counts limits inverted
        if (countsAreTransformed)
        {
            // Checking count limits          
            count = upperLimitCount + (lowerLimitCount - count);
        }     
        float degrees = countToDegrees * (count - lowerLimitCount);       

        return BuiltAngle(degrees + offset);
    }

    /**
     * Obtiene los conteos de encoder ajustados a los límites para la articulación.
     * @param count Conteos de encoder
     * @param lowerLimitCount Limite inferior de los conteos de encoder
     * @param upperLimitCount Limite superior de los conteos de encoder
     * @return int Conteos de encoder
     */
    private int ApplyCountLimits(int count, int lowerLimitCount, int upperLimitCount)
    {
        // Checking count limits
        if (count < lowerLimitCount)
            count = lowerLimitCount;
        if (count > upperLimitCount)
            count = upperLimitCount;
        return count;
    }

    /**
     * Modifica si los conteos de encoder han sido transformados.
     * @param transformed Counteos de encoder transformados
     * @return void
     */
    public void SetCountsAreTransformed(bool transformed)
    {
        countsAreTransformed = transformed;
    }

    /**
     * Obtiene si los conteos de encoder han sido transformados.
     * @return bool Counteos de encoder transformados
     */
    public bool CountsAreTransformed()
    {
        return countsAreTransformed;
    }
}
