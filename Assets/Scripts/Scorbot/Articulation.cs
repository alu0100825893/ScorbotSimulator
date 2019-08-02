using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Articulation : MonoBehaviour {

    private Vector3 angle;
    private List<float> limit;
    private string plane;

    public int lowerLimitCount;
    public int upperLimitCount;
    public int homeCount;
    private bool countsAreTransformed = false;
    public float degrees;
    private int count;
    private float countToDegrees;
    private float degreesToCount;
    public float offset;

    private float minSpeed;
    private float maxSpeed;
    private int percSpeed = 50; // %
    private float currentSpeed = 0f; 

    public Articulation()
    {
        angle = new Vector3(0f, 0f, 0f);
        limit = new List<float>();
        limit.Add(0f);
        limit.Add(0f);      
        plane = PlaneHelper.XY;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetAngle()
    {
        return angle;
    }
    

    // Seconds
    public float GetTimeToRotate(Vector3 finalAngle)
    {
        float angleToRotate = Angle(finalAngle) - Angle();
        return Mathf.Abs(angleToRotate / currentSpeed);
    }

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

    public string GetPlane()
    {
        return plane;
    }

    // Relative to actual state
    public void Rotate(float beta, bool absolute = false)
    {       
        // Apply limit
        if(absolute)
            beta = ApplyLimit(beta, 0f);
        else
            beta = ApplyLimit(beta, Angle());

        // Apply new angle
        if (absolute)
        {
            angle = BuiltAngle(beta);
            //transform.rotation = Quaternion.Euler(angle);//
            transform.localRotation = Quaternion.Euler(angle);
        }
        else
        {

            angle += BuiltAngle(beta);
            transform.localRotation = Quaternion.Euler(angle);
        }

        //Debug.Log("-------- New : " + angle);          
        //transform.localRotation = Quaternion.Euler(angle);
    }

    // Degrees
    private float ApplyLimit(float angle, float currentAngle)
    {
        float newAngle = angle;
        if (angle + currentAngle > limit[0])
            newAngle = limit[0] - currentAngle;

        if (angle + currentAngle < limit[1])
            newAngle = limit[1] - currentAngle;
        return newAngle;
    }

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


    // Refactor: Setters into one?
    public void SetAngle(Vector3 newAngle)
    {
        angle = newAngle;
        transform.localRotation = Quaternion.Euler(angle);
    }

    public void SetLimit(float max, float min)
    {
        limit.Clear();
        
        {
            limit.Add(max);
            limit.Add(min);
        }

    }

    // Count
    public void SetLimit(int minCount, int maxCount, int homeC, float degrees, float offset = 0f)
    {
        // Special case: minCount < minCount
        if (minCount > maxCount) {
            countsAreTransformed = true;
            lowerLimitCount = maxCount;
            upperLimitCount = minCount;
            homeCount = upperLimitCount + (lowerLimitCount - homeC);            
        }
        else { 
            lowerLimitCount = minCount;
            upperLimitCount = maxCount;
            homeCount = homeC;
        }
        this.degrees = degrees;
        degreesToCount = (Mathf.Abs(maxCount) + Mathf.Abs(minCount)) / degrees;
        countToDegrees = 1 / degreesToCount;
        this.offset = offset;
         
        SetLimit(degrees + offset, offset);
        //Debug.Log(limit[0] + " " + limit[1]);
        //Debug.Log("Home: " + homeCount * countToDegrees);
                
    }

    public void SetPlane(string newPlane)
    {
        plane = newPlane;
    }

    public void SetSpeed(float min, float max)
    {
        minSpeed = min;
        maxSpeed = max;        
        UpdateCurrentSpeed();
    }

    public void SetPercSpeed(int perc)
    {
        percSpeed = perc;
        // Limits: 1- 100
        if (perc > 100)
            perc = 100;
        if (perc < 1)
            perc = 1;

        UpdateCurrentSpeed();
    }

    public int GetSpeed()
    {
        return percSpeed;
    }

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

    public void GoHome(float duration)
    {
        StartCoroutine(MoveCoroutine(GetAngleHome(), duration));        
    }

    public Vector3 GetAngleHome()
    {
        float hDegrees = 0f;
        if (homeCount <= 0)
            hDegrees = (Mathf.Abs(homeCount) + Mathf.Abs(lowerLimitCount)) * countToDegrees;
        else
            hDegrees = (homeCount - lowerLimitCount) * countToDegrees;

        return BuiltAngle(hDegrees + offset);
    }


    public IEnumerator MoveCoroutine(Vector3 finalAngle, float duration)
    {     
        Vector3 startAngle = GetAngle();
        Vector3 endAngle = finalAngle;

        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {    
            Quaternion newRotation = Quaternion.Euler(LerpAngleNoShortest(startAngle, endAngle, t));
            transform.localRotation = newRotation;
            yield return null;
        }
     
        SetAngle(endAngle);

        // Sometimes t skips 1.0, correcting that    
        transform.localRotation = Quaternion.Euler(LerpAngleNoShortest(startAngle, endAngle, 1.0f));
    }

    // Interpolates between angles even if it's not the shortest path
    private Vector3 LerpAngleNoShortest(Vector3 startAngle, Vector3 endAngle, float t)
    {
        float x = Mathf.Lerp(startAngle.x, endAngle.x, t);
        float y = Mathf.Lerp(startAngle.y, endAngle.y, t);
        float z = Mathf.Lerp(startAngle.z, endAngle.z, t);
        return new Vector3(x, y, z);
    }

    public void UpdateAngleAsGlobal(Vector3 angle)
    {        
        float globalAngle = GetGlobalAngle();

        // Global rotation to absolute local rotation 
        float newAngle = Angle() - globalAngle + Angle(angle);
        Rotate(newAngle, true);
    }

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

    // Degrees
    public Vector3 CountToAngle(int count)
    {
        
        // Checking count limits
        count = ApplyCountLimits(count, lowerLimitCount, upperLimitCount);

        // Special case: counts limits inverted
        if (countsAreTransformed)
        {
            // Checking count limits          
            count = upperLimitCount + (lowerLimitCount - count);
        }     

        float degrees = countToDegrees * (count - lowerLimitCount);       
        //Debug.Log(degrees);

        return BuiltAngle(degrees + offset);
    }

    private int ApplyCountLimits(int count, int lowerLimitCount, int upperLimitCount)
    {
        // Checking count limits
        if (count < lowerLimitCount)
            count = lowerLimitCount;
        if (count > upperLimitCount)
            count = upperLimitCount;
        return count;
    }

    public void SetCountsAreTransformed(bool transformed)
    {
        countsAreTransformed = transformed;
    }
    public bool CountsAreTransformed()
    {
        return countsAreTransformed;
    }
}
