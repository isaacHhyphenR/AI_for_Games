using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    [SerializeField] float cohesionRadius;
    [SerializeField] float separationRadius;
    [SerializeField] float cohesionforce;
    [SerializeField] float separationforce;
    [SerializeField] float maxSeparationForce;

    private void Update()
    {
        Combined();
        //Cohesion();
        //Separation();
    }

    void Combined()
    {
        float range = cohesionRadius;
        if (separationRadius > cohesionRadius)
        {
            range = separationRadius;
        }
        Collider[] boidArray = Physics.OverlapSphere(transform.position, range);
        foreach (var boid in boidArray) //adds force to each of them
        {
            if (boid.gameObject.tag == "Boid")
            {
                Vector3 vector = Vector3.zero;
                float distance = Vector3.Distance(transform.position, boid.transform.position);
                //COHESION
                if (distance <= cohesionRadius)
                {
                    Vector3 cohesion = transform.position - boid.transform.position;
                    cohesion.Normalize();
                    cohesion *= cohesionforce;
                    vector += cohesion;
                }
                if (distance <= separationRadius)
                {
                    Vector3 separation = boid.transform.position - transform.position;
                    if (distance != 0) //inverse scales with distance
                    {
                        separation /= distance;
                    }
                    else
                    {
                        separation /= 0.0000001f;
                    }
                    separation *= separationforce;
                    separation = Vector3.ClampMagnitude(separation, maxSeparationForce);
                    vector += separation;
                }
                boid.gameObject.GetComponent<Boid>().AddBeaconVector(vector);
            }
        }
    }
    void Cohesion()
    {
        Collider[] cohesionArray = Physics.OverlapSphere(transform.position, cohesionRadius);
        foreach (var boid in cohesionArray) //adds force to each of them
        {
            if (boid.gameObject.tag == "Boid")
            {
                Vector3 cohesion = transform.position - boid.transform.position;
                cohesion.Normalize();
                boid.gameObject.GetComponent<Boid>().AddBeaconVector(cohesion, cohesionforce);
            }
        }
    }

    void Separation()
    {
        Collider[] separationArray = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var boid in separationArray) //sum the positions
        {
            if (boid.gameObject.tag == "Boid")
            {
                //figures out how much force you apply to this boid
                float distance = Vector3.Distance(transform.position, boid.transform.position);
                Vector3 separation = boid.transform.position - transform.position;
                if (distance != 0) //inverse scales with distance
                {
                    separation /= distance;
                }
                else
                {
                    separation /= 0.0000001f;
                }
                separation = Vector3.ClampMagnitude(separation, maxSeparationForce);
                boid.gameObject.GetComponent<Boid>().AddBeaconVector(separation, cohesionforce);
            }
        }
    }

    public void SetCohesionRadius(float value)
    {
        cohesionRadius = value;
    }
    public void SetSeparationRadius(float value)
    {
        separationRadius = value;
    }
}
