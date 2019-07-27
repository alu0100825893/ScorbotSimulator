using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetModel : MonoBehaviour {

    private string targetName;
    private List<Vector3> angles = null;
    public float pitch = 0f;
    public float roll = 0f;
    private bool valid = false;
    private bool sync = false;

    private Transform relativeTo = null;
    private bool isRelative = false;
    private Vector3 relativePos;
    private float relativeP = 0f;
    private float relativeR = 0f;


    public Material targetMaterial;
    public Material normalMaterial;
    public Renderer rend; // From child which visually represents this target


    private void Update()
    {
        // Generate data
        // Check valid
        if (isRelative)
        {
            transform.position = relativeTo.position + relativePos;
        }
    }

    public string GetName()
    {
        return targetName;
    }

    public void SetName(string name)
    {
        targetName = name;
    }

    public void SetAngles(List<Vector3> angles)
    {
        this.angles = angles;
        if (angles != null)
        {
            CalculatePitch();
            CalculateRoll();
        }
    }

    public List<Vector3> GetAngles()
    {
        return angles;
    }

    public Vector3 GetAngles(int i)
    {
        return angles[i];
    }

    public Vector3 GetPositionInScorbot()
    {
        return new Vector3(transform.position.x * 10f, transform.position.z * 10f, transform.position.y * 10f);
    }

    public void SetCurrentTarget(bool current)
    {
        if(current)
            rend.material = targetMaterial;
        else
            rend.material = normalMaterial;

    }

    private void CalculatePitch()
    {
        float p = 0f;
        for (int i = 1; i < angles.Count - 1; i++)
        {
            p += angles[i].z;
        }
        pitch = p;
    }

    private void CalculateRoll()
    {
        roll = angles[4].x;
    }

    public float GetPitch()
    {        
        return pitch;
    }
    // simulation
    public float GetRoll()
    {
        return -roll;
    }

    public Vector3 GetPosmm()
    {
        return new Vector3(transform.position.x * 10f, transform.position.y * 10f, transform.position.z * 10f);
    }

    public void SetRelativeTo(Transform target, Vector3 pos, float p, float r)
    {
        relativeTo = target;
        relativePos = pos;
        relativeP = p;
        relativeR = r;
        SetisRelative(true);     
    }

    public void SetisRelative(bool relative)
    {
        isRelative = relative;
    }

    public void SetValid(bool value)
    {
        valid = value;
    }

    public bool GetValid()
    {
        return valid;
    }

    public void SetSync(bool value)
    {
        sync = value;
    }

    public bool GetSync()
    {
        return sync;
    }
}
