using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookSound : MonoBehaviour
{
    private AudioSource hookSound;
    // Start is called before the first frame update
    void Start()
    {
        hookSound = GetComponent<AudioSource>();
        AnimationController.animationEnd.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        hookSound.Play();
    }
}
