using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer backgroundImage;
    [SerializeField] private Sprite[] images;
    private int currentImageIndex = 1;

    private float changeDelay = 1f;
    private float changeDelayCounter = 0f;

    private void Start()
    {
        HookController.hookInWater.AddListener(OnHookInWater);
        backgroundImage.transform.SetParent(targetCamera.transform);
        backgroundImage.transform.localPosition = new Vector3(0, 0, targetCamera.nearClipPlane + 0.01f);
        ScaleSpriteToScreen();
    }

    private void ChangeImage(int index)
    {
        currentImageIndex = index;
        backgroundImage.sprite = images[index];
        ScaleSpriteToScreen();
    }

    private void ScaleSpriteToScreen()
    {
        if (backgroundImage.sprite == null)
        {
            return;
        }

        float worldScreenHeight = targetCamera.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector2 spriteSize = backgroundImage.sprite.bounds.size;
        Vector2 scale = new Vector2(worldScreenWidth / spriteSize.x, worldScreenHeight / spriteSize.y);

        backgroundImage.transform.localScale = scale;
    }
    private void Update(){
        if (changeDelayCounter < changeDelay)
        {
            changeDelayCounter += Time.deltaTime;
        }
    }

    private void OnHookInWater(bool inWater)
    {
        if (changeDelayCounter < changeDelay)
        {
            return;
        }

        if (inWater)
        {
            // Change image to 0 after 1 second
            Invoke(nameof(ChangeImageToZero), 1);
        }
        else if (!inWater)
        {
            ChangeImage(1);
        }
        changeDelayCounter = 0f;
    }

    private void ChangeImageToZero()
    {
        ChangeImage(0);
    }


}
