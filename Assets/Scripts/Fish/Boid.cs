using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Boid : ScriptableObject
{
    public string fishName;
    public float minMoveSpeed;
    public float maxMoveSpeed;
    public float cohesionRadius;
    public float cohesionWeight;
    public float separationRadius;
    public float separationForce;
    public float separationWeight;
    public float alignmentRadius;
    public float alignmentWeight;
    public float obstacleAvoidanceRadius;
}
