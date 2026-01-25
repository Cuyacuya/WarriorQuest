using UnityEngine;

[UnityEngine.CreateAssetMenu(fileName = "WarriorSO", menuName = "Warrior/WarriorSO", order = 0)]

public class WarriorSO : UnityEngine.ScriptableObject
{
    [Header("전사 전용 스텟")]
    public float maxHp = 150f;
    public float moveSpeed = 4f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.7f;
    public float defense = 10f;
}
