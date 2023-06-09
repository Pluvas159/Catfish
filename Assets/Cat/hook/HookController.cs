using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
public class HookController : MonoBehaviour
{
    private Rigidbody2D hookRigidbody;
    public static bool isInWater;
    private bool isRetracting;
    private bool isThrown;
    public bool isStalling { get; set; }
    private Vector3 originalPosition;
    private Vector3 retractPosition;
    private float waterLevel;
    private float hookEffectivity;
    [SerializeField]
    private float retractSpeed;
    private GameObject lightChild;
    private float rotationSpeed = 5f;
    private SpriteRenderer spriteRenderer;

    private CinemachineImpulseSource impulseSource;

    [SerializeField]
    private HookType hookType;

    private BoxCollider2D boxCollider;


    public static UnityEvent<bool> hookInWater = new();
    public static UnityEvent hookThrown = new();
    public static UnityEvent hookRetracted = new();



    public void Start()
    {
        isInWater = false;
        isRetracting = false;
        isThrown = false;
        isStalling = false;
        hookRigidbody = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        lightChild = transform.GetChild(0).gameObject;
        lightChild.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        hookEffectivity = hookType.hookEffectivity;
        spriteRenderer.sprite = hookType.inventoryItem.icon;
        Shop.hookBought.AddListener(ChangeHookType);
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isRetracting && transform.childCount < 2)
            {
                isStalling = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isStalling) isStalling = false;
        }

        if (isRetracting && !isStalling && isInWater)
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
                isRetracting = true;
                hookInWater.Invoke(false);
            }
        }
    }


    public void FixedUpdate()
    {
        if (isInWater)
        {
            if (boxCollider.enabled) boxCollider.enabled = false;
            if (hookRigidbody.velocity.y >= 0 && !isRetracting)
            {
                hookRigidbody.velocity = Vector2.zero;
                hookRigidbody.isKinematic = true;
                isRetracting = true;
            }

        }
        else if (isRetracting)
        {
            transform.position = Vector2.MoveTowards(transform.position, originalPosition, retractSpeed * Time.deltaTime );
            if (transform.position == originalPosition)
            {
                hookRigidbody.gravityScale = 0;
                hookRetracted.Invoke();
                spriteRenderer.enabled = false;
                isRetracting = false;
            }
        }
        UpdateHookRotation();
    }

    public void ThrowHook(float value)
    {
        spriteRenderer.enabled = true;
        hookRigidbody.isKinematic = false;
        hookRigidbody.gravityScale = 1;
        hookRigidbody.AddForce(value * 20 * new Vector2(1,1), ForceMode2D.Impulse);
        isThrown = true;
        boxCollider.enabled = true;
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

    public float GetHookEffectivity()
    {
        return hookEffectivity;
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
            hookInWater.Invoke(true);
            CameraShakeManager.instance.CameraShake(impulseSource);
        }
    }

    public void ChangeHookType(HookType newHookType)
    {
        hookType = newHookType;
        hookEffectivity = hookType.hookEffectivity;
        spriteRenderer.sprite = hookType.inventoryItem.icon;
    }
}
