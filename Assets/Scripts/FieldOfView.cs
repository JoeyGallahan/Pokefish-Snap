using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Taken and modified from https://youtu.be/rQG9aUWarwE
 */

[System.Serializable]
public class FieldOfView :MonoBehaviour
{
    [SerializeField] public float viewRadius;
    [SerializeField] [Range(0, 360)] public float viewAngle;

    public LayerMask targetMask { get; private set; }
    public LayerMask environmentMask { get; private set; }

    public List<Vector3> visibleObstacles = new List<Vector3>();
    public List<GameObject> visibleTargets = new List<GameObject>();

    public Vector3 closestObstacle = Vector3.positiveInfinity;

    public void Init(float radius, float angle, LayerMask targetLayer, LayerMask environmentLayer)
    {
        viewRadius = radius;
        viewAngle = angle;
        targetMask = targetLayer;
        environmentMask = environmentLayer;
    }

    public void SearchForTarget()
    {
        visibleTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask); //Get anything that may be targeted by the fov within the view sphere

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform; //Get a target

            Vector3 dirToTarget = (target.position - transform.position).normalized; //Find the normalized direction to the target

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2.0f) //If the angle between this object and the target is within the angle allowed
            {
                float disToTarget = Vector3.Distance(transform.position, target.position); //Get the distance between this obj and the target
                if (!Physics.Raycast(transform.position, dirToTarget, disToTarget, environmentMask)) //Raycast to see if there's something in the environment blocking the view
                {
                    visibleTargets.Add(target.gameObject);
                }
            }
        }
    }

    public void SearchForObstacles()
    {
        visibleObstacles.Clear();
        closestObstacle = Vector3.positiveInfinity;

        Collider[] obstaclesInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, environmentMask); //Get anything that may be targeted by the fov within the view sphere

        for (int i = 0; i < obstaclesInViewRadius.Length; i++)
        {
            Transform obstacle = obstaclesInViewRadius[i].transform; //Get a target

            Vector3 dirToTarget = (obstacle.position - transform.position).normalized; //Find the normalized direction to the target

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2.0f) //If the angle between this object and the target is within the angle allowed
            {
                Vector3 hitPoint = obstaclesInViewRadius[i].ClosestPoint(transform.position);
                visibleObstacles.Add(hitPoint);

                if (closestObstacle == Vector3.positiveInfinity)
                {
                    closestObstacle = hitPoint;
                    Debug.Log(obstaclesInViewRadius[i].name + " at " + hitPoint);
                }
                else if (Vector3.Distance(hitPoint, transform.position) < Vector3.Distance(closestObstacle, transform.position))
                {
                    closestObstacle = hitPoint;
                    Debug.Log(obstaclesInViewRadius[i].name + " at " + hitPoint);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angleDegrees * Mathf.Deg2Rad));
    }
}
