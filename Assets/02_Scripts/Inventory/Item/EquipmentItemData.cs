using System.Text;
using UnityEngine;

public enum EquipType
{
    Weapon,
    Armor,
    Accessory,
}

namespace WarriorQuest.InventorySystem.Item
{
    [CreateAssetMenu(fileName = "EquipmentItemDataSO", menuName = "Warrior/Item/EquipmentItem", order = 0)]
    public class EquipmentItemData : ItemData
    {
        //장비 타입
        public EquipType equipType;
        //공격력
        public float attackDamage;
        //방어력
        public float defence;
        //공격 속도
        public float attackCoolDown;
        
        public override string GetInfo()
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append($"ATK : {attackDamage}\n");
            sb.Append($"DEF : {defence}\n");
            sb.Append($"SPD : {attackCoolDown}\n");
            return sb.ToString();
        }
    }
}