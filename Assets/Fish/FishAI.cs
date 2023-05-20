using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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
    public bool isHooked = false;

    private Vector2 direction;
    public Vector2 startingPosition { get; set; }
    private Vector2 targetPosition;
    [SerializeField]
    private Transform hook;
    private HookController hookController;
    [SerializeField]
    private float hookCheckRadius = 100f;

    private float timeToDeath = 5f;
    private float timeToDeathCounter = 0f;

    private bool dead = false;

    [SerializeField]
    private float requiredHookEffectivity = 1f;

    [SerializeField]
    private int neededNumberOfSpaces = 10;
    private int currentNumberOfSpaces = 0;

    [SerializeField] private float neighbourRadius = 5f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float wanderWeight = 1f;

    private HerdManager herdManager;
    private List<FishAI> neighbours = new List<FishAI>();
    public static event Action<FishAI> FishHooked = delegate { };

    [SerializeField]
    private FishType fishType;

    private Rigidbody2D rb;

    private CinemachineImpulseSource impulseSource;
    void Start()
    {
        herdManager = GetComponentInParent<HerdManager>();
        startingPosition = transform.position;
        targetPosition = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
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
        targetPosition = startingPosition + new Vector2(UnityEngine.Random.Range(-rectangleWidth / 2, rectangleWidth / 2), UnityEngine.Random.Range(-rectangleHeight / 2, rectangleHeight / 2));
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isHooked && !dead)
        {
            currentNumberOfSpaces++;
            direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), -1f);
            CameraShakeManager.instance.CameraShake(impulseSource);
            if (currentNumberOfSpaces >= neededNumberOfSpaces) {
                timeToDeathCounter = timeToDeath;
            }
        }

        if (isHooked && !dead)
        {
            timeToDeathCounter += Time.deltaTime;
            if (timeToDeathCounter >= timeToDeath)
            {
                if (currentNumberOfSpaces >= neededNumberOfSpaces) {
                    dead = true;
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    hookController.enabled = true;
                    rb.isKinematic = true;
                    transform.SetParent(hook);
                }
                else {
                    hookController.enabled = true; isHooked = false; hook = null;HookController.hookRetracted.RemoveListener(OnHookRetracted);}
            }
        }
    }

    private void FixedUpdate()
    {
        Rotate();
        FlipFishIfUpsideDown();

        if (isHooked)
        {

            if (direction != Vector2.zero && !dead)
            {
                rb.velocity = direction.normalized * speed * 3f;
                hook.position = transform.position;
            }
        }
        else
        {
            UpdateNeighbors();

            Vector2 alignment = CalculateAlignment() * alignmentWeight;
            Vector2 cohesion = CalculateCohesion() * cohesionWeight;
            Vector2 separation = CalculateSeparation() * separationWeight;
            Vector2 wander = GetWanderDirection() * wanderWeight;

            direction = alignment + cohesion + separation + wander;

            if (direction != Vector2.zero)
            {
                rb.velocity = direction.normalized * speed;
            }
        }

    }

    private void Rotate()
    {
        if (dead)
        {
            Vector3 hookRotation = hook.rotation.eulerAngles;

            // Add 90 degrees to the z-axis rotation
            hookRotation.z += 90;

            // Set the transform's rotation to the new rotation
            transform.rotation = Quaternion.Euler(hookRotation);
            return;
        }
        // Get the angle of the velocity vector in radians
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);

        // Convert the angle to degrees
        angle = angle * Mathf.Rad2Deg;

        // Set the rotation of the rigidbody
        rb.rotation = angle;
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

            hookController = collision.gameObject.GetComponent<HookController>();
            if (hookController.GetHookEffectivity() < requiredHookEffectivity || !hookController.enabled)
            {
                return;
            }
            hookController.enabled = false;
            hook = collision.transform;
            hook.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            isHooked = true;
            HookController.hookRetracted.AddListener(OnHookRetracted);
            direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), -1f);
            currentNumberOfSpaces = 0;
            timeToDeathCounter = 0f;
            //change needed number of spaces based on hook effectivity, the higher the effectivity compared to the required effectivity, the less spaces are needed
            neededNumberOfSpaces = Mathf.RoundToInt(requiredHookEffectivity / hookController.GetHookEffectivity() * 10f * requiredHookEffectivity);
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
        dead = false;
        timeToDeathCounter = 0f;
        currentNumberOfSpaces = 0;
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

    public FishType GetFishType()
    {
        return fishType;
    }
}
