using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAI : MonoBehaviour
{
    public float speed = 2f;
    public float maxForce = 2f;
    public float neighborRadius = 1f;
    public float separationWeight = 1.5f;
    public float cohesionWeight = 1f;
    public float alignmentWeight = 1f;
    public float wanderRadius = 5f;
    public float wanderWeight = 0.5f;
    private Vector2 wanderTarget;
    private bool rotated;
    public LayerMask waterLayer;
    public LayerMask hookLayer;
    private bool isInWater = true;
    private bool isHooked = false;
    private Vector2 startingPosition;
    public float reverseDirectionDelay = 0.3f;

    public event Action<FishAI> FishHooked = delegate { };




    private Rigidbody2D rb;
    private List<FishAI> neighbors;
    void Start()
    {
        startingPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        neighbors = new List<FishAI>();
        wanderTarget = GetRandomWanderTarget();
    }


    private Vector2 Separation()
    {
        Vector2 steer = Vector2.zero;
        int count = 0;

        foreach (FishAI neighbor in neighbors)
        {
            float distance = Vector2.Distance(transform.position, neighbor.transform.position);
            if (distance < neighborRadius)
            {
                Vector2 direction = (Vector2)(transform.position - neighbor.transform.position);
                steer += direction.normalized / distance;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > maxForce)
        {
            steer = steer.normalized * maxForce;
        }

        return steer;
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);
        Gizmos.DrawLine(transform.position,transform.position + transform.up * 3);
    }*/

    private Vector2 Cohesion()
    {
        Vector2 center = Vector2.zero;
        int count = 0;

        foreach (FishAI neighbor in neighbors)
        {
            float distance = Vector2.Distance(transform.position, neighbor.transform.position);
            if (distance < neighborRadius)
            {
                center += (Vector2)neighbor.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            center /= count;
            Vector2 direction = center - (Vector2)transform.position;
            return direction.normalized * speed;
        }

        return Vector2.zero;
    }

    private Vector2 Alignment()
    {
        Vector2 avgVelocity = Vector2.zero;
        int count = 0;

        foreach (FishAI neighbor in neighbors)
        {
            float distance = Vector2.Distance(transform.position, neighbor.transform.position);
            if (distance < neighborRadius)
            {
                avgVelocity += neighbor.rb.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            avgVelocity /= count;
            return avgVelocity.normalized * speed;
        }

        return Vector2.zero;
    }

    private void UpdateNeighbors()
    {
        neighbors.Clear();
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, neighborRadius);

        foreach (Collider2D nearbyObject in nearbyObjects)
        {
            FishAI fishAI = nearbyObject.GetComponent<FishAI>();
            if (fishAI != null && fishAI != this)
            {
                neighbors.Add(fishAI);
            }
        }
    }

    private Vector2 GetFishTopDirection()
    {
        return transform.up;
    }

    private void FlipFishIfUpsideDown()
    {
        if (GetFishTopDirection().y < 0 && !rotated)
        {
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
            rotated = true;
        }
        else if (GetFishTopDirection().y > 0 && rotated)
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
            rotated = false;
        }
    }

    void Update()
    {
        if (isInWater)
        {
            UpdateNeighbors();

            Vector2 separationForce = Separation() * separationWeight;
            Vector2 cohesionForce = Cohesion() * cohesionWeight;
            Vector2 alignmentForce = Alignment() * alignmentWeight;
            Vector2 wanderForce = Wander() * wanderWeight;

            Vector2 totalForce = separationForce + cohesionForce + alignmentForce + wanderForce;
            rb.velocity += totalForce * Time.deltaTime;

            // Clamp the fish's velocity
            if (rb.velocity.magnitude > speed)
            {
                rb.velocity = rb.velocity.normalized * speed;
            }

            // Rotate the fish in the direction of its velocity
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        FlipFishIfUpsideDown();
    }


    private Vector2 GetRandomWanderTarget()
    {
        float randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        Vector2 randomTarget = (Vector2)transform.position + randomDirection * wanderRadius;
        return randomTarget;
    }

    private Vector2 Wander()
    {
        if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            wanderTarget = GetRandomWanderTarget();
        }

        Vector2 desiredVelocity = (wanderTarget - (Vector2)transform.position).normalized * speed;
        return desiredVelocity - rb.velocity;
    }

    private IEnumerator ReverseDirectionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = -rb.velocity;
    }

    public bool IsHooked
    {
        get { return isHooked; }
    }



    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            isInWater = false;
            // Reverses the fish's direction after a delay
            StartCoroutine(ReverseDirectionAfterDelay(reverseDirectionDelay));
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            isInWater = true;
        }

        if (((1 << other.gameObject.layer) & hookLayer) != 0 && other.gameObject.transform.childCount == 1)
        {
            // Hook the fish
            transform.SetParent(other.transform);
            rb.isKinematic = true;
            isHooked = true;
            // Disable the FishAI script when hooked
            this.enabled = false;
            //transform.position = other.transform.position;

            // Invoke the FishHooked event
            FishHooked?.Invoke(this);
        }
    }

    public void Reinitialize()
    {
        isHooked = false;
        this.enabled = true;
        rb.isKinematic = false;
    }

    public Vector2 StartingPosition
    {
        get { return startingPosition; }
    }


}
