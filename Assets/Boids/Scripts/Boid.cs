using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Boid : ScriptableObject
{
    public string fishName;
    public Vector3 minMoveSpeed;
    public float maxMoveSpeed;
    public float cohesionRadius;
    public float separationRadius;
    public float alignmentRadius;
    public float separationForce;
}
