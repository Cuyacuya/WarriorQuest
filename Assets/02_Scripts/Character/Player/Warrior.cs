using UnityEngine;

namespace WarriorQuest.Character.Player
{
    public class Warrior : Player
    {
        [Header("전사 고유 스탯")]
        [SerializeField] private WarriorSO warriorSo;

        #region 유니티 라이프사이클
        protected override void Awake()
        {
            maxHp = warriorSo.maxHp;
            moveSpeed = warriorSo.moveSpeed;
            attackDamage = warriorSo.attackDamage;
            attackCooldown = warriorSo.attackCooldown;

            Debug.Log($"전사 클래스가 생성되었습니다. 방어력 : {warriorSo.defense}");
            base.Awake();
        }
        #endregion
        protected override void Attack()
        {
            Debug.Log("공격!!!");

        }

        //애니메이션의 이벤트에서 호출될 메서드
        public void OnAttackAnimationEvent()
        {
            //공격 판정 처리 예정
            Debug.Log("공격 애니메이션 이벤트 발생 - 공격 판정 처리");
        }

        public override void TakeDamage(float damage)
        {
            //방어력 적용
            float actualDamage = Mathf.Max(damage- warriorSo.defense, 5f);

            base.TakeDamage(actualDamage);

            Debug.Log($"Warrior가 {actualDamage}를 받았습니다. 현재 체력 : {curHp}/{maxHp}");
        }
    }

}
