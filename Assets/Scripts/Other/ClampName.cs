using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * Mantiene una etiqueta de una posición sobre la misma y actualiza la información de las coordenadas.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ClampName : MonoBehaviour {
    // Visual name tag
    public Transform textPanel; 
    // Constants
    private const float LETTER = 13f;
    private const float MARGIN = 6f;
    // Position of this visual tag is selected
    public bool selected = false;
    // Position name
    private string targetName;
    
    void Update () {
        // Update coordiantes
        textPanel.position = Camera.main.WorldToScreenPoint(transform.position);
        // Update coordinates info
        if(selected)
            textPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = GetComponent<TargetModel>().GetPositionInScorbot().ToString();

    }

    /**
     * Modifica el nombre que muestra la etiqueta.
     * @param text
     * @return void
     */
    public void SetText(string text)
    {
        targetName = text;
        // Name
        textPanel.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        // Coordiantes info. Default
        textPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = Vector3.zero.ToString();        
        // Only name
        textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(MARGIN + LETTER * text.Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
    }

    /**
     * Modifica si la etiqueta está seleccionada para redimensionar el tamaño y mostrar o no las coordenadas.
     * @param selected Seleccionada
     * @return void
     */
    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (!this.selected)
        {
            // Coordinates info. Empty
            textPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            // Adjust size to fit name
            textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(MARGIN + LETTER * targetName.Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
        }
        else // Adjust size to fit coordinates
            textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(MARGIN + LETTER * Vector3.zero.ToString().Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
    }
}
