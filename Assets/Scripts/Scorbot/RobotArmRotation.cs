using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RobotArmRotation : MonoBehaviour
{
    // Partes del robot
    private GameObject baseRobotArm;
    private GameObject shoulderRobotArm;
    private GameObject elbowRobotArm;
    private GameObject pitchRobotArm;
    private GameObject rollRobotArm;

    // Angulos de las partes del robot, NO ENLAZADO con el simulador
    private Vector3 baseAngles;
    private Vector3 shoulderAngles;
    private Vector3 elbowAngles;
    private Vector3 pitchAngles;
    private Vector3 rollAngles;

    private bool SettingHome;

    // Sliders
    private Slider baseRotationSlider;
    private Slider shoulderRotationSlider;
    private Slider elbowRotationSlider;
    private Slider pitchRotationSlider;
    private Slider rollRotationSlider;

    private Vector3 elbowStart;

    public GameObject text1;//
    public GameObject text2;//
    public GameObject text3;//

    private TextMeshProUGUI stateText;

    void Start()
    {
        // Referencias de las partes del robot
        baseRobotArm = GameObject.Find("Base");
        shoulderRobotArm = GameObject.Find("Shoulder");
        elbowRobotArm = GameObject.Find("Elbow");
        pitchRobotArm = GameObject.Find("Pitch");
        rollRobotArm = GameObject.Find("Roll");

        // Angulos iniciales de las partes del robot, relativos a su padre
        baseAngles = baseRobotArm.transform.localEulerAngles;
        shoulderAngles = shoulderRobotArm.transform.localEulerAngles;
        elbowAngles = elbowRobotArm.transform.localEulerAngles;
        pitchAngles = pitchRobotArm.transform.localEulerAngles;
        rollAngles = rollRobotArm.transform.localEulerAngles;
               
        SettingHome = false;

        // Sliders
        baseRotationSlider = GameObject.Find("BaseRotation").GetComponent<Slider>();        
        shoulderRotationSlider = GameObject.Find("ShoulderRotation").GetComponent<Slider>();
        elbowRotationSlider = GameObject.Find("ElbowRotation").GetComponent<Slider>();
        pitchRotationSlider = GameObject.Find("PitchRotation").GetComponent<Slider>();
        rollRotationSlider = GameObject.Find("RollRotation").GetComponent<Slider>();

        stateText = GameObject.Find("StateText").GetComponent<TextMeshProUGUI>();
        stateText.text = "Ready";

    }
        

    void Update()
    {
    
    }

    /*
     * Metodos para cambiar la rotacion de cada parte con barras deslizadoras. El eje esta fijo
     */
    public void BaseAngle(float newValue)
    {
        baseAngles.y = newValue;
        baseRobotArm.transform.localRotation = Quaternion.Euler(baseAngles);

        ShowRotationValues(baseRobotArm);
    }

    public void ShoulderAngle(float newValue)
    {
        shoulderAngles.x = newValue;
        shoulderRobotArm.transform.localRotation = Quaternion.Euler(shoulderAngles);

        ShowRotationValues(shoulderRobotArm);
    }

    public void ElbowAngle(float newValue)
    {        
        elbowAngles.y = newValue;
        elbowRobotArm.transform.localRotation = Quaternion.Euler(new Vector3(elbowAngles.x, -elbowAngles.y, elbowAngles.z));
       
        ShowRotationValues(elbowRobotArm);
    }

    public void PitchAngle(float newValue)
    {
        pitchAngles.y = newValue;
        pitchRobotArm.transform.localRotation = Quaternion.Euler(new Vector3(pitchAngles.x, -pitchAngles.y, pitchAngles.z));

        ShowRotationValues(pitchRobotArm);
    }

    public void RollAngle(float newValue)
    {
        rollAngles.x = newValue;
        rollRobotArm.transform.localRotation = Quaternion.Euler(new Vector3(rollAngles.x, rollAngles.y, rollAngles.z));

        ShowRotationValues(rollRobotArm);
    }

    /*
     * Metodo para imprimir informacion de las rotaciones de una parte del robot
     */
    private void ShowRotationValues(GameObject obj)
    {
        float auxX = obj.transform.eulerAngles.x;
        float auxY = obj.transform.eulerAngles.y;
        float auxZ = obj.transform.eulerAngles.z;
        string output = "";

        text1.GetComponent<TextMeshProUGUI>().text = "eulerAngles: \n" + auxX + " " + auxY + " " + auxZ;
        output += text1.GetComponent<TextMeshProUGUI>().text + "\n";

        auxX = obj.transform.localEulerAngles.x;
        auxY = obj.transform.localEulerAngles.y;
        auxZ = obj.transform.localEulerAngles.z;
        text2.GetComponent<TextMeshProUGUI>().text = "localEulerAngles: \n" + auxX + " " + auxY + " " + auxZ;
        output += text2.GetComponent<TextMeshProUGUI>().text + "\n";

        auxX = obj.transform.rotation.eulerAngles.x;
        auxY = obj.transform.rotation.eulerAngles.y;
        auxZ = obj.transform.rotation.eulerAngles.z;
        text3.GetComponent<TextMeshProUGUI>().text = "rotation.EulerAngles: \n" + auxX + " " + auxY + " " + auxZ;
        output += text3.GetComponent<TextMeshProUGUI>().text + "\n";

        //Debug.Log(output);
    }

    public void SetHome()
    {
        if (!SettingHome)
        {
            SettingHome = true;
            StartCoroutine(SetHomeStart());
        }
    }

    /* 
     * Corutinas para rotar el robot 
     */

    private IEnumerator SetHomeStart()
    {   /*
        //Debug.Log("Setting home...");
        stateText.text = "Setting home...";
        // Base     
        baseAngles.y = RobotArmHelper.BASE_HOME;
        yield return StartCoroutine(BaseHome());

        // Shoulder            
        shoulderAngles.x = RobotArmHelper.SHOULDER_HOME;
        yield return StartCoroutine(ShoulderHome());

        // Elbow
        elbowAngles.y = RobotArmHelper.ELBOW_HOME;
        yield return StartCoroutine(ElbowHome());

        // Pitch
        pitchAngles.y = RobotArmHelper.PITCH_HOME;
        yield return StartCoroutine(PitchHome());
        
        // Roll
        rollAngles.x = RobotArmHelper.ROLL_HOME;
        yield return StartCoroutine(RollHome());

        UpdateSliders();
        stateText.text = "Ready";
        SettingHome = false;
        */
        yield return null;
    }

    private IEnumerator BaseHome()
    {
        float duration = 2.0f; // Diff / SPEED?
        Quaternion startRotation = baseRobotArm.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(baseAngles);
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            Quaternion newRotation = Quaternion.Lerp(startRotation, endRotation, t);
            baseRobotArm.transform.localRotation = newRotation;
            ShowRotationValues(baseRobotArm);
            yield return null;
        }
    }

    private IEnumerator ShoulderHome()
    {
        float duration = 2.0f; // Diff / SPEED?
        Quaternion startRotation = shoulderRobotArm.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(shoulderAngles);
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            Quaternion newRotation = Quaternion.Lerp(startRotation, endRotation, t);
            shoulderRobotArm.transform.localRotation = newRotation;
            ShowRotationValues(shoulderRobotArm);
            yield return null;
        }
    }

    private IEnumerator ElbowHome()
    {
        
        float duration = 2.0f; // Diff / SPEED?
        Quaternion startRotation = elbowRobotArm.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(elbowAngles.x, -elbowAngles.y, elbowAngles.z);
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            Quaternion newRotation = Quaternion.Lerp(startRotation, endRotation, t);
            elbowRobotArm.transform.localRotation = newRotation;
            ShowRotationValues(elbowRobotArm);
            yield return null;
        }
    }

    private IEnumerator PitchHome()
    {

        float duration = 2.0f; // Diff / SPEED?
        Quaternion startRotation = pitchRobotArm.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(pitchAngles.x, -pitchAngles.y, pitchAngles.z);
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            Quaternion newRotation = Quaternion.Lerp(startRotation, endRotation, t);
            pitchRobotArm.transform.localRotation = newRotation;
            ShowRotationValues(pitchRobotArm);
            yield return null;
        }
    }

    private IEnumerator RollHome()
    {

        float duration = 2.0f; // Diff / SPEED?
        Quaternion startRotation = rollRobotArm.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(rollAngles);
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / duration)
        {
            Quaternion newRotation = Quaternion.Lerp(startRotation, endRotation, t);
            rollRobotArm.transform.localRotation = newRotation;
            ShowRotationValues(rollRobotArm);
            yield return null;
        }
    }

    private void UpdateSliders()
    {
        // Sliders actualizados
        /*
        baseRotationSlider.value = baseRobotArm.transform.localRotation.eulerAngles.y;        
        shoulderRotationSlider.value = shoulderRobotArm.transform.localRotation.eulerAngles.x;
        elbowRotationSlider.value = elbowRobotArm.transform.localRotation.eulerAngles.y; // TODO: Not working in real time
        pitchRotationSlider.value = pitchRobotArm.transform.localRotation.eulerAngles.y;
        rollRotationSlider.value = rollRobotArm.transform.localRotation.eulerAngles.x;

        //Debug.Log(elbowRobotArm.transform.localRotation.eulerAngles.y);
        */

        baseRotationSlider.value = baseAngles.y;
        shoulderRotationSlider.value = shoulderAngles.x;
        elbowRotationSlider.value = elbowAngles.y; 
        pitchRotationSlider.value = pitchAngles.y;
        rollRotationSlider.value = rollAngles.x;
    }

}

