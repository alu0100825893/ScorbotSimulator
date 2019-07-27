using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class CommandControl : MonoBehaviour {

    private CatmullRomSpline spline;
    private Controller controller;
    private GameController gameController;
    private TextMeshProUGUI stateOutput;
    
    private bool isHereFromSimulation = true;

    void Start()
    {
        GameController.HereFromDel += SetIsHereFromSimulation;
        controller = GetComponent<GameController>().controller;
        spline = GetComponent<GameController>().spline;
        gameController = GetComponent<GameController>();
        

        stateOutput = GetComponent<GameController>().stateOutput;
    }

    private void SetIsHereFromSimulation(bool fromSimulation)
    {
        isHereFromSimulation = fromSimulation;     
    }


    public List<string> GetNames()
    {
        List<string> names = new List<string>();
        foreach (string name in Enum.GetNames(typeof(CommandHelper)))
            names.Add(name);
        return names;
    }

    public void Move(IK robot ,Transform target)
    {
        if (target.GetComponent<TargetModel>().GetAngles() != null)
        {
            robot.Move(target);
            //Debug.Log("Using angles");
        }
        else
        {
            robot.CCDAlg(target);
            target.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
            //Debug.Log("Using IK");
        }

        //
        if(gameController.GetOnlineMode())
            controller.RunCommandOnline("move" + " " + target.GetComponent<TargetModel>().GetName());
    }

    public void MoveL(IK robot, Transform target)
    {
        Vector3 a = robot.GetE().position;
        Vector3 b = target.position;

        float resolution = 0.1f; // Line quality, small=high 
        int loops = Mathf.FloorToInt(1f / resolution);

        // Linear trayectory
        Transform[] trayectory = new Transform[loops];

        for (int i = 1; i < loops; i++) // Last one is target, skip
        {
            Transform transf = new GameObject().transform;
            float t = i * resolution;
            //transf.position = Vector3.Lerp(robot.E.position, target.position, t);
            transf.position = Vector3.Lerp(a, b, t);
            trayectory[i - 1] = transf;
        }
        trayectory[loops - 1] = target;

        robot.CCDAlg(trayectory, false);

        //
        if (gameController.GetOnlineMode())
            controller.RunCommandOnline("movel" + " " + target.GetComponent<TargetModel>().GetName());
    }
    
    public void MoveC(IK robot, Transform finalPoint, Transform middlePoint)
    {        
        List<Transform> list = new List<Transform>();
        list.Add(robot.GetE());
        list.Add(middlePoint);
        list.Add(finalPoint);

        Vector3[] positions = spline.PrepareSpline(list);
        List<Vector3> points = spline.GetTrayectory(positions);


        Transform[] trayectory = new Transform[points.Count - 1]; // skip first one, Efector
   
        for (int i = 0; i < trayectory.Length - 1; i++) // Last one is target, skip
        {
            Transform transf = new GameObject().transform;
            transf.position = points[i + 1];
            trayectory[i] = transf;
        }
        trayectory[trayectory.Length - 1] = finalPoint;

        robot.CCDAlg(trayectory, false);

        //
        if (gameController.GetOnlineMode())
            controller.RunCommandOnline("move" + " " + finalPoint.GetComponent<TargetModel>().GetName() + " " + middlePoint.GetComponent<TargetModel>().GetName());
    }

    // mm. pos in real Scorbot
    public void Teach(IK robot, Transform target, Vector3 pos, float p, float r, bool online = true)
    {

        Vector3 posReal = new Vector3(pos.x, pos.y, pos.z);
        // mm to cm. pos in simulation
        pos = new Vector3(pos.x / 10f, pos.z / 10f, pos.y / 10f);

        Vector3 startPos = target.position;
        Vector3 startPitch = target.GetComponent<TargetModel>().GetAngles()[3];
        Vector3 startRoll = target.GetComponent<TargetModel>().GetAngles()[4];
        if (target.GetComponent<TargetModel>().GetAngles() == null)
        {

            stateOutput.text = "No angles data";
            //Debug.Log("No angles data");
            //return;

            // Just fill something
            List<Vector3> angles = new List<Vector3>(5);
            for (int i = 0; i < 5; i++)
                angles.Add(Vector3.zero);
            target.GetComponent<TargetModel>().SetAngles(angles);
            //Debug.Log(angles.Count);
        }

        // DO HERE and recover angles?

        // Apply pitch and roll to target
        target.GetComponent<TargetModel>().GetAngles()[3] = robot.GetArticulations()[3].BuiltAngle(p);
        target.GetComponent<TargetModel>().GetAngles()[4] = robot.GetArticulations()[4].BuiltAngle(-r);
     
        // Check if it's an unreachable point     
        target.position = pos;

        
        if (!robot.TargetInRange(target, true))
        {
            //stateOutput.text = "Unreachable point";
            //Debug.Log("Unreachable Teach");
            stateOutput.text = "Unreachable Teach";
            // Restore target
            target.position = startPos;
            target.GetComponent<TargetModel>().GetAngles()[3] = startPitch;
            target.GetComponent<TargetModel>().GetAngles()[4] = startRoll;
            return;
        }

        target.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
        //Debug.Log("Success Teach");
        stateOutput.text = "Success Teach";
        // Calculate new config with new restrictions


        if(gameController.GetOnlineMode() && online)
        {
            List<float> xyzpr = new List<float>() { posReal.x, posReal.y, posReal.z, p, r };
            controller.Connection.GetComponent<SerialController>().WriteToControllerTeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
            stateOutput.text = "Success Online Teach";
        }
    }

    // mm. pos in real Scorbot
    public void TeachR(IK robot, Transform target, Transform relativeToTarget, Vector3 pos, float p, float r)
    {
        
        Vector3 newPos = new Vector3(relativeToTarget.position.x * 10f, relativeToTarget.position.z * 10f, relativeToTarget.position.y * 10f)
            + pos;
        Teach(robot, target, newPos, relativeToTarget.GetComponent<TargetModel>().GetPitch() + p, 
            (-relativeToTarget.GetComponent<TargetModel>().GetRoll()) + r);

        target.GetComponent<TargetModel>().SetRelativeTo(relativeToTarget, new Vector3(pos.x, pos.z, pos.y) / 10f, p, r);

        if (gameController.GetOnlineMode())
        {
            
        }

    }

    public void Here(IK robot, Transform target)
    {
        
        if (gameController.GetOnlineMode())
        {
            // 2 options. use pos from robot (simulation) or real robot
            if (isHereFromSimulation)
            {
                // TEST????

                // Copy angles from robot
                target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
                target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());

                // Get pos and p, r from simulation. Do teach to real Scorbot


                List<float> xyzpr = new List<float>() { target.position.x, target.position.z, target.position.y,
                    target.GetComponent<TargetModel>().GetPitch(), -target.GetComponent<TargetModel>().GetRoll() };
                controller.Connection.GetComponent<SerialController>().WriteToControllerTeach(target.GetComponent<TargetModel>().name, xyzpr);
                //Teach(robot, target, pos, posPitchRoll[3], posPitchRoll[4]);
                stateOutput.text = "Success Here";
            }
            else // From real Scorbot
            {
                
                
                List<String[]> listString = new List<string[]>();
                // This stops main thread

                //controller.RunCommandHereOnline(target.GetComponent<TargetModel>().GetName());
                //listString = controller.RunCommandListpvOnline(target.GetComponent<TargetModel>().GetName());

                string result = "";

                Regex rx = new Regex("^.:(.+?)$");
                List<int> counts = new List<int>();
                List<float> posPitchRoll = new List<float>();

                foreach (String[] a in listString)
                {
                    //aux += a.Length.ToString();
                    foreach (string b in a)
                    {
                        //Debug.Log(b);                  
                        MatchCollection matches = rx.Matches(b);
                        foreach (Match match in matches)
                        {
                            GroupCollection groups = match.Groups;
                            result += groups[1].Value + "?";
                            posPitchRoll.Add(float.Parse(groups[1].Value));
                        }
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    counts.Add((int)posPitchRoll[0]);
                    posPitchRoll.RemoveAt(0);
                }

                result += posPitchRoll.Count + " " + counts.Count + " " + counts[0] + " " + posPitchRoll[0];
                stateOutput.text = result;

                // Just set robot to angles of real robot, then teach? with POS PITCH ROLL recovered (use target)
                //target.GetComponent<TargetModel>().SetAngles(robot.transform.GetComponent<ScorbotModel>().CountsToAngles(counts));
                //target.position = robot.GetPosFromCounts(counts);
                // 18 19.8 46.9

                // Or do teach to get same pos
                Teach(robot, target, new Vector3(posPitchRoll[0], posPitchRoll[1], posPitchRoll[2]), posPitchRoll[3], posPitchRoll[4], false);
            }
        }
        else // Offline
        {
            // Copy angles from robot
            target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
            target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());
            
            
            //List<int> counts = new List<int>() { -13541, -22691, -3489, 56937, 27};
            //target.position = robot.GetPosFromCounts(counts);
            //stateOutput.text =  robot.GetPosFromCounts(counts).ToString();
        }

    }


    public void SyncTargets(IK robot, Transform target)
    {

        List<String[]> listString = new List<string[]>();
        // This stops main thread
              
        listString = controller.RunCommandListpvOnline(target.GetComponent<TargetModel>().GetName());

        string result = "";

        Regex rx = new Regex("^.:(.+?)$");
        List<int> counts = new List<int>();
        List<float> posPitchRoll = new List<float>();

        foreach (String[] a in listString)
        {
            //aux += a.Length.ToString();
            foreach (string b in a)
            {
                //Debug.Log(b);                  
                MatchCollection matches = rx.Matches(b);
                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    result += groups[1].Value + "?";
                    posPitchRoll.Add(float.Parse(groups[1].Value));
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            counts.Add((int)posPitchRoll[0]);
            posPitchRoll.RemoveAt(0);
        }

        Teach(robot, target, new Vector3(posPitchRoll[0], posPitchRoll[1], posPitchRoll[2]), posPitchRoll[3], posPitchRoll[4], false);
    }

    public void Home(IK robot)
    {
        // Error: One home does not reach HOME
        robot.Home();
        robot.Home();
        robot.Home();

        //
        if (gameController.GetOnlineMode())
            controller.RunCommandOnline("home");        
     
    }

    
}
