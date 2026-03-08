using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarriorQuest.InventorySystem
{
    public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ItemData itemData;

        public ItemData ItemData
        {
            get => itemData;
            set => UpdateItem(value);
        }
        
        //슬롯 인덱스
        public int slotIndex;

        //선택 여부
        public bool isSelected;

        //디폴트 슬롯 여부
        public bool isDefaultSelected = false;

        //슬롯 하위에 있는 UI 오브젝트들
        public GameObject item;
        public Image selectedMarkImage;
        public Image itemImage;
        public TextMeshProUGUI equipText;

        public static event Action<Slot> OnSlotSelected;

        private void Start()
        {
            InitSlot();
        }

        #region 마우스 이벤트 처리
        public void OnPointerEnter(PointerEventData eventData)
        {
            //마우스 오버
            selectedMarkImage.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected) selectedMarkImage.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //선택 처리
            isSelected = !isSelected; //토글방식
            selectedMarkImage.enabled = isSelected;

            //선택 이벤트 발생
            OnSlotSelected?.Invoke(this);
        }
        #endregion

        #region 초기화 설정
        
        private void InitSlot()
        {
            //디폴트 선택 여부 확인
            isSelected = isSelected || isDefaultSelected;
            selectedMarkImage.enabled = isSelected;
            
            //ItemData가 없으면 비활성화
            item?.SetActive(itemData != null);
        }

        private void UpdateItem(ItemData value)
        {
            itemData = value;

            if (itemData == null)
            {
                itemImage.sprite = null;
                item?.SetActive(false);
                equipText.text = "";
                
            }
            else
            {
                if (value.itemIcon != null)
                {
                    itemImage.sprite = value.itemIcon;
                    item?.SetActive(true);
                    equipText.text = value.isEquip ? "E" : "";
                }
            }
        }

        #endregion
    }
}