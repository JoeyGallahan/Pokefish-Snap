using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidController))]
public class BoidSeparation : MonoBehaviour
{
    [SerializeField] private BoidController boid;

    private void Awake()
    {
        boid = GetComponent<BoidController>();
    }

    private void Update()
    {
        Separate();
    }

    private void Separate()
    {
        List<GameObject> boids = boid.FindAllNearbyObjs(boid.Info.separationRadius, gameObject.layer, boid.Info.fishName);
        Vector3 averagePosition = Vector3.zero;

        for (int i = 0; i < boids.Count; i++)
        {
            averagePosition += (boids[i].transform.position - transform.position);
        }

        if (boids.Count > 0)
        {
            averagePosition /= boids.Count;

            boid.vel -= Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / boid.Info.separationRadius) * boid.Info.separationForce;
        }
    }
}
