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

    private TargetControl TargetControl;
    private CommandControl CommandControl;
    private IK Robot;
    private bool Online;

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
        TargetControl = GameC.GetComponent<TargetControl>();
        CommandControl = GameC.GetComponent<CommandControl>();
        Robot = GameC.GetComponent<GameController>().robot;
        Online = false;

        GameController.OnlineDel += Online_Offline;
    }

    private void Online_Offline(bool T)
    {
        Online = T;
        if (Online)
            Connection.GetComponent<SerialController>().Open_Port();
        else
            Connection.GetComponent<SerialController>().Close_Port();
    }

    public bool IsOnline()
    {
        return Online;
    }

    public void RunCommandOffline(string line_command)
    {
        string[] ListCommandLine = line_command.Split(' ');

        switch (ListCommandLine[0].ToLower())
        {
            case "home":
                CommandControl.Home(Robot);
                break;
            case "move":
                break;
            case "movel":
                CommandControl.Home(Robot);
                break;
            case "movec":
                CommandControl.Home(Robot);
                break;
            case "teach":
                /* */

                Transform target = TargetControl.GetTarget(ListCommandLine[1]);
                //target.position;
                //target.GetComponent<TargetModel>().GetPitch();
                //target.GetComponent<TargetModel>().GetRoll();


                //CommandControl.Teach(Robot, TargetControl.GetTarget(ListCommandLine[1]),);
                break;
            case "here":
                CommandControl.Home(Robot);
                break;
            case "defp":
                CommandControl.Home(Robot);
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
        Terminal.GetComponent<Terminal>().Input_View(true);

    }

    public void RunCommandOnline(string line_command)
    {
        Connection.GetComponent<SerialController>().WriteToController(line_command);
    }

    public void RunCommandUIOnline(string command, string point, string point2 = "")
    {
        Connection.GetComponent<SerialController>().WriteToControllerFromIU(command, point, point2);
    }

    public List<string[]> RunCommandListpvOnline(string point)
    {
        return Connection.GetComponent<SerialController>().WriteToControllerListpv(point);
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
