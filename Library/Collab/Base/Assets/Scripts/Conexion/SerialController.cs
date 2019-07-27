using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using CommandTerminal;
using System.Linq;

public class SerialController : MonoBehaviour
{
    // Objeto usado para la comunicacón serial
    private SerialPort serialPort;
    private Thread portReadingThread;

    //Configuración del puerto
    private string portName = "COM1";
    private int baudRate = 9600;
    private const int readTimeout = 700;
    private const int writeTimeout = 500;

    // Gameobjects vinculado a la Terminal
    public GameObject T;
    private Terminal Terminal;

    //Ultimo caracter leido del puerto
    char last_character;

    private void Start()
    {
        Terminal = T.GetComponent<Terminal>();
        last_character = ' ';
    }

    // ------------------------------------------------------------------------
    // Inicia el puerto para realizar la comunicación
    // ------------------------------------------------------------------------

    public bool Open_Port()
    {
        bool _looping = true;
        char u = ' ';
        try
        {
            serialPort = new SerialPort(portName, baudRate);
        }
        catch (Exception ex)
        {
            return false;
        }

        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;
        serialPort.Handshake = Handshake.None;
        serialPort.ReadTimeout = readTimeout;
        serialPort.WriteTimeout = writeTimeout;
        serialPort.Parity = Parity.None;
        serialPort.StopBits = StopBits.One;
        serialPort.DataBits = 8;

        try
        {
            serialPort.Open();
            char[] t = ("A" + "\r").ToCharArray();
            serialPort.Write(t, 0, t.Length);
            while (_looping)
            {
                try
                {
                    u = (char)serialPort.ReadChar();
                }
                catch (Exception ex)
                { }

                if (u == '>')
                    _looping = false;
            }
        }
        catch (Exception ex)
        {
            return false;  
        }
        return true;
    }

    // ------------------------------------------------------------------------
    // Cierra el puerto
    // ------------------------------------------------------------------------
    public void Close_Port()
    {
        if (serialPort != null)
            serialPort.Close();

        if (portReadingThread != null)
        {
            portReadingThread.Join();
            portReadingThread.Abort();
        }
    }

    private void OnDestroy()
    {
        Close_Port();
    }

    // ------------------------------------------------------------------------
    // Cierra el puerto
    // ------------------------------------------------------------------------

    public void Change_Port(string port)
    {
        portName = port;
    }

    public string Get_Port()
    {
        return portName;
    }

    public string[] List_Port()
    {
        return SerialPort.GetPortNames();
    }

    
    // ---------------------------------------------------------------------------------
    // Escribe mensajes por el puerto
    // ---------------------------------------------------------------------------------

    public void WriteToController(string line_command)
    {
        bool _looping = true;
        char u = ' ';
        char[] t = (line_command + "\r").ToCharArray();
        string[] ListCommandLine = line_command.Split(' ');

        for (int i = 0; i < t.Length; i++)
        {
            serialPort.Write(t, i, 1);
            while (_looping)
            {
                Thread.Sleep(10);
                try
                {
                    u = (char)serialPort.ReadChar();
                }
                catch (Exception ex)
                {}

                if (u == t[i])
                    _looping = false;
            }
            _looping = true;
        }
                
        portReadingThread = new Thread(ReadSerial);
        portReadingThread.Start();
    }

    // ---------------------------------------------------------------------------------
    // Lectura del bufer de entrada char a char
    // ---------------------------------------------------------------------------------

    private void ReadSerial()
    {
        char u = ' ';
        string strData = "";
        bool _looping = true;

        while (_looping)
        {     
            Thread.Sleep(10);
            try
            {
                u = (char)serialPort.ReadChar();
               
                if (u == '\r')
                {
                    last_character = '\r';
                    if (strData.Contains("Done"))
                        Terminal.Log(TerminalLogType.Success, "{0}", strData.Replace("\n", ""));
                    else if (strData.Contains("Invalid") || strData.Contains("Unknown"))
                        Terminal.Log(TerminalLogType.Warning, "{0}", strData.Replace("\n", ""));
                    else
                        Terminal.Log(TerminalLogType.Log, "{0}", strData.Replace("\n", ""));
                    strData = "";
                }

                if (u == '>')
                {
                    if (last_character == ' ')
                    {
                        Terminal.Input_Text(true);
                        Terminal.Log(TerminalLogType.Warning, "{0}", strData.Replace("\n", ""));
                    }
                    else
                    {
                        Terminal.Input_View(true);
                        Terminal.Input_Text(false);
                    }

                    last_character = ' ';
                    _looping = false;
                }
                strData = strData + u.ToString();
            }
            catch (Exception ex)
            {}
        }

        portReadingThread.Abort();
        portReadingThread.Join(); 
    }

    // --------------------------------------------------------------------------------------
    // Comandos ejecutados desde la interfaz y no necesitan devolucion de datos 
    // --------------------------------------------------------------------------------------

    public bool WriteToControllerFromIU(string command, string point, string point2 = "")
    {
        char[] t = (command + " " + point + " " + point2 + "\r").ToCharArray();
        bool _looping = true;
        char u = ' ';
        string tmp = "";

        for (int i = 0; i < t.Length; i++)
        {
            serialPort.Write(t, i, 1);
            while (_looping)
            {
                Thread.Sleep(10);
                try
                {
                    u = (char)serialPort.ReadChar();
                }
                catch (Exception ex)
                { }

                if (u == t[i])
                    _looping = false;
            }
            _looping = true;
        }

        while (_looping)
        {
            Thread.Sleep(10);
            try
            {
                u = (char)serialPort.ReadChar();

                if (u == '>')
                {
                    _looping = false;
                }
                tmp += u.ToString();
            }
            catch (Exception ex)
            {
            }
        }
        Debug.Log(tmp.Contains("Done"));
        return tmp.Contains("Done");
    }

    // --------------------------------------------------------------------------------------
    // Comando teach 
    // --------------------------------------------------------------------------------------

    public bool WriteToControllerTeach(string point , List<float> xyzpr)
    {
        char[] t = ("teach " + point + "\r").ToCharArray();
        bool _looping = true;
        char u = ' ';
        string strData = "";
        int count = 0;

        for (int i = 0; i < t.Length; i++)
        {
            serialPort.Write(t, i, 1);
            while (_looping)
            {
                Thread.Sleep(10);
                try
                {
                    u = (char)serialPort.ReadChar();
                }
                catch (Exception ex)
                { }

                if (u == t[i])
                    _looping = false;
            }
            _looping = true;
        }

        while (_looping)
        {
            Thread.Sleep(10);
            try
            {
                u = (char)serialPort.ReadChar();

                if (u == '>')
                {
                    if (count < 5)
                    {
                        char[] p = (xyzpr[count].ToString("0.000") + "\r").ToCharArray();
                        serialPort.Write(p, 0, p.Length);
                        count++;
                        strData = "";
                    }
                    else
                    {
                        _looping = false;
                    }
                    
                }
                strData += u.ToString();
            }
            catch (Exception ex)
            {
            }
        }
        Debug.Log(strData.Contains("Done"));
        return strData.Contains("Done");
    }

    // --------------------------------------------------------------------------------------
    // Comando Listpv de el punto pasado y devuelve el valor de los encoder 
    // --------------------------------------------------------------------------------------

    public List<string[]> WriteToControllerListpv(string point)
    {
        char[] t = ("listpv " + point + "\r").ToCharArray();
        bool _looping = true;
        char u = ' ';

        for (int i = 0; i < t.Length; i++)
        {
            serialPort.Write(t, i, 1);
            while (_looping)
            {
                Thread.Sleep(10);
                try
                {
                    u = (char)serialPort.ReadChar();
                }
                catch (Exception ex)
                {
                }
                if (u == t[i])
                    _looping = false;
            }
            _looping = true;
        }
        return ReadSerialListpv();
    }

    List<string[]> ReadSerialListpv()
    {
        char u = ' ';
        bool _looping = true;
        bool Once = false;
        string strData = "";
        string[] P;
        List<string[]> Param = new List<string[]>();

        while (_looping)
        {
            try
            {
                u = (char)serialPort.ReadChar();

                if (u == '\r')
                {
                    if (Once)
                    {
                        P = (strData.Split(' '));
                        P = P.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        Param.Add(P);
                    }
                    Once = true;
                    strData = "";
                }

                if (u == '>')
                {
                    _looping = false;
                }

                strData = strData + u.ToString();
            }
            catch (Exception ex)
            {

            }

        }
        return (Param);
    }
}
