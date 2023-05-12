using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdManager : MonoBehaviour
{
    public float hookCheckRadius = 10f;
    public LayerMask hookLayer;

    private List<FishAI> fishes;

    private FishAI hookedFish;

    private void Start()
    {
        fishes = new List<FishAI>(GetComponentsInChildren<FishAI>());

        foreach (var fish in fishes)
        {
            fish.FishHooked += OnFishHooked;
        }

        HookController.hookRetracted.AddListener(OnHookRetracted);
    }

    private void OnFishHooked(FishAI fish)
    {
        hookedFish = fish;
    }

    private void OnHookRetracted()
    {
        if (hookedFish != null)
        {

            hookedFish.gameObject.SetActive(false);
            hookedFish.transform.SetParent(transform);
            StartCoroutine(RespawnFish(hookedFish, 5f));
            hookedFish = null;
        }
    }

    private IEnumerator RespawnFish(FishAI fish, float delay)
    {
        yield return new WaitForSeconds(delay);
        fish.gameObject.SetActive(true);
        fish.transform.position = fish.StartingPosition;
        fish.Reinitialize();
    }

    private void FixedUpdate()
    {
        foreach (var fish in fishes)
        {
            // Check if hook is nearby
            Collider2D nearbyHook = Physics2D.OverlapCircle(fish.transform.position, hookCheckRadius, hookLayer);
            bool isHookNearby = nearbyHook != null;

            // Disable fish if hook isn't close or fish is hooked
            if (!isHookNearby)
            {
                DisableFish(fish);
            }
            else if (!fish.IsHooked)
            {
                EnableFish(fish);
            }
        }
    }

    private void DisableFish(FishAI fish)
    {
        if (fish.gameObject.activeSelf)
        {
            fish.gameObject.SetActive(false);
        }
    }

    private void EnableFish(FishAI fish)
    {
        if (!fish.gameObject.activeSelf)
        {
            fish.gameObject.SetActive(true);
        }
    }
}
