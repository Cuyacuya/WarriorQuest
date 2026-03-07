using System;
using UnityEngine;

namespace _02_Scripts.Event
{
    [CreateAssetMenu(fileName = "HealthEventSO", menuName = "Warrior/HealthEventSO", order = 0)]
    public class HealthEventSO : ScriptableObject
    {
        public event Action<float, float> listeners;
        
        public void Subscribe(Action<float, float> listener) { listeners += listener; }
        public void UnSubscribe(Action<float, float> listener) { listeners -= listener; }

        public void Raise(float curHp, float maxHp)
        {
            listeners?.Invoke(curHp, maxHp);
        }
    }
}