# Player-Warrior 상속 관계 검토

## 검토 대상 파일
- `Assets/02_Scripts/Character/Player/Player.cs`
- `Assets/02_Scripts/Character/Player/Warrior.cs`
- `Assets/02_Scripts/Character/Interface/IDamageable.cs`

## 검토 결과 요약

전체적인 상속 구조는 적절하게 설계되어 있습니다. 추상 클래스와 인터페이스를 활용한 기본적인 OOP 패턴을 잘 따르고 있습니다. 다만, 몇 가지 개선이 필요한 부분이 있습니다.

---

## 1. 심각한 버그 (Critical)

### 1.1 Warrior.TakeDamage() 방어력 미적용 버그

**위치**: `Warrior.cs` 27-35라인

```csharp
public override void TakeDamage(float damage)
{
    // 방어력 계산은 하지만...
    float actualDamage = Mathf.Max(damage - defense, 5f);

    // 실제로는 원본 damage를 전달함 (버그!)
    base.TakeDamage(damage);  // ← actualDamage가 아닌 damage 전달

    Debug.Log($"Warrior가 {actualDamage}를 받았습니다...");
}
```

**문제점**: `actualDamage`를 계산하지만 `base.TakeDamage()`에 원본 `damage`를 전달하고 있어 방어력이 실제로 적용되지 않습니다.

**수정 방안**:
```csharp
base.TakeDamage(actualDamage);  // damage → actualDamage
```

---

## 2. 네이밍 오류 (Naming Issues)

### 2.1 네임스페이스 오타

**위치**: `Player.cs` 8라인, `Warrior.cs` 4라인

```csharp
namespace WarriorQuest.Characte.Player  // "Characte" ← 오타
```

**문제점**: `Character`가 `Characte`로 잘못 작성되어 있습니다. `IDamageable`은 `WarriorQuest.Character.Interface`로 올바르게 되어 있어 네임스페이스 간 일관성이 없습니다.

**수정 방안**:
```csharp
namespace WarriorQuest.Character.Player
```

### 2.2 프로퍼티 오타

**위치**: `Player.cs` 31라인

```csharp
public float AttackDamge => attackDamage;  // "Damge" ← 오타
```

**수정 방안**:
```csharp
public float AttackDamage => attackDamage;
```

---

## 3. 설계 개선 권장 사항 (Design Improvements)

### 3.1 OnEnable/OnDisable의 virtual 키워드 누락

**위치**: `Player.cs` 65-76라인

```csharp
protected void OnEnable()   // virtual 키워드 없음
protected void OnDisable()  // virtual 키워드 없음
```

**문제점**: 서브클래스(Warrior)에서 추가적인 이벤트 구독이 필요할 경우 올바르게 override할 수 없습니다.

**수정 방안**:
```csharp
protected virtual void OnEnable()
{
    inputHandler.OnMoveAction += OnMove;
    inputHandler.OnAttackAction += OnAttack;
    inputHandler.OnInteractAction += OnInteraction;
}

protected virtual void OnDisable()
{
    inputHandler.OnMoveAction -= OnMove;
    inputHandler.OnAttackAction -= OnAttack;
    inputHandler.OnInteractAction -= OnInteraction;
}
```

### 3.2 Awake()에서 스탯 초기화 방식

**위치**: `Warrior.cs` 11-20라인

```csharp
protected override void Awake()
{
    // 부모 필드를 직접 수정
    maxHp = 150f;
    moveSpeed = 4f;
    attackDamage = 25f;
    attackCooldown = 0.7f;

    base.Awake();  // 이후에 부모 Awake 호출
}
```

**현재 방식의 장점**:
- `base.Awake()`에서 `curHp = maxHp`가 호출되므로 순서상 문제없음

**잠재적 문제점**:
- 부모의 `protected` 필드를 직접 수정하는 것은 캡슐화 원칙에 어긋남
- 부모 클래스 구현이 변경되면 깨질 수 있음

**대안 (선택적)**:
```csharp
// 방법 1: 생성자 또는 초기화 메서드 패턴
protected virtual void InitializeStats() { }

// 방법 2: ScriptableObject로 스탯 데이터 분리
[SerializeField] private CharacterStats stats;
```

---

## 4. 잘 설계된 부분 (Good Practices)

### 4.1 추상 클래스 패턴
- `Player`를 추상 클래스로 만들어 직접 인스턴스화 방지
- `Attack()` 메서드를 추상으로 선언하여 서브클래스 구현 강제

### 4.2 인터페이스 활용
- `IDamageable` 인터페이스로 데미지 시스템 계약 정의
- 적(Enemy)과 플레이어 모두 같은 인터페이스 구현 가능

### 4.3 RequireComponent 사용
- 필수 컴포넌트를 명시적으로 선언하여 런타임 오류 방지

### 4.4 애니메이터 해시 캐싱
- `Animator.StringToHash()`를 `static readonly`로 캐싱하여 성능 최적화

### 4.5 virtual/override 패턴
- `Awake()`, `TakeDamage()`, `Die()` 등을 virtual로 선언하여 확장성 제공

---

## 5. 수정 우선순위

| 우선순위 | 항목 | 심각도 |
|---------|------|--------|
| 1 | Warrior.TakeDamage() 방어력 미적용 버그 | Critical |
| 2 | 네임스페이스 오타 (Characte → Character) | Medium |
| 3 | 프로퍼티 오타 (AttackDamge → AttackDamage) | Low |
| 4 | OnEnable/OnDisable virtual 키워드 추가 | Low |

---

## 6. 결론

상속 구조 자체는 잘 설계되어 있으나, **Warrior.TakeDamage()의 버그**는 게임 밸런스에 직접적인 영향을 주므로 반드시 수정해야 합니다. 나머지는 코드 품질과 유지보수성 개선을 위한 권장 사항입니다.

---

*검토일: 2026-01-25*
