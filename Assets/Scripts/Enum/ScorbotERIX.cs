﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorbotERIX {

    public const int INDEX = 0;

    public const float BASE_DEGREES_MIN = 0f;
    public const float BASE_DEGREES_MAX = 270f;
    public const int BASE_COUNT_MIN = -190186;
    public const int BASE_COUNT_MAX = 138899;
    public const int BASE_COUNT_HOME = -13539;
    public const float BASE_SPEED_MIN = 79f; // degrees/seg 
    public const float BASE_SPEED_MAX = 112f; // degrees/seg 
    
    public const float SHOULDER_DEGREES_MIN = 0f;
    public const float SHOULDER_DEGREES_MAX = 145f;
    /*
    public const int SHOULDER_COUNT_MIN = -74957; // Temporal fix
    public const int SHOULDER_COUNT_MAX = 128285; // Temporal fix
    public const int SHOULDER_COUNT_HOME = 110777; // Temporal fix
    */

    public const int SHOULDER_COUNT_MIN = 128285; 
    public const int SHOULDER_COUNT_MAX = -74957; 
    public const int SHOULDER_COUNT_HOME = -57449; 
    
    public const float SHOULDER_SPEED_MIN = 68f; // degrees/seg 
    public const float SHOULDER_SPEED_MAX = 99f; // degrees/seg 
    
    public const float ELBOW_DEGREES_MIN = 0f;
    public const float ELBOW_DEGREES_MAX = 210f;
    public const int ELBOW_COUNT_MIN = -115649;
    public const int ELBOW_COUNT_MAX = 139121;
    public const int ELBOW_COUNT_HOME = 17518;
    public const float ELBOW_SPEED_MIN = 79f; // degrees/seg 
    public const float ELBOW_SPEED_MAX = 112f; // degrees/seg 

    public const float PITCH_DEGREES_MIN = 0f;
    public const float PITCH_DEGREES_MAX = 196f;
    public const int PITCH_COUNT_MIN = -1018;
    public const int PITCH_COUNT_MAX = 200285;
    public const int PITCH_COUNT_HOME = -26748;
    public const float PITCH_SPEED_MIN = 87f; // degrees/seg 
    public const float PITCH_SPEED_MAX = 133f; // degrees/seg 

    public const float ROLL_DEGREES_MIN = 0f;
    public const float ROLL_DEGREES_MAX = 737f;
    public const int ROLL_COUNT_MIN = -209547;
    public const int ROLL_COUNT_MAX = 209658;
    public const int ROLL_COUNT_HOME = 0;
    public const float ROLL_SPEED_MIN = 166f; // degrees/seg 
    public const float ROLL_SPEED_MAX = 166f; // degrees/seg 
    
}