using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationController : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator animator;

    public static UnityEvent animationEnd = new();
    void Start()
    {
        animator = GetComponent<Animator>();
        HookController.hookThrown.AddListener(ThrowHook);
        HookController.hookRetracted.AddListener(backToIdle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ThrowHook()
    {
        animator.SetTrigger("throw");
    }

    public void onAnimationEnd()
    {
        animationEnd.Invoke();
    }

    void backToIdle()
    {
        animator.SetTrigger("retract");
    }
}
