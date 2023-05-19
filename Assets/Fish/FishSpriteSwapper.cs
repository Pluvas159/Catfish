using UnityEngine;

public class FishSpriteSwapper : MonoBehaviour
{
    public FishType fishType;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // This method will be called by the animation events
    public void SwapSprite(int index)
    {
        if (fishType != null && fishType.fishSprites.Length > index)
        {
            spriteRenderer.sprite = fishType.fishSprites[index];
        }
    }
}
