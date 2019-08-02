using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAxis : MonoBehaviour {


	void Update () {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);       
    }
}
