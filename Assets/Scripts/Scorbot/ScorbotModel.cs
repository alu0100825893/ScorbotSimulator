using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/**
 * Estructura de datos para un Scorbot. Contiene las articulationes y carga la configuración de un Scorbot, además
 * de realizar operaciones como abrir/cerrar la pinza. También permiter copiar la configuración actuar y transformar
 * conteos de encoder a ángulos. Un Scorbot necesita este componente, el componente de cinemática inversa "IK" y
 * el componente de su pinza "GripScorbot...". Además, su modelo 3D debe tener en cada articulatión el componente
 * "Articulation", un componente de detección de colisiones "Mesh Collider" y sus etiquetas (articulaciones) 
 * deben ser "Model".
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ScorbotModel : MonoBehaviour {

    // Target position
    public Transform D;
    // End effector
    public Transform E;
    // Articulations
    public Articulation[] articulations;
    // Set this to specific Scorbot index in Unity Inspector
    public int scorbotIndex;

    // Constants
    private const string SCORBOTIX_FILE = "ScorbotIXConfig.txt";
    private const string SCORBOTVPLUS_FILE = "ScorbotVPlusConfig.txt";

    private void Awake()
    {
        // Load config from 1 Scorbot
        switch (scorbotIndex) {
            case ScorbotERIX.INDEX:
                // Planes
                articulations[0].SetPlane(PlaneHelper.XZ);
                articulations[1].SetPlane(PlaneHelper.XY);
                articulations[2].SetPlane(PlaneHelper.XY);
                articulations[3].SetPlane(PlaneHelper.XY);
                articulations[4].SetPlane(PlaneHelper.YZ);

                try
                {
                    LoadFromFile(SCORBOTIX_FILE, articulations);
                }
                catch (Exception e) // Error in file, just use default config and save new file
                {
                    SaveDefaultToFile(SCORBOTIX_FILE, typeof(ScorbotERIX));
                    
                    //Angle limit
                    articulations[0].SetLimit(ScorbotERIX.BASE_COUNT_MIN, ScorbotERIX.BASE_COUNT_MAX,
                        ScorbotERIX.BASE_COUNT_HOME, ScorbotERIX.BASE_DEGREES_MAX, ScorbotERIX.BASE_OFFSET_UNITY);
                    articulations[1].SetLimit(ScorbotERIX.SHOULDER_COUNT_MIN, ScorbotERIX.SHOULDER_COUNT_MAX,
                        ScorbotERIX.SHOULDER_COUNT_HOME, ScorbotERIX.SHOULDER_DEGREES_MAX, ScorbotERIX.SHOULDER_OFFSET_UNITY);
                    articulations[2].SetLimit(ScorbotERIX.ELBOW_COUNT_MIN, ScorbotERIX.ELBOW_COUNT_MAX,
                        ScorbotERIX.ELBOW_COUNT_HOME, ScorbotERIX.ELBOW_DEGREES_MAX, ScorbotERIX.ELBOW_OFFSET_UNITY);
                    articulations[3].SetLimit(ScorbotERIX.PITCH_COUNT_MIN, ScorbotERIX.PITCH_COUNT_MAX,
                        ScorbotERIX.PITCH_COUNT_HOME, ScorbotERIX.PITCH_DEGREES_MAX, ScorbotERIX.PITCH_OFFSET_UNITY);
                    articulations[4].SetLimit(ScorbotERIX.ROLL_COUNT_MIN, ScorbotERIX.ROLL_COUNT_MAX,
                        ScorbotERIX.ROLL_COUNT_HOME, ScorbotERIX.ROLL_DEGREES_MAX, ScorbotERIX.ROLL_OFFSET_UNITY);
                    // Speed limit
                    articulations[0].SetSpeed(ScorbotERIX.BASE_SPEED_MIN, ScorbotERIX.BASE_SPEED_MAX);
                    articulations[1].SetSpeed(ScorbotERIX.SHOULDER_SPEED_MIN, ScorbotERIX.SHOULDER_SPEED_MAX);
                    articulations[2].SetSpeed(ScorbotERIX.ELBOW_SPEED_MIN, ScorbotERIX.ELBOW_SPEED_MAX);
                    articulations[3].SetSpeed(ScorbotERIX.PITCH_SPEED_MIN, ScorbotERIX.PITCH_SPEED_MAX);
                    articulations[4].SetSpeed(ScorbotERIX.ROLL_SPEED_MIN, ScorbotERIX.ROLL_SPEED_MAX);
                }
                break;
            case ScorbotERVPlus.INDEX:
                // Planes
                articulations[0].SetPlane(PlaneHelper.XZ);
                articulations[1].SetPlane(PlaneHelper.XY);
                articulations[2].SetPlane(PlaneHelper.XY);
                articulations[3].SetPlane(PlaneHelper.XY);
                articulations[4].SetPlane(PlaneHelper.YZ);

                try
                {
                    LoadFromFile(SCORBOTVPLUS_FILE, articulations);
                }
                catch (Exception e) // Error in file, just use default config and save new file
                {
                    SaveDefaultToFile(SCORBOTVPLUS_FILE, typeof(ScorbotERVPlus));
                    
                    // Angle limit
                    articulations[0].SetLimit(ScorbotERVPlus.BASE_COUNT_MIN, ScorbotERVPlus.BASE_COUNT_MAX,
                        ScorbotERVPlus.BASE_COUNT_HOME, ScorbotERVPlus.BASE_DEGREES_MAX, ScorbotERVPlus.BASE_OFFSET_UNITY);
                    articulations[1].SetLimit(ScorbotERVPlus.SHOULDER_COUNT_MIN, ScorbotERVPlus.SHOULDER_COUNT_MAX,
                        ScorbotERVPlus.SHOULDER_COUNT_HOME, ScorbotERVPlus.SHOULDER_DEGREES_MAX, ScorbotERVPlus.SHOULDER_OFFSET_UNITY);
                    articulations[2].SetLimit(ScorbotERVPlus.ELBOW_COUNT_MIN, ScorbotERVPlus.ELBOW_COUNT_MAX,
                        ScorbotERVPlus.ELBOW_COUNT_HOME, ScorbotERVPlus.ELBOW_DEGREES_MAX, ScorbotERVPlus.ELBOW_OFFSET_UNITY);
                    articulations[3].SetLimit(ScorbotERVPlus.PITCH_COUNT_MIN, ScorbotERVPlus.PITCH_COUNT_MAX,
                        ScorbotERVPlus.PITCH_COUNT_HOME, ScorbotERVPlus.PITCH_DEGREES_MAX, ScorbotERVPlus.PITCH_OFFSET_UNITY);
                    articulations[4].SetLimit(ScorbotERVPlus.ROLL_COUNT_MIN, ScorbotERVPlus.ROLL_COUNT_MAX,
                        ScorbotERVPlus.ROLL_COUNT_HOME, ScorbotERVPlus.ROLL_DEGREES_MAX, ScorbotERVPlus.ROLL_OFFSET_UNITY);
                    // Speed limit
                    articulations[0].SetSpeed(ScorbotERVPlus.BASE_SPEED_MIN, ScorbotERVPlus.BASE_SPEED_MAX);
                    articulations[1].SetSpeed(ScorbotERVPlus.SHOULDER_SPEED_MIN, ScorbotERVPlus.SHOULDER_SPEED_MAX);
                    articulations[2].SetSpeed(ScorbotERVPlus.ELBOW_SPEED_MIN, ScorbotERVPlus.ELBOW_SPEED_MAX);
                    articulations[3].SetSpeed(ScorbotERVPlus.PITCH_SPEED_MIN, ScorbotERVPlus.PITCH_SPEED_MAX);
                    articulations[4].SetSpeed(ScorbotERVPlus.ROLL_SPEED_MIN, ScorbotERVPlus.ROLL_SPEED_MAX);
                }
                break;
            case ScorbotERIXV2.INDEX:
                // Planes
                articulations[0].SetPlane(PlaneHelper.XZ);
                articulations[1].SetPlane(PlaneHelper.XY);
                articulations[2].SetPlane(PlaneHelper.XY);
                articulations[3].SetPlane(PlaneHelper.XY);
                articulations[4].SetPlane(PlaneHelper.YZ);
                // Angle limit
                articulations[0].SetLimit(ScorbotERIXV2.BASE_COUNT_MIN, ScorbotERIXV2.BASE_COUNT_MAX,
                    ScorbotERIXV2.BASE_COUNT_HOME, ScorbotERIXV2.BASE_DEGREES_MAX, ScorbotERIXV2.BASE_OFFSET_UNITY);
                articulations[1].SetLimit(ScorbotERIXV2.SHOULDER_COUNT_MIN, ScorbotERIXV2.SHOULDER_COUNT_MAX,
                    ScorbotERIXV2.SHOULDER_COUNT_HOME, ScorbotERIXV2.SHOULDER_DEGREES_MAX, ScorbotERIXV2.SHOULDER_OFFSET_UNITY);
                articulations[2].SetLimit(ScorbotERIXV2.ELBOW_COUNT_MIN, ScorbotERIXV2.ELBOW_COUNT_MAX,
                    ScorbotERIXV2.ELBOW_COUNT_HOME, ScorbotERIXV2.ELBOW_DEGREES_MAX, ScorbotERIXV2.ELBOW_OFFSET_UNITY);
                articulations[3].SetLimit(ScorbotERIXV2.PITCH_COUNT_MIN, ScorbotERIXV2.PITCH_COUNT_MAX,
                    ScorbotERIXV2.PITCH_COUNT_HOME, ScorbotERIXV2.PITCH_DEGREES_MAX, ScorbotERIXV2.PITCH_OFFSET_UNITY);
                articulations[4].SetLimit(ScorbotERIXV2.ROLL_COUNT_MIN, ScorbotERIXV2.ROLL_COUNT_MAX,
                    ScorbotERIXV2.ROLL_COUNT_HOME, ScorbotERIXV2.ROLL_DEGREES_MAX, ScorbotERIXV2.ROLL_OFFSET_UNITY);
                // Speed limit
                articulations[0].SetSpeed(ScorbotERIXV2.BASE_SPEED_MIN, ScorbotERIXV2.BASE_SPEED_MAX);
                articulations[1].SetSpeed(ScorbotERIXV2.SHOULDER_SPEED_MIN, ScorbotERIXV2.SHOULDER_SPEED_MAX);
                articulations[2].SetSpeed(ScorbotERIXV2.ELBOW_SPEED_MIN, ScorbotERIXV2.ELBOW_SPEED_MAX);
                articulations[3].SetSpeed(ScorbotERIXV2.PITCH_SPEED_MIN, ScorbotERIXV2.PITCH_SPEED_MAX);
                articulations[4].SetSpeed(ScorbotERIXV2.ROLL_SPEED_MIN, ScorbotERIXV2.ROLL_SPEED_MAX);
                break;
        }
    }

    /**
	 * Crea un fichero con la configuracion del Scorbot por defecto.
     * @param file Fichero nuevo
     * @param type Clase de configuracion del Scorbot
	 * @return void
	 */
    private void SaveDefaultToFile(string file, System.Type type)
    {     
        string[] lines = new string[41];
        int lineNumber = 0;

        foreach (FieldInfo info in type .GetFields().Where(x => x.IsStatic && x.IsLiteral))
        {
            lines[lineNumber] = info.Name + " " + info.GetValue(info);
            lineNumber++;           
        }
        System.IO.File.WriteAllLines(file, lines);
    }

    /**
	 * Carga un fichero para la configuracion del Scorbot.
     * @param file Fichero
     * @param articulations Articulaciones del Scorbot
	 * @return void
	 */
    private void LoadFromFile(string file, Articulation[] articulations)
    {   
        // Read file
        string[] lines = System.IO.File.ReadAllLines(file);

        // Get configuration
        string[] config = new string[41];
        for(int i = 0; i < lines.Length; i++)
        {
            // Separate name and value
            string[] values = lines[i].Split(' ');
            // Save value
            config[i] = values[1];
        }

        // Load configuration
        int configPos = 1;
        for(int i = 0; i < articulations.Length; i++)
        {
            // Angle limit
            articulations[i].SetLimit(int.Parse(config[configPos]), int.Parse(config[configPos+1]),
                int.Parse(config[configPos+2]), float.Parse(config[configPos+4]), float.Parse(config[configPos+5]));
            // Speed limit
            articulations[i].SetSpeed(float.Parse(config[configPos + 6]), float.Parse(config[configPos + 7]));
            configPos += 8;
        }
    }

    /**
     * Copia las articulaciones y efector final del Scorbot de este modelo (toda la configuración), 
     * para dejarlos en las variables pasadas.
     * @param art Articulationes
     * @param artE Efector final
     * @return void
     */
    public void InitToCopy(out Articulation[] art, out Transform artE)
    {       
        GameObject g;
        art = new Articulation[articulations.Length];
        for (int i = 0; i < articulations.Length; i++)
        {
            g = new GameObject();
            g.name = "CopyArticulation" + scorbotIndex;
            g.transform.position = articulations[i].transform.position;
            g.AddComponent<Articulation>();
            // Copy articulation config
            g.GetComponent<Articulation>().SetPlane(articulations[i].GetPlane());
            g.GetComponent<Articulation>().SetLimit(articulations[i].lowerLimitCount, articulations[i].upperLimitCount, articulations[i].homeCount,
                articulations[i].degrees, articulations[i].offset);
            g.GetComponent<Articulation>().SetCountsAreTransformed(articulations[i].CountsAreTransformed());
            // Parenting
            art[i] = g.GetComponent<Articulation>();

            if (i >= 1)
            {
                art[i].transform.parent = art[i - 1].transform;
            }
        }
        // Copy E config
        g = new GameObject();
        g.name = "CopyE" + scorbotIndex;
        g.transform.position = E.position;
        g.transform.parent = art[art.Length - 1].transform;
        artE = g.transform;
    }

    /**
     * Copia los ángulos de las articulaciones de este modelo en las articulaciones pasadas.
     * @param art Articulationes
     * @return void
     */
    public void UpdateToCopy(Articulation[] art)
    {
        for (int i = 0; i < art.Length; i++)
        {
            art[i].SetAngle(articulations[i].GetAngle());
        }
    }

    /**
     * Transforma conteos de encoder a ángulos de las articulaciones del Scorbot de este modelo.
     * @param counts Conteos de encoder
     * @return List<Vector3> Ángulos
     */
    public List<Vector3> CountsToAngles(List<int> counts)
    {
        List<Vector3> angles = new List<Vector3>();

        for(int i = 0; i < articulations.Length; i++)
        {
            angles.Add(articulations[i].CountToAngle(counts[i]));
        }
        return angles;
    }

    /**
     * Obtiene la posición del efector final en las articulaciones pasadas con los ángulos propuestos.
     * @param art Articulationes
     * @param artE Efector final
     * @param angles Ángulos
     * @return Vector3 Coordenadas
     */
    public Vector3 GetPosFromAngles(Articulation[] art, Transform artE, List<Vector3> angles)
    {       
        for (int i = 0; i < art.Length; i++)
        {
            art[i].SetAngle(angles[i]);
        }        
        return artE.position;
    }

    /**
     * Cambioa los ángulos de las articulaciones del Scorbot de este modelo a otros ángulos.
     * @param angles Ángulos
     * @return void
     */
    public void SetAngles(List<Vector3> angles)
    {
        for (int i = 0; i < articulations.Length; i++)
        {
            articulations[i].SetAngle(angles[i]);
        }
    }

    /**
     * Abre la pinza del Scorbot.
     * @return void
     */
    public void Open()
    {
        switch (scorbotIndex)
        {
            case ScorbotERIX.INDEX:
                GetComponent<GripScorbotERIX>().Open();
                break;
            case ScorbotERVPlus.INDEX:
                GetComponent<GripScorbotERVPlus>().Open();
                break;
            case ScorbotERIXV2.INDEX:
                GetComponent<GripScorbotERIX>().Open();
                break;
        }
    }

    /**
     * Cierra la pinza del Scorbot.
     * @return void
     */
    public void Close()
    {
        switch (scorbotIndex)
        {
            case ScorbotERIX.INDEX:
                GetComponent<GripScorbotERIX>().Close();
                break;
            case ScorbotERVPlus.INDEX:
                GetComponent<GripScorbotERVPlus>().Close();
                break;
            case ScorbotERIXV2.INDEX:
                GetComponent<GripScorbotERIX>().Close();
                break;
        }
    }

    public void GoHome()
    {
        for (int i = 0; i < articulations.Length; i++)
        {
            articulations[i].SetAngle(articulations[i].GetAngleHome());
        }
    }
}
