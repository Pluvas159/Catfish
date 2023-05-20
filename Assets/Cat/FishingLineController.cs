using Unity.VisualScripting;
using UnityEngine;

public class FishingLineController : MonoBehaviour
{
    [SerializeField]
    private GameObject lineStart;

    [SerializeField]
    private GameObject hook;
    private HookController hookController;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        hookController = hook.GetComponent<HookController>();
        lineRenderer.SetPosition(0, lineStart.transform.position);
        lineRenderer.SetPosition(1, lineStart.transform.position);
        AnimationController.animationEnd.AddListener(ShowLine);
        HookController.hookRetracted.AddListener(HideLine);
        lineRenderer.enabled = false;
     
    }

    private void HideLine()
    {
        lineRenderer.enabled = false;
    }

    private void ShowLine()
    {
        lineRenderer.enabled = true;
    }

    private void Update()
    {
        UpdateFishingLine();
    }


    void UpdateFishingLine()
    {
        lineRenderer.SetPosition(1, hook.transform.position);
    }

}
