using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    BoidSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    public Vector3 bobPos;
    public bool isChasingBob = false;

    bool scared = false;

    Vector3 spawnPosition;

    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        cachedTransform = transform;
    }

    public void Initialize(BoidSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        spawnPosition = position;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (material != null)
        {
            material.color = col;
        }
    }

    public void UpdateBoid()
    {
        Vector3 acceleration = Vector3.zero;

        Vector3 offsetToSpawn = spawnPosition - position;
        if (offsetToSpawn.magnitude > 0.1f)
        {
            acceleration = SteerTowards(offsetToSpawn) * settings.spawnSteerForce;
        }

        //if (target != null)
        //{
        //    Vector3 offsetToTarget = (target.position - position);
        //    acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        //}

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            //steer boid toward avg direction
            var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            //steer boid to avg center of group
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            //steer boid away from individuals nearby, introducing chaos
            var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight; 

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        if (transform.position.y > -1.6f)
        {
            Vector3 pos = cachedTransform.position;
            pos.y = -1.6f;

            Vector3 downwardSteer = Vector3.down * settings.avoidCollisionWeight;
            acceleration += downwardSteer;

            cachedTransform.position = pos;
        }

        if (transform.position.y < -10f)
        {
            Vector3 upwardSteer = Vector3.up * settings.avoidCollisionWeight * 3f;
            acceleration += upwardSteer;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst))
        {
            if (hit.collider.CompareTag("FishAvoid"))
            {
                return true;
            }
        }
        else { }
        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

}