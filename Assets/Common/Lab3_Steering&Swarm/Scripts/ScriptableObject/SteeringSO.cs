using UnityEngine;

[CreateAssetMenu(fileName = "Steering", menuName = "Lab3/SteeringSO")]
public class SteeringSO : ScriptableObject
{
    [Header("Weights")]
    public float arriveWeight = 1f;
    public float separationWeight = 1f;
    public float cohesionWeight = 0.4f;
    public float alignmentWeight = 1f;
    public float avoidanceWeight = 1f;
        
    [Header("Use diff things")]
    public bool separation = true;
    public bool cohesion = true;
    public bool alignment = true;
    public bool avoidance = true;
        
    [Header("Debug")]
    public bool drawDebug = true;
}
