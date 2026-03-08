using System.Collections.Generic;
using UnityEngine;

namespace WarriorQuest.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private List<Slot> slots;
        private void OnEnable()
        {
            Slot.OnSlotSelected += SlotSelected;
        }
        private void OnDisable()
        {
            Slot.OnSlotSelected -= SlotSelected;
        }

        #region 슬롯 처리 메서드
        
        private void SlotSelected(Slot slot)
        {
            ClearAllSelectedSlots();
            
            slot.isSelected = true;
            slot.selectedMarkImage.enabled = true;

            Debug.Log($"선택된 슬롯 : {slot.name}");
        }

        private void ClearAllSelectedSlots()
        {
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    slot.isSelected = false;
                    slot.selectedMarkImage.enabled = false;
                }
            }
        }
        
        #endregion
    }
}
