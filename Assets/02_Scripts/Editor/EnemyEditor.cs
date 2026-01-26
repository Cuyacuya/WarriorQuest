using UnityEditor;
using UnityEngine;
using WarriorQuest.Character.Enemy;
using WarriorQuest.Character.Enemy.FSM;

[CustomEditor(typeof(Enemy), true)]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Enemy enemy = (Enemy)target;

        //기본 인스펙터 그리기
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        GUI.enabled = Application.isPlaying;

        //현재 상태 표시 레이블
        EditorGUILayout.LabelField("현재 상태", enemy.CurStateName);

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Idle 상태"))
        {
            enemy.ChangeState<IdleState>();
        }
        if(GUILayout.Button("Chase 상태"))
        {
            enemy.ChangeState<ChaseState>();
        }
        if(GUILayout.Button("Attack 상태"))
        {
            enemy.ChangeState<AttackState>();
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
    }
}
