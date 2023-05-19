using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FishAI : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.5f;

[SerializeField]
    private float rectangleWidth = 50;
    [SerializeField]
    private float rectangleHeight = 20;
    private bool rotated;
    [SerializeField]
    private LayerMask waterLayer;
    [SerializeField]
    private LayerMask hookLayer;
    private bool isInWater = true;
    private bool isHooked = false;
    public Vector2 startingPosition { get; set; }
    private Vector2 targetPosition;
    [SerializeField]
    private LayerMask hook;
    [SerializeField]
    private float hookCheckRadius = 100f;

    [SerializeField] private float neighbourRadius = 5f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float wanderWeight = 1f;

     private HerdManager herdManager;
     private List<FishAI> neighbours = new List<FishAI>();
    public event Action<FishAI> FishHooked = delegate { };




    private Rigidbody2D rb;
    void Start()
    {
        herdManager = GetComponentInParent<HerdManager>();
        startingPosition = transform.position;
        targetPosition = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    private bool IsHookNearby()
    {
        Collider2D nearbyHook = Physics2D.OverlapCircle(transform.position, hookCheckRadius, hookLayer);
        if (nearbyHook)
        {
            return true;
        }
        return false;
    }

    public void CheckForHook()
    {
        if (isHooked) return;
        bool hookNearby = IsHookNearby();
        if (!hookNearby && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else if (!gameObject.activeSelf && hookNearby)
        {
            gameObject.SetActive(true);
        }

    }

    private void GetNewTargetPosition()
{
    targetPosition = startingPosition + new Vector2(UnityEngine.Random.Range(-rectangleWidth/2, rectangleWidth/2), UnityEngine.Random.Range(-rectangleHeight/2, rectangleHeight/2));
}

    private void FixedUpdate()
    {
        UpdateNeighbors();

        Vector2 alignment = CalculateAlignment() * alignmentWeight;
        Vector2 cohesion = CalculateCohesion() * cohesionWeight;
        Vector2 separation = CalculateSeparation() * separationWeight;
        Vector2 wander = GetWanderDirection() * wanderWeight;

        Vector2 direction = alignment + cohesion + separation + wander;

        if (direction != Vector2.zero)
        {
            rb.velocity = direction.normalized * speed;
        }

        // Clamp the fish's velocity

        // If the fish is close enough to the target position, choose a new random target position
        // if (targetPosition == Vector2.zero || Vector2.Distance(transform.position, targetPosition) < 0.1f)
        // {
        //     GetNewTargetPosition();
        // }

        // // Calculate the direction vector and normalize it
        // Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        // Set the velocity
        // rb.velocity = direction * speed;

        //set the rotation
        // Rotate the fish in the direction of its velocity
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Flip the fish if it is upside down
        FlipFishIfUpsideDown();
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isHooked && ((1 << collision.gameObject.layer) & hookLayer) != 0 && collision.gameObject.transform.childCount == 1)
        {
            // Hook the fish
            transform.SetParent(collision.transform);
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
            isHooked = true;
            this.enabled = false;
            HookController.hookRetracted.AddListener(OnHookRetracted);

        }
    }

    private void OnHookRetracted()
    {
        HookController.hookRetracted.RemoveListener(OnHookRetracted);
        FishHooked?.Invoke(this);
    }

    public void Reinitialize()
    {
        isHooked = false;
        this.enabled = true;
        rb.isKinematic = false;
    }

    private void UpdateNeighbors()
    {
        neighbours.Clear();
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, neighbourRadius);

        foreach (Collider2D nearbyObject in nearbyObjects)
        {
            FishAI fishAI = nearbyObject.GetComponent<FishAI>();
            if (fishAI != null && fishAI != this)
            {
                neighbours.Add(fishAI);
            }
        }
    }

    private Vector2 CalculateAlignment()
    {
        if (neighbours.Count == 0) return rb.velocity;
        Vector2 alignment = Vector2.zero;
        foreach (FishAI neighbour in neighbours)
        {
            alignment += (Vector2)neighbour.rb.velocity;
        }
        return alignment / neighbours.Count;
    }

    private Vector2 CalculateCohesion()
    {
        if (neighbours.Count == 0) return Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        foreach (FishAI neighbour in neighbours)
        {
            cohesion += (Vector2)neighbour.transform.position;
        }
        cohesion /= neighbours.Count;
        return (cohesion - (Vector2)transform.position).normalized;
    }

    private Vector2 CalculateSeparation()
    {
        Vector2 steer = Vector2.zero;
        int count = 0;

        foreach (FishAI neighbour in neighbours)
        {
            float distance = Vector2.Distance(transform.position, neighbour.transform.position);
            if (distance < neighbourRadius)
            {
                Vector2 direction = (Vector2)(transform.position - neighbour.transform.position);
                steer += direction.normalized / distance;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > speed)
        {
            steer = steer.normalized * speed;
        }

        return steer;
    }

    private Vector2 GetWanderDirection()
    {
        if (targetPosition == Vector2.zero || Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            GetNewTargetPosition();
        }

        return (targetPosition - (Vector2)transform.position).normalized;
    }
}
