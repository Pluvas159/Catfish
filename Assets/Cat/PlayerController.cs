using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private GameObject hook;
    private HookController hookController;
    private bool inOriginalPosition = true;

    public int stars;
    
    public ThrowForceIndicator throwForceIndicator;  // The main camera

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple PlayerControllers in scene");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hook = transform.GetChild(0).gameObject;
        hookController = hook.GetComponent<HookController>();
        AnimationController.animationEnd.AddListener(ThrowHook);
        HookController.hookRetracted.AddListener(hookRetracted);

        throwForceIndicator.SetActive(false); // Ensure the slider is initially inactive
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inOriginalPosition)
        {
            throwForceIndicator.SetActive(true); // Activate the slider
            throwForceIndicator.IsIncreasing(true);
        }
        else if (Input.GetKeyUp(KeyCode.Space) && inOriginalPosition)
        {
            throwForceIndicator.SetActive(false); // Deactivate the slider
            ThrowingAnimation();
        }
        throwForceIndicator.SetPosition(transform.position);
    }

    private void hookRetracted()
    {
        inOriginalPosition = true;
        throwForceIndicator.SetActive(false); // Deactivate the slider when the hook is retracted
    }

    private void ThrowingAnimation() 
    {
        inOriginalPosition = false;
        HookController.hookThrown.Invoke();
    }

    private void ThrowHook()
    {
        hookController.ThrowHook(throwForceIndicator.value);
    }
}
