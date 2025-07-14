# 🍳 프로젝트: Aero Diner

Unity로 개발한 Top-down 시점의 레스토랑 타이쿤 게임입니다. 플레이어는 주방장이 되어 제한 시간 내에 손님들의 주문을 받고, 레시피에 맞춰 요리를 만들어 판매하며 식당을 성장시켜야 합니다.

(에디터를 실행할 때에는 `StartScene` 부터 시작하시면 됩니다.)

## 🧑🏻‍🍳 플레이 방법

함께 요리사가 되어보아요!
게임시작 후 영업에 사용할 메뉴를 선택합니다. (유니티 에디터에서는 전체 해금 치트를 사용할 수 있습니다)
<img width="1922" height="1080" alt="스크린샷 2025-07-14 170016" src="https://github.com/user-attachments/assets/a7ff62d4-cd6b-409a-a6d8-87725c511ba7" />

<기본 설정 키>
* 이동 : WASD
* 설비 상호작용 : J
* 아이템 : 들기 / 내려놓기 K

기본 제공되는 토마토 파스타 레시피북을 공개합니다! 후반기에 개발될 일상 파트에서는 더 다양한 레시피를 해금할 수 있습니다!

<img width="709" height="654" alt="image" src="https://github.com/user-attachments/assets/99065b8f-d119-4d08-a09a-bf180f4a2c5c" />



## ✨ 주요 기능

### 🧑‍🍳 요리 & 레시피 시스템

* **데이터 기반 레시피**: `ScriptableObject`로 구현된 `FoodData`와 `IngredientData`를 통해 재료, 레시피, 가격 등 모든 음식 정보를 관리하여 확장 및 밸런싱이 용이합니다.

* **조합 시스템**: `RecipeManager`가 플레이어가 가진 재료 조합을 실시간으로 분석하여 만들 수 있는 음식을 판별합니다.

* **다양한 조리 도구**: 재료를 꺼내는 `IngredientStation`, 플레이어가 직접 상호작용해야 하는 `PassiveStation` (도마 등), 재료를 올려두면 자동으로 완성되는 `AutomaticStation` (냄비 등)이 구현되어 있습니다.

### 👨‍👩‍👧‍👦 손님 시뮬레이션

* **상태 머신 AI**: 손님의 행동은 `Waiting` -> `Ordering` -> `Eating` -> `Leaving` 등 체계적인 상태 머신(`State Machine`)으로 제어됩니다.

* **동적 주문 및 대기**: `TableManager`를 통해 빈 테이블을 찾아 손님을 배치하며, 손님들은 각자의 인내심을 갖고 음식을 기다립니다. 주문 내용은 `CustomerOrderPanel` UI를 통해 실시간으로 확인할 수 있습니다.

* **오브젝트 풀링**: `PoolManager`를 사용해 손님 오브젝트를 재활용함으로써 잦은 생성/삭제로 인한 성능 저하를 방지합니다.

### 📈 일일 운영 및 성장

* **Day/Restaurant 사이클**: `RestaurantManager`의 관리 하에 낮에는 영업을, 영업이 끝나면 `ResultPanel`을 통해 일일 매출과 서빙 통계를 정산합니다.

* **메뉴 관리 시스템**: `MenuManager`를 통해 게임 진행도에 따라 새로운 메뉴를 해금하고, 영업 시작 전 그날 판매할 메뉴를 직접 선택할 수 있습니다.

* **데이터 저장/로드**: 각종 설정과 게임 진행 상황(소지금, 현재 날짜, 해금된 메뉴)는 JSON 파일 형태로 저장되어 언제든지 이어할 수 있습니다.

### ⚙️ 설정 시스템

* **키 리바인딩**: `KeyRebindManager`를 통해 플레이어의 모든 조작 키를 원하는 대로 변경할 수 있습니다.

* **오디오/비디오 설정**: BGM/SFX 볼륨 조절, 해상도 및 화면 모드(창 모드, 전체 화면) 변경 기능을 제공합니다.

## 🎮 조작법

| **키** | **기능** | **비고** |
|---|---|---|
| `WASD` | 플레이어 이동 |  |
| `J` | 상호작용 | 수동 설비 등 오브젝트와 상호작용 |
| `K` | 아이템 들기 / 내려놓기 | 재료 상자, 선반 등에서 아이템을 들거나 내려놓음 |
| `ESC` | 일시정지 메뉴 호출 / 뒤로 가기 |  |

## 🏗️ 프로젝트 구조

| **구분** | **주요 스크립트** | **설명** |
|---|---|---|
| **Core Systems** | `GameManager`, `UIManager`, `SaveLoadManager`, `PoolManager`, `EventBus`, `RecipeManager` | 게임의 전반적인 상태, UI, 데이터, 이벤트 등 핵심 로직을 담당하는 매니저 그룹 |
| **Player** | `PlayerController`, `PlayerInventory`, `PlayerInputActions` | 플레이어의 입력, 이동, 아이템 소지 등 플레이어 관련 기능 |
| **Customer** | `CustomerController`, `CustomerState`, `CustomerSpawner`, `TableManager`, `Customer.cs (ScriptableObject)` | 손님의 행동 AI, 생성, 테이블 배치 등 손님 관련 시스템 |
| **Station** | `PassiveStation`, `AutomaticStation`, `IngredientStation`, `Shelf`, `Trashcan`, `IngredientData.cs (ScriptableObject)` | 모든 상호작용 가능한 조리대와 레시피 데이터 |
| **UI** | `UIRoot`, `Fader`, `MenuPanel`, `ResultPanel`, `KeyRebindManager`, `CustomerOrderPanel` | 씬별 UI 로드, 화면 페이드, 각종 정보 패널 등 UI/UX 관련 기능 |
| **Scene/Data** | `GameEntry`, `LoadingManager`, `SaveData.cs` | 씬 전환 로직 및 저장 데이터 구조 정의 |

## 🛠️ 기술 스택

* **Unity 2022.3.19f1**

* **C# 11**

* **Unity New Input System**: 유연한 키 입력 및 리바인딩 처리

* **Addressable Asset System**: UI 등 에셋의 비동기 로딩 및 메모리 관리

* **Newtonsoft.Json**: 게임 데이터의 안정적인 직렬화/역직렬화

* **DOTween**: UI 애니메이션 및 동적 연출

## 🎯 주요 디자인 패턴

### Singleton Pattern

게임 전체에서 단 하나만 존재해야 하는 매니저 클래스들의 전역 접근성을 위해 사용되었습니다.

```csharp
// 01.Scripts/Utils/Singleton.cs
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            // ... (인스턴스 반환 로직)
        }
    }
}

// 사용 예시
BGMManager.Instance.PlayBGM(BGMType.MainScene);
```

### State Machine Pattern

손님의 복잡한 행동 로직을 각 상태(`IState`)별로 분리하여 체계적으로 관리합니다.

```csharp
// 01.Scripts/Customer/CustomerController.cs
public class CustomerController : MonoBehaviour
{
    private StateMachine _stateMachine;

    private void Awake()
    {
        _stateMachine = new StateMachine();
        var spawningState = new SpawningState(this, _stateMachine);
        // ... (다른 상태들 초기화)
        _stateMachine.Initialize(spawningState);
    }
}
```

### Object Pooling Pattern

`PoolManager`를 통해 손님, 이펙트 등 반복적으로 사용되는 오브젝트를 미리 생성해두고 재활용하여 성능을 최적화합니다.

```csharp
// 01.Scripts/Managers/PoolManager.cs
public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, Pool<Poolable>> _pools = new Dictionary<string, Pool<Poolable>>();

    public Poolable Get(string key) { /* ... */ }
    public void Release(Poolable obj) { /* ... */ }
}
```

### Event-Driven (Pub/Sub) Pattern

EventBus를 통해 시스템 간의 직접적인 참조를 없애고 결합도를 낮췄습니다. 한 시스템이 이벤트를 발생시키면, 해당 이벤트를 구독하는 다른 시스템들이 반응합니다.

```csharp
// 01.Scripts/Events/EventBus.cs
public class EventBus
{
    // ... (이벤트 딕셔너리 및 구독/발행 메소드)
}

// 사용 예시
EventBus.OnSFXRequested(SFXType.CustomeServe);
EventBus.OnBGMRequested(BGMEventType.PlayMainTheme);
```

## 🎨 씬 구성

| **씬 이름** | **설명** |
|---|---|
| `StartScene` | 게임 시작 화면 및 메인 메뉴 |
| `LoadingScene` | 씬 전환 시 사용되는 로딩 화면 |
| `MainScene` | 실제 게임플레이가 이루어지는 메인 씬 |
| `DayScene` | 일상 씬 ( 현재 미구현, 후반기 구현 예정) |

## 🎵 오디오 시스템

* **`BGMManager` / `SFXManager`**: 배경음악과 효과음을 분리하여 관리합니다.

* **`EventBus` 연동**: `PlayBGM`, `PlaySFX` 등의 이벤트를 `EventBus`로 발행하면 각 오디오 매니저가 이를 수신하여 사운드를 재생합니다.

* **`StationSFXResolver`**: 조리대의 타입에 따라 각기 다른 효과음(재료 써는 소리, 튀기는 소리 등)이 재생되도록 구현했습니다.

## 👥 개발팀

* **팀명**: 17조

* **팀원**: 김민범, 김재영, 김동현, 고윤아
