using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class IdleState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            Debug.Log("IdleState 진입");
        }

        public void OnUpdate(Enemy enemy)
        {
            Debug.Log("IdleState 갱신");
        }

        public void OnExit(Enemy enemy)
        {
            Debug.Log("IdleState 종료");
        }
    }
}