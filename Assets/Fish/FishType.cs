using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish")]
public class FishType : ScriptableObject
{
    public string fishName;
    public Sprite[] fishSprites;
    public InventoryItem inventoryItem;
}
