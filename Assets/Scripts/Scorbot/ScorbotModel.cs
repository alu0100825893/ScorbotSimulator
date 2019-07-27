using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorbotModel : MonoBehaviour {

    public Transform D;
    public Transform E;

    public Articulation[] articulations;

    // Params and test 270 0 as limit
    private void Awake()
    {
        
        // Test robot
        /*
        // Angles      
        articulations[0].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[1].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[2].SetAngle(new Vector3(0f, 0f, 0f));
        
        // Planes
        articulations[0].SetPlane(PLANE_XZ);
        articulations[1].SetPlane(PLANE_XY);
        articulations[2].SetPlane(PLANE_XY);

        // Limits
        articulations[0].SetLimit(90f, -90f);// Error: articulations[0].SetLimit(270f, 0f);
        articulations[1].SetLimit(90f, -90f);
        articulations[2].SetLimit(90f, -90f);
        */

        // Final robot
        // Angles      
        /*
        articulations[0].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[1].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[2].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[3].SetAngle(new Vector3(0f, 0f, 0f));
        articulations[4].SetAngle(new Vector3(0f, 0f, 0f));
        */

        // Planes
        articulations[0].SetPlane(PlaneHelper.XZ);
        articulations[1].SetPlane(PlaneHelper.XY);
        articulations[2].SetPlane(PlaneHelper.XY);
        articulations[3].SetPlane(PlaneHelper.XY);
        articulations[4].SetPlane(PlaneHelper.YZ);

        // Limits
        /*
        articulations[0].SetLimit(170f, -170f);
        articulations[1].SetLimit(90f, -90f);
        articulations[2].SetLimit(90f, -90f);
        articulations[3].SetLimit(90f, -90f);
        */

        articulations[0].SetLimit(ScorbotERIX.BASE_COUNT_MIN, ScorbotERIX.BASE_COUNT_MAX,
            ScorbotERIX.BASE_COUNT_HOME, ScorbotERIX.BASE_DEGREES_MAX, -171f);      
        articulations[1].SetLimit(ScorbotERIX.SHOULDER_COUNT_MIN, ScorbotERIX.SHOULDER_COUNT_MAX,
            ScorbotERIX.SHOULDER_COUNT_HOME, ScorbotERIX.SHOULDER_DEGREES_MAX, -42.5f);
        articulations[2].SetLimit(ScorbotERIX.ELBOW_COUNT_MIN, ScorbotERIX.ELBOW_COUNT_MAX,
            ScorbotERIX.ELBOW_COUNT_HOME, ScorbotERIX.ELBOW_DEGREES_MAX, -120f);
        articulations[3].SetLimit(ScorbotERIX.PITCH_COUNT_MIN, ScorbotERIX.PITCH_COUNT_MAX,
            ScorbotERIX.PITCH_COUNT_HOME, ScorbotERIX.PITCH_DEGREES_MAX, -98f);
        articulations[4].SetLimit(ScorbotERIX.ROLL_COUNT_MIN, ScorbotERIX.ROLL_COUNT_MAX,
            ScorbotERIX.ROLL_COUNT_HOME, ScorbotERIX.ROLL_DEGREES_MAX, -737f / 2f);

        //articulations[0].SetLimit(-190186, 138899, -13539, 270f, -171f);
        //articulations[1].SetLimit(-74957, 128285, 110777, 145f, -42.5f); // Temporal fix
        //articulations[2].SetLimit(-115649, 139121, 17518, 210f, -120f);
        //articulations[3].SetLimit(-1018, 200285, -26748, 196f, -98f);
        //articulations[4].SetLimit(-209547, 209658, 0, 737f, -737f / 2f);

        articulations[0].SetSpeed(ScorbotERIX.BASE_SPEED_MIN, ScorbotERIX.BASE_SPEED_MAX);
        articulations[1].SetSpeed(ScorbotERIX.SHOULDER_SPEED_MIN, ScorbotERIX.SHOULDER_SPEED_MAX);
        articulations[2].SetSpeed(ScorbotERIX.ELBOW_SPEED_MIN, ScorbotERIX.ELBOW_SPEED_MAX);
        articulations[3].SetSpeed(ScorbotERIX.PITCH_SPEED_MIN, ScorbotERIX.PITCH_SPEED_MAX);
        articulations[4].SetSpeed(ScorbotERIX.ROLL_SPEED_MIN, ScorbotERIX.ROLL_SPEED_MAX);

        //articulations[0].SetSpeed(79f, 112f);
        //articulations[1].SetSpeed(68f, 99f);
        //articulations[2].SetSpeed(76f, 112f);
        //articulations[3].SetSpeed(87f, 133f);
        //articulations[4].SetSpeed(166f, 166f);

        //articulations[3].Rotate(45f);
        //Home(0f);           
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void InitToCopy(out Articulation[] art, out Transform artE)
    {

        GameObject g;
        art = new Articulation[articulations.Length];
        for (int i = 0; i < articulations.Length; i++)
        {
            g = new GameObject();
            g.name = "CopyArticulation";
            g.transform.position = articulations[i].transform.position;
            g.AddComponent<Articulation>();
            // Copy articulation config
            g.GetComponent<Articulation>().SetPlane(articulations[i].GetPlane());
            g.GetComponent<Articulation>().SetLimit(articulations[i].lowerLimitCount, articulations[i].upperLimitCount, articulations[i].homeCount,
                articulations[i].degrees, articulations[i].offset);
            g.GetComponent<Articulation>().SetCountsAreTransformed(articulations[i].CountsAreTransformed());
            // Parenting
            art[i] = g.GetComponent<Articulation>();

            if (i >= 1)
            {
                art[i].transform.parent = art[i - 1].transform;
            }
        }
        // Copy E config
        g = new GameObject();
        g.name = "CopyE";
        g.transform.position = E.position;
        g.transform.parent = art[art.Length - 1].transform;
        artE = g.transform;
    }

    public void UpdateToCopy(Articulation[] art)
    {
        for (int i = 0; i < art.Length; i++)
        {
            art[i].SetAngle(articulations[i].GetAngle());
        }
    }
  
    public List<Vector3> CountsToAngles(List<int> counts)
    {
        List<Vector3> angles = new List<Vector3>();

        for(int i = 0; i < articulations.Length; i++)
        {
            angles.Add(articulations[i].CountToAngle(counts[i]));
        }
        return angles;
    }
    
    public Vector3 GetPosFromAngles(Articulation[] art, Transform artE, List<Vector3> angles)
    {       
        for (int i = 0; i < art.Length; i++)
        {
            art[i].SetAngle(angles[i]);
        }        
        return artE.position;
    }
 
    public void SetAngles(List<Vector3> angles)
    {
        for (int i = 0; i < articulations.Length; i++)
        {
            articulations[i].SetAngle(angles[i]);
        }
    }
}
