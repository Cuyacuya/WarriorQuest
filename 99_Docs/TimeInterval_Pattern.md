# Unity 시간 간격 계산 패턴

## 개요

Unity에서 특정 로직을 매 프레임이 아닌 **일정 간격으로 실행**해야 할 때 사용하는 패턴들을 정리한 문서입니다.

---

## 사용 목적

| 목적 | 설명 |
|------|------|
| 성능 최적화 | `Physics.OverlapCircle` 같은 무거운 연산을 매 프레임 실행하면 부담 |
| 게임 밸런스 | 공격 쿨다운, 스킬 재사용 대기시간 구현 |
| 자연스러운 동작 | 적이 일정 간격으로 플레이어를 "인식"하는 느낌 |

---

## 방법 1: Time.time 비교 방식

### 코드

```csharp
[SerializeField] private float detectInterval = 0.3f;
private float lastDetectTime = 0f;

public bool PlayerDetectable()
{
    if (Time.time >= lastDetectTime + detectInterval)
    {
        lastDetectTime = Time.time;
        return true;
    }
    return false;
}
```

### 동작 원리

```
Time.time (게임 경과 시간)
    │
    0.0초 ──► PlayerDetectable() → true  (lastDetectTime = 0.0)
    0.1초 ──► PlayerDetectable() → false (0.1 < 0.0 + 0.3)
    0.2초 ──► PlayerDetectable() → false (0.2 < 0.0 + 0.3)
    0.3초 ──► PlayerDetectable() → true  (lastDetectTime = 0.3)
    0.4초 ──► PlayerDetectable() → false
    ...
```

### 특징

| 항목 | 내용 |
|------|------|
| 장점 | 코드가 간단함, Update 외부에서도 호출 가능 |
| 단점 | Time.time 값이 계속 증가하여 장시간 실행 시 정밀도 문제 발생 가능 |

---

## 방법 2: 카운트다운 방식 (권장)

### 코드

```csharp
[SerializeField] private float detectInterval = 0.3f;
private float detectTimer = 0f;

public bool PlayerDetectable()
{
    detectTimer -= Time.deltaTime;

    if (detectTimer <= 0f)
    {
        detectTimer = detectInterval;  // interval 값으로 리셋
        return true;
    }
    return false;
}
```

### 동작 원리

```
detectTimer 값 변화 (interval = 0.3초)
    │
    0.3 → 0.2 → 0.1 → 0.0 → [리셋] → 0.3 → 0.2 → ...
                       │
                    return true
```

### 특징

| 항목 | 내용 |
|------|------|
| 장점 | 값이 항상 0 ~ interval 범위 유지, 정밀도 문제 없음 |
| 단점 | Update() 내에서 호출해야 함 (Time.deltaTime 필요) |

---

## 방법 3: 코루틴 방식

### 코드

```csharp
private void Start()
{
    StartCoroutine(DetectRoutine());
}

IEnumerator DetectRoutine()
{
    while (true)
    {
        // 감지 로직 실행
        DetectPlayer();
        yield return new WaitForSeconds(0.3f);
    }
}

private void DetectPlayer()
{
    // 실제 감지 로직
}
```

### 특징

| 항목 | 내용 |
|------|------|
| 장점 | Update와 분리되어 가독성 좋음, 복잡한 타이밍 처리 가능 |
| 단점 | 코루틴 관리 필요 (시작/중단), 상태 추적 어려움 |

---

## 방법 4: InvokeRepeating 방식

### 코드

```csharp
private void Start()
{
    InvokeRepeating("DetectPlayer", 0f, 0.3f);
}

private void DetectPlayer()
{
    // 감지 로직
}
```

### 특징

| 항목 | 내용 |
|------|------|
| 장점 | 가장 간단한 코드 |
| 단점 | 문자열로 메서드 호출 (타입 안전성 낮음), 유연성 부족 |

---

## 방법 비교 요약

| 방법 | 코드 복잡도 | 정밀도 안전 | 유연성 | Update 필요 |
|------|------------|------------|--------|------------|
| Time.time 비교 | 낮음 | X | 높음 | X |
| 카운트다운 | 낮음 | O | 높음 | O |
| 코루틴 | 중간 | O | 중간 | X |
| InvokeRepeating | 매우 낮음 | O | 낮음 | X |

---

## Q&A

### Q: Time.time 방식은 장시간 실행 시 문제가 되지 않나요?

**A: 네, 문제가 될 수 있습니다.**

`Time.time`은 **float** 타입이고, float는 약 **7자리 유효숫자**만 정확하게 표현합니다.

#### 정밀도 저하 시점

| 게임 실행 시간 | Time.time 값 | 정밀도 |
|---------------|-------------|--------|
| 1분 | 60 | 0.0000001초 |
| 1시간 | 3,600 | 0.000001초 |
| 10시간 | 36,000 | 0.00001초 |
| 100시간 (약 4일) | 360,000 | 0.0001초 |
| 1000시간 (약 41일) | 3,600,000 | 0.001초 |

**41일 이상** 연속 실행하면 밀리초 단위의 정밀도가 떨어지기 시작합니다.

#### 발생 가능한 문제

```csharp
// 100시간 후
Time.time = 360000.0f
lastDetectTime = 360000.0f
detectInterval = 0.3f

// 기대값: 360000.0 + 0.3 = 360000.3
// 실제값: float 정밀도 문제로 360000.25 같은 값이 될 수 있음
```

### Q: 그러면 어떤 방식을 써야 하나요?

**A: 카운트다운 방식을 권장합니다.**

```csharp
[SerializeField] private float detectInterval = 0.3f;
private float detectTimer = 0f;

public bool PlayerDetectable()
{
    detectTimer -= Time.deltaTime;

    if (detectTimer <= 0f)
    {
        detectTimer = detectInterval;  // 0.3으로 리셋
        return true;
    }
    return false;
}
```

#### 이유

1. **값 범위 고정**: 항상 0 ~ interval 사이에서만 변화
2. **정밀도 안정**: 작은 숫자만 다루므로 float 정밀도 문제 없음
3. **일시정지 대응**: `Time.deltaTime`이 자동으로 일시정지 처리

---

## 권장 사항

1. **일반적인 경우**: 카운트다운 방식 사용
2. **Update 외부에서 호출 필요**: 코루틴 방식 사용
3. **단순 반복 작업**: InvokeRepeating 사용
4. **Time.time 비교**: 짧은 플레이 세션 게임에서만 사용

---

## 참고

- Unity 공식 문서에서도 장시간 실행 시 `Time.time`의 정밀도 문제를 언급하고 있습니다.
- 모바일 게임, MMO, 서버처럼 장시간 실행되는 환경에서는 특히 주의가 필요합니다.
