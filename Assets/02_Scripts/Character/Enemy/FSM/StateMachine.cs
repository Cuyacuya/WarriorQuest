using System;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class StateMachine
    {
        private Enemy enemy;

        //생성자
        public StateMachine(Enemy enemy)
        {
            this.enemy = enemy;
        }

        //현재 상태를 저장하는 변수
        public IState curState;

        //상태 전환 메서드
        public void ChangeState(IState newState)
        {
            curState?.OnExit(enemy);
            curState = newState;
            curState.OnEnter(enemy);
        }

        //상태 업데이트 메서드
        public void Update()
        {
            curState?.OnUpdate(enemy);
        }
    }
}
