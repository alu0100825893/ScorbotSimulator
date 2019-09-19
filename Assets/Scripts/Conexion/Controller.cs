using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using CommandTerminal;
using System;

public class Controller : MonoBehaviour {

    //Gameobjects vinculado a la Conexión, Terminal y GameController
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
    Vector3 XYZ;
    float P;
    float R;
    Transform Target;
    Transform Target2;

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

    public bool Online_Offline(bool T)
    {
        Online = T;
        if (T)
            return Connection.GetComponent<SerialController>().Open_Port();
        else
            return Connection.GetComponent<SerialController>().Close_Port();
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

    public bool RunCommandUITeachr(string point, string point2, List<float> xyzpr)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerTeachr(point, point2, xyzpr);
    }

    public bool RunCommandUIShiftc(string point, string eje, string value)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerShiftc(point, eje, value);
    }

    public List<string[]> RunCommandListpvOnline(string point)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerListpv(point);
    }

    public void RunCommandOffline(string line_command)
    {
        Term.Input = false;
        Term.Input_text = false;
        bool help = false;
        int value;
        bool isNumeric;

        if (Count_Command_Data >= 1 && Count_Command_Data < 6)
        {
            this.Teach(line_command);
        }

        else
        {
            Term.History.Push(line_command);
            Term.Log(TerminalLogType.Command, "{0}", line_command);

            string[] ListCommandLine = line_command.Split(' ');
            if (ListCommandLine.Count() > 1)
            {
                if (ListCommandLine[1].ToLower().Equals("-h"))
                {
                    Help(ListCommandLine[0].ToLower());
                    Term.Input_View(true);
                    help = true;
                }
                else
                    Target = TargetControl.GetTarget(ListCommandLine[1]);
            }

            if (!help)
            {
                switch (ListCommandLine[0].ToLower())
                {
                    case "home":
                        CommandControl.Home(Robot);
                        Term.Input_View(true);
                        break;
                    case "move":
                        if (Target == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                            CommandControl.Move(Robot, Target);
                        Term.Input_View(true);
                        break;
                    case "movel":
                        if (Target == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                            CommandControl.MoveL(Robot, Target);
                        Term.Input_View(true);
                        break;
                    case "movec":
                        Target2 = TargetControl.GetTarget(ListCommandLine[2]);
                        if (Target == null || Target2 == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                            CommandControl.MoveC(Robot, Target, Target2);
                        Term.Input_View(true);
                        break;
                    case "teach":
                        if (Target == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                        {
                            XYZ = Target.GetComponent<TargetModel>().GetPositionInScorbot();
                            float truncated = (float)(Math.Truncate((double)XYZ.x * 100.0) / 100.0);
                            Term.Log(TerminalLogType.Log, "{0}", "       X -- [" + truncated + "]");
                            Count_Command_Data++;
                            Term.Input_Text(true);
                        }
                        break;
                    case "here":
                        if (Target == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                            CommandControl.Here(Robot, Target);
                        Term.Input_View(true);
                        break;
                    case "teachr":
                        Target2 = TargetControl.GetTarget(ListCommandLine[2]);
                        if (Target == null || Target2 == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else
                        {
                            float truncated = (float)(Math.Truncate((double)Target.position.x * 100.0) / 100.0);
                            Term.Log(TerminalLogType.Log, "{0}", "       X -- [" + truncated + "]");
                            Count_Command_Data++;
                            Term.Input_Text(true);
                        }
                        break;
                    case "speed":
                        //0-100
                        isNumeric = int.TryParse(ListCommandLine[1], out value);
                        if (isNumeric)
                            CommandControl.Speed(Robot, value);
                        Term.Input_View(true);
                        break;
                    case "speedl":
                        //0-300
                        isNumeric = int.TryParse(ListCommandLine[1], out value);
                        if (isNumeric)
                            CommandControl.SpeedL(Robot, value);
                        Term.Input_View(true);
                        break;
                    case "shiftc":
                        if (ListCommandLine.Count() == 5)
                            Term.Log(TerminalLogType.Warning, "{0}", "Comando mal escrito");
                        else if (Target == null)
                            Term.Log(TerminalLogType.Warning, "{0}", "Punto no creado");
                        else {
                            isNumeric = int.TryParse(ListCommandLine[3], out value);
                            float valueFloat;
                            bool isFloat = float.TryParse(ListCommandLine[4], out valueFloat);
                            if (isNumeric && isFloat && (value >=0) && (value <= 4))
                                CommandControl.Shiftc(Robot, Target, value, valueFloat);
                            else
                            {
                                if(isNumeric)
                                    Term.Log(TerminalLogType.Warning, "{0}", "Valor no permitido");
                                else
                                    Term.Log(TerminalLogType.Warning, "{0}", "Eje no permitido");
                            }
                        }
                        Term.Input_View(true);
                        break;
                    case "help":
                        Help();
                        Term.Input_View(true);
                        break;
                    case "clear":
                        Term.Buffer.Clear();
                        Term.Input_View(true);
                        break;
                    default:
                        Term.Log(TerminalLogType.Warning, "{0}", "Comando mal escrito");
                        Term.Input_View(true);
                        break;
                }
            }
        }
        help = false;
    }

    private void Teach(string line_command)
    {
        float value, truncated;
        bool isNumeric = float.TryParse(line_command, out value);

        if (isNumeric)
        {
            if (Count_Command_Data == 1)
            {
                XYZ.x = value;
                truncated = (float)(Math.Truncate((double)XYZ.y * 100.0) / 100.0);
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                Term.Log(TerminalLogType.Log, "{0}", "       Y -- [" + truncated + "]");
                Count_Command_Data++;
                Term.Input_Text(true);
            }
            else if (Count_Command_Data == 2)
            {
                XYZ.y = value;
                truncated = (float)(Math.Truncate((double)XYZ.z * 100.0) / 100.0);
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                Term.Log(TerminalLogType.Log, "{0}", "       Z -- [" + truncated + "]");
                Count_Command_Data++;
                Term.Input_Text(true);
            }
            else if (Count_Command_Data == 3)
            {
                XYZ.z = value;
                truncated = (float)(Math.Truncate((double)Target.GetComponent<TargetModel>().GetPitch() * 100.0) / 100.0);
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                Term.Log(TerminalLogType.Log, "{0}", "       P -- [" + truncated + "]");
                Count_Command_Data++;
                Term.Input_Text(true);
            }
            else if (Count_Command_Data == 4)
            {
                P = value;
                truncated = (float)(Math.Truncate((double)Target.GetComponent<TargetModel>().GetRoll() * 100.0) / 100.0);
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                Term.Log(TerminalLogType.Log, "{0}", "       R -- [" + truncated + "]");
                Count_Command_Data++;
                Term.Input_Text(true);
            }
            else if (Count_Command_Data == 5)
            {
                R = value;
                Term.Log_End(TerminalLogType.Log, "{0}", line_command);
                if(Target2 == null)
                    CommandControl.Teach(Robot, Target, XYZ, P, R);
                else
                    CommandControl.TeachR(Robot, Target, Target2, XYZ, P, R);
                Count_Command_Data = 0;
                Term.Input_View(true);
            }
        }

        else
        {
            Term.Input_Text(true);
        }
    }

    private void Help()
    {
        foreach (var item in ListOfCommand)
            Term.Log(TerminalLogType.Log, "{0}", item.name);
    }

    private void Help(string line_command)
    {
        Command Comando = ListOfCommand.Where(c => c.name.ToLower().Equals(line_command.ToLower())).FirstOrDefault();
        for (int i = 0; i < Comando.arguments.Count; i++)
        {
            Term.Log(TerminalLogType.Log, "     {0} {1}", Comando.arguments[i], Comando.description[i]);
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

    public List<string> ListCommand(string command)
    {
        var list = ListOfCommand.FindAll(c => c.name.ToLower().StartsWith(command.ToLower()));
        List<string> listCommand = new List<string>();
        foreach(var item in list)
        {
            listCommand.Add(item.name);
        }
        return listCommand;
    }
}
