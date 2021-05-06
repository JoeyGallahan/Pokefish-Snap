using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidController))]
public class BoidAlignment : MonoBehaviour
{
    [SerializeField] private BoidController boid;

    private void Awake()
    {
        boid = GetComponent<BoidController>();
    }

    // Update is called once per frame
    void Update()
    {
        Align();
    }

    private void Align()
    {
        List<GameObject> boids = boid.FindAllNearbyObjs(boid.Info.alignmentRadius, gameObject.layer, boid.Info.fishName);
        Vector3 averageVel = Vector3.zero;

        for (int i = 0; i < boids.Count; i++)
        {
            averageVel += boids[i].GetComponent<BoidController>().vel;
        }

        if (boids.Count > 0)
        {
            averageVel /= boids.Count;

            boid.vel += Vector3.Lerp(boid.vel, averageVel, Time.deltaTime);
        }
    }
}
