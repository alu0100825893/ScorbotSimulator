using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetControl : MonoBehaviour {

    //private List<TargetModel> targets = new List<TargetModel>();
    private List<Transform> targets = new List<Transform>();
    private Transform targetPrefab;
    private Transform targetNamePrefab;
    private Transform canvas;
    private const int MAX_NAME_LENGTH = 5;

    void Start () {
        targetPrefab = GetComponent<GameController>().targetPrefab;
        targetNamePrefab = GetComponent<GameController>().targetNamePrefab;
        canvas = GetComponent<GameController>().canvas;
    }
	
	void Update () {
		
	}

    public Transform CreateTarget(string name, Vector3 pos, List<Vector3> angles)
    {
        // Create name text and include in Canvas
        Transform textPanel = Instantiate(targetNamePrefab);
        //textPanel.parent = canvas;
        textPanel.SetParent(canvas);
        textPanel.SetAsFirstSibling();

        // Create target in pos
        Transform newTarget = Instantiate(targetPrefab).transform;
        newTarget.position = pos;

        // Bind name text to target and set name
        newTarget.GetComponent<ClampName>().textPanel = textPanel;
        newTarget.GetComponent<ClampName>().SetText(name);

        // Built target model
        newTarget.GetComponent<TargetModel>().SetName(name);
        newTarget.GetComponent<TargetModel>().SetAngles(angles);
        return newTarget;
    }
    
    public Transform Add(string name, Vector3 pos, List<Vector3> angles)
    {
        // Create target
        Transform newTarget = CreateTarget(name, pos, angles);
                             
        targets.Add(newTarget);
        return newTarget;
    }

    public void Remove(Transform target)
    {
        if (Count() == 0)
            return;

        string name = target.GetComponent<TargetModel>().GetName();
        int index;
        //Debug.Log(name);
        // Find position of target
        for (index = 0; index < Count(); index++)
        {
            if (GetTarget(index).GetComponent<TargetModel>().GetName().Equals(name))
                break;
        }
        //Debug.Log(index);
        targets.RemoveAt(index);
        DestroyTarget(target);
    }

    public void DestroyTarget(Transform target)
    {
        Destroy(target.GetComponent<ClampName>().textPanel.gameObject);
        Destroy(target.gameObject);
    }

    public List<string> GetNames()
    {
        List<string> names = new List<string>();
        foreach (Transform target in targets)
            names.Add(target.GetComponent<TargetModel>().GetName());
        return names;
    }

    public Transform GetTarget(int i)
    {
        //return targets[i].GetObj();        
        return targets[i];        
    }

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

    public int Count()
    {
        return targets.Count;
    }

    public bool ValidName(string name)
    {
        foreach(string n in GetNames())
        {
            if (n.ToLower().Equals(name.ToLower()))
                return false;
        }
        return true;
    }    

    public bool ValidNameLength(string name)
    {
        return name.Length <= MAX_NAME_LENGTH;
    }
}
