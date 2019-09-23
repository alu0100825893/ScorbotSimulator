using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Mantiene "Orientación" de la interfáz gráfica orientada con respecto al sistemas de coordenadas.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class WorldAxis : MonoBehaviour {


	void Update () {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);       
    }
}
