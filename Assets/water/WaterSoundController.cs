using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSoundController : MonoBehaviour
{

    // Assuming that you've assigned the AudioClip in the Inspector.
    private AudioSource waterOutsideAudioSource;
    private AudioSource waterInsideAudioSource;
    private AudioSource splashAudioSource;

    private float changeSoundCooldown = 1f; // The cooldown time in seconds.
    private float lastChangeSoundTime; // The time when ChangeSound was last called.

    void Start()
    {
        waterOutsideAudioSource = GetComponents<AudioSource>()[0];
        waterInsideAudioSource = GetComponents<AudioSource>()[1];
        splashAudioSource = GetComponents<AudioSource>()[2];

        waterInsideAudioSource.Stop();
        HookController.hookInWater.AddListener(ChangeSound);
    }

    private void ChangeSound(bool inWater)
    {
        // If the cooldown has not passed yet, return without doing anything.
        if (Time.time - lastChangeSoundTime < changeSoundCooldown)
        {
            return;
        }

        // Update the time when ChangeSound was last called.
        lastChangeSoundTime = Time.time;

        if (inWater)
        {
            splashAudioSource.Play();
            PlayInsideWater();
        }
        else
        {
            PlayOutsideWater();
        }
    }

    private void PlayInsideWater()
    {
        StartCoroutine(FadeIn(waterInsideAudioSource, 1f));
        StartCoroutine(FadeOut(waterOutsideAudioSource, 1f));
    }

    private void PlayOutsideWater()
    {
       StartCoroutine(FadeIn(waterOutsideAudioSource, 1f));
       StartCoroutine(FadeOut(waterInsideAudioSource, 1f));
    }

    IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
    {
        float targetVolume = audioSource.volume;
        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += targetVolume * Time.deltaTime / fadeTime;

            yield return null;
        }
    }

}
