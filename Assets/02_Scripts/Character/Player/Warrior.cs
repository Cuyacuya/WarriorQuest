using System;
using UnityEngine;
using WarriorQuest.Characte.Player;

namespace WarriorQuest.Characte.Player
{
    public class Warrior : Player
    {
        [Header("전사 전용 스텟")]
        [SerializeField] private WarriorSO warriorSO;

        #region 유니티 생명주기
        protected override void Awake()
        {
            maxHp = warriorSO.maxHp;
            moveSpeed = warriorSO.moveSpeed;
            attackDamage = warriorSO.attackDamage;
            attackCooldown = warriorSO.attackCooldown;

            Debug.Log($"전사 클래스가 생성되었습니다. 방어력 : {warriorSO.defense}");
            base.Awake();
        }
        #endregion
        protected override void Attack()
        {
            Debug.Log("공격!!!");

        }

        //애니메이션의 이벤트에서 호출할 메서드
        public void OnAttackAnimationEvent()
        {
            //실제 공격 처리 로직
            Debug.Log("공격 애니메이션 이벤트 발생 - 실제 공격 처리");
        }

        public override void TakeDamage(float damage)
        {
            //방어력 적용
            float actualDamage = Mathf.Max(damage- warriorSO.defense, 5f);

            base.TakeDamage(damage);

            Debug.Log($"Warrior가 {actualDamage}를 받았습니다. 현재 체력 : {curHp}/{maxHp}");
        }
    }

}
