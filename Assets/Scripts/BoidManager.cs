using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;

    public float baitDistance = 1f;

    void Start()
    {
        boids = FindObjectsByType<Boid>(FindObjectsSortMode.None);
        foreach (Boid b in boids)
        {
            b.Initialize(settings, null);
        }
    }

    void Update()
    {
        if (boids != null)
        {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", boids.Length);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Length; i++)
            {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                boids[i].UpdateBoid();
            }

            boidBuffer.Release();
        }
    }

    //public void Released()
    //{
    //    if (!hasReleased)
    //    {
    //        hasReleased = true;

    //        var bobPos = FindObjectOfType<AnchorSpring>().transform.position;
    //        StartCoroutine(SetBobPos(bobPos));

    //        Boid closestBoid = null;
    //        float closestDistSqr = Mathf.Infinity;

    //        foreach (Boid boid in boids)
    //        {
    //            float distSqr = (boid.position - bobPos).sqrMagnitude;
    //            if (distSqr < closestDistSqr)
    //            {
    //                closestDistSqr = distSqr;
    //                closestBoid = boid;
    //            }
    //        }

    //        if (closestBoid != null)
    //        {
    //            closestBoid.isChasingBob = true;
    //        }
    //    }
    //    else return;

    //}

    //IEnumerator SetBobPos(Vector3 bobPos)
    //{
    //    yield return new WaitForSeconds(4f);
    //    _bobPos = bobPos;
    //}

    //public void Retract()
    //{
    //    if (hasReleased)
    //    {
    //        hasReleased = false;
    //        Scare();
    //    } else
    //    {
    //        return;
    //    }
    //}

    //public void Scare()
    //{
    //    var bobPos = FindObjectOfType<AnchorSpring>().transform.position;
    //    _bobPos = bobPos;

    //    Boid closestBoid = null;
    //    float closestDistSqr = Mathf.Infinity;

    //    foreach (Boid boid in boids)
    //    {
    //        float distSqr = (boid.position - bobPos).sqrMagnitude;
    //        if (distSqr < closestDistSqr)
    //        {
    //            closestDistSqr = distSqr;
    //            closestBoid = boid;
    //        }
    //    }

    //    if (closestBoid != null)
    //    {
    //        closestBoid.isChasingBob = false;
    //        closestBoid.bobPos = _bobPos;
    //    }
    //}

    //IEnumerator SearchForNewFish()
    //{
    //    yield return new WaitForSeconds(4f);
    //    hasReleased = false;
    //    Released();
    //}

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}