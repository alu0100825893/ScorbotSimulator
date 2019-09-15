using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IK : MonoBehaviour {

    private ScorbotModel scorbotModel;

    private Transform D;
    private Transform E;
    private Articulation[] articulations;

    private Articulation[] art;
    private Transform artE;

    private int MAX_ITINERATIONS = 1500;
    private const float EPSILON = 0.01f; //0.02 0.01 
    
    private bool stepByStep = false;

    private bool isProcessing = false;

    private int speedMillimeter = 50;

    
    void Start() {
        // Scorbot model access
        scorbotModel = GetComponent<ScorbotModel>();
        D = scorbotModel.D;
        E = scorbotModel.E;
        articulations = scorbotModel.articulations;
        // Scorbot model copy
        scorbotModel.InitToCopy(out art,out artE);               
    }
    
	void Update () {

	}
    
    private void OnDrawGizmos()
    {
        if (!D)
            return;
        Gizmos.color = Color.blue;     
        Gizmos.DrawLine(E.position, D.position);
    }    
    
    // move
    public void CCDAlg(Transform D)
    {
        if (!isProcessing)
        {
            scorbotModel.UpdateToCopy(art);
            StartCoroutine(IKAlg(art, D, artE));
        }
    }

    // movel movec
    public void CCDAlg(Transform[] trajectory, bool speedDegrees = true)
    {
        if (!isProcessing)
        {
            scorbotModel.UpdateToCopy(art);
            StartCoroutine(CCDAlgMulti(art, trajectory, artE, speedDegrees));
        }
    }

    // isProcessing no valid
    private IEnumerator CCDAlgMulti(Articulation[] articulations, Transform[] trayectory, Transform E, bool speedDegrees = false)
    {
        
        for (int i = 0; i < trayectory.Length; i++)
        {           
            yield return StartCoroutine(IKAlg(articulations, trayectory[i], E, speedDegrees));            
        }

        // Destroy objects, skip last one, target        
        for (int i = 0; i < trayectory.Length - 1; i++)
            Destroy(trayectory[i].gameObject);        
    }
    

    private IEnumerator IKAlg(Articulation[] articulations,Transform D, Transform E, bool speedDegrees = true)
    {
        isProcessing = true;

        float duration = 0f;
        if (!speedDegrees)
        {            
            duration = CalculateTimeSpeedL(E.position, D.position, speedMillimeter);
        }
        //Debug.Log("time: " + duration);
        //Debug.Log("Dist: " + Vector3.Distance(E.position, D.position));

        AlgCDD(articulations, D, E);

        //Debug.Log("Moving " + speedDegrees);
        
        if (speedDegrees)
        {
            List<Vector3> endAngles = new List<Vector3>();
            foreach (Articulation art in articulations)
                endAngles.Add(art.GetAngle());
            duration = CalculateTimeSpeed(this.articulations, endAngles);
        }        
        

        //Debug.Log("time: " + duration);
        //Debug.Log("Dist: " + Vector3.Distance(E.position, D.position));


        Coroutine[] moves = new Coroutine[articulations.Length];

        for (int i = 0; i < articulations.Length; i++)
        {                     
            moves[i] = StartCoroutine(this.articulations[i].MoveCoroutine(articulations[i].GetAngle(), duration));
        }

        // Wait moves
        for (int i = 0; i < articulations.Length; i++)
        {
            yield return moves[i];
        }
        
        isProcessing = false;
    }

    public void Move(Transform target)
    {
        
        if(!isProcessing)
        {
            isProcessing = true;      
            float duration = CalculateTimeSpeed(this.articulations, target.GetComponent<TargetModel>().GetAngles());

            Coroutine[] moves = new Coroutine[articulations.Length];
            for (int i = 0; i < articulations.Length; i++)
            {
                moves[i] = StartCoroutine(this.articulations[i].MoveCoroutine(target.GetComponent<TargetModel>().GetAngles(i), duration));
            }
            StartCoroutine(MoveCoroutive(moves));
        }        
        
    }
    private IEnumerator MoveCoroutive(Coroutine[] moves)
    {
        // Wait moves
        for (int i = 0; i < articulations.Length; i++)
        {
            yield return moves[i];
        }
        //
        isProcessing = false;
    }

    private void AlgCDD(Articulation[] articulations, Transform D, Transform E, bool skipPitch = false)
    {
        // Skip pitch
        int pitch = 0;
        Vector3 pitchAngle = Vector3.zero;
        if (skipPitch)
        {            
            pitch = 1;
            pitchAngle = D.GetComponent<TargetModel>().GetAngles()[3];
        }

        float lastDistance = Mathf.Infinity;
        for (int i = 0; i < MAX_ITINERATIONS; i++)
        {
            //Debug.Log("IT" + i);
            
            //if (stepByStep)
            //    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            //StartCoroutine(CCD(articulations, D, E, articulations[0]));
            CCD(articulations, D, E, articulations[0]);

            // Doing Base before and avoiding Roll
            for (int artIndex = articulations.Length - 1 - 1 - pitch; artIndex >= 1; artIndex--)
            {
                //if (stepByStep)
                //    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                //StartCoroutine(CCD(articulations,D, E, articulations[artIndex]));
                CCD(articulations, D, E, articulations[artIndex]);

                // Update pitch as global
                if (skipPitch)
                {
                    //articulations[3].UpdateAngleAsGlobal(new Vector3(0f ,0f ,45f));
                    articulations[3].UpdateAngleAsGlobal(pitchAngle);
                }
            }

            //Debug.Log("Distance in IT " + i + ": " + Vector3.Distance(D.position, E.position));

            float newDistance = Vector3.Distance(D.position, E.position);

            if (newDistance < EPSILON || (lastDistance == newDistance)) // epsilon in == distance?
            //if (newDistance < EPSILON) // epsilon in == distance?
            {
                //Debug.Log("DONE" + newDistance + " it:" + i);
                return;
            }
            
            lastDistance = newDistance;
        }
        //Debug.Log(" it:" + MAX_ITINERATIONS);

    }
    
    //
    private float CalculateTimeSpeed(Articulation[] startArt, List<Vector3> endAngles)
    {
        float duration = 0f;
        // Max time
        for (int i = 0; i < startArt.Length; i++)
        {
            float aux = startArt[i].GetTimeToRotate(endAngles[i]);
            if (duration < aux)
                duration = aux;
        }
        return duration;
    }

    private float CalculateTimeSpeedL(Vector3 startPoint, Vector3 endPoint, float speedMillimeter)
    {
        return (Vector3.Distance(startPoint, endPoint) * 10f) / (float)speedMillimeter;
    }   

    private void CCD(Articulation[] articulations, Transform D, Transform E, Articulation R)
    {
        // Calculate angle. Radians  
        float beta1 = 0.0f;
        float beta2 = 0.0f;

        // Vertical plane
        if (R.GetPlane().Equals(PlaneHelper.XY))
        {
            Vector3 rLocalPos = Vector3.zero;
            Vector3 dLocalPos = GetLocalPosition(D, articulations[1].transform);
            Vector3 eLocalPos = GetLocalPosition(E, articulations[1].transform);
                    
            if (!articulations[1].Equals(R))            
                rLocalPos = GetLocalPosition(R.transform, articulations[1].transform);             
            else
                rLocalPos = Vector3.zero;

            //Debug.Log("r: " + rPos);

            beta1 = CalculateAngle(dLocalPos, rLocalPos, R.GetPlane());        
            beta2 = CalculateAngle(eLocalPos , rLocalPos, R.GetPlane());            
        }
        // Horizontal plane
        else
        {
            beta1 = CalculateAngle(D.position, R.GetPosition(), R.GetPlane());
            beta2 = CalculateAngle(E.position, R.GetPosition(), R.GetPlane());
        }
      
        float beta = 0.0f;
        switch(R.GetPlane())
        {
            case PlaneHelper.XY:
                beta = (beta1 - beta2);
                break;
            case PlaneHelper.XZ:
                beta = (beta2 - beta1);
                break;
        } 
        
        // What if there is another one like O0?
        // XY or XZ?
        // >= <= ??
        /*
        if ((Mathf.Abs(articulations[0].GetAngle().y) > 90f) && (Mathf.Abs(articulations[0].GetAngle().y) < 270f)) // Negative zone
            beta = (beta2 - beta1);
        */

        // Shortest path
        if(R.GetPlane().Equals(PlaneHelper.XY)) {
            while (beta > Mathf.PI)
                beta -= 2 * Mathf.PI;

            while (beta < -Mathf.PI)
                beta += 2 * Mathf.PI;
        }

        //Debug.Log("beta: " + beta);

        // Degrees
        beta = beta * Mathf.Rad2Deg;
        //Debug.Log("beta12: " + beta); 
               

        // Apply angle
        R.Rotate(beta);

        //yield return null;
    }

    // Radians
    private float CalculateAngle(Vector3 end, Vector3 start, string plane)
    {
        Vector3 v = end - start;
        v = v.normalized;
  
        switch(plane)
        {
            case PlaneHelper.XY:
                return Mathf.Atan2(v.y, v.x);                
            case PlaneHelper.XZ:
                return Mathf.Atan2(v.z, v.x);              
            default: return 0.0f;
        } // Add ZY return Mathf.Atan2(v.y, v.z);  ?
    }

    private Vector3 GetLocalPosition(Transform obj, Transform origin)
    {
        return origin.transform.InverseTransformPoint(obj.position);
    }

    public bool TargetInRange(Transform D, bool skipPitch = false)
    {
        bool inRange = false;
        
        for (int i = 0; i < 1; i++) // only one
        {
            switch (i)
            {
                case 0:
                    // Seems like real robot does this, not from current state
                    //scorbotModel.UpdateToCopy(art);
                    SetAnglesCopyHome();
                    break;
                case 1:
                    SetAnglesCopyHome();
                    break;
                case 2:
                    SetAnglesCopyZero();
                    break;
            }
            
            //scorbotModel.UpdateToCopy(art);
            if (skipPitch) // Load restrictions from target. Pitch and roll
            {
                art[3].SetAngle(D.GetComponent<TargetModel>().GetAngles(3));
                art[4].SetAngle(D.GetComponent<TargetModel>().GetAngles(4));
            }

            AlgCDD(art, D, artE, skipPitch);

            // Target close enough?
            //Debug.Log(Vector3.Distance(D.position, artE.position));
            if (Vector3.Distance(D.position, artE.position) < EPSILON) {
                if (skipPitch )
                {
                    float pitchRequired = D.GetComponent<TargetModel>().GetAngles(3).z;
                    if ((Mathf.Abs(pitchRequired - art[3].GetGlobalAngle())) < EPSILON) {
                        // Succeed if pitch has same global angle

                        inRange = true;
                        break;
                    }
                }
                else
                {
                    inRange = true;
                    break;
                }
            }

        }
        return inRange;
    }

    public void Home(float duration = 1f)
    {      
        for (int artIndex = articulations.Length - 1; artIndex >= 0; artIndex--)
        {
            articulations[artIndex].GoHome(duration);
        }
      
        /*
        articulations[0].CountToAngle(-13541);
        articulations[1].CountToAngle(-22691);
        articulations[2].CountToAngle(-3489);
        articulations[3].CountToAngle(56937);
        articulations[4].CountToAngle(27);
        */
        /*
        articulations[0].CountToAngle(138000);
        articulations[1].CountToAngle(-72000);
        articulations[2].CountToAngle(139000);
        articulations[3].CountToAngle(200000);
        articulations[4].CountToAngle(209000);
        */

        /*
        articulations[0].CountToAngle(-187000);
        articulations[1].CountToAngle(125000);
        articulations[2].CountToAngle(-113000);
        articulations[3].CountToAngle(0);
        articulations[4].CountToAngle(-209500);
        */
    }

    public void SetD(Transform D)
    {
        this.D = D;
    }

    public void SetSpeed(int speed)
    {
        for (int artIndex = articulations.Length - 1; artIndex >= 0; artIndex--)
        {
            articulations[artIndex].SetPercSpeed(speed);
        }
    }

    public void SetSpeedL(int speed)
    {
        speedMillimeter = speed;
        if (speedMillimeter < 1)
            speedMillimeter = 1;

        if (speedMillimeter > 300)
            speedMillimeter = 300;
    }

    public int GetSpeed()
    {
        return articulations[0].GetSpeed();
    }

    public int GetSpeedL()
    {
        return speedMillimeter;
    }

    public List<Vector3> GetAnglesFromCopy()
    {
        List<Vector3> angles = new List<Vector3>();
        foreach (Articulation articulation in art)
            angles.Add(articulation.GetAngle());
        return angles;
    }

    public List<Vector3> GetAngles()
    {
        List<Vector3> angles = new List<Vector3>();
        foreach (Articulation articulation in articulations)
            angles.Add(articulation.GetAngle());
        return angles;
    }

    private void SetAnglesCopyZero()
    {
        foreach(Articulation a in art)
        {
            a.SetAngle(Vector3.zero);
        }
    }

    private void SetAnglesCopyHome()
    {
        foreach (Articulation a in art)
        {
            a.SetAngle(a.GetAngleHome());
        }
    }

    public Articulation[] GetArticulations()
    {
        return articulations;
    }

    public Transform GetD()
    {
        return D;
    }

    public Transform GetE()
    {
        return E;
    }

    public Vector3 GetPosFromCounts(List<int> counts)
    {
        return scorbotModel.GetPosFromAngles(art, artE, scorbotModel.CountsToAngles(counts));
    }
}
