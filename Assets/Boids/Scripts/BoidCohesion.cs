using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoidController))]
public class BoidCohesion : MonoBehaviour
{
    [SerializeField] private BoidController boid;

    private void Awake()
    {
        boid = GetComponent<BoidController>();    
    }

    // Update is called once per frame
    void Update()
    {
        Cohesion();
    }

    private void Cohesion()
    {
        List<GameObject> boids = boid.FindAllNearbyObjs(boid.Info.cohesionRadius, gameObject.layer, boid.Info.fishName);
        Vector3 averagePosition = Vector3.zero;

        for (int i = 0; i < boids.Count; i++)
        {           
            averagePosition += (boids[i].transform.position - transform.position);
        }

        if (boids.Count > 0)
        {
            averagePosition /= boids.Count;

            boid.vel += Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / boid.Info.cohesionRadius);
        }
    }
}
