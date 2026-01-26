using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public interface IState
    {
        //상태 진입 시 호출될 함수
        void OnEnter(Enemy enemy);
        //상태 유지 시 호출될 함수
        void OnUpdate(Enemy enemy);
        //상태 탈출 시 호출될 함수
        void OnExit(Enemy enemy);
    }
}
