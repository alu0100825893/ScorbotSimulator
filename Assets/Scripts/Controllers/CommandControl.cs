using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/**
 * La función principal de este componente es definir las tareas a realizar por cada comando y ejecutarlas
 * ya sea en offline u online. La sincronización de posiciones es otra de sus funciones, ya que aunque
 * no es un comando en sí, la sincronización es una sucesión de comandos, por lo que se la trata como
 * una extensión de los comandos.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class CommandControl : MonoBehaviour {

    private CatmullRomSpline spline;
    private Controller controller;
    private GameController gameController;
    private TextMeshProUGUI stateOutput;//
    private StateMessageControl stateMessageControl;
    
    private bool isHereFromSimulation = true;

    void Start()
    {
        // Events
        GameController.HereFromDel += SetIsHereFromSimulation;
                       
        spline = GetComponent<GameController>().spline;

        // Controllers
        controller = GetComponent<GameController>().controller;
        gameController = GetComponent<GameController>();
        stateMessageControl = GetComponent<StateMessageControl>();

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
        // Target with valid data
        if (target.GetComponent<TargetModel>().GetValid())
        {
            robot.Move(target);         
        }
        else // Target with invalid data
        {                    
            stateMessageControl.WriteMessage("Error. MOVE Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;            
        }
                
        stateMessageControl.WriteMessage("Done. MOVE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);

        // 
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("move", target.GetComponent<TargetModel>().GetName());
            if (done)
            {
                string aux = "";
                if (!target.GetComponent<TargetModel>().GetSync())
                    aux = ". NO SYNC";
                stateMessageControl.WriteMessage("Done. Online MOVE \"" + target.GetComponent<TargetModel>().GetName() + "\"" + aux, done);
            }
            else
                stateMessageControl.WriteMessage("Error. Online MOVE \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
        }

        //controller.RunCommandOnline("move" + " " + target.GetComponent<TargetModel>().GetName());
    }
   
    public void MoveL(IK robot, Transform target)
    {
        if(!target.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEL Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }

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
        stateMessageControl.WriteMessage("Done. MOVEL \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);

        //
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("movel", target.GetComponent<TargetModel>().GetName());
            if (done)
            {
                string aux = "";
                if (!target.GetComponent<TargetModel>().GetSync())
                    aux = ". NO SYNC";
                stateMessageControl.WriteMessage("Done. Online MOVEL \"" + target.GetComponent<TargetModel>().GetName() + "\"" + aux, done);
            }
            else
                stateMessageControl.WriteMessage("Error. Online MOVEL \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
        }

        //controller.RunCommandOnline("movel" + " " + target.GetComponent<TargetModel>().GetName());
    }
    
    // Targets in range?
    public void MoveC(IK robot, Transform finalPoint, Transform middlePoint)
    {
        if (!finalPoint.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEC Unreachable position \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }
        if (!middlePoint.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEC Unreachable position \"" + middlePoint.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }

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
        stateMessageControl.WriteMessage("Done. MOVEC \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"" +
                    " \"" + middlePoint.GetComponent<TargetModel>().GetName() + "\"", true);

        //
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("movec", finalPoint.GetComponent<TargetModel>().GetName(), middlePoint.GetComponent<TargetModel>().GetName());

            if (done)
            {
                string aux = "";
                if (!finalPoint.GetComponent<TargetModel>().GetSync() || !middlePoint.GetComponent<TargetModel>().GetSync())
                    aux = ". NO SYNC";
                stateMessageControl.WriteMessage("Done. Online MOVEC \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"" +
                    " \"" + middlePoint.GetComponent<TargetModel>().GetName() + aux, done);
            }
            else
                stateMessageControl.WriteMessage("Error. Online MOVEC \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"" +
                    " \"" + middlePoint.GetComponent<TargetModel>().GetName() + "\"", done);
        }
        //controller.RunCommandOnline("move" + " " + finalPoint.GetComponent<TargetModel>().GetName() + " " + middlePoint.GetComponent<TargetModel>().GetName());
    }

    // mm. pos in real Scorbot
    public bool Teach(IK robot, Transform target, Vector3 pos, float p, float r, bool online = true, bool offline = true)
    {
        Vector3 posReal = new Vector3(pos.x, pos.y, pos.z);
        if (offline)
        {            
            // mm to cm. pos in simulation
            pos = new Vector3(pos.x / 10f, pos.z / 10f, pos.y / 10f);

            Vector3 startPos = target.position;
            Vector3 startPitch = target.GetComponent<TargetModel>().GetAngles()[3];
            Vector3 startRoll = target.GetComponent<TargetModel>().GetAngles()[4];

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
                //stateOutput.text = "Unreachable Teach";
                stateMessageControl.WriteMessage("Error. TEACH Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
                // Restore target
                target.position = startPos;
                target.GetComponent<TargetModel>().GetAngles()[3] = startPitch;
                target.GetComponent<TargetModel>().GetAngles()[4] = startRoll;
                return false;
            }

            target.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
            target.GetComponent<TargetModel>().SetSync(false);
            //Debug.Log("Success Teach");
            //stateOutput.text = "Success Teach";
            stateMessageControl.WriteMessage("Done. TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
            stateMessageControl.UpdatePositionLog();
            // Calculate new config with new restrictions
        }

        if (gameController.GetOnlineMode() && online)
        {
            List<float> xyzpr = new List<float>() { posReal.x, posReal.y, posReal.z, p, r };
            bool done = controller.RunCommandUITeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
            //controller.Connection.GetComponent<SerialController>().WriteToControllerTeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
            if (done)
            {
                stateMessageControl.WriteMessage("Done. Online TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                target.GetComponent<TargetModel>().SetSync(true);
                stateMessageControl.UpdatePositionLog();
            }
            else
                stateMessageControl.WriteMessage("Error. Online TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            //stateOutput.text = "Success Online Teach";
        }

        return true;
    }

    // Generate data for relative position
    // Check valid
    // mm. pos in real Scorbot
    public void TeachR(IK robot, Transform target, Transform relativeToTarget, Vector3 pos, float p, float r)
    {
        
        Vector3 newPos = (new Vector3(relativeToTarget.position.x, relativeToTarget.position.z, relativeToTarget.position.y) * 10f)
            + pos;
        bool teachDone = Teach(robot, target, newPos, relativeToTarget.GetComponent<TargetModel>().GetPitch() + p, 
            relativeToTarget.GetComponent<TargetModel>().GetRoll() + r);

        if (teachDone)
            stateMessageControl.WriteMessage("Done. TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\"", teachDone);
        else
        {
            stateMessageControl.WriteMessage("Error. TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\"", teachDone);
            return;
        }
        
        target.GetComponent<TargetModel>().SetRelativeTo(relativeToTarget, new Vector3(pos.x, pos.z, pos.y) / 10f, p, r);
                
        if (gameController.GetOnlineMode())
        {
            //
        }

    }

    public void Here(IK robot, Transform target)
    {
        
        if (gameController.GetOnlineMode())
        {
            // 2 options. Use pos from simulation or real Scorbot
            if (isHereFromSimulation)
            {                
                // Copy angles from simulation
                target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
                target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());
                stateMessageControl.WriteMessage("Done. HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
                target.GetComponent<TargetModel>().SetSync(false);
                stateMessageControl.UpdatePositionLog();

                // Get pos and p, r from simulation. Do teach to real Scorbot

                List<float> xyzpr = new List<float>() { target.position.x * 10f, target.position.z  * 10f, target.position.y  * 10f,
                    target.GetComponent<TargetModel>().GetPitch(), target.GetComponent<TargetModel>().GetRoll() };
                bool done = controller.RunCommandUITeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
                //Teach(robot, target, pos, posPitchRoll[3], posPitchRoll[4]);
                if (done)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                    target.GetComponent<TargetModel>().SetSync(true);
                    stateMessageControl.UpdatePositionLog();
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                //stateOutput.text = "Success Here Simulation";
            }
            else // From real Scorbot
            {
                bool here = controller.RunCommandUIOnline("here", target.GetComponent<TargetModel>().GetName());
                if (here)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE(HERE) \"" + target.GetComponent<TargetModel>().GetName() + "\"", here);
                    target.GetComponent<TargetModel>().SetSync(false);
                    stateMessageControl.UpdatePositionLog();
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE(HERE) \"" + target.GetComponent<TargetModel>().GetName() + "\"", here);

                bool done = SyncScorbotToSimulation(robot, target);

                if (done)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE(SYNC) \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE(SYNC) \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);

            }
        }
        else // Offline
        {
            // Copy angles from simulation
            target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
            target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());
            target.GetComponent<TargetModel>().SetSync(false);
            stateMessageControl.UpdatePositionLog();

            stateMessageControl.WriteMessage("Done. HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);

            //List<int> counts = new List<int>() { -13541, -22691, -3489, 56937, 27};
            //target.position = robot.GetPosFromCounts(counts);
            //stateOutput.text =  robot.GetPosFromCounts(counts).ToString();
        }

    }

    public bool Listpv(Transform target, out List<int> counts, out List<float> posPitchRoll)
    {
        //List<int> counts = new List<int>();
        counts = new List<int>();
        //List<float> posPitchRoll = new List<float>();
        posPitchRoll = new List<float>();

        List<String[]> listString = new List<string[]>();
        // This stops main thread    
        listString = controller.RunCommandListpvOnline(target.GetComponent<TargetModel>().GetName());
        if(listString == null || listString.Count == 0)                 
            return false;
        
        // Transform listpv data
        string result = "";

        Regex rx = new Regex("^.:(.+?)$");        

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

        return true;
    }

    // Online.
    public bool SyncScorbotToSimulation(IK robot, Transform target)
    {        
        List<String[]> listString = new List<string[]>();
        // This stops main thread    
        //listString = controller.RunCommandListpvOnline(target.GetComponent<TargetModel>().GetName());

        List<int> counts;
        List<float> posPitchRoll;
        bool listpv = Listpv(target, out counts, out posPitchRoll);
        if (listpv)
            stateMessageControl.WriteMessage("Done. Online SYNC(LISTPV) \"" + target.GetComponent<TargetModel>().GetName() + "\"", listpv);
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC(LISTPV) \"" + target.GetComponent<TargetModel>().GetName() + "\"", listpv);
            return false;
        }

        // Do teach only in simulation
        bool done = Teach(robot, target, new Vector3(posPitchRoll[0], posPitchRoll[1], posPitchRoll[2]), posPitchRoll[3], posPitchRoll[4], false);
        if (done)
        {
            stateMessageControl.WriteMessage("Done. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            target.GetComponent<TargetModel>().SetSync(true);
            stateMessageControl.UpdatePositionLog();
            return true;
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            return false;
        }
    }

    // defp
    // Online
    public bool SyncSimulationToScorbot(IK robot, Transform target)
    {
        // defp, in case it doest exist
        Defp(target);

        // Teach to Scorbot
        Vector3 pos = target.GetComponent<TargetModel>().GetPositionInScorbot();
        float p = target.GetComponent<TargetModel>().GetPitch();
        float r = target.GetComponent<TargetModel>().GetRoll();
        bool done = Teach(robot, target, pos, p, r, true, false);
        if (done)
        {
            stateMessageControl.WriteMessage("Done. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            target.GetComponent<TargetModel>().SetSync(true);
            stateMessageControl.UpdatePositionLog();
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            return false;
        }

        return true;
    }

    // byIndex: X, Y, Z, P, R (0..4)
    public void Shiftc(IK robot, Transform target, int byIndex, float value)
    {
        // Null angles
        /*
        if()
        if (!robot.TargetInRange(target, true))
        {
            //stateOutput.text = "Unreachable point";
            //Debug.Log("Unreachable Teach");
            //stateOutput.text = "Unreachable Teach";
            stateMessageControl.WriteMessage("Error. Teach Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            // Restore target
            target.position = startPos;
            target.GetComponent<TargetModel>().GetAngles()[3] = startPitch;
            target.GetComponent<TargetModel>().GetAngles()[4] = startRoll;
            return false;
        }
        */

        target.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());

        Vector3 pos = target.GetComponent<TargetModel>().GetPositionInScorbot();
        float p = target.GetComponent<TargetModel>().GetPitch();
        float r = target.GetComponent<TargetModel>().GetRoll();
        Vector3 offsetPos = Vector3.zero;

        switch(byIndex)
        {
            case 0:
                offsetPos = new Vector3(value, 0f, 0f);
                break;
            case 1:
                offsetPos = new Vector3(0f, value, 0f);
                break;
            case 2:
                offsetPos = new Vector3(0f, 0f, value);
                break;
            case 3:
                p += value;
                break;
            case 4:
                r += value;
                break;
        }
        pos += offsetPos;

        bool done = Teach(robot, target, pos, p, r);
        if (done)
            stateMessageControl.WriteMessage("Done. SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
        else
            stateMessageControl.WriteMessage("Error. SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
    }

    public void Home(IK robot)
    {
        // Error: One home does not reach HOME
        robot.Home();
        robot.Home();
        robot.Home();
        stateMessageControl.WriteMessage("Done. HOME", true);
        //
        if (gameController.GetOnlineMode())
        {
            controller.RunCommandOnline("home");
        }
     
    }

    // Enable control. Real Scorbot
    public void CON()
    {
        if (gameController.GetOnlineMode())
        {
            controller.RunCommandOnline("con");
            stateMessageControl.WriteMessage("Done. Online CON(Control Enabled)", true);
        }
    }

    public void Speed(IK robot, int value)
    {
        robot.SetSpeed(value);
        stateMessageControl.WriteMessage("Done. SPEED \"" + robot.GetSpeed() + "\"", true);

        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("speed", value.ToString());
            if (done)
                stateMessageControl.WriteMessage("Done. Online SPEED \"" + value.ToString() + "\"", done);
            else
                stateMessageControl.WriteMessage("Error. Online SPEDD \"" + value.ToString() + "\"", done);
        }
    }

    public void SpeedL(IK robot, int value)
    {
        robot.SetSpeedL(value);
        stateMessageControl.WriteMessage("Done. SPEED \"" + robot.GetSpeedL() + "\"", true);

        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("speedl", value.ToString());
            if (done)
                stateMessageControl.WriteMessage("Done. Online SPEEDL \"" + value.ToString() + "\"", done);
            else
                stateMessageControl.WriteMessage("Error. Online SPEDDL \"" + value.ToString() + "\"", done);
        }
    }

    public void Defp(Transform target)
    {
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("defp", target.GetComponent<TargetModel>().GetName());
            if (done)
                stateMessageControl.WriteMessage("Done. Online DEFP \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
            else
                stateMessageControl.WriteMessage("Done. Online DEFP. Already exits \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
        }
    }
}
