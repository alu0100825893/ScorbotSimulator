using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * Algoritmo CCD que calcula los ángulo de las articulaciones del Scorbot para llegar a una posición especificada.
 * Según el caso, hace uso de copias de las articulaciones o no.
 * @author Oscar Catari Gutiérrez - E-mail: oscarcatari@outlook.es - Universidad de La Laguna
 * @version 1.0
 * @since 02-05-2019
 */
public class IK : MonoBehaviour {
    // Scorbot model
    private ScorbotModel scorbotModel;
    // Target position
    private Transform D;
    // End Effector
    private Transform E;
    // Articulations
    private Articulation[] articulations;
    // Copy of articulations
    private Articulation[] art;
    // Copy of end effector
    private Transform artE;

    // Max itinerations with CCD algorithm
    private int MAX_ITINERATIONS = 1500;
    // Min distance to target position
    private const float EPSILON = 0.01f; //0.02 0.01 

    // If doing operations
    private bool isProcessing = false;
    // "speedl" value
    private int speedMillimeter = 50;
        
    void Start() {
        // Scorbot model access
        scorbotModel = GetComponent<ScorbotModel>();
        // Target position
        D = scorbotModel.D;
        // End Effector
        E = scorbotModel.E;
        // Articulations
        articulations = scorbotModel.articulations;
        // Scorbot model copy
        scorbotModel.InitToCopy(out art, out artE);
    }

    /**
     * Actualiza la copia del efector final desde la posición Home. 
     * @return void
     */
    public void UpdateCopyEffector()
    {
        SetAnglesCopyHome();
        artE.position = E.position;
    }
        
    private void OnDrawGizmos()
    {
        // Debug line to target position
        if (!D)
            return;
        Gizmos.color = Color.blue;     
        Gizmos.DrawLine(E.position, D.position);
    }    
       
    /**
     * Ejecuta el algoritmo CCD para mover el Scorbot hacia la posición objetivo. Se usa la velocidad normal "Speed".
     * @param D Posición final
     * @return void
     */
    public void CCDAlg(Transform D)
    {
        // "Move"
        if (!isProcessing)
        {
            scorbotModel.UpdateToCopy(art);
            StartCoroutine(IKAlg(art, D, artE));
        }
    }


    /**
     * Inicia el algoritmo CCD para mover las articulaciones siguiento la trajectoria especificada a la 
     * velocidad normal o lineal. Se usa para los comando "Movel" o "Movec".
     * @param trayectory Trajectoria
     * @param speedDegrees Velocidad normal. En otro caso se usa la velocidad lineal
     * @return void
     */
    public void CCDAlg(Transform[] trajectory, bool speedDegrees = true)
    {
        // "Movel" and "Movec"
        if (!isProcessing)
        {
            scorbotModel.UpdateToCopy(art);
            StartCoroutine(CCDAlgMulti(art, trajectory, artE, speedDegrees));
        }
    }

    /**
     * Ejecuta el algoritmo CCD para mover las articulaciones siguiento la trajectoria especificada a la 
     * velocidad normal o lineal.
     * @param articulations Articulaciones
     * @param trayectory Trajectoria
     * @param E Efector final
     * @param speedDegrees Velocidad normal. En otro caso se usa la velocidad lineal
     * @return Proceso en segundo plano
     */
    private IEnumerator CCDAlgMulti(Articulation[] articulations, Transform[] trayectory, Transform E, bool speedDegrees = false)
    {
        // Move following trayectory
        for (int i = 0; i < trayectory.Length; i++)
        {           
            yield return StartCoroutine(IKAlg(articulations, trayectory[i], E, speedDegrees));            
        }

        // Adjustment in order to correct pitch
        Move(trayectory[trayectory.Length - 1]);

        // Destroy objects, skip last one, target        
        for (int i = 0; i < trayectory.Length - 1; i++)
            Destroy(trayectory[i].gameObject);        
    }

    /**
     * Ejecuta el algoritmo CCD y mueve las articulaciones a la velocidad normal o lineal.
     * @param articulations Articulaciones
     * @param D Posición objetivo
     * @param E Efector final
     * @param speedDegrees Velocidad normal. En otro caso se usa la velocidad lineal
     * @return Proceso en segundo plano
     */
    private IEnumerator IKAlg(Articulation[] articulations,Transform D, Transform E, bool speedDegrees = true)
    {
        isProcessing = true;

        float duration = 0f;
        // "Speedl"
        if (!speedDegrees)
        {            
            duration = CalculateTimeSpeedL(E.position, D.position, speedMillimeter);
        }
        
        // Execute CCD algorithm
        AlgCDD(articulations, D, E);        
        
        // "Speed"
        if (speedDegrees)
        {
            List<Vector3> endAngles = new List<Vector3>();
            foreach (Articulation art in articulations)
                endAngles.Add(art.GetAngle());
            duration = CalculateTimeSpeed(this.articulations, endAngles);
        }        
        
        // Move
        Coroutine[] moves = new Coroutine[articulations.Length];
        for (int i = 0; i < articulations.Length; i++)
        {                     
            moves[i] = StartCoroutine(this.articulations[i].MoveCoroutine(articulations[i].GetAngle(), duration));
        }

        // Wait movements
        for (int i = 0; i < articulations.Length; i++)
        {
            yield return moves[i];
        }
        
        isProcessing = false;
    }

    /**
     * Mueve el Scorbot a una posición. La posición debe tener la configuración de ángulos ya calculada.
     * @param Posicíon (objetivo)
     * @return void
     */
    public void Move(Transform target)
    {        
        if(!isProcessing)
        {
            isProcessing = true;      
            // Time 
            float duration = CalculateTimeSpeed(this.articulations, target.GetComponent<TargetModel>().GetAngles());
            // Start movements
            Coroutine[] moves = new Coroutine[articulations.Length];
            for (int i = 0; i < articulations.Length; i++)
            {
                moves[i] = StartCoroutine(this.articulations[i].MoveCoroutine(target.GetComponent<TargetModel>().GetAngles(i), duration));
            }
            //  Wait movements
            StartCoroutine(MoveCoroutive(moves));
        }                
    }

    /**
     * Ejecuta el proceso de esperar a que acaben un conjunto de procesos en segundo plano.
     * @param moves Procesos en segundo plano
     * @return Proceso en segundo plano
     */
    private IEnumerator MoveCoroutive(Coroutine[] moves)
    {
        // Wait moves
        for (int i = 0; i < articulations.Length; i++)
        {
            yield return moves[i];
        }
        
        isProcessing = false;
    }

    /**
     * Ejecuta el algoritmo CCD para hallar los ángulos que las articulaciones necesitan para llegar a la posición 
     * objetivo. No se aplica a la articulación "Roll" ya que no tiene efecto en la posición del efector final.
     * @param articulations Articulaciones
     * @param D Posición objetivo
     * @param E Efector final
     * @param skipPitch Mantener el valor de del pitch
     * @return void
     */
    private void AlgCDD(Articulation[] articulations, Transform D, Transform E, bool skipPitch = false)
    {
        // Skip pitch
        int pitch = 0;
        Vector3 pitchAngle = Vector3.zero;
        if (skipPitch)
        {            
            pitch = 1;
            pitchAngle = D.GetComponent<TargetModel>().GetAngles()[3];
        }

        float lastDistance = Mathf.Infinity;
        // Calculating...
        for (int i = 0; i < MAX_ITINERATIONS; i++)
        {            
            CCD(articulations, D, E, articulations[0]);

            // Doing Base before and avoiding Roll
            for (int artIndex = articulations.Length - 1 - 1 - pitch; artIndex >= 1; artIndex--)
            {                
                CCD(articulations, D, E, articulations[artIndex]);

                // Update pitch as global
                if (skipPitch)
                {                  
                    articulations[3].UpdateAngleAsGlobal(pitchAngle);
                }
            }

            float newDistance = Vector3.Distance(D.position, E.position);

            if (newDistance < EPSILON || (lastDistance == newDistance))          
            {               
                return;
            }
            
            lastDistance = newDistance;
        }
    }

    /**
     * Calcula el tiempo que las articulaciones tardan en rotar para llegar a una configuraciçon de ángulos final.
     * @param startArt Articulaciones
     * @param endAngles Ángulos finales
     * @return float Tiempo (seg)
     */
    private float CalculateTimeSpeed(Articulation[] startArt, List<Vector3> endAngles)
    {
        float duration = 0f;
        // Max time
        for (int i = 0; i < startArt.Length; i++)
        {
            float aux = startArt[i].GetTimeToRotate(endAngles[i]);
            if (duration < aux)
                duration = aux;
        }
        return duration;
    }

    /**
     * Calcula el tiempo que se tarda en llegar de un punto a otro a una velocidad constante. Comando "Speedl"
     * @param startPoint Posición inicial
     * @param endPoint Posición final
     * @param speedMillimeter Velocidad lineal (mm/seg)
     * @return float Tiempo (seg)
     */
    private float CalculateTimeSpeedL(Vector3 startPoint, Vector3 endPoint, float speedMillimeter)
    {
        return (Vector3.Distance(startPoint, endPoint) * 10f) / (float)speedMillimeter;
    }

    /**
     * Aplica el algoritmo CCD para una articulación y la rota.
     * @param articulations Articulaciones
     * @param D Posición objetivo
     * @param E Efector final
     * @param R Articulación actual
     * @return void
     */
    private void CCD(Articulation[] articulations, Transform D, Transform E, Articulation R)
    {
        // Calculate angle. Radians  
        float beta1 = 0.0f;
        float beta2 = 0.0f;

        // Vertical plane
        if (R.GetPlane().Equals(PlaneHelper.XY))
        {
            Vector3 rLocalPos = Vector3.zero;
            Vector3 dLocalPos = GetLocalPosition(D, articulations[1].transform);
            Vector3 eLocalPos = GetLocalPosition(E, articulations[1].transform);
                    
            if (!articulations[1].Equals(R))            
                rLocalPos = GetLocalPosition(R.transform, articulations[1].transform);             
            else
                rLocalPos = Vector3.zero;
            
            beta1 = CalculateAngle(dLocalPos, rLocalPos, R.GetPlane());        
            beta2 = CalculateAngle(eLocalPos , rLocalPos, R.GetPlane());            
        }
        // Horizontal plane
        else
        {
            beta1 = CalculateAngle(D.position, R.GetPosition(), R.GetPlane());
            beta2 = CalculateAngle(E.position, R.GetPosition(), R.GetPlane());
        }
      
        float beta = 0.0f;
        switch(R.GetPlane())
        {
            case PlaneHelper.XY:
                beta = (beta1 - beta2);
                break;
            case PlaneHelper.XZ:
                beta = (beta2 - beta1);
                break;
        } 
        
        // Shortest path
        if(R.GetPlane().Equals(PlaneHelper.XY)) {
            while (beta > Mathf.PI)
                beta -= 2 * Mathf.PI;

            while (beta < -Mathf.PI)
                beta += 2 * Mathf.PI;
        }
        
        // Degrees
        beta = beta * Mathf.Rad2Deg; 
        // Apply angle
        R.Rotate(beta);     
    }
    
    /**
     * Obtiene el ángulo (Radianes) que forman dos vectores.
     * @param end Vector fin
     * @param start Vector inicio
     * @param plane Plano
     * @return Ángulo (Radianes)
     */
    private float CalculateAngle(Vector3 end, Vector3 start, string plane)
    {
        // Radians
        Vector3 v = end - start;
        v = v.normalized;
  
        switch(plane)
        {
            case PlaneHelper.XY:
                return Mathf.Atan2(v.y, v.x);                
            case PlaneHelper.XZ:
                return Mathf.Atan2(v.z, v.x);              
            default: return 0.0f;
        }
    }

    /**
     * Obtiene la posición de un objeto si se toma como origen de coordenas otro origen.
     * @param obj Objeto
     * @param origin Origen de coordenadas
     * @return Vector3 Coordenas
     */
    private Vector3 GetLocalPosition(Transform obj, Transform origin)
    {
        return origin.transform.InverseTransformPoint(obj.position);
    }

    /**
     * Comprueba que una posición es alcanzable por el Scorbot. Se usan las copias de las articulaciones.s
     * @param D Posición objetivo
     * @param skipPitch Mantener el valor del pitch
     * @return bool Posición alcanzable
     */
    public bool TargetInRange(Transform D, bool skipPitch = false)
    {
        bool inRange = false;
        
        for (int i = 0; i < 1; i++) // Only one
        {
            switch (i)
            {
                case 0:
                    // Seems like real robot does this, not from current state
                    //scorbotModel.UpdateToCopy(art);
                    SetAnglesCopyHome();
                    break;
                case 1:
                    SetAnglesCopyHome();
                    break;
                case 2:
                    SetAnglesCopyZero();
                    break;
            }
                
            if (skipPitch) // Load restrictions from target. Pitch and roll
            {
                art[3].SetAngle(D.GetComponent<TargetModel>().GetAngles(3));
                art[4].SetAngle(D.GetComponent<TargetModel>().GetAngles(4));
            }

            // Execute CCD algorithm
            AlgCDD(art, D, artE, skipPitch);

            // Target close enough?       
            if (Vector3.Distance(D.position, artE.position) < EPSILON) {
                
                if (skipPitch)
                {
                    float pitchRequired = D.GetComponent<TargetModel>().GetAngles(3).z;
                    if ((Mathf.Abs(pitchRequired - art[3].GetGlobalAngle())) < EPSILON) {
                        // Succeed if pitch has same global angle
                        inRange = true;
                        break;
                    }
                }
                else
                {
                    inRange = true;
                    break;
                }
            }
        }
        return inRange;
    }

    /**
     * Mueve el Scorbot a su posición HOME en el tiempo especificado.
     * @param duration Duración (seg)
     * @return void
     */
    public void Home(float duration = 1f)
    {      
        for (int artIndex = articulations.Length - 1; artIndex >= 0; artIndex--)
        {
            articulations[artIndex].GoHome(duration);
        }      
    }

    /**
     * Cambia la posición a la que se quiere llegar.
     * @param D Posición objetivo
     * @return void
     */
    public void SetD(Transform D)
    {
        this.D = D;
    }

    /**
     * Cambia la velocidad de las articulaciones del Scorbot.
     * @param speed Velocidad (porcentaje 1-100)
     * @return void
     */
    public void SetSpeed(int speed)
    {
        for (int artIndex = articulations.Length - 1; artIndex >= 0; artIndex--)
        {
            articulations[artIndex].SetPercSpeed(speed);
        }
    }

    /**
     * Cambia la velocidad lineal actual. 1-300
     * @param speed Velocidad lineal (milimetros/seg)
     * @return void
     */
    public void SetSpeedL(int speed)
    {
        speedMillimeter = speed;
        if (speedMillimeter < 1)
            speedMillimeter = 1;

        if (speedMillimeter > 300)
            speedMillimeter = 300;
    }

    /**
     * Obtiene la velocidad (porcentaje) de las articulaciones actual.
     * @return int Velocidad (porcentaje)
     */
    public int GetSpeed()
    {
        return articulations[0].GetSpeed();
    }

    /**
     * Obtiene la velocidad lineal actual.
     * @return int Velocidad lineal (milimetros/seg)
     */
    public int GetSpeedL()
    {
        return speedMillimeter;
    }

    /**
     * Obtiene los ángulos de las copias de las articulaciones.
     * @return List<Vector3>
     */
    public List<Vector3> GetAnglesFromCopy()
    {
        List<Vector3> angles = new List<Vector3>();
        foreach (Articulation articulation in art)
            angles.Add(articulation.GetAngle());
        return angles;
    }

    /**
     * Obtiene los ángulos de las articulaciones.
     * @return List<Vector3> Ángulos
     */
    public List<Vector3> GetAngles()
    {
        List<Vector3> angles = new List<Vector3>();
        foreach (Articulation articulation in articulations)
            angles.Add(articulation.GetAngle());
        return angles;
    }

    /**
     * Mueve (instantaneo) las copias de las articulaciones a su posición inicial.
     * @return void
     */
    private void SetAnglesCopyZero()
    {
        foreach(Articulation a in art)
        {
            a.SetAngle(Vector3.zero);
        }
    }

    /**
     * Mueve (instantaneo) las copias de las articulaciones a la posición HOME.
     * @return void
     */
    private void SetAnglesCopyHome()
    {
        foreach (Articulation a in art)
        {
            a.SetAngle(a.GetAngleHome());
        }
    }

    /**
     * Obtiene las articulaciones del Scorbot.
     * @return Articulation[] Articulaciones
     */
    public Articulation[] GetArticulations()
    {
        return articulations;
    }

    /**
     * Obtiene la posición a la que se quiere llegar.
     * @return Transform Posición objetivo
     */
    public Transform GetD()
    {
        return D;
    }

    /**
     * Obtiene el efector final.
     * @return Transform Efector final
     */
    public Transform GetE()
    {
        return E;
    }

    /**
     * Obtiene la posición del efector final a partir de los conteos de encoder.
     * @param counts Conteos de encoder
     * @return Vector3 Coordenadas
     */
    public Vector3 GetPosFromCounts(List<int> counts)
    {
        return scorbotModel.GetPosFromAngles(art, artE, scorbotModel.CountsToAngles(counts));
    }
}
