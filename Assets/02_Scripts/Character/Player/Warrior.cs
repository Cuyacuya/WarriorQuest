using UnityEngine;
using WarriorQuest.Characte.Player;

namespace WarriorQuest.Characte.Player
{
    public class Warrior : Player
    {
        [Header("전사 전용 스텟")] [SerializeField] private float defense = 10f;

        #region 유니티 생명주기
        protected override void Awake()
        {
            maxHp = 150f;
            moveSpeed = 4f;
            attackDamage = 25f;
            attackCooldown = 0.7f;

            Debug.Log($"전사 클래스가 생성되었습니다. 방어력 : {defense}");
            base.Awake();
        }
        #endregion
        protected override void Attack()
        {
            Debug.Log("공격!!!");
        }

        public override void TakeDamage(float damage)
        {
            //방어력 적용
            float actualDamage = Mathf.Max(damage-defense, 5f);

            base.TakeDamage(damage);

            Debug.Log($"Warrior가 {actualDamage}를 받았습니다. 현재 체력 : {curHp}/{maxHp}");
        }
    }

}
