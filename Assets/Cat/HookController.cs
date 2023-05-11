using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HookController : MonoBehaviour
{
    private Rigidbody2D hookRigidbody;
    public static bool isInWater;
    private bool isRetracting;
    private bool isThrown;
    private bool isStalling;
    private bool inOriginalPosition;
    private Vector3 originalPosition;
    private Vector3 retractPosition;
    private float waterLevel;
    [SerializeField]
    private float hookEffectivity;
    [SerializeField]
    private float retractSpeed;
    private GameObject lightChild;
    private float rotationSpeed = 5f;
    private SpriteRenderer spriteRenderer;


    public static UnityEvent hookInWater = new();
    public static UnityEvent hookThrown = new();
    public static UnityEvent hookRetracted = new();

    public void Start()
    {
        isInWater = false;
        isRetracting = false;
        isThrown = false;
        isStalling = false;
        inOriginalPosition = true;
        hookRigidbody = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        lightChild = transform.GetChild(0).gameObject;
        lightChild.SetActive(false);
        AnimationController.animationEnd.AddListener(ThrowHook);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inOriginalPosition) ThrowAnimation();
            else if (isRetracting)
            {
                isStalling = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isStalling) isStalling = false;
        }

        if (isRetracting && !isStalling)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float sideMovementSpeed = 10.0f; // You can adjust this value or make it a serialized field to set it in the Inspector
            Vector3 newPosition = transform.position + new Vector3(horizontalInput * sideMovementSpeed * Time.deltaTime, 0, 0);
            newPosition = Vector2.MoveTowards(newPosition, retractPosition, retractSpeed * Time.deltaTime);
            transform.position = newPosition;
            if (transform.position == retractPosition)
            {
                isInWater = false;
                lightChild.SetActive(false);
                isRetracting = false;
                hookInWater.Invoke();
            }
        }
    }


    public void FixedUpdate()
    {
        if (isInWater)
        {
            if (hookRigidbody.velocity.y >= 0 && !isRetracting)
            {
                hookRigidbody.velocity = Vector2.zero;
                hookRigidbody.isKinematic = true;
                isRetracting = true;
            }

        }
        else if (!isThrown && !inOriginalPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, originalPosition, retractSpeed * Time.deltaTime );
            if (transform.position == originalPosition)
            {
                hookRigidbody.gravityScale = 0;
                inOriginalPosition = true;
                hookRetracted.Invoke();
                spriteRenderer.enabled = false;
            }
        }
        UpdateHookRotation();
    }

    public void ThrowHook()
    {
        spriteRenderer.enabled = true;
        hookRigidbody.isKinematic = false;
        hookRigidbody.gravityScale = 1;
        hookRigidbody.AddForce(10 * hookRigidbody.mass * new Vector2(1,1), ForceMode2D.Impulse);
        isThrown = true;
        inOriginalPosition = false;
    }

    public void ThrowAnimation()
    {
        hookThrown.Invoke();
    }

    private void UpdateHookRotation()
    {
        if (!isInWater && !isThrown) return;
        float angle;

        if (isRetracting && hookRigidbody.isKinematic)
        {
            // Calculate the direction from the current position to the target position
            Vector2 direction = (originalPosition - transform.position).normalized;

            // Calculate the angle based on the direction
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        else
        {
            // Calculate the angle based on the hook's velocity
            angle = Mathf.Atan2(hookRigidbody.velocity.y, hookRigidbody.velocity.x) * Mathf.Rad2Deg;
        }

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90)); // Subtract 90 degrees to make the hook point in the direction of movement

        // Rotate the hook using Lerp and rotation speed
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }





    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("water") && !isInWater )
        {
            isInWater = true;
            lightChild.SetActive(true);
            isThrown = false;
            waterLevel = transform.position.y;
            retractPosition = new Vector3(originalPosition.x, waterLevel, 0);
            hookRigidbody.gravityScale = -1/hookEffectivity;
            hookRigidbody.velocity = new Vector2(0, hookRigidbody.velocity.y);
            hookInWater.Invoke();
        }
    }
}
