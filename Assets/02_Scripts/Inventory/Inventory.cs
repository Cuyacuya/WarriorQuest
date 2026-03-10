using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.InputSystem;
using WarriorQuest.Character.Player;
using WarriorQuest.InventorySystem.Item;

namespace WarriorQuest.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private List<Slot> slots;
        
        //Player 컴포넌트 참조
        private Player player;
        
        //아이템 저장 배열
        private ItemData[] items;
        //선택 슬롯 인덱스
        private int selectedSlotIndex = -1;

        #region 유니티 생명주기

        private void Awake()
        {
            //인벤토리 초기화
            items = new ItemData[slots.Count];
            
            //슬롯 인덱스 설정
            for (int i = 0; i < items.Length; i++)
            {
                slots[i].slotIndex = i;
            }
            
            //첫 번째 슬롯을 기본 선택
            slots[0].isDefaultSelected = true;
            selectedSlotIndex = 0;
        }

        private IEnumerator Start()
        {
            yield return null;
            var playerObj = GameObject.FindWithTag("Player");
            playerObj.TryGetComponent<Player>(out player);
        }

        private void OnEnable()
        {
            Slot.OnSlotSelected += SlotSelected;
        }
        private void OnDisable()
        {
            Slot.OnSlotSelected -= SlotSelected;
        }

        private void OnGUI() //버튼 만들기 (래거시 GUI)
        {
            if (GUILayout.Button("아이템 초기 지급"))
            {
                ItemData hpPortion = Resources.Load<ItemData>("ItemData/HpPortionLarge");
                AddItem(hpPortion);
                
                //기본 장착 무기
                ItemData sword = Resources.Load<ItemData>("ItemData/RustySword");
                AddItem(sword);
                sword = Resources.Load<ItemData>("ItemData/IronSword");
                AddItem(sword);
                sword = Resources.Load<ItemData>("ItemData/LegendSword");
                AddItem(sword);
            }
        }

        private void Update()
        {
            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                if (selectedSlotIndex != -1 && items[selectedSlotIndex] != null)
                {
                    UseItem(selectedSlotIndex);
                }
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (selectedSlotIndex != -1 && items[selectedSlotIndex] != null)
                {
                    EquipItem((selectedSlotIndex));
                }
            }
        }

        #endregion
        

        #region 슬롯 처리 메서드
        
        private void SlotSelected(Slot slot)
        {
            ClearAllSelectedSlots();
            
            slot.isSelected = true;
            slot.selectedMarkImage.enabled = true;
            selectedSlotIndex = slot.slotIndex;

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

        #region 아이템 처리 메서드

        private bool AddItem(ItemData item)
        {
          //빈 슬롯 찾기
          int emptyIndex = FindEmptySlot();
          if (emptyIndex == -1)
          {
              Debug.Log("인벤토리에 빈 슬롯이 없습니다.");
              return false;
          }
          
          //아이템 추가
          items[emptyIndex] = item;
          slots[emptyIndex].ItemData = item;
          
          Debug.Log("아이템이 인벤토리에 추가되었습니다.");
          return true;
        }
        
        //아이템 제거
        private void RemoveItem(int index)
        {
            if (items[index] != null)
            {
                items[index] = null;
                slots[index].ItemData = null;
            }
        }
    
        //아이템 사용 및 장착
        public void UseItem(int index)
        {
            var item = items[index];
            if (item.itemType != ItemType.Consumable)
            {
                Debug.Log("소모성 아이템이 아닙니다.");
                return;
            }

            UseConsumableItem(item as ConsumableItemData);
        }
        
        //장착 아이템
        public void EquipItem(int index)
        {
            var item = items[index];
            if (item.itemType != ItemType.Equipment)
            {
                Debug.Log("장착할 수 없는 아이템 입니다.");
                return;
            }
            EquipSelectedItem(item as EquipmentItemData);
        }
        
        //빈 슬롯 인덱스 찾기
        private int FindEmptySlot()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) return i;
            }

            return -1;
        }
        #endregion

        #region 아이템 사용 및 장착

        private void UseConsumableItem(ConsumableItemData item)
        {
            //플레이어 힐링
            player.Heal(item.hpRecovery);
            RemoveItem(selectedSlotIndex);
        }

        private void EquipSelectedItem(EquipmentItemData item)
        {
            //기존 장착된 아이템 장착 해제
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] is EquipmentItemData equippedItem && equippedItem.isEquip)
                {
                    //이전 장착 해제
                    equippedItem.isEquip = false;
                    slots[i].ItemData = items[i];
                    break;
                }
            }
            
            //장비 장착
            player.EquipWeapon(item);
            item.isEquip = true;
            slots[selectedSlotIndex].ItemData = item;
        }
        #endregion
    }
}
