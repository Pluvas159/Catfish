using UnityEngine;

[CreateAssetMenu(fileName = "New Hook type", menuName = "Hook")]
public class HookType : ScriptableObject
{
    public InventoryItem inventoryItem;
    public float hookEffectivity;
}
