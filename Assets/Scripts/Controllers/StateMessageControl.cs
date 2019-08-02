using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * La función principal de este componente es comunicar el resultado de una acción mediante un mensaje
 * en el “Estado actual” de la interfaz gráfica. También se encarga de mantener el registro de estos mensajes
 * como un historial.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class StateMessageControl : MonoBehaviour {

    private TargetControl targetControl;

    // Main state message
    private TextMeshProUGUI stateOutput; 
    private Animator stateOutputPanel;

    // Log state message
    private TextMeshProUGUI messageLog;
    private GameObject contentLog;
    private List<string> messages;
    private int indexBlock = 0;
    private bool newBlock = false;
    private const int MAX_MASSAGES = 100;
    private const float LINE_HEIGHT = 28.9f;

    // Positions state
    private TextMeshProUGUI positionLog;
    private TextMeshProUGUI positionSyncLog;
    private GameObject contentPositionLog;
    private TextMeshProUGUI positionCountLog;

    void Start () {
        targetControl = GetComponent<TargetControl>();

        stateOutput = GetComponent<GameController>().stateOutput;
        stateOutputPanel = GetComponent<GameController>().stateOutput.transform.parent.GetComponent<Animator>();
        messageLog = GetComponent<GameController>().messageLog;
        contentLog = GetComponent<GameController>().messageLog.transform.parent.gameObject;

        positionLog = GetComponent<GameController>().positionLog;
        positionSyncLog = GetComponent<GameController>().positionSyncLog;
        contentPositionLog = GetComponent<GameController>().positionLog.transform.parent.gameObject;
        positionCountLog = GetComponent<GameController>().positionCountLog;

        messages = new List<string>();
    }

    public void WriteMessage(string text, bool success)
    {
        stateOutput.text = text;
        stateOutputPanel.SetBool("success", success);
        if(messages.Count >= MAX_MASSAGES)      
            messages.RemoveAt(0);

        string aux = "";
        if (newBlock)
        {
            newBlock = false;
            indexBlock++;            
        }
        aux = "(" + indexBlock + ") ";
        messages.Add(aux + text);
        UpdateMessageLog();
    }

    public void UpdateMessageLog()
    {
        string aux = "";
    
        foreach (string m in messages)
            aux += m + "\n";
   
        messageLog.text = aux;        
        AdjustContent(messageLog.gameObject, contentLog);                
    }

    public void UpdatePositionLog()
    {
        int nSync = 0;
        string auxLog = "";
        string auxSyncLog = "";

        for (int i = 0; i < targetControl.Count(); i++)
        {
            TargetModel model = targetControl.GetTarget(i).GetComponent<TargetModel>();
            auxLog += model.GetName() + "\n";

            if (!model.GetSync())
            {
                auxSyncLog += "No sync" + "\n";
            }
            else
            {
                auxSyncLog += "\n";
                nSync++;
            }
        }

        positionLog.text = auxLog;
        positionSyncLog.text = auxSyncLog;
        AdjustContent(positionLog.gameObject, contentPositionLog);

        // Count data
        UpdateCountData(nSync, targetControl.Count());
    }

    private void UpdateCountData(int nSync, int nTotal)
    {
        positionCountLog.text = "Positions(" + nSync + "/" + nTotal + ")";
    }

    private void AdjustContent(GameObject textMesh, GameObject content)
    {
        // Redimensionamiento de ventana
        float height = CalculateHeight(textMesh);
             
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, height);

        // Reposicionamiento de la ventana        
        float y = CalculatePosY(content, height);    
        content.GetComponent<RectTransform>().Translate(new Vector3(0f, y, 0f));
    }

    private float CalculateHeight(GameObject textMesh)
    {
        int nLines = textMesh.GetComponent<TextMeshProUGUI>().textInfo.lineCount;
        float height = LINE_HEIGHT * nLines;
        // Temporal fix. Error when nlines is 2 or max, spacing is not right in "content"
        
        if (MAX_MASSAGES != nLines)
            height += LINE_HEIGHT;
            
        return height;
    }

    private float CalculatePosY(GameObject content, float height)
    {
        float y = content.GetComponent<RectTransform>().position.y - (height / 2);
        return y;
    }

    public void NewBlock()
    {
        newBlock = true;
    }
}
