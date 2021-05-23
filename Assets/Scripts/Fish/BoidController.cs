using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *  https://youtu.be/rQG9aUWarwE mixed with https://youtu.be/_d8M3Y-hiUs
 */

public class BoidController : MonoBehaviour
{
    [SerializeField] private Boid info;
    public Vector3 vel;
    public Vector3 acc;
    public Vector3 position;
    public Vector3 forward;

    private int fishLayer;
    [SerializeField] private int environmentLayer;

    private const float visionAngle = 270.0f;

    public Boid Info { get => info; }

    [SerializeField] private FieldOfView alignFOV;
    [SerializeField] private FieldOfView cohesionFOV;
    [SerializeField] private FieldOfView separationFOV;
    [SerializeField] private FieldOfView collisionFOV;

    float collisionAvoidDst = 2.0f;

    private void Awake()
    {
        fishLayer = gameObject.layer;

        alignFOV.Init(info.alignmentRadius, visionAngle, 1<<fishLayer, 1<<environmentLayer);
        cohesionFOV.Init(info.cohesionRadius, visionAngle, 1<<fishLayer, 1<<environmentLayer);
        separationFOV.Init(info.separationRadius, visionAngle, 1<<fishLayer, 1<<environmentLayer);
        collisionFOV.Init(info.obstacleAvoidanceRadius, visionAngle, 1 << fishLayer, 1 << environmentLayer);

        position = transform.position;
        forward = transform.forward;

        int negativeMaybe = Random.Range(0, 2);

        if (negativeMaybe == 0)
        {
            negativeMaybe = -1;
        }

        vel = new Vector3(negativeMaybe * Random.Range(info.minMoveSpeed, info.maxMoveSpeed), 0, Random.Range(info.minMoveSpeed, info.maxMoveSpeed));
    }
    
    private void Update()
    {
        position = transform.position;
        acc = Vector3.zero;

        Align();
        Cohesion();
        Separate();
        CheckForCollision();

        Move();

    }

    private void Move()
    {
        vel += acc * Time.deltaTime;
        float speed = vel.magnitude;
        Vector3 dir = vel / speed;
        speed = Mathf.Clamp(speed, info.minMoveSpeed, info.maxMoveSpeed);
        vel = dir * speed;

        forward = dir;

        transform.position += vel * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(vel);
    }

    private void Align()
    {
        alignFOV.SearchForTarget();

        Vector3 averageVel = Vector3.zero;

        foreach(GameObject obj in alignFOV.visibleTargets)
        {
            BoidController fish = obj.GetComponent<BoidController>();
            if (fish.Info.fishName.Equals(info.fishName))
            {
                averageVel += fish.vel;
            }
        }

        if (alignFOV.visibleTargets.Count > 0)
        {
            averageVel /= (float)alignFOV.visibleTargets.Count;

            acc += Vector3.Lerp(vel, averageVel, Time.deltaTime) * info.alignmentWeight;
        }
    }

    private void Cohesion()
    {
        cohesionFOV.SearchForTarget();

        Vector3 averagePosition = Vector3.zero;

        foreach (GameObject obj in cohesionFOV.visibleTargets)
        {
            BoidController fish = obj.GetComponent<BoidController>();
            if (fish.Info.fishName.Equals(info.fishName))
            {
                averagePosition += (fish.position - position);
            }
        }

        if (cohesionFOV.visibleTargets.Count > 0)
        {
            averagePosition /= (float)cohesionFOV.visibleTargets.Count;

            acc += Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / Info.cohesionRadius) * info.cohesionWeight;
        }
    }

    private void Separate()
    {
        separationFOV.SearchForTarget();

        Vector3 averagePosition = Vector3.zero;

        foreach (GameObject obj in separationFOV.visibleTargets)
        {
            BoidController fish = obj.GetComponent<BoidController>();
            if (fish.Info.fishName.Equals(info.fishName))
            {
                averagePosition += (fish.position - position);
            }
        }

        if (separationFOV.visibleTargets.Count > 0)
        {
            averagePosition /= (float)separationFOV.visibleTargets.Count;

            acc -= Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / Info.separationRadius) * info.separationForce;
        }
    }

    private void CheckForCollision()
    {
        Debug.Log(name + " collision check...");
        collisionFOV.SearchForObstacles();

        Vector3 safePoint = Vector3.zero;
        if (collisionFOV.visibleObstacles.Count > 0)
        {
            safePoint = FindSafePoint(collisionFOV.closestObstacle);
        }

        acc += safePoint;
    }

    private Vector3 FindSafePoint(Vector3 avoid)
    {
        int raysEachWay = 50;
        float offset = visionAngle / (float)raysEachWay;
        float distToTarget = Vector3.Distance(avoid, position);

        for (int i = -1; i < 2; i+=2)
        {
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    Vector3 dir = forward;
                    dir.x += (offset * x) * i;
                    dir.y += (offset * y) * i;

                    Debug.DrawRay(position, dir, Color.white);
                    Debug.Log("fuck " + x + " , " + y + " : " + transform.position + dir);
                    if (!Physics.Raycast(position, dir, distToTarget, 1<<environmentLayer))
                    {
                        Debug.Log("Gotee");
                        return transform.position + dir;
                    }
                }
            }
        }

        return -forward;
    }
}
/*
public void FindAllNearbyObjs(float radius, int layer)
{
    Collider[] colls = Physics.OverlapSphere(transform.position, radius, 1<<layer);

    nearOBJs.Clear();
    fishFriends.Clear();
    for (int i = 0; i < colls.Length; i++)
    {
        if (colls[i].gameObject != this.gameObject)
        {
            nearOBJs.Add(colls[i].gameObject);

            BoidController fish = colls[i].gameObject.GetComponent<BoidController>();
            if (fish != null && fish.Info.fishName.Equals(info.fishName))
            {
                fishFriends.Add(fish);
            }
        }
    }
}

private void Move()
{         
    transform.position += vel * Time.deltaTime;

    transform.rotation = Quaternion.LookRotation(vel); //Quaternion.Lerp(transform.rotation, Quaternion.Euler(vel), Time.deltaTime);
}

private void Align()
{
    FindAllNearbyObjs(Info.alignmentRadius, gameObject.layer);

    Vector3 averageVel = Vector3.zero;

    for (int i = 0; i < fishFriends.Count; i++)
    {
        averageVel += fishFriends[i].vel;
    }

    if (fishFriends.Count > 0)
    {
        averageVel /= (float)fishFriends.Count;

        vel += Vector3.Lerp(vel, averageVel, Time.deltaTime);
    }
}
private void Cohesion()
{
    FindAllNearbyObjs(Info.cohesionRadius, gameObject.layer);

    Vector3 averagePosition = Vector3.zero;

    for (int i = 0; i < fishFriends.Count; i++)
    {
        averagePosition += (fishFriends[i].transform.position - transform.position);
    }

    if (fishFriends.Count > 0)
    {
        averagePosition /= (float)fishFriends.Count;

        vel += Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / Info.cohesionRadius);
    }
}

private void Separate()
{
    FindAllNearbyObjs(Info.separationRadius, gameObject.layer);

    Vector3 averagePosition = Vector3.zero;

    for (int i = 0; i < fishFriends.Count; i++)
    {
        averagePosition += (fishFriends[i].transform.position - transform.position);
    }

    if (fishFriends.Count > 0)
    {
        averagePosition /= (float)fishFriends.Count;

        vel -= Vector3.Lerp(Vector3.zero, averagePosition, averagePosition.magnitude / Info.separationRadius) * Info.separationForce;
    }
}
*/
