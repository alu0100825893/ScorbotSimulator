using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * La función principal de este componente es la creación y eliminación de las posiciones de la simulación
 * mediante el manejo de una lista de posiciones, además de la validación de sus nombres.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class TargetControl : MonoBehaviour {
    // Positions list
    private List<Transform> targets = new List<Transform>();
    // Prefab position. New positions are duplicated from this
    private Transform targetPrefab;
    // Prefab position name. New positions visual tag (name, coordinates) are duplicated from this
    private Transform targetNamePrefab;
    // Screen
    private Transform canvas;
    // Constants
    private const int MAX_NAME_LENGTH = 5;

    void Start () {
        // Prefab position. New positions are duplicated from this
        targetPrefab = GetComponent<GameController>().targetPrefab;
        // Prefab position name. New positions visual tag (name, coordinates) are duplicated from this
        targetNamePrefab = GetComponent<GameController>().targetNamePrefab;
        // Screen
        canvas = GetComponent<GameController>().canvas;
    }

    /**
	 * Crea una posición a partir de un nombre, coordenadas y ángulos (articulaciones del Scorbot).
	 * @param name Nombre de la posición
	 * @param pos Coordenadas de la posición
	 * @param angles Ángulos (articulaciones del Scorbot) de la posición
	 * @return Transform Posición (objeto)
	 */
    public Transform CreateTarget(string name, Vector3 pos, List<Vector3> angles)
    {
        // Create name text and include in Canvas
        Transform textPanel = Instantiate(targetNamePrefab);      
        textPanel.SetParent(canvas);
        textPanel.SetAsFirstSibling();

        // Create target in pos
        Transform newTarget = Instantiate(targetPrefab).transform;
        newTarget.position = pos;

        // Bind text to target and set name
        newTarget.GetComponent<ClampName>().textPanel = textPanel;
        newTarget.GetComponent<ClampName>().SetText(name);

        // Build target model
        newTarget.GetComponent<TargetModel>().SetName(name);
        newTarget.GetComponent<TargetModel>().SetAngles(angles);
        return newTarget;
    }

    /**
	 * Crea y añade una posición la lista de posiciones a partir de un nombre, coordenadas y 
     * ángulos (articulaciones del Scorbot).
	 * @param name Nombre de la posición
	 * @param pos Coordenadas de la posición
	 * @param angles Ángulos (articulaciones del Scorbot) de la posición
	 * @return Transform Posición (objeto)
	 */
    public Transform Add(string name, Vector3 pos, List<Vector3> angles)
    {
        // Create target
        Transform newTarget = CreateTarget(name, pos, angles);
        // Add to positions list   
        targets.Add(newTarget);
        return newTarget;
    }

    /**
	 * Elimina una posición (objeto) de la lista de posiciones y la destruye.
	 * @param target Posición (objeto)
	 * @return void
	 */
    public void Remove(Transform target)
    {
        // If positions list is empty, do nothing
        if (Count() == 0)
            return;

        string name = target.GetComponent<TargetModel>().GetName();
        int index;

        // Find target position in list
        for (index = 0; index < Count(); index++)
        {
            if (GetTarget(index).GetComponent<TargetModel>().GetName().Equals(name))
                break;
        }

        // Position being used as reference
        Transform rFrom = targets[index].GetComponent<TargetModel>().GetRelativeFrom();
        if (rFrom) {
            rFrom.GetComponent<TargetModel>().SetNoRelativeTo();
        }
        // Position using other position as reference
        Transform rTo = targets[index].GetComponent<TargetModel>().GetRelativeTo();
        if (rTo)
        {
            rTo.GetComponent<TargetModel>().SetRelativeFrom(null);
        }

        // Delete position (object)
        targets.RemoveAt(index);
        DestroyTarget(target);
    }

    /**
	 * Destruye una posición (objeto), además de su etiqueda visual (nombre y coordenadas).
	 * @param target Posición (objeto)
	 * @return void
	 */
    public void DestroyTarget(Transform target)
    {
        // Delete visual tag
        Destroy(target.GetComponent<ClampName>().textPanel.gameObject);
        // Delete position (object)
        Destroy(target.gameObject);
    }

    /**
	 * Obtiene los nombres de todas las posiciones actuales.
	 * @return List<string> Lista de nombres de las posiciones
	 */
    public List<string> GetNames()
    {
        List<string> names = new List<string>();
        foreach (Transform target in targets)
            names.Add(target.GetComponent<TargetModel>().GetName());
        return names;
    }

    /**
	 * Obtiene una posición (objeto) mediante su índice en la lista de posiciones.
     * @param i Índice de la lista de posiciones
	 * @return Transform Posición (objeto)
	 */
    public Transform GetTarget(int i)
    {       
        return targets[i];        
    }

    /**
	 * Obtiene una posición (objeto) mediante su nombre.
     * @param name Nombre de una posición
	 * @return Transform Posición (objeto)
	 */
    public Transform GetTarget(string name)
    {
        Transform t = null;
        foreach (Transform target in targets) {
            if (name.Equals(target.GetComponent<TargetModel>().GetName()))
            {
                t = target;
                break;
            }
        }
        return t;
    }

    /**
	 * Obtiene el número de posiciones totales actualmente creadas.
	 * @return int Número de posiciones
	 */
    public int Count()
    {
        return targets.Count;
    }

    /**
	 * Verifica que un nombre no está en uso por una posición.
	 * @return bool Si el nombre es válido
	 */
    public bool ValidName(string name)
    {
        foreach(string n in GetNames())
        {
            if (n.ToLower().Equals(name.ToLower()))
                return false;
        }
        return true;
    }

    /**
	 * Verifica que un nombre no sobrepasa el tamaño máximo.
	 * @return bool Si el nombre es válido
	 */
    public bool ValidNameLength(string name)
    {
        return name.Length <= MAX_NAME_LENGTH;
    }
}
