using UnityEngine;

namespace WarriorQuest.Character.Player
{
    public class Warrior : Player
    {
        [Header("���� ���� ����")]
        [SerializeField] private WarriorSO warriorSO;

        #region ����Ƽ �����ֱ�
        protected override void Awake()
        {
            maxHp = warriorSO.maxHp;
            moveSpeed = warriorSO.moveSpeed;
            attackDamage = warriorSO.attackDamage;
            attackCooldown = warriorSO.attackCooldown;

            Debug.Log($"���� Ŭ������ �����Ǿ����ϴ�. ���� : {warriorSO.defense}");
            base.Awake();
        }
        #endregion
        protected override void Attack()
        {
            Debug.Log("����!!!");

        }

        //�ִϸ��̼��� �̺�Ʈ���� ȣ���� �޼���
        public void OnAttackAnimationEvent()
        {
            //���� ���� ó�� ����
            Debug.Log("���� �ִϸ��̼� �̺�Ʈ �߻� - ���� ���� ó��");
        }

        public override void TakeDamage(float damage)
        {
            //���� ����
            float actualDamage = Mathf.Max(damage- warriorSO.defense, 5f);

            base.TakeDamage(actualDamage);

            Debug.Log($"Warrior�� {actualDamage}�� �޾ҽ��ϴ�. ���� ü�� : {curHp}/{maxHp}");
        }
    }

}
