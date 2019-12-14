using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Configuración del Scorbot ER V+.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class ScorbotERVPlus {

    public const int INDEX = 1;

    public const int BASE_COUNT_MIN = 24266;
    public const int BASE_COUNT_MAX = -18772;
    public const int BASE_COUNT_HOME = 0;
    public const float BASE_DEGREES_MIN = 0f;
    public const float BASE_DEGREES_MAX = 310f;
    public const float BASE_OFFSET_UNITY = -173.8f;
    public const float BASE_SPEED_MIN = 79f; // degrees/seg 
    public const float BASE_SPEED_MAX = 112f; // degrees/seg 

    public const int SHOULDER_COUNT_MIN = 17100;
    public const int SHOULDER_COUNT_MAX = -346;
    public const int SHOULDER_COUNT_HOME = -6;
    public const float SHOULDER_DEGREES_MIN = 0f;
    public const float SHOULDER_DEGREES_MAX = 165f; //+130 -35
    public const float SHOULDER_OFFSET_UNITY = -46.5f;
    public const float SHOULDER_SPEED_MIN = 68f; // degrees/seg 
    public const float SHOULDER_SPEED_MAX = 99f; // degrees/seg 

    public const int ELBOW_COUNT_MIN = -3207; //-15207. This is a temporal FIX. These limits are wrong
    //public const int ELBOW_COUNT_MIN = -15207; //-15207. This is a temporal FIX. These limits are wrong
    public const int ELBOW_COUNT_MAX = 11946;
    public const int ELBOW_COUNT_HOME = -3;
    public const float ELBOW_DEGREES_MIN = 0f;
    public const float ELBOW_DEGREES_MAX = 260f; //+-130 = 260
    public const float ELBOW_OFFSET_UNITY = -150f;
    public const float ELBOW_SPEED_MIN = 79f; // degrees/seg 
    public const float ELBOW_SPEED_MAX = 112f; // degrees/seg 

    public const int PITCH_COUNT_MIN = -903;
    public const int PITCH_COUNT_MAX = 5664;
    public const int PITCH_COUNT_HOME = -2;
    public const float PITCH_DEGREES_MIN = 0f;
    public const float PITCH_DEGREES_MAX = 260f; //+-130 = 260
    public const float PITCH_OFFSET_UNITY = -125f;
    public const float PITCH_SPEED_MIN = 87f; // degrees/seg 
    public const float PITCH_SPEED_MAX = 133f; // degrees/seg 

    public const int ROLL_COUNT_MIN = -15944;
    public const int ROLL_COUNT_MAX = 15940;
    public const int ROLL_COUNT_HOME = 0;
    public const float ROLL_DEGREES_MIN = 0f;
    public const float ROLL_DEGREES_MAX = 1140f; // +-570 = 1140
    public const float ROLL_OFFSET_UNITY = -1140f / 2f;
    public const float ROLL_SPEED_MIN = 166f; // degrees/seg 
    public const float ROLL_SPEED_MAX = 166f; // degrees/seg 
    
}
