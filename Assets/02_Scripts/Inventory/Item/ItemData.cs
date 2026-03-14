using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
}

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Warrior/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;
    public bool isEquip;
    
    [TextArea]
    public string description;

    public virtual string GetInfo()
    {
        return "";
    }
}
