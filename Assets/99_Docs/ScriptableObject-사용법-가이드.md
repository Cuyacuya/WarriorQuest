# ScriptableObject 사용법 가이드

## ScriptableObject란?

Unity에서 제공하는 데이터 컨테이너 클래스입니다. MonoBehaviour와 달리 씬에 부착하지 않고 **프로젝트 에셋**으로 존재합니다.

### 주요 특징
- 메모리 효율적 (데이터 공유)
- 씬 독립적
- 에디터에서 쉽게 수정 가능
- 프리팹 간 데이터 공유

---

## 1. 데이터 컨테이너 (Data Container)

가장 기본적인 사용법입니다. 캐릭터 스탯, 아이템 정보 등을 저장합니다.

### 예제: 캐릭터 스탯

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "WarriorQuest/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("기본 스탯")]
    public float maxHp = 100f;
    public float moveSpeed = 5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    [Header("추가 스탯")]
    public float defense = 0f;
    public float criticalChance = 0.1f;
}
```

### 사용법

```csharp
public class Player : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;

    private float curHp;

    void Awake()
    {
        curHp = stats.maxHp;
    }
}
```

### 장점
- **Warrior**, **Mage**, **Archer** 등 각 클래스별 스탯을 별도 에셋으로 관리
- 밸런스 조정 시 코드 수정 없이 에셋만 수정
- 여러 인스턴스가 동일한 데이터 공유 (메모리 절약)

---

## 2. 아이템/인벤토리 시스템

```csharp
[CreateAssetMenu(fileName = "Item", menuName = "WarriorQuest/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public ItemType itemType;
    public int maxStackSize = 99;

    [Header("장비 스탯 (장비 아이템일 경우)")]
    public float attackBonus;
    public float defenseBonus;
    public float hpBonus;
}

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Accessory,
    Material
}
```

### 아이템 데이터베이스

```csharp
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "WarriorQuest/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    public ItemData GetItemById(string id)
    {
        return allItems.Find(item => item.name == id);
    }
}
```

---

## 3. 이벤트 시스템 (Event Architecture)

ScriptableObject를 이벤트 채널로 사용하여 **컴포넌트 간 결합도를 낮출 수 있습니다**.

### GameEvent 기본 구조

```csharp
[CreateAssetMenu(fileName = "GameEvent", menuName = "WarriorQuest/Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> listeners = new List<GameEventListener>();

    public void Raise()
    {
        // 역순으로 호출 (리스너가 자신을 제거할 경우 대비)
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        listeners.Remove(listener);
    }
}
```

### GameEventListener 컴포넌트

```csharp
public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UnityEvent response;

    void OnEnable() => gameEvent.RegisterListener(this);
    void OnDisable() => gameEvent.UnregisterListener(this);

    public void OnEventRaised() => response.Invoke();
}
```

### 사용 예시

```
[Player Death Event] (ScriptableObject)
        │
        ├── UI Manager: 게임오버 화면 표시
        ├── Audio Manager: 사망 효과음 재생
        └── Enemy AI: 승리 애니메이션 실행
```

### 값을 전달하는 이벤트

```csharp
[CreateAssetMenu(fileName = "IntEvent", menuName = "WarriorQuest/Events/Int Event")]
public class IntEvent : ScriptableObject
{
    private List<System.Action<int>> listeners = new List<System.Action<int>>();

    public void Raise(int value)
    {
        foreach (var listener in listeners)
        {
            listener.Invoke(value);
        }
    }

    public void Subscribe(System.Action<int> action) => listeners.Add(action);
    public void Unsubscribe(System.Action<int> action) => listeners.Remove(action);
}
```

---

## 4. 변수 시스템 (Variable System)

전역 변수를 ScriptableObject로 관리하여 싱글톤 패턴을 대체합니다.

### FloatVariable

```csharp
[CreateAssetMenu(fileName = "FloatVariable", menuName = "WarriorQuest/Variables/Float")]
public class FloatVariable : ScriptableObject
{
    public float initialValue;

    [System.NonSerialized]
    public float runtimeValue;

    void OnEnable()
    {
        runtimeValue = initialValue;
    }
}
```

### 사용 예시: HP 바 연동

```csharp
// Player.cs
public class Player : MonoBehaviour
{
    [SerializeField] private FloatVariable currentHp;
    [SerializeField] private FloatVariable maxHp;

    public void TakeDamage(float damage)
    {
        currentHp.runtimeValue -= damage;
    }
}

// HealthBarUI.cs - Player 참조 없이 HP 표시 가능
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private FloatVariable currentHp;
    [SerializeField] private FloatVariable maxHp;
    [SerializeField] private Image fillImage;

    void Update()
    {
        fillImage.fillAmount = currentHp.runtimeValue / maxHp.runtimeValue;
    }
}
```

---

## 5. 런타임 세트 (Runtime Sets)

게임 내 특정 타입의 오브젝트들을 추적합니다.

```csharp
[CreateAssetMenu(fileName = "RuntimeSet", menuName = "WarriorQuest/Runtime Set")]
public class RuntimeSet<T> : ScriptableObject
{
    public List<T> items = new List<T>();

    public void Add(T item)
    {
        if (!items.Contains(item))
            items.Add(item);
    }

    public void Remove(T item)
    {
        if (items.Contains(item))
            items.Remove(item);
    }
}
```

### 예시: 적 추적

```csharp
[CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "WarriorQuest/Enemy Runtime Set")]
public class EnemyRuntimeSet : RuntimeSet<Enemy> { }

// Enemy.cs
public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyRuntimeSet runtimeSet;

    void OnEnable() => runtimeSet.Add(this);
    void OnDisable() => runtimeSet.Remove(this);
}

// EnemyCounter.cs - 적 수 표시
public class EnemyCounter : MonoBehaviour
{
    [SerializeField] private EnemyRuntimeSet enemies;
    [SerializeField] private Text countText;

    void Update()
    {
        countText.text = $"남은 적: {enemies.items.Count}";
    }
}
```

---

## 6. 전략 패턴 (Strategy Pattern)

AI 행동, 스킬 효과 등을 ScriptableObject로 분리합니다.

### 스킬 시스템

```csharp
public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public float cooldown;
    public float manaCost;

    public abstract void Execute(Character caster, Character target);
}

[CreateAssetMenu(fileName = "FireballSkill", menuName = "WarriorQuest/Skills/Fireball")]
public class FireballSkill : SkillBase
{
    public float damage = 50f;
    public GameObject fireballPrefab;

    public override void Execute(Character caster, Character target)
    {
        var fireball = Instantiate(fireballPrefab, caster.transform.position, Quaternion.identity);
        fireball.GetComponent<Projectile>().Initialize(target, damage);
    }
}

[CreateAssetMenu(fileName = "HealSkill", menuName = "WarriorQuest/Skills/Heal")]
public class HealSkill : SkillBase
{
    public float healAmount = 30f;

    public override void Execute(Character caster, Character target)
    {
        target.Heal(healAmount);
    }
}
```

### AI 행동 패턴

```csharp
public abstract class AIBehavior : ScriptableObject
{
    public abstract void Execute(Enemy enemy);
}

[CreateAssetMenu(fileName = "PatrolBehavior", menuName = "WarriorQuest/AI/Patrol")]
public class PatrolBehavior : AIBehavior
{
    public float patrolSpeed = 2f;
    public float waitTime = 2f;

    public override void Execute(Enemy enemy)
    {
        // 순찰 로직
    }
}

[CreateAssetMenu(fileName = "ChaseBehavior", menuName = "WarriorQuest/AI/Chase")]
public class ChaseBehavior : AIBehavior
{
    public float chaseSpeed = 5f;
    public float attackRange = 1.5f;

    public override void Execute(Enemy enemy)
    {
        // 추적 로직
    }
}
```

---

## 7. 오디오 시스템

```csharp
[CreateAssetMenu(fileName = "AudioCue", menuName = "WarriorQuest/Audio/Audio Cue")]
public class AudioCue : ScriptableObject
{
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.8f, 1.2f)]
    public float pitchVariation = 1f;

    public AudioClip GetRandomClip()
    {
        if (clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public void Play(AudioSource source)
    {
        var clip = GetRandomClip();
        if (clip == null) return;

        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(2f - pitchVariation, pitchVariation);
        source.Play();
    }
}
```

---

## 8. 대화 시스템

```csharp
[CreateAssetMenu(fileName = "Dialogue", menuName = "WarriorQuest/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    public Sprite portrait;

    [TextArea(3, 5)]
    public string[] dialogueLines;

    public DialogueData nextDialogue;  // 연결된 다음 대화
}
```

---

## 9. 게임 설정 (Settings)

```csharp
[CreateAssetMenu(fileName = "GameSettings", menuName = "WarriorQuest/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("난이도")]
    public float enemyDamageMultiplier = 1f;
    public float playerDamageMultiplier = 1f;

    [Header("게임플레이")]
    public float respawnTime = 3f;
    public int maxLives = 3;

    [Header("오디오")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
}
```

---

## 10. WarriorQuest 프로젝트에 적용 예시

현재 `Player`와 `Warrior` 구조를 ScriptableObject로 개선하는 방법:

### Before (현재)

```csharp
// Warrior.cs
protected override void Awake()
{
    maxHp = 150f;        // 하드코딩
    moveSpeed = 4f;
    attackDamage = 25f;
    attackCooldown = 0.7f;
    base.Awake();
}
```

### After (ScriptableObject 사용)

```csharp
// CharacterStats.cs (ScriptableObject)
[CreateAssetMenu(fileName = "CharacterStats", menuName = "WarriorQuest/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float maxHp;
    public float moveSpeed;
    public float attackDamage;
    public float attackCooldown;
    public float defense;
}

// Warrior.cs
public class Warrior : Player
{
    [SerializeField] private CharacterStats stats;  // Inspector에서 할당

    protected override void Awake()
    {
        maxHp = stats.maxHp;
        moveSpeed = stats.moveSpeed;
        attackDamage = stats.attackDamage;
        attackCooldown = stats.attackCooldown;
        defense = stats.defense;
        base.Awake();
    }
}
```

### 폴더 구조 예시

```
Assets/
├── 06_ScriptableObjects/
│   ├── Characters/
│   │   ├── WarriorStats.asset
│   │   ├── MageStats.asset
│   │   └── ArcherStats.asset
│   ├── Items/
│   │   ├── Sword_Basic.asset
│   │   └── Potion_Health.asset
│   └── Events/
│       ├── OnPlayerDeath.asset
│       └── OnEnemyKilled.asset
```

---

## 주의사항

1. **빌드 시 값 초기화**: 에디터에서 수정한 runtimeValue는 빌드에서 초기화됨
2. **참조 관리**: 삭제된 ScriptableObject를 참조하면 Missing Reference 발생
3. **메모리**: 대용량 데이터는 Resources.Load 또는 Addressables 사용 권장

---

## 참고 자료

- [Unity Official - ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [Ryan Hipple - Game Architecture with ScriptableObjects (Unite 2017)](https://www.youtube.com/watch?v=raQ3iHhE_Kk)

---

*작성일: 2026-01-25*
