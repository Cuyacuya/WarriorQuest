using UnityEngine;

namespace WarriorQuest.InventorySystem.Item
{
    [CreateAssetMenu(fileName = "ConsumableItemDataSO", menuName = "Warrior/Item/ConsumableItemData", order = 0)]
    public class ConsumableItemData : ItemData
    {
        //HP 회복량
        public float hpRecovery;
        //MP 회복량
        public float mpRecovery;
    }
}