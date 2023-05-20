using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI starAmountText;

    public void Start()
    {
        GameManager.starAmountChanged.AddListener(RefreshOverlay);
    }

    public void RefreshOverlay()
    {
        starAmountText.text = PlayerController.instance.stars.ToString();
    }
   
}
