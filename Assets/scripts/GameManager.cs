using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private GameObject Menu;
    [SerializeField]
    private PlayerController playerController;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Menu.activeSelf)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
        
    }

    public void HideMenu()
    {
        Menu.SetActive(false);
        playerController.enabled = true;
    }

    public void ShowMenu()
    {
        Menu.SetActive(true);
        playerController.enabled = false;
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
