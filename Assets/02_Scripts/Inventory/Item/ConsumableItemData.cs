using System.Text;
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

        //아이템 정보 문자열 반환
        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"HP : {hpRecovery}\n");
            sb.Append($"MP : {mpRecovery}\n");
            
            return sb.ToString();
        }
    }
}