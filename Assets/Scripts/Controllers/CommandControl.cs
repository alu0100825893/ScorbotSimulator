using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
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

    // Catmull-Rom spline. It draws trajectories between positions
    private CatmullRomSpline spline;

    // Controllers
    private Controller controller; // Terminal
    private GameController gameController;   
    private StateMessageControl stateMessageControl;
    
    // Command HERE. If mode "From simulation" is active, true (default)
    private bool isHereFromSimulation = true;

    void Start()
    {
        // Events
        GameController.HereFromDel += SetIsHereFromSimulation;

        // Catmull-Rom spline. It draws trajectories between positions
        spline = GetComponent<GameController>().spline;

        // Controllers
        controller = GetComponent<GameController>().controller;
        gameController = GetComponent<GameController>();
        stateMessageControl = GetComponent<StateMessageControl>();

    }

    /**
	 * Cambia el modo del comando HERE. 
	 * @param fromSimulation Modo "From simulation" activo, true
	 * @return void
	 */
    private void SetIsHereFromSimulation(bool fromSimulation)
    {
        isHereFromSimulation = fromSimulation;     
    }

    /**
	 * Permite obtener la lista de nombres de los comandos disponibles. El archivo "CommandHelper" contiene 
     * los comandos.
	 * @return List<string> Lista de nombres de los comandos.
	 */
    public List<string> GetNames()
    {
        List<string> names = new List<string>();
        foreach (string name in Enum.GetNames(typeof(CommandHelper)))
            names.Add(name);
        return names;
    }

    /**
     * Mueve el Scorbot a una posición ya definida.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @return void
     */
    public void Move(IK robot ,Transform target)
    {
        // Target with valid data
        if (target.GetComponent<TargetModel>().GetValid())
        {
            // Move Scorbot to target. It uses "speed"
            robot.Move(target);         
        }
        else // Target with invalid data
        {                    
            stateMessageControl.WriteMessage("Error. MOVE Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;            
        }
                
        stateMessageControl.WriteMessage("Done. MOVE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);

        // Online mode
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
    }

    /**
     * Mueve el Scorbot a una posición ya definida. El movimiento es una línea recta. 
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @return void
     */
    public void MoveL(IK robot, Transform target)
    {
        // If target with invalid data
        if (!target.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEL Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }
        // Scorbot end effector
        Vector3 a = robot.GetE().position;
        // Position coordinates
        Vector3 b = target.position;

        float resolution = 0.1f; // Line quality, small=high quality
        int loops = Mathf.FloorToInt(1f / resolution);

        // Linear trajectory
        Transform[] trajectory = new Transform[loops];

        for (int i = 1; i < loops; i++) // Last one is target, skip
        {
            // New object
            Transform transf = new GameObject().transform;
            float t = i * resolution;
            // Modify coordinates. Interpolation from a to b
            transf.position = Vector3.Lerp(a, b, t);
            // Add to trajectory
            trajectory[i - 1] = transf;
        }
        // Add target to trajectory
        trajectory[loops - 1] = target;

        // Move Scorbot following trajectory. It uses "speedl"
        robot.CCDAlg(trajectory, false);
        stateMessageControl.WriteMessage("Done. MOVEL \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);

        // Online mode
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
    }

    /**
     * Mueve el Scorbot a una posición ya definida pasando por otra posición. El movimiento es una curva generada
     * por el algoritmo del spline de CatmullRom.
     * @param robot Scorbot
     * @param finalPoint Posición final(objeto)
     * @param middlePoint Posición intermedia (objeto)
     * @return void
     */
    public void MoveC(IK robot, Transform finalPoint, Transform middlePoint)
    {
        // Valid final position
        if (!finalPoint.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEC Unreachable position \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }
        // Valid intermediate position
        if (!middlePoint.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. MOVEC Unreachable position \"" + middlePoint.GetComponent<TargetModel>().GetName() + "\"", false);
            return;
        }

        List<Transform> list = new List<Transform>();
        // Scorbot final effector
        list.Add(robot.GetE());
        // Intermadiate position
        list.Add(middlePoint);
        // Final position
        list.Add(finalPoint);

        // Add 2 control points
        Vector3[] positions = spline.PrepareSpline(list);
        // Get final trajectory
        List<Vector3> points = spline.GetTrayectory(positions);

        // Build objects from final trajectory
        Transform[] trayectory = new Transform[points.Count - 1]; // skip first one, Effector
   
        for (int i = 0; i < trayectory.Length - 1; i++) // Last one is target, skip
        {
            Transform transf = new GameObject().transform;
            transf.position = points[i + 1];
            trayectory[i] = transf;
        }
        trayectory[trayectory.Length - 1] = finalPoint;
        // Move Scorbot following trajectory
        robot.CCDAlg(trayectory, false);
        stateMessageControl.WriteMessage("Done. MOVEC \"" + finalPoint.GetComponent<TargetModel>().GetName() + "\"" +
                    " \"" + middlePoint.GetComponent<TargetModel>().GetName() + "\"", true);

        // Online mode
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
    }

    /**
     * Modifica una posición con nuevos valores los cuales son una posición (x, y, z), inclinación frontal e 
     * inclinación lateral. Estos valores deben entar en el contexto del Scorbot real.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @param pos Coordenadas
     * @param p Pitch
     * @param r Roll
     * @param online Ejecutar modo online
     * @param offline Ejecutar modo offline
     * @return bool Éxito
     */
    public bool Teach(IK robot, Transform target, Vector3 pos, float p, float r, bool online = true, bool offline = true)
    {

        Vector3 posReal = new Vector3(pos.x, pos.y, pos.z);
        // Offline mode
        if (offline)
        {            
            // mm to cm. Interchange y and z. Position in simulation
            pos = new Vector3(pos.x / 10f, pos.z / 10f, pos.y / 10f);
            // Copy initial values
            Vector3 startPos = target.position;
            Vector3 startPitch = target.GetComponent<TargetModel>().GetAngles()[3];
            Vector3 startRoll = target.GetComponent<TargetModel>().GetAngles()[4];
            
            // Apply pitch and roll to target
            target.GetComponent<TargetModel>().GetAngles()[3] = robot.GetArticulations()[3].BuiltAngle(p);
            float auxR = -r;
            if (robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
            {
                auxR = r;
            }
            target.GetComponent<TargetModel>().GetAngles()[4] = robot.GetArticulations()[4].BuiltAngle(auxR);

            // Apply new coordinatesC  
            target.position = pos;
            // Check if it's an unreachable point
            if (!robot.TargetInRange(target, true))
            {               
                stateMessageControl.WriteMessage("Error. TEACH Unreachable position \"" + target.GetComponent<TargetModel>().GetName() + "\"", false);
                // Restore position (object) values
                target.position = startPos;
                target.GetComponent<TargetModel>().GetAngles()[3] = startPitch;
                target.GetComponent<TargetModel>().GetAngles()[4] = startRoll;
                return false;
            }
                        
            // Recover angles data. Apply to position (object)
            target.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
            target.GetComponent<TargetModel>().SetSync(false);
        
            stateMessageControl.WriteMessage("Done. TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
            stateMessageControl.UpdatePositionLog();

            // if this position is being used by another position (relative), that position is not sync anymore
            Transform relativePosition = target.GetComponent<TargetModel>().GetRelativeFrom();
            if (target.GetComponent<TargetModel>().GetRelativeFrom())
            {
                relativePosition.GetComponent<TargetModel>().SetSync(false);
                // Updating relative position
                relativePosition.GetComponent<TargetModel>().UpdateRelativePosition();

                // Update angles data                    
                if (robot.TargetInRange(relativePosition))
                {
                    // Reachable. Load data to target
                    relativePosition.GetComponent<TargetModel>().SetAngles(robot.GetAnglesFromCopy());
                    relativePosition.GetComponent<TargetModel>().SetValid(true);
                }
                else // Unreachable
                {
                    relativePosition.GetComponent<TargetModel>().SetValid(false);
                }
            }

            // This position is relative to another, teach destroys relativity
            if (target.GetComponent<TargetModel>().GetRelativeTo())
            {
                target.GetComponent<TargetModel>().SetNoRelativeTo();
            }
        }

        // Online mode
        if (gameController.GetOnlineMode() && online)
        {
            // Build data to send
            if(robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
            {
                posReal = posReal * 10f;
                p = p * 10f;
                r = r * 10f;           
            }

            List<float> xyzpr = new List<float>() { posReal.x, posReal.y, posReal.z, p, r };
            bool done = controller.RunCommandUITeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
            if (done)
            {
                stateMessageControl.WriteMessage("Done. Online TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                target.GetComponent<TargetModel>().SetSync(true);
                
                if (target.GetComponent<TargetModel>().GetRelativeFrom())
                    target.GetComponent<TargetModel>().GetRelativeFrom().GetComponent<TargetModel>().SetSync(true);
                

                stateMessageControl.UpdatePositionLog();
            }
            else
            {
                stateMessageControl.WriteMessage("Error. Online TEACH \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                return false;
            }
        }

        return true;
    }

    /**
     * Define una posición relativa a otra posición con nuevos valores los cuales son una posición (x, y, z), 
     * inclinación frontal e inclinación lateral. La posición permanecerá relativa. Los valores deben estar
     * en el contexto del Scorbot real.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @param relativeToTarget Posición (objeto) relativa a usar como referencia
     * @param pos Coordenadas relativas
     * @param p Pitch relativo
     * @param r Roll relativo
     * @param online Ejecutar modo online
     * @param offline Ejecutar modo offline
     * @return bool Éxito
     */
    public bool TeachR(IK robot, Transform target, Transform relativeToTarget, Vector3 pos, float p, float r, bool online = true, bool offline = true)
    {
        // Offline mode
        if (offline)
        {
            // New relative position
            Vector3 newPos = (new Vector3(relativeToTarget.position.x, relativeToTarget.position.z, relativeToTarget.position.y) * 10f)
                + pos;

            // Apply new relative values
            bool teachDone = Teach(robot, target, newPos, relativeToTarget.GetComponent<TargetModel>().GetPitch() + p,
                relativeToTarget.GetComponent<TargetModel>().GetRoll() + r, false);

            if (teachDone)
                stateMessageControl.WriteMessage("Done. TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\" \"" +
                    relativeToTarget.GetComponent<TargetModel>().GetName() + "\"", teachDone);
            else
            {
                stateMessageControl.WriteMessage("Error. TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\" \"" +
                    relativeToTarget.GetComponent<TargetModel>().GetName() + "\"", teachDone);
                return false;
            }

            // Activate automatic values update
            target.GetComponent<TargetModel>().SetRelativeTo(relativeToTarget, new Vector3(pos.x, pos.z, pos.y) / 10f, p, r);
        }            

        // Online mode        
        if (gameController.GetOnlineMode() && online)
        {
            // Build data to send            
            if (robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
            {
                pos = pos * 10f;
                p = p * 10f;
                r = r * 10f;
            }

            List<float> xyzpr = new List<float>() { pos.x, pos.y, pos.z, p, r };
            bool done = controller.RunCommandUITeachr(target.GetComponent<TargetModel>().GetName(),
                relativeToTarget.GetComponent<TargetModel>().GetName(), xyzpr);
            if (done)
            {
                stateMessageControl.WriteMessage("Done. Online TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\" \"" +
                relativeToTarget.GetComponent<TargetModel>().GetName() + "\"", done);
                target.GetComponent<TargetModel>().SetSync(true);
                stateMessageControl.UpdatePositionLog();
            }
            else
            {
                stateMessageControl.WriteMessage("Error. Online TEACHR \"" + target.GetComponent<TargetModel>().GetName() + "\" \"" +
                relativeToTarget.GetComponent<TargetModel>().GetName() + "\"", done);
                return false;
            }
        }
        return true;
    }

    /**
     * Modifica una posición con los valores actuales del Scorbot. Modifica una posición en el controlador con los 
     * valores actuales del Scorbot real o el de la simulación. En modo "From simulation" el Scorbot es el de 
     * la simulación, mientras que en modo "From Scorbot" es el Scorbot real. En modo "From simulation" se ejecuta 
     * el comando "Teach" (online) con los valores del Scorbot de la simulación. En modo "From Scorbot" se ejecuta 
     * el comando "Here" (online) en el Scorbot real, seguidamente de "Listpv" (online) para recuperar los datos 
     * del "Here" y se realiza un "Teach" (Offline) para cargar esos datos.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @return void
     */
    public void Here(IK robot, Transform target)
    {
        
        // Online mode
        if (gameController.GetOnlineMode())
        {
            // 2 options. Use pos from simulation or real Scorbot
            if (isHereFromSimulation) // Here. Mode "From simulation"
            {                
                // Copy angles from simulation into the position (object)
                target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
                target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());
                stateMessageControl.WriteMessage("Done. HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
                target.GetComponent<TargetModel>().SetSync(false);
                stateMessageControl.UpdatePositionLog();

                // Get pos, p, r from simulation. Do teach to real Scorbot
                float multPos = 10f;
                float multDegrees = 1f;
                if (robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
                {
                    multPos = 100f;
                    multDegrees = 10f;
                }

                List<float> xyzpr = new List<float>() { target.position.x * multPos, target.position.z  * multPos, target.position.y  * multPos,
                    target.GetComponent<TargetModel>().GetPitch() * multDegrees, target.GetComponent<TargetModel>().GetRoll() * multDegrees };
                
                bool done = controller.RunCommandUITeach(target.GetComponent<TargetModel>().GetName(), xyzpr);
           
                if (done)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                    target.GetComponent<TargetModel>().SetSync(true);
                    stateMessageControl.UpdatePositionLog();
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
              
            }
            else // Here. Mode "From real Scorbot"
            {
                // Real scorbot here
                bool here = controller.RunCommandUIOnline("here", target.GetComponent<TargetModel>().GetName());
                if (here)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE(HERE) \"" + target.GetComponent<TargetModel>().GetName() + "\"", here);
                    target.GetComponent<TargetModel>().SetSync(false);
                    stateMessageControl.UpdatePositionLog();
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE(HERE) \"" + target.GetComponent<TargetModel>().GetName() + "\"", here);

                // Get data from real Scorbot into the position (object)
                Thread.Sleep(200);
                bool done = SyncScorbotToSimulation(robot, target);

                if (done)
                {
                    stateMessageControl.WriteMessage("Done. Online HERE(SYNC) \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
                }
                else
                    stateMessageControl.WriteMessage("Error. Online HERE(SYNC) \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);

            }
        }
        else // Offline mode
        {
            // Copy angles from simulation into the position (object)
            target.position = new Vector3(robot.GetE().position.x, robot.GetE().position.y, robot.GetE().position.z);
            target.GetComponent<TargetModel>().SetAngles(robot.GetAngles());
            target.GetComponent<TargetModel>().SetSync(false);
            stateMessageControl.UpdatePositionLog();

            stateMessageControl.WriteMessage("Done. HERE \"" + target.GetComponent<TargetModel>().GetName() + "\"", true);
        }

    }

    /**
     * Se obtienen los valores de una posición los cuales son una posición (x, y, z), inclinación frontal (pitch) 
     * e inclinación lateral (roll). Además de los conteos de encoder para cada articulación del Scorbot real. Este
     * comando no el el mismo que el "listpv" de la simulación, aunque su función la misma.
     * @param target Posición (objeto)
     * @param counts Conteos de encoder
     * @param posPitchRoll Coordenadas, pitch y roll
     * @return bool Éxito
     */
    public bool Listpv(Transform target, out List<int> counts, out List<float> posPitchRoll)
    {     
        counts = new List<int>();      
        posPitchRoll = new List<float>();

        // Only online mode
        if (!gameController.GetOnlineMode())
        {
            stateMessageControl.WriteMessage("Error. Online LISTPV \"" + target.GetComponent<TargetModel>().GetName() + "\". Mode online required", false);
            return false;
        }

        List<String[]> listString = new List<string[]>();
        // This stops main thread. Get position data from real Scorbot
        listString = controller.RunCommandListpvOnline(target.GetComponent<TargetModel>().GetName());
        if(listString == null || listString.Count == 0)
            return false;
        
        // Merge data
        string aux = "";
        foreach (String[] a in listString)
        {
            foreach (string b in a)
            {
                aux += b;
            }
        }
        aux = aux.Trim();

        // Transforming listpv data into lists
        List<string> auxList = new List<string>();
        int k = aux.Length;
        while (k >= 0)
        {
            int index = aux.IndexOf(':', k);
            if (index != -1)
            {
                Debug.Log(aux.Substring(k - 1));
                auxList.Insert(0, aux.Substring(k - 1));
                aux = aux.Remove(index - 1);
                k = aux.Length;

            }
            else
            {
                k--;
            }
        }
        
        string result = "";
        Regex rx = new Regex("^.:(.+?)$");

        foreach (string b in auxList)
        {               
     
            MatchCollection matches = rx.Matches(b);
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                result += groups[1].Value + "?";
                posPitchRoll.Add(float.Parse(groups[1].Value));
            }
        }
        
        // If the position is relative to another position, listpv will send 5 relative values (this causes error)
        if (posPitchRoll.Count == 10)
        {
            for (int i = 0; i < 5; i++)
            {
                counts.Add((int)posPitchRoll[0]);
                posPitchRoll.RemoveAt(0);
            }
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online LISTPV \"" + target.GetComponent<TargetModel>().GetName() + "\". Relative position", false);
            
            return false;
        }
               
        return true;
    }

    /**
     * Recupera los valores de una posición del Scorbot real en la misma posición en la simulación.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @return bool Éxito
     */
    public bool SyncScorbotToSimulation(IK robot, Transform target)
    {        
        // Only online mode

        List<String[]> listString = new List<string[]>();
              
        List<int> counts;
        List<float> posPitchRoll;
        // This stops main thread    
        bool listpv = Listpv(target, out counts, out posPitchRoll);
        if (listpv)
            stateMessageControl.WriteMessage("Done. Online SYNC(LISTPV) \"" + target.GetComponent<TargetModel>().GetName() + "\"", listpv);
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC(LISTPV) \"" + target.GetComponent<TargetModel>().GetName() + "\"", listpv);
            return false;
        }

        if (robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
        {
            for(int i = 0; i < posPitchRoll.Count; i++)           
                posPitchRoll[i] = posPitchRoll[i] / 10f;            
        }           

        // Do teach only in simulation
        bool done = Teach(robot, target, new Vector3(posPitchRoll[0], posPitchRoll[1], posPitchRoll[2]), posPitchRoll[3], posPitchRoll[4], false);
      
        if (done)
        {
            stateMessageControl.WriteMessage("Done. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            target.GetComponent<TargetModel>().SetSync(true);
            if (target.GetComponent<TargetModel>().GetRelativeFrom())
                target.GetComponent<TargetModel>().GetRelativeFrom().GetComponent<TargetModel>().SetSync(true);
            stateMessageControl.UpdatePositionLog();
            return true;
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            return false;
        }
    }
    
    /**
     * Guarda los valores de una posición de la simulación en la misma posición en el Scorbot real.
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @return bool Éxito
     */
    public bool SyncSimulationToScorbot(IK robot, Transform target)
    {
        // defp, in case it doest exist        
        Defp(target);

        // If position is unreachable, error        
        if (!target.GetComponent<TargetModel>().GetValid())
        {
            stateMessageControl.WriteMessage("Error. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\" Invalid position", false);
            return false;
        }
        

        // Teach to real Scorbot
        Vector3 pos = target.GetComponent<TargetModel>().GetPositionInScorbot();
        float p = target.GetComponent<TargetModel>().GetPitch();
        float r = target.GetComponent<TargetModel>().GetRoll();        
        bool done = Teach(robot, target, pos, p, r, true, false);

        if (done)
        {
            stateMessageControl.WriteMessage("Done. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            target.GetComponent<TargetModel>().SetSync(true);
            if (target.GetComponent<TargetModel>().GetRelativeFrom())
                target.GetComponent<TargetModel>().GetRelativeFrom().GetComponent<TargetModel>().SetSync(true);
            stateMessageControl.UpdatePositionLog();
        }
        else
        {
            stateMessageControl.WriteMessage("Error. Online SYNC \"" + target.GetComponent<TargetModel>().GetName() + "\"", done);
            return false;
        }

        return true;
    }
        
    /**
     * Modifica una posición sumando valores los cuales son una posición (x, y,z), inclinación frontal (p) e 
     * inclinación lateral (r). El valor puede ser solo uno de los anteriores y debe estar en el contexto del
     * Scorbot real. byIndex: X, Y, Z, P, R (0..4)
     * @param robot Scorbot
     * @param target Posición (objeto)
     * @param byIndex Índice del tipo de parámetro
     * @param value Valor
     * @return void
     */
    public void Shiftc(IK robot, Transform target, int byIndex, float value)
    {
        // Offline mode        
        string[] parameterName = { "x", "y", "z", "p", "r" };
        // If is a relative position, error
        if (target.GetComponent<TargetModel>().GetRelativeTo() != null)
        {
            stateMessageControl.WriteMessage("Error. SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\" " +
                parameterName[byIndex] + " by " + value + ". Relative position", false);
            return;
        }
            
        // Values from position in real Scorbot context
        Vector3 pos = target.GetComponent<TargetModel>().GetPositionInScorbot();
        float p = target.GetComponent<TargetModel>().GetPitch();
        float r = target.GetComponent<TargetModel>().GetRoll();

        // Apply value to corresponding parameter        
        Vector3 offsetPos = Vector3.zero;
        // byIndex: X, Y, Z, P, R (0..4)
        switch (byIndex)
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

        bool done = Teach(robot, target, pos, p, r, false);
        if (done)
            stateMessageControl.WriteMessage("Done. SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\" " +
                parameterName[byIndex] + " by " + value, done);
        else
            stateMessageControl.WriteMessage("Error. SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\" " +
                parameterName[byIndex] + " by " + value, done);

        // Online mode
        if (gameController.GetOnlineMode())
        {
            if (robot.GetComponent<ScorbotModel>().scorbotIndex == ScorbotERVPlus.INDEX)
            {             
                done = controller.RunCommandUIShiftc(target.GetComponent<TargetModel>().GetName(), parameterName[byIndex],
                (value * 10f).ToString()); // Number format?
            }
            else
            {
                done = controller.RunCommandUIShiftc(target.GetComponent<TargetModel>().GetName(), parameterName[byIndex],
                value.ToString()); // Number format?
            }
            
            if (done)
            {
                stateMessageControl.WriteMessage("Done. Online SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\" " +
                parameterName[byIndex] + " by " + value, done);
                target.GetComponent<TargetModel>().SetSync(true);
                stateMessageControl.UpdatePositionLog();
            }
            else
                stateMessageControl.WriteMessage("Error. Online SHIFTC \"" + target.GetComponent<TargetModel>().GetName() + "\" " +
                parameterName[byIndex] + " by " + value, done);
        }
    }

    /**
     * Mueve el Scorbot a su posición HOME.
     * @param robot Scorbot
     * @return void
     */
    public void Home(IK robot)
    {
        // Offline mode

        // Error: One home does not reach HOME
        robot.Home();
        robot.Home();
        robot.Home();
        stateMessageControl.WriteMessage("Done. HOME", true);

        // Online mode
        if (gameController.GetOnlineMode())
        {
            controller.RunCommandOnline("home");
        }     
    }
     
    /**
     * Activa el control del Scorbot real. El control se desactiva cuando se detecta un fallo mecánico por
     * movimientos como el comando "movec".
     * @return void
     */
    public void CON()
    {
        // Only online mode
        if (gameController.GetOnlineMode())
        {
            // Enable control. Real Scorbot
            controller.RunCommandOnline("con");
            stateMessageControl.WriteMessage("Done. Online CON(Control Enabled)", true);
        }
    }

    /**
     * Modifica la velocidad del comando "Move" en el Scorbot. Límite de 1 a 100.
     * @param robot Scorbot
     * @param value Velocidad. Debe estar entre 1-100
     * @return void
     */
    public void Speed(IK robot, int value)
    {
        // Offline mode
        robot.SetSpeed(value);
        stateMessageControl.WriteMessage("Done. SPEED \"" + robot.GetSpeed() + "\"", true);

        // Online mode
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("speed", value.ToString());
            if (done)
                stateMessageControl.WriteMessage("Done. Online SPEED \"" + value.ToString() + "\"", done);
            else
                stateMessageControl.WriteMessage("Error. Online SPEED \"" + value.ToString() + "\"", done);
        }
    }

    /**
     * Modifica la velocidad del comando "Movel" y "Movec" en el Scorbot. Límite de 1 a 300.
     * @param robot Scorbot
     * @param value Velocidad
     * @return void
     */
    public void SpeedL(IK robot, int value)
    {
        // Offline mode
        robot.SetSpeedL(value);
        stateMessageControl.WriteMessage("Done. SPEED \"" + robot.GetSpeedL() + "\"", true);

        // Online mode
        if (gameController.GetOnlineMode())
        {
            bool done = controller.RunCommandUIOnline("speedl", value.ToString());
            if (done)
                stateMessageControl.WriteMessage("Done. Online SPEEDL \"" + value.ToString() + "\"", done);
            else
                stateMessageControl.WriteMessage("Error. Online SPEEDL \"" + value.ToString() + "\"", done);
        }
    }

    /**
     * Define una posición en el controlador del Scorbot real. La posición creada requiere sincronización.
     * @param target Posición (objeto)
     * @return void
     */
    public void Defp(Transform target)
    {
        // Only online mode
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
