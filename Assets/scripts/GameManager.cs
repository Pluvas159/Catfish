using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private GameObject overlay;

    public static UnityEvent starAmountChanged = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple GameManagers in scene");
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        playerController.enabled = false;
        HideInventory();
        HideOverlay();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.activeSelf)
            {
                HideMenu();
            }
            else
            {
                if (inventory.gameObject.activeSelf) HideInventory();
                ShowMenu();
            }
        }
        
    }

    public void HideMenu()
    {
        menu.SetActive(false);
        ShowOverlay();
        playerController.enabled = true;
    }

    public void ShowMenu()
    {
        if (inventory.gameObject.activeSelf) HideInventory();
        if (overlay.activeSelf) HideOverlay();
        menu.SetActive(true);
        playerController.enabled = false;
    }

    public void QuitGame()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void ShowInventory()
    {
        if (inventory.gameObject.activeSelf) HideInventory();
        else {
            menu.SetActive(false);
            inventory.gameObject.SetActive(true);
            ShowOverlay();
            };

    }

    public void HideInventory()
    {
        inventory.gameObject.SetActive(false);
        menu.SetActive(true);
        HideOverlay();
    }

    public void ShowOverlay()
    {
        overlay.SetActive(true);
    }

    public void HideOverlay()
    {
        overlay.SetActive(false);
    }

    public void AddStars(int amount) {
        PlayerController.instance.stars += amount;
        starAmountChanged.Invoke();
        
    }

}
