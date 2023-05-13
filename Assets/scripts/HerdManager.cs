using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdManager : MonoBehaviour
{
    
    public List<FishAI> allFish = new();
    [SerializeField]
    private float respawnDelay = 5f;


    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            FishAI fish = transform.GetChild(i).GetComponent<FishAI>();
            allFish.Add(fish);
            fish.FishHooked += OnFishHooked;

        }
    }


    private void FixedUpdate()
    {
        allFish.ForEach((fish) => fish.CheckForHook());
    }

    private void OnFishHooked(FishAI fish)
    {
        fish.gameObject.SetActive(false);
        fish.transform.SetParent(transform);
        StartCoroutine(RespawnFish(fish));
    }

    private IEnumerator RespawnFish(FishAI fish)
    {

        yield return new WaitForSeconds(respawnDelay);
        fish.gameObject.SetActive(true);
        fish.transform.position = fish.startingPosition;
        fish.Reinitialize();
    }
}
