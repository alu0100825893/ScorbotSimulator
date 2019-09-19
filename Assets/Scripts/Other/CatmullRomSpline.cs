using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este algoritmo es generar la trayectoria de movimiento del Scorbot en el comando "movec". 
 * El algoritmo de spline de Catmull-Rom es una técnica para interpolar curvas suavizadas mediante un polinomio cúbico.
 * El número de puntos mínimos para formar un spline de Catmull-Rom es 4 (p0, p1, p2, p3). Sin embargo, 
 * el primero (p0) y el último (p3) son puntos para controlar la forma de la curva, por lo que todos 
 * los puntos generados están entre el punto p1 y p2.
 * Para generar los puntos es necesario hacerlo mediante un polinomio cúbico (Ver: Función GetCatmullRomPosition),  * donde t toma valores de 0 a 1,  * siendo t = 0 el punto p1 y siendo t = 1 el punto p2.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class CatmullRomSpline {

    //Has to be at least 4 points 
    private Vector3[] controlPointsList;
    //Are we making a line or a loop?
    private bool isLooping = false;
    // Trajectory
    private List<Vector3> trayectory;

    /**
     * Calcula la trayectoria compuesta por puntos que pasan por unos puntos específicos. Se usa el algoritmo
     * del spline de Catmull-Rom.
     * @param path Puntos por los que tiene que pasar la trayectoria
     * @return List<Vector3> Trayectoria
     */
    public List<Vector3> GetTrayectory(Vector3[] path)
    {
        // Final trajectory
        trayectory = new List<Vector3>();
        // trajectory points
        controlPointsList = path;

        // Get the Catmull-Rom spline between the points.
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            //Cant calculate between the endpoints
            //Neither do we need to calculate from the second to the last endpoint
            //...if we are not making a looping line
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
            {
                continue;
            }
            // Calculate and save points from Catmull-Rom spline with 4 points
            CalculateCatmullRomSpline(i);
        }
        return trayectory;
    }

    /** 
     * Calcula los puntos de la trayectoria que pasa por 2 puntos. Los puntos se añaden a una trayectoria.
     * @param pos Punto inicial del CatmullRom spline
     * @return void
     */
    private void CalculateCatmullRomSpline(int pos)
    {
        // Calculate a spline between 2 points derived with the Catmull-Rom spline algorithm

        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = controlPointsList[ClampListPos(pos - 1)];
        Vector3 p1 = controlPointsList[pos];
        Vector3 p2 = controlPointsList[ClampListPos(pos + 1)];
        Vector3 p3 = controlPointsList[ClampListPos(pos + 2)];

        //The start position of the line
        Vector3 lastPos = p1;
        trayectory.Add(lastPos);

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = 0.1f;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                   
            //Save this pos 
            lastPos = newPos;
            trayectory.Add(lastPos);
        }
    }

    /** 
     * Ajusta los índices de los puntos de la trayectoria para permitir un bucle del spline.
     * @param pos Índice
     * @return int Índice
     */
    private int ClampListPos(int pos)
    {
        // Clamp the list positions to allow looping
        if (pos < 0)
        {
            pos = controlPointsList.Length - 1;
        }

        if (pos > controlPointsList.Length)
        {
            pos = 1;
        }
        else if (pos > controlPointsList.Length - 1)
        {
            pos = 0;
        }

        return pos;
    }

    /**
     * Calcula la posición en el momento t de la trayectoria que pasa por el punto p1 y p2. Dependiento de
     * los puntos p0 y p3 se puede controlar la forma de la trayectoria generada.
     * @param t Momento de la trayectoria. Debe estar entre 0-1
     * @param p0 Punto que controla la forma de la curva generada
     * @param p1 Punto de la trayectoria
     * @param p2 Punto de la trayectoria
     * @param p3 Punto que controla la forma de la curva generada
     * @return Vector3 Posición en el momento t de la trayectoria
     */
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
        // https://www.habrador.com/tutorials/interpolation/1-catmull-rom-splines/
        // http://www.iquilezles.org/www/articles/minispline/minispline.htm
        // http://www.mvps.org/directx/articles/catmull/

        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

    /**
     * Construye una lista de coordenadas a partir de los puntos de una trayectoria. Se añaden
     * 2 puntos que actuan como controladores de la curva de la trayectoria. Los 2 puntos son 
     * copias del punto inicial y final.
     * @param targetsArray Array de positiones del spline
     * @return Vector3[] Lista de coordenadas de las posiciones
     */
    public Vector3[] PrepareSpline(List<Transform> targetsArray)
    {
        // New array with 2 more positions
        Vector3[] positions = new Vector3[targetsArray.Count + 2];
        // New positions. Copies from start and end positions
        Vector3 startPos = targetsArray[0].position;
        Vector3 endPos = targetsArray[targetsArray.Count - 1].position;
         // Add new positions
        positions[0] = startPos;
        positions[targetsArray.Count + 2 - 1] = endPos;
        // Add positions
        for (int i = 0; i < targetsArray.Count; i++)
            positions[i + 1] = targetsArray[i].position;

        return positions;
    }
}
