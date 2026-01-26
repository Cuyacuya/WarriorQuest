using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Warrior/EnemySO")]
public class EnemySO : ScriptableObject
{
    public float maxHp = 80f;
    public float moveSpeed = 1f;
    public float chaseDistance = 5f;
    public float attackDistance = 2f;
    public float attackCooldown = 1f;
}
