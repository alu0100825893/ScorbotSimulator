using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Estructura de datos que representa una posición. Almacena datos como la información del Scorbot (simulación) 
 * para alcanzar la posición y también realiza operaciones para mantener una posición relativa a otra. Los datos
 * se almacenan en el contexto de Unity, para usarlos en el Scorbot real se aplican transformaciones.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class TargetModel : MonoBehaviour {

    // Position name
    private string targetName;
    // Scorbot articulations angles
    private List<Vector3> angles = null;
    public float pitch = 0f;
    public float roll = 0f;
    // Valid angles data ? (invalid when a position is out of range)
    private bool valid = false;
    // Is this posicion saved in the real Scorbot?
    private bool sync = false;
    // This position is being used by another position (relativeFrom)
    private Transform relativeFrom = null;

    // This position is relative to another position (relativeTo)
    private Transform relativeTo = null;
    private Vector3 relativePos;
    // Relative pitch
    private float relativeP = 0f;
    // Relative roll
    private float relativeR = 0f;

    // Colors
    public Material targetMaterial;
    public Material normalMaterial;
    // Renderer. Change colors
    public Renderer rend; // From child which visually represents this target

    /**
     * Obetiene el nombre de la posición.
     * @return string Nombre
     */
    public string GetName()
    {
        return targetName;
    }

    /**
     * Modifica el nombre de la posición.
     * @param name Nombre
     * @return void
     */
    public void SetName(string name)
    {
        targetName = name;
    }

    /**
     * Modifica los ángulos de las articulaciones del Scorbot guardados para alcanzar la posición.
     * @param angles Ángulos 
     * @return void
     */
    public void SetAngles(List<Vector3> angles)
    {
        this.angles = angles;
        if (angles != null)
        {
            CalculatePitch();
            CalculateRoll();
        }
    }

    /**
     * Obtiene los ángulos de las articulaciones del Scorbot
     * @return List<Vector3> Ángulos 
     */
    public List<Vector3> GetAngles()
    {
        return angles;
    }

    /**
     * Obtiene los ángulos de una articulación especifica del Scorbot
     * @param i Índice de una articulación del Scorbot
     * @return List<Vector3> Ángulos 
     */
    public Vector3 GetAngles(int i)
    {
        return angles[i];
    }

    /**
     * Obtiene las coordenadas de la posición transformadas a coordenadas en el Scorbot real. Se multiplica por 10
     * y se intercambiam los ejes "z" e "y".
     * @return Vector3 Coordenadas 
     */
    public Vector3 GetPositionInScorbot()
    {
        return new Vector3(transform.position.x * 10f, transform.position.z * 10f, transform.position.y * 10f);
    }

    /**
     * Cambia el color de la posiciión a violeta (posición seleccionada como objetivo en las listas desplegables)
     * @param current Objetivo actual
     * @return void 
     */
    public void SetCurrentTarget(bool current)
    {
        if (current)
            rend.material = targetMaterial;
        else
            rend.material = normalMaterial;

    }

    /**
     * Calcula el ángulo pitch del Scorbot real.
     * @return void 
     */
    private void CalculatePitch()
    {
        float p = 0f;
        for (int i = 1; i < angles.Count - 1; i++)
        {
            p += angles[i].z;
        }
        pitch = p;
    }

    /**
     * Calcula el ángulo roll del Scorbot real.
     * @return void 
     */
    private void CalculateRoll()
    {
        roll = angles[4].x;
    }

    /**
     * Obtiene el ángulo pitch del Scorbot real.
     * @return float Ángulo pitch 
     */
    public float GetPitch()
    {
        return pitch;
    }

    /**
     * Obtiene el ángulo roll del Scorbot real.
     * @return float Ángulo pitch 
     */
    public float GetRoll()
    {
        if (GameController.indexRobot == ScorbotERVPlus.INDEX)        
            return roll;
        
        return -roll;
    }

    /**
     * Transforma la posición para que permanezca relativa a otra posición (target).
     * @param target Posición relativa
     * @param pos Coordenadas relativas
     * @param p Pitch relativo
     * @param r Roll relativo
     * @return void 
     */
    public void SetRelativeTo(Transform target, Vector3 pos, float p, float r)
    {
        relativeTo = target;
        relativePos = pos;
        relativeP = p;
        relativeR = r;      

        target.GetComponent<TargetModel>().SetRelativeFrom(transform);
    }

    /**
     * Destroys relativity of this position. "Teachr" creates relativity, "Teach" destroys
     * @return void
     */
    public void SetNoRelativeTo()
    {
        relativeTo.GetComponent<TargetModel>().SetRelativeFrom(null);

        relativeTo = null;
        relativePos = Vector3.zero;
        relativeP = 0f;
        relativeR = 0f;        
    }

    /**
     * Modifica si la posición tiene los ángulos de las articulaciones del Scorbot  son válidos (no fuera del alcance).
     * @param value Ángulos válidos
     * @return void
     */
    public void SetValid(bool value)
    {
        valid = value;
    }

    /**
     * Obtiene si la posición tiene los ángulos de las articulaciones del Scorbot  son válidos (no fuera del alcance).
     * @return bool Válido
     */
    public bool GetValid()
    {
        return valid;
    }

    /**
     * Modifica si la posición está guardada en el Scorbot real.
     * @param value Sincronizada
     * @return void
     */
    public void SetSync(bool value)
    {
        sync = value;
    }

    /**
     * Obtiene si la posición está guardada en el Scorbot real.
     * @return bool Sincronizada
     */
    public bool GetSync()
    {
        return sync;
    }

    /**
     * Obtiene la posición (posición relativa) que está usando esta posición como referencia.
     * @return Transform Posición relativa
     */
    public Transform GetRelativeFrom()
    {
        return relativeFrom;
    }

    /**
     * Modifica que posición (posición relativa) está usando esta posición como referencia.
     * @param relativeFrom Posición relativa
     * @return void
     */
    public void SetRelativeFrom(Transform relativeFrom)
    {
        this.relativeFrom = relativeFrom;
    }

    /**
     * Obtiene la posición que se esta usando como referencia. 
     * @return Transform
     */
    public Transform GetRelativeTo()
    {
        return relativeTo;
    }

    /**
     * Actualiza las coordenadas de la posición para que permanezca relativa a su posición de referencia.
     * @return void
     */
    public void UpdateRelativePosition()
    {
        transform.position = relativeTo.position + relativePos;
    }

    /**
     * Obtiene el valor relativo de las coordenadas. Estos no son las coordenadas finales.
     * @return Vector3 Posición relativa
     */
    public Vector3 GetRelativePos()
    {
        return relativePos;
    }

    /**
     * Obtiene el valor relativo de las coordenadas en el contexto del Scorbot real. Estos no son las coordenadas 
     * finales.
     * @return Vector3 Posición relativa
     */
    public Vector3 GetRelativePosInScorbot()
    {
        return new Vector3(relativePos.x, relativePos.z, relativePos.y) * 10f;
    }

    /**
     * Obtiene el valor del pitch relativo. Esto no es el pitch final.
     * @return p Pitch relativo
     */
    public float GetRelativeP()
    {
        return relativeP;
    }

    /**
     * Obtiene el valor del roll relativo. Esto no es el roll final.
     * @return p Pitch relativo
     */
    public float GetRelativeR()
    {
        return relativeR;
    }
}
