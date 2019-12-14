using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * La función principal de este componente es comunicar el resultado de una acción mediante un mensaje
 * en el "Estado actual" de la interfaz gráfica. También se encarga de mantener el registro de estos mensajes
 * como un historial "Log".
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class StateMessageControl : MonoBehaviour {
    // Controllers
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

    void Awake () {
        // Controllers
        targetControl = GetComponent<TargetControl>();
        // Main state message
        stateOutput = GetComponent<GameController>().stateOutput;
        stateOutputPanel = GetComponent<GameController>().stateOutput.transform.parent.GetComponent<Animator>();
        // Log state message
        messageLog = GetComponent<GameController>().messageLog;
        contentLog = GetComponent<GameController>().messageLog.transform.parent.gameObject;        
        positionLog = GetComponent<GameController>().positionLog;
        positionSyncLog = GetComponent<GameController>().positionSyncLog;
        contentPositionLog = GetComponent<GameController>().positionLog.transform.parent.gameObject;
        positionCountLog = GetComponent<GameController>().positionCountLog;
        messages = new List<string>();
    }

    /**
	 * Escribe un nuevo mensaje en el "Estado actual" de la interfaz gráfica. Cambia su color a rojo (error). 
     * También se actualiza la ventana "Log".
     * o verde (éxito).
	 * @param text Mensaje
	 * @param success Éxito o error
	 * @return void
	 */
    public void WriteMessage(string text, bool success)
    {
        // Write text
        stateOutput.text = text;
        // Change panel color
        stateOutputPanel.SetBool("success", success);
        // If there are too many messages, delete oldest
        if(messages.Count >= MAX_MASSAGES)      
            messages.RemoveAt(0);

        string aux = "";
        // if message belongs to a new messages block
        if (newBlock)
        {
            newBlock = false;
            indexBlock++;            
        }
        // Write block id
        aux = "(" + indexBlock + ") ";
        // Save new message
        messages.Add(aux + text);
        // Add new message to Log
        UpdateMessageLog();
    }

    /**
	 * Actualiza la información de la ventana "Log".
	 * @return void
	 */
    public void UpdateMessageLog()
    {
        string aux = "";    
        // Get all messages
        foreach (string m in messages)
            aux += m + "\n";
        // Write all messages
        messageLog.text = aux;    
        // Fix containers to fit messages
        AdjustContent(messageLog.gameObject, contentLog);                
    }
    /**
	 * Actualiza la información de la ventana "Positions(0/0)".
	 * @return void
	 */
    public void UpdatePositionLog()
    {
        int nSync = 0;
        string auxLog = "";
        string auxSyncLog = "";
        // Get positions name and sync state
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
        // Write positions name
        positionLog.text = auxLog;
        // Write positions sync state
        positionSyncLog.text = auxSyncLog;
        // Fix containers to fit names
        AdjustContent(positionLog.gameObject, contentPositionLog);

        // Count data
        UpdateCountData(nSync, targetControl.Count());
    }

    /**
	 * Actualiza la información sobre el número de posiciones y su sincronización. Ventana "Positions(0/0)"
     * @param nSync Número de posiciones sincronizadas
     * @param nTotal Número de posiciones totales
	 * @return void
	 */
    private void UpdateCountData(int nSync, int nTotal)
    {
        // Update positions count
        positionCountLog.text = "Positions(" + nSync + "/" + nTotal + ")";
    }

    /**
	 * Redimensiona el contenedor del texto para que se ajuste a ese texto.
     * @param textMesh Texto
     * @param content Contenedor del texto
	 * @return void
	 */
    private void AdjustContent(GameObject textMesh, GameObject content)
    {
        // Resize window 
        float height = CalculateHeight(textMesh);             
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, height);

        // Window reposition
        float y = CalculatePosY(content, height);    
        content.GetComponent<RectTransform>().Translate(new Vector3(0f, y, 0f));
    }

    /**
	 * Calcula al altura que debería tener un contenedor para mostrar el texto.
     * @param textMesh Texto
	 * @return float Altura
	 */
    private float CalculateHeight(GameObject textMesh)
    {
        int nLines = textMesh.GetComponent<TextMeshProUGUI>().textInfo.lineCount;
        float height = LINE_HEIGHT * nLines;
        // Temporal fix. Error when nlines is 2 or max, spacing is not right in "content"        
        if (MAX_MASSAGES != nLines)
            height += LINE_HEIGHT;
            
        return height;
    }

    /**
	 * Calcula la posición y que debería tener un contenedor para mostrar el texto.
     * @param textMesh Texto
     * @param height Altura del contenedor del texto
	 * @return float Posición y
	 */
    private float CalculatePosY(GameObject content, float height)
    {
        float y = content.GetComponent<RectTransform>().position.y - (height / 2);
        return y;
    }

    /**
	 * Genera el siguiente mensaje en un bloque nuevo.
	 * @return void
	 */
    public void NewBlock()
    {
        newBlock = true;
    }
}
