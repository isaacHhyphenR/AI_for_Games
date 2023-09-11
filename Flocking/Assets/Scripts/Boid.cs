using Unity.VisualScripting;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [SerializeField] DebugPanel debugPanel;
    [SerializeField] float speed;
    [SerializeField] float cohesionRadius;
    [SerializeField] float separationRadius;
    [SerializeField] float alignmentRadius;
    [SerializeField] float cohesionForce;
    [SerializeField] float separationForce;
    [SerializeField] float maxSeparationForce;
    [SerializeField] float alignmentForce;
    [SerializeField] float maxForce;
    [SerializeField] Vector3 screenBounds; //the XYZ at which the boids should wrap around
    [SerializeField] Renderer[] renders;
    [SerializeField] bool dynamicColor;
    [SerializeField] float beaconForce;
    [SerializeField] float boundingForce;
    [SerializeField] float boundingRange;

    Vector3 velocity;
    Vector3 prevVelocity; //for alignment purposes

    Vector3 beaconVector;

    private void Start()
    {
        velocity = transform.forward;
        velocity *= speed;
        prevVelocity = velocity;
        transform.Translate(velocity * Time.deltaTime);
    }

    private void Update()
    {
        Move();
        if(dynamicColor)
        {
            SetColor();
        }
    }

    private void Move()
    {
        prevVelocity = velocity;
        Vector3 force = Cohesion() * cohesionForce + Separation() * separationForce + Alignment() * alignmentForce + BeaconForce() * beaconForce + Bounding() * boundingForce;
        velocity += force * Time.deltaTime * speed;
        velocity = Vector3.ClampMagnitude(velocity, speed);
        transform.LookAt(transform.position + velocity * Time.deltaTime, Vector3.up); //turn to face the correct direction
        transform.position += velocity * Time.deltaTime; //actually do the movement

        //Wraps around the screen
        //transform.position = ScreenWrap();
    }

    private Vector3 ScreenWrap()
    {
        Vector3 newPos = transform.position;
        if (transform.position.x > screenBounds.x)
        {
            newPos.x = -screenBounds.x;
        }
        else if (transform.position.x < -screenBounds.x)
        {
            newPos.x = screenBounds.x;
        }
        if (transform.position.y > screenBounds.y) //y
        {
            newPos.y = -screenBounds.y;
        }
        else if (transform.position.y < -screenBounds.y)
        {
            newPos.y = screenBounds.y;
        }
        if (transform.position.z > screenBounds.z) //z
        {
            newPos.z = -screenBounds.z;
        }
        else if (transform.position.z < -screenBounds.z)
        {
            newPos.z = screenBounds.z;
        }

        return newPos;
    }
    private Vector3 Cohesion()
    {
        //Gets the average position of surrounding boids
        Vector3 cohesion = Vector3.zero;
        Collider[] cohesionArray = Physics.OverlapSphere(transform.position, cohesionRadius);
        int cohesionSize = 0; //how many boids are actually in the radius
        foreach (var boid in cohesionArray) //sum the positions
        {
            if (boid.gameObject != gameObject && boid.gameObject.tag == gameObject.tag)
            {
                cohesion += boid.transform.position;
                cohesionSize++;
            }
        }
        cohesion /= cohesionSize; //average
        //translates that into a directional vector
        float distance = Vector3.Distance(transform.position, cohesion); //determines magnitude
        if (distance > cohesionRadius || cohesionSize < 1) //no cohesion force if there are no boids in radius
        {
            return Vector3.zero;
        }
        distance /= cohesionRadius;
        cohesion -= transform.position;
        cohesion.Normalize();
        cohesion *= distance;

        return Vector3.ClampMagnitude(cohesion - velocity, maxForce);
    }

    private Vector3 Separation()
    {
        //Sums the vectors form all surrounding boids
        Vector3 separation = Vector3.zero;
        Collider[] separationArray = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var boid in separationArray) //sum the positions
        {
            if (boid.gameObject != gameObject && boid.gameObject.tag == gameObject.tag)
            {
                //figures out how much force this boid applies to you
                float distance = Vector3.Distance(transform.position, boid.transform.position);
                Vector3 boidForce = transform.position - boid.transform.position;
                if (distance != 0) //inverse scales with distance
                {
                    boidForce /= distance;
                }
                else
                {
                    boidForce /= 0.0000001f;
                }
                boidForce = Vector3.ClampMagnitude(boidForce, maxSeparationForce);
                separation += boidForce;
            }
        }
        separation.Normalize();
        return separation;
    }

    private Vector3 Alignment()
    {
        //Sums the vectors form all surrounding boids
        Vector3 alignment = Vector3.zero;
        Collider[] alignmentArray = Physics.OverlapSphere(transform.position, alignmentRadius);
        int alignmentSize = 0; //how many boids are actually in the radius
        foreach (var boid in alignmentArray) //sum the positions
        {
            if (boid.gameObject.tag == gameObject.tag)
            {
                alignment += boid.GetComponent<Boid>().GetVelocity();
                alignmentSize++;
            }
        }
        alignment /= alignmentSize;
        alignment.Normalize();
        return alignment * speed;
    }

    private Vector3 Bounding()
    {
        Vector3 bounding = Vector3.zero;
        //left
        if(transform.position.x > screenBounds.x - boundingRange)
        {
            Vector3 reboundForce = new Vector3(-1,0,0);
            float distance = Mathf.Abs(screenBounds.x - transform.position.x) / boundingRange;
            if (transform.position.x < screenBounds.x) //if still on the screen, use distance
            {
                reboundForce /= distance;
            }
            else //else just be really strong
            {
                reboundForce /= 0.000001f;
            }
            bounding += reboundForce;
        }
        //right
        else if (transform.position.x < -screenBounds.x + boundingRange)
        {
            Vector3 reboundForce = new Vector3(1, 0, 0);
            float distance = Mathf.Abs(-screenBounds.x - transform.position.x) / boundingRange;
            if (transform.position.x > screenBounds.x) //if still on the screen, use distance
            {
                reboundForce /= distance;
            }
            else
            {
                reboundForce /= 0.000001f;
            }
            bounding += reboundForce;
        }
        //Bottom
        if (transform.position.z > screenBounds.z - boundingRange)
        {
            Vector3 reboundForce = new Vector3(0, 0, -1);
            float distance = Mathf.Abs(screenBounds.z - transform.position.z) / boundingRange;
            if (transform.position.z < screenBounds.z) //if still on the screen, use distance
            {
                reboundForce /= distance;
            }
            else
            {
                reboundForce /= 0.000001f;
            }
            bounding += reboundForce;
        }
        //Top
        else if (transform.position.z < -screenBounds.z + boundingRange)
        {
            Vector3 reboundForce = new Vector3(0, 0, 1);
            float distance = Mathf.Abs(-screenBounds.z - transform.position.z) / boundingRange;
            if (transform.position.z > screenBounds.z) //if still on the screen, use distance
            {
                reboundForce /= distance;
            }
            else
            {
                reboundForce /= 0.000001f;
            }
            bounding += reboundForce;
        }
        return bounding;
    }
    public Vector3 GetVelocity()
    {
        return prevVelocity;
    }

    private void SetColor()
    {
        //figures out where the boid is relative to the background/camera
        float relativeHeight = transform.position.y + screenBounds.y; //accounts for negative back
        relativeHeight /= screenBounds.y * 2;
        //sets color relative to that-- blue close, red far away
        float blue = 1f - relativeHeight;// 0.5f;
        float red = 1f - relativeHeight; //(1f - relativeHeight);
        float green = 1f - relativeHeight;// 0.5f;// 0.5f;
        Color color = new Color(red, green, blue);
        foreach(Renderer render in renders)
        {
            render.material.SetColor("_Color", color);
        }
    }

    public void AddBeaconVector(Vector3 newVector)
    {
        beaconVector += newVector;
    }
    public void AddBeaconVector(Vector3 newVector, float newForce)
    {
        beaconVector += newVector * newForce;
    }

    Vector3 BeaconForce()
    {
        beaconVector.Normalize();
        Vector3 returnVector = beaconVector;
        beaconVector = Vector3.zero;
        return returnVector;
    }

    public Vector3 GetBounds()
    {
        return screenBounds;
    }

    public void SetSpeed(float value)
    {
        speed = value;
    }
    public void SetCohesionRadius(float value)
    {
        cohesionRadius = value;
    }
    public void SetSeparationRadius(float value)
    {
        separationRadius = value;
    }
    public void SetAlignmentRadius(float value)
    {
        alignmentRadius = value;
    }
    public void SetCohesionForce(float value)
    {
        cohesionForce = value;
    }
    public void SetSeparationForce(float value)
    {
        separationForce = value;
    }
    public void SetAlignmentForce(float value)
    {
        alignmentForce = value;
    }
    public void SetBeaconForce(float value)
    {
        beaconForce = value;
    }
}
