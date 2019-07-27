using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClampName : MonoBehaviour {

    public Transform textPanel; // Visual name tag
    private float letter = 12f;
    private float margin = 6f;
    public bool selected = false;
    private string targetName;
    
    private void Start()
    {

    }

    void Update () {
        //if(textPanel)
            textPanel.position = Camera.main.WorldToScreenPoint(transform.position);
        if(selected)
            textPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = GetComponent<TargetModel>().GetPositionInScorbot().ToString();

    }

    public void SetText(string text)
    {
        targetName = text;
        textPanel.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        textPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = Vector3.zero.ToString();
        
        // Only name
        textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(margin + letter * text.Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
        
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (!this.selected)
        {
            textPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(margin + letter * targetName.Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
        }
        else
            textPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(margin + letter * Vector3.zero.ToString().Length, textPanel.GetComponent<RectTransform>().sizeDelta.y);
    }
}
