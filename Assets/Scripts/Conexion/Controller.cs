using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using CommandTerminal;

public class Controller : MonoBehaviour {

    public GameObject Terminal;
    public GameObject Connection;
    public GameObject GameC;

    private Terminal Term;
    private SerialController Conexion;

    private TargetControl TargetControl;
    private CommandControl CommandControl;
    private IK Robot;
    private bool Online;
    private bool Data;

    int Count_Command_Data;
    Vector3 xyz;
    Transform target;

    private struct Command
    {
        public string name;
        public List<string> arguments;
        public List<string> description;
    }

    private List<Command> ListOfCommand = new List<Command>();

    // Use this for initialization
    void Start() {
        Load_Command();
        Term = Terminal.GetComponent<Terminal>();
        TargetControl = GameC.GetComponent<TargetControl>();
        CommandControl = GameC.GetComponent<CommandControl>();
        Robot = GameC.GetComponent<GameController>().robot;
        Online = false;
        Data = false;
        Count_Command_Data = 0;
    }

    public void Online_Offline(bool T)
    {
        Online = T;
        if (T)
            Connection.GetComponent<SerialController>().Open_Port();
        else
            Connection.GetComponent<SerialController>().Close_Port();
    }

    public bool IsOnline()
    {
        return Online;
    }

    public void Change_Port(string port)
    {
        Connection.GetComponent<SerialController>().Change_Port(port);
    }

    public string Get_Port()
    {
        return Connection.GetComponent<SerialController>().Get_Port();
    }

    public string[] List_Ports()
    {
        return Connection.GetComponent<SerialController>().List_Ports();
    }

    public void RunCommandOnline(string line_command)
    {
        Term.Input = false;
        Term.Input_text = false;
        if (Term.Buffer.Logs.Count != 0)
        {
            var t = Term.Buffer.Logs.LastOrDefault().message.ToLower().Contains("done");
            var g = Term.Buffer.Logs.LastOrDefault().message.ToLower().Contains("position");
            var k = Term.Buffer.Logs.LastOrDefault().message.ToLower();
            int u = 0;
        }
        if (Term.Buffer.Logs.Count !=0)
            if (Term.Buffer.Logs.LastOrDefault().message.ToLower().Contains("done") || Term.Buffer.Logs.LastOrDefault().message.ToLower().Contains("position"))
                Data = false;

        if (line_command.ToLower() == "clear")
        {
            Term.Buffer.Clear();
            Term.Input = true;
        }
        else if (Data)
        {
            Term.Log_End(TerminalLogType.Log, "{0}", line_command);
            Connection.GetComponent<SerialController>().WriteToController(line_command, "Data");
        }
        else if (line_command.ToLower().Contains("teach") || line_command.ToLower().Contains("setpv"))
        {
            Data = true;
            Term.Log(TerminalLogType.Command, "{0}", line_command);
            Term.History.Push(line_command);
            Connection.GetComponent<SerialController>().WriteToController(line_command, "Data");
        }
        else
        {
            if (line_command.Length != 0)
            {
                Term.Log(TerminalLogType.Command, "{0}", line_command);
                Term.History.Push(line_command);
            }
            Connection.GetComponent<SerialController>().WriteToController(line_command, "NoData");
        }
        
    }

    public bool RunCommandUIOnline(string command, string point, string point2 = "", string param = "")
    {
        return Connection.GetComponent<SerialController>().WriteToControllerFromIU(command, point, point2,param);
    }

    public bool RunCommandUITeach(string point, List<float> xyzpr)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerTeach(point, xyzpr);
    }

    public List<string[]> RunCommandListpvOnline(string point)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerListpv(point);
    }

    public void RunCommandOffline(string line_command)
    {
        string[] ListCommandLine = line_command.Split(' ');

        if (Count_Command_Data !=0 && Count_Command_Data < 6)
        {
            int n;
            bool isNumeric = int.TryParse(line_command, out n);

            if (isNumeric)
            {
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                Term.Log(TerminalLogType.Log, "{0}", "[" + target.position.x + "]");
                Count_Command_Data++;
                Term.Input_Text(true);

            }

           CommandControl.Teach(Robot, TargetControl.GetTarget(ListCommandLine[1]),
           target.position, target.GetComponent<TargetModel>().GetPitch(),
           target.GetComponent<TargetModel>().GetRoll());
        }

        switch (ListCommandLine[0].ToLower())
        {
            case "home":
                CommandControl.Home(Robot);
                break;
            case "move":
                CommandControl.Move(Robot, TargetControl.GetTarget(ListCommandLine[1]));
                break;
            case "movel":
                CommandControl.MoveL(Robot, TargetControl.GetTarget(ListCommandLine[1]));
                break;
            case "movec":
                if (TargetControl.Count() <= 1)
                    return;
                CommandControl.MoveC(Robot, TargetControl.GetTarget(ListCommandLine[1]),
                    TargetControl.GetTarget(ListCommandLine[2]));
                break;
            case "teach":

                break;
            case "here":
                if (TargetControl.Count() <= 0)
                    return;
                CommandControl.Here(Robot, TargetControl.GetTarget(ListCommandLine[1]));
                break;
            case "defp":
                break;
            case "teachr":
                break;
            case "speed":

                break;
            case "speedl":
                break;
            case "shiftc":

                break;
            case "jaw":
                break;
            case "profile":

                break;
            case "moves":
                break;
            case "help":
                break;
            default:
                break;
        }
        //Terminal.GetComponent<Terminal>().Input_View(true);

    }

    private void Teach(string line_command)
    {
        string[] ListCommandLine = line_command.Split(' ');
       

        target = TargetControl.GetTarget(ListCommandLine[1]);
        if (target == null)
            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
        else
        {
            Term.Log(TerminalLogType.Log, "{0}", "[" + target.position.x + "]");
            Count_Command_Data++;
            Term.Input_Text(true);
        }
    }

    private void Load_Command()
    {
        var doc = XDocument.Load("commands.xml");
        var clients = doc.Descendants().Where(e => e.Name.LocalName == "Command").ToList();
        string key;

        foreach (var client in clients)
        {
            key = client.Elements().First(e => e.Name.LocalName == "Name").Value;
            var args = client.Descendants().Where(e => e.Name.LocalName == "Argument").ToList();

            List<string> arg = new List<string>();
            foreach (var argument in args)
            {
                arg.Add(argument.Value);
            }

            var desc = client.Descendants().Where(e => e.Name.LocalName == "Description").ToList();

            List<string> des = new List<string>();
            foreach (var argument in desc)
            {
                des.Add(argument.Value);
            }

            Command command = new Command()
            {
                name = key,
                arguments = arg,
                description = des
            };
            ListOfCommand.Add(command);
        }
    }
}
