using System;
using System.Collections.Generic;
using _02_Scripts.Event;
using UnityEngine;
using UnityEngine.UI;

namespace WarriorQuest.UI
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private int maxHpCount = 5;
        [SerializeField] private Sprite heartFull;
        [SerializeField] private Sprite heartHalf;
        [SerializeField] private Sprite heartEmpty;
        [SerializeField] private HealthEventSO healthEventSO;
        
        public List<Image> hpImages = new List<Image>();

        private void OnEnable()
        {
            healthEventSO.Subscribe(SetHpHeart);
        }
        private void OnDisEnable()
        {
            healthEventSO.UnSubscribe(SetHpHeart);
        }
        
        
        public void SetHpHeart(float curHp, float maxHp)
        {
            //각 하트당 HP 계산
            float healthPerHeart = maxHp / maxHpCount;
            
            //현재 HP를 하트 개수로 변환(75 / 20 = 3.75)
            float totalHearts = curHp / healthPerHeart;
            
            //Full Heart 계산 (소수점 버림 처리)
            int fullHearts = Mathf.FloorToInt(totalHearts);
            
            //나머지를 하트의 절반 채울 것인지 여부
            float remainer = totalHearts - fullHearts;
            
            //하트 업데이트
            for (int i = 0; i < maxHpCount; i++)
            {
                if (fullHearts > i)
                {
                    hpImages[i].sprite = heartFull;
                }
                else if (i == fullHearts && remainer > 0)
                {
                    hpImages[i].sprite = remainer > 0.5f ? heartFull : heartHalf;
                }
                else hpImages[i].sprite = heartEmpty;
            }
        }
    }
}

