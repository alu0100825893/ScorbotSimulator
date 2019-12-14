using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Permite la modificación de la posición del efector final de los Scorbots mediante un fichero.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.11
 * @since 13-12-2019
 */
public class EffectorFileControl : MonoBehaviour {
    
    // Constants
    private const string SCORBOTIX_EFFECTOR_FILE = "ScorbotIXEffectorPosition.txt";
    private const string SCORBOTVPLUS_EFFECTOR_FILE = "ScorbotVPlusEffectorPosition.txt";
    public const string NUMBER_FORMAT = "0.00";

    // Controllers
    private GameController gameController;
    private StateMessageControl stateMessageControl;

    void Start () {
        gameController = GetComponent<GameController>();
        stateMessageControl = GetComponent<StateMessageControl>();
    }

    /**
	 * Carga las coordenadas x y z del efector final de los Scorbots. Contexto de los Scorbots reales. 
     * La carga se hace mediante fichero o se usa el que hay por defecto en el programa.
     * @param scorbots Array de Scorbots
	 * @return void
	 */
    public void Load(IK[] scorbots)
    {
        foreach(IK scorbot in scorbots)
        {
            string file = "";
            switch(scorbot.GetComponent<ScorbotModel>().scorbotIndex)
            {
                case ScorbotERIX.INDEX:
                    file = SCORBOTIX_EFFECTOR_FILE;
                    break;
                case ScorbotERVPlus.INDEX:
                    file = SCORBOTVPLUS_EFFECTOR_FILE;
                    break;
                default: // Skip unknown scorbot
                    continue;
            }

            LoadEffector(file, scorbot);
        }        
    }

    /**
	 * Carga las coordenadas x y z del efector final de un Scorbot. Contexto del Scorbot real. 
     * La carga se hace mediante fichero o se usa el que hay por defecto en el programa.
     * @param file Fichero
     * @param scorbot Scorbot
	 * @return void
	 */
    private void LoadEffector(string file, IK scorbot)
    {
        try
        {          
            // Inicial position. HOME          
            scorbot.GetComponent<ScorbotModel>().GoHome();

            List<float[]> values = new List<float[]>();
            // Read file
            string[] lines = System.IO.File.ReadAllLines(file);

            // Effector values. First line
            string[] effectorPos = lines[0].Split(' ');

            // Expected coordinates x, y, z
            if (effectorPos.Length == 3)
            {
                Vector3 pos = (new Vector3(float.Parse(effectorPos[0]), float.Parse(effectorPos[2]), float.Parse(effectorPos[1])) / 10f);
                scorbot.GetComponent<ScorbotModel>().E.position = pos;
         
                // Updating copy end effector 
                scorbot.UpdateCopyEffector();

                stateMessageControl.WriteMessage("Done. Effector values loaded (From file). " + scorbot.name , true);
            }
            else // Error
            {
                stateMessageControl.WriteMessage("Error. Effector values not valid (From file). " + scorbot.name, false);
            }
            
        }
        catch (Exception e) // File not found
        {
            // Default effector
            Vector3 effectorPos = scorbot.GetComponent<ScorbotModel>().E.position * 10f;

            // Oneline
            string[] lines = new string[] {
                effectorPos.x.ToString(NUMBER_FORMAT) + " " +
                effectorPos.z.ToString(NUMBER_FORMAT) + " " +
                effectorPos.y.ToString(NUMBER_FORMAT)
            };

            // Overwrite everything
            System.IO.File.WriteAllLines(file, lines);        
            stateMessageControl.WriteMessage("Done. Default effector (Not from file)." + scorbot.name, true);
        }
    }
}
