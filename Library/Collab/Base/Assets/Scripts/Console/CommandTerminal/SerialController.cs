using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Collections;
using CommandTerminal;
using System.Text;

public class SerialController : MonoBehaviour
{
    // Object from the .Net framework used to communicate with serial devices.
    private SerialPort serialPort;

    public string portName = "COM1";
    public int baudRate = 9600;

    private const int readTimeout = 500;
    private const int writeTimeout = 500;

    // Internal reference to the Thread and the object that runs in it.
    private Thread portReadingThread;

    // Data
    private char[] charData;
    private string strData;
    private int V,T;
    private bool _looping;

    public GameObject Terminal;

    // ------------------------------------------------------------------------
    // Inicia el puerto para realizar la comunicación
    // ------------------------------------------------------------------------

    public void Activar()
    {
        Terminal.GetComponent<Terminal>().Logs(false, "{0}", "Hola");

        _looping = true;
        serialPort = new SerialPort(portName, baudRate);

        if (serialPort == null)
        {
            Terminal.GetComponent<Terminal>().Logs(false, "{0}", "Conexión no establecida");
            return;
        }

        //serialPort.DtrEnable = true;
        serialPort.ReadTimeout = readTimeout;
        serialPort.WriteTimeout = writeTimeout;
        serialPort.Parity = Parity.None;
        serialPort.StopBits = StopBits.One;
        serialPort.DataBits = 8;

        serialPort.Open();

        Terminal.GetComponent<Terminal>().Logs(false, "{0}", serialPort.IsOpen);

        //portReadingThread = new Thread(ReadSerial);
        //portReadingThread.Start();
        
    }

    private void Update()
    {
        V = serialPort.BytesToRead;

        if (V != 0)
        {
            Terminal.GetComponent<Terminal>().Logs(false, "{0}", V);
            T = serialPort.Read(charData, 0, V);
            strData = charData.ToString();

            if (strData != "")
                Terminal.GetComponent<Terminal>().Logs(false, "{0}", strData);
        }
    }


    // ------------------------------------------------------------------------
    // Invocado cuando se destruye el Gameobject
    // Para la lectura del puerto y cierra el mismo.
    // ------------------------------------------------------------------------
    private void OnDestroy()
    {
        _looping = false;  // This is a necessary command to stop the thread.
                           // if you comment this line, Unity gets frozen when you stop the game in the editor.                           
        portReadingThread.Join();
        portReadingThread.Abort();
        serialPort.Close();
    }

    // ---------------------------------------------------------------------------------
    // Escribe mensajes por el puerto
    // ---------------------------------------------------------------------------------

    public void WriteToController(string line_command)
    {
        char[] t = (line_command + "\r").ToCharArray();
        serialPort.Write(t, 0, t.Length);

    }

    // ---------------------------------------------------------------------------------
    // Configura el puerto y comienza a escuchar el puerto por si hay posibles mensajes
    // ---------------------------------------------------------------------------------

    void ReadSerial()
    {
        while (_looping)
        {
            V = serialPort.BytesToRead;
            Terminal.GetComponent<Terminal>().Logs(false, "{0}", V);

            if (V != 0)
            {
                Terminal.GetComponent<Terminal>().Logs(false,"{0}",V);
                T = serialPort.Read(charData, 0, V);
                strData = charData.ToString();

                if (strData != "")
                    Terminal.GetComponent<Terminal>().Logs(false, "{0}", strData);
            }

            Thread.Sleep(2000);
        }
    }
}
