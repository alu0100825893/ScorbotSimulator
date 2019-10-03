using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es la restauración de los datos de las posiciones de la simulación
 * en caso de que un comando en el Scorbot real bloque el programa. Las posiciones fuera del alcance o que no
 * se pueden cumplir las restricciones de pitch, se pondrán en una posición por defecto. ANTES de CARGAR los
 * datos SELECCIONAR el SCORBOT deseado. (normal|relative) name x y z p r (relativeTo).
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.1
 * @since 03-10-2019
 */
public class BackUpFileControl : MonoBehaviour {

    // Constans
    private const string BACKUP_FILE = "backup.txt";
    private const string NORMAL = "normal";
    private const string RELATIVE = "relative";

    // Controllers
    private TargetControl targetControl;
    private GameController gameController;
    private CommandControl commandControl;

	void Start () {
        targetControl = GetComponent<TargetControl>();
        gameController = GetComponent<GameController>();
        commandControl = GetComponent<CommandControl>();
    }

    /**
     * Guarda las posiciones actuales y sus datos en el fichero "backup.txt". (normal|relative) name x y z p r (relativeTo). 
     * @return void
     */
    public void Save()
    {
        string[] lines = new string[targetControl.Count()];

        // Get positions data
        for (int i = 0; i < targetControl.Count(); i++)
        {
            string aux = "";
            TargetModel target = targetControl.GetTarget(i).GetComponent<TargetModel>();

            // Relative position?
            if (target.GetRelativeTo() == null)
            {
                aux += NORMAL + " ";
                aux += target.GetName() + " ";
                aux += target.GetPositionInScorbot().x + " ";
                aux += target.GetPositionInScorbot().y + " ";
                aux += target.GetPositionInScorbot().z + " ";
                aux += target.GetPitch() + " ";
                aux += target.GetRoll();
            }
            else
            {
                aux += RELATIVE + " ";
                aux += target.GetName() + " ";
                aux += target.GetRelativePosInScorbot().x + " ";
                aux += target.GetRelativePosInScorbot().y + " ";
                aux += target.GetRelativePosInScorbot().z + " ";
                aux += target.GetRelativeP() + " ";
                aux += target.GetRelativeR() + " "; 
                aux += target.GetRelativeTo().GetComponent<TargetModel>().GetName();
            }
            
            lines[i] = aux;
        }
                
        // Overwrite everything
        System.IO.File.WriteAllLines(BACKUP_FILE, lines);

        gameController.backupFileOutput.text = "File saved.";
    }

    /**
     * Lee el fichero "backup.txt" y carga las posiciones y sus datos. (normal|relative) name x y z p r (relativeTo). 
     * Las posiciones fuera del alcance o que no se pueden cumplir las restricciones de pitch, se pondrán en una 
     * posición por defecto. ANTES de CARGAR los datos SELECCIONAR el SCORBOT deseado.
     * @return void
     */
    public void Load()
    {
        try
        {
            int error = 0;
            // Relative positions
            List<string[]> names = new List<string[]>();
            List<float[]> values = new List<float[]>();


            // Read file
            string[] lines = System.IO.File.ReadAllLines(BACKUP_FILE);

            // Transform data
            foreach (string line in lines)
            {
                // Target values
                string[] target = line.Split(' ');

                bool relative = (target[0] == RELATIVE) ? true : false;
                string name = target[1];

                Vector3 pos = new Vector3(float.Parse(target[2]), float.Parse(target[3]), float.Parse(target[4]));
                float p, r;
                p = float.Parse(target[5]);
                r = float.Parse(target[6]);

                // Create position
                gameController.CreateDefaultTarget(name);

                // Realative positions will be calculated after, in case it needs a position not loaded yet
                if (relative)
                {
                    string[] n = { name, target[7] };
                    names.Add(n);
                    float[] v = { pos.x, pos.y, pos.z, p, r};
                    values.Add(v);
                    continue;
                }

                // Update position data
                Transform t = targetControl.GetTarget(name);
                if (!commandControl.Teach(gameController.robot, t, pos, p, r, false, true))
                    error++;
            }

            // Relative positions
            for(int i = 0; i < names.Count; i++)
            {                
                // Update position data
                Transform t = targetControl.GetTarget(names[i][0]);
                Transform relativeTo = targetControl.GetTarget(names[i][1]);
                Vector3 pos = new Vector3(values[i][0], values[i][1], values[i][2]);
                if (!commandControl.TeachR(gameController.robot, t, relativeTo, pos, values[i][3], values[i][4], 
                    false, true))
                    error++;
            }

            gameController.backupFileOutput.text = "File loaded. ";
            if (error >= 1)
                gameController.backupFileOutput.text += error + " Error(s). Did you select right Scorbot? ";
        }
        catch (Exception e)
        {
            gameController.backupFileOutput.text = "File not found.";
        }
    }
}
