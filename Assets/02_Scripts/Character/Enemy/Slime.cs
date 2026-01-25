using System;
using System.Collections.Generic;
using UnityEngine;
using WarriorQuest.Character.Enemy;
using WarriorQuest.Character.Enemy.FSM;

public class Slime : Enemy
{
    protected override void InisStates()
    {
        //인덱서 방식으로 상태 초기화
        states = new Dictionary<Type, IState>
        {
            [typeof(IdleState)] = new IdleState(),
            [typeof(ChaseState)] = new ChaseState(),
            [typeof(AttackState)] = new AttackState(),
        };

        Debug.Log("Slime 상태 초기화 완료");
    }
}
