using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour
{
    [SerializeField] private Boid info;
    public Vector3 vel;

    public Boid Info { get => info; }

    private void Awake()
    {
        vel = new Vector3(Random.Range(info.minMoveSpeed.x, info.maxMoveSpeed), Random.Range(info.minMoveSpeed.y, info.maxMoveSpeed), Random.Range(info.minMoveSpeed.z, info.maxMoveSpeed));
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public List<GameObject> FindAllNearbyObjs(float radius, int layer, string boidType = null)
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, radius, 1<<layer);
        List<GameObject> nearbyObjs = new List<GameObject>();

        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].gameObject != this.gameObject)
            {
                if (boidType == null || (boidType != null && boidType.Equals(info.fishName)))
                {
                    nearbyObjs.Add(colls[i].gameObject);
                }
            }
        }

        return nearbyObjs;
    }

    private void Move()
    {         
        if(vel.magnitude > info.maxMoveSpeed)
        {
            vel = vel.normalized * info.maxMoveSpeed;
        }
        transform.position += vel * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(vel);
    }
}
