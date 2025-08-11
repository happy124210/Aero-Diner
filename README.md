# 🍳 프로젝트: Aero Diner

<img width="2000" height="1125" alt="1" src="https://github.com/user-attachments/assets/c229fdf4-fbf0-4922-a1af-1377b8f80cd5" />

<details>
<summary>&nbsp; 게임 소개</summary>
<br>
  
| 제목 | Aero Diner |
| --- | --- |
| 장르 | 2D / 요리 / 타이쿤 / 도트 / 탑뷰 |
| 플랫폼 | PC |
| 플레이 시간 | 30분 ~ 1시간 (데모 기준) |

</details>

<details>
<summary>&nbsp; 게임 목표</summary>
<br>

- 스토리를 따라 비행선 식당을 경영 및 부흥시키는 것이 목표입니다.
- 매 일차마다 일상 → 영업 → 결산 → 일자 변경 순으로 흐름이 진행됩니다.
- 골드를 재화로 사용합니다.

</details>

<details>
<summary>&nbsp; 튜토리얼</summary>
<br>

<img width="2000" height="1125" alt="2" src="https://github.com/user-attachments/assets/7f6fefe7-7b4e-4bf1-ba53-1e2bae5bd14c" />


- 게임 진행에 필요한 기본 지식을 알려주기 위한 튜토리얼 입니다.
- 이동, 재료 가공 및 조리, 서빙과 같은 기본적인 동작부터 상점 이용, 설비 관리 등 재화 사용처에 대한 지식을 안내합니다.

</details>

<details>
<summary>&nbsp; 일상 단계</summary>
<br>

<img width="2000" height="1125" alt="3" src="https://github.com/user-attachments/assets/498d6332-3806-4b33-bb46-2e228d49110d" />


- 오늘의 영업에 사용할 레시피, 설비를 준비하거나 구매하는 단계입니다.

**상점**
- 상점에서는 골드를 소모하여 레시피나 설비를 구입할 수 있습니다.
- 일부 레시피, 설비는 구입하려면 선행 조건을 달성해야 합니다. 특정 레시피를 구입하거나 퀘스트를 클리어 해야 해금됩니다.
- 레시피는 1회만 구입 가능하지만 설비는 횟수 제한 없이 구매 가능합니다.
- 만약 구입에 필요한 골드가 부족하다면 관련 팝업 메시지로 안내됩니다.
- 구입한 설비는 맵 우측의 창고에 배치되며, 만약 빈 공간이 없다면 관련 팝업 메시지로 안내됩니다.

**인벤토리**
- 인벤토리에서는 보유한 재료, 설비, 레시피에 대한 정보를 확인할 수 있습니다.
- 재료탭에서는 자신이 사용할 수 있는 재료의 종류를 확인할 수 있으며, 추후 재고 시스템이 추가될 경우 활용될 재고 수량 안내 정보 역시 존재합니다.
- 설비 탭에서는 자신이 보유한 설비 목록을 확인할 수 있으며, 각 설비의 배치/보관 수량 역시 표시됩니다.
- 레시피 탭에서는 자신이 보유한 레시피 목록을 확인할 수 있으며, 각 레시피에 필요한 재료와 조리 방법을 확인할 수 있습니다.

**설비 배치**
- 각 설비를 원하는 위치에 배치할 수 있으며, 설비를 들고 있는 동안 배치 그리드가 표시됩니다.
- 좌측 수거 구역에 배치된 설비는 영업이 시작될 때 제거되며, 영업 시작 전 설비 제거에 대한 경고 팝업 메시지가 출력됩니다.
- 우측 창고 구역에 배치된 설비는 영업 동안엔 표시되지 않고 보관됩니다. 상점에서 구입한 설비 역시 이곳에 보관됩니다.

</details>

<details>
<summary>&nbsp; 영업 단계</summary>
<br>

<img width="2000" height="1125" alt="4" src="https://github.com/user-attachments/assets/67875e58-7124-46e7-b175-85e4d2bf9af8" />


- 손님이 방문하여 오늘의 메뉴 중 자신이 원하는 것을 주문하고 기다립니다. 플레이어는 손님이 떠나기 전 해당 요리를 만들고 서빙하여 골드를 벌어들일 수 있습니다.

**오늘의 메뉴**
- 오늘 영업에 사용할 레시피를 선택할 수 있으며, 손님들 역시 해당 목록 내에서만 주문합니다.
- 우하단 시작 하기 버튼 클릭 시, 영업 단계가 시작됩니다.

**손님**
- 손님은 일정 주기마다 가게에 방문하며 빈 자리가 있다면 그곳에 착석합니다.
- 자리가 없다면 줄을 서서 빈 자리가 생기는 것을 인내심이 고갈될 때까지 기다립니다. 빈 자리가 생길 시, 먼저 줄을 선 손님부터 착석 및 인내심이 초기화됩니다.
- 착석한 손님은 오늘의 메뉴 중 원하는 것을 주문한 뒤, 인내심이 고갈될 때까지 기다립니다.
- 인내심이 고갈되기 전에 요리를 서빙 받았다면 몇 초간 식사 후 재화를 지불하며 가게를 떠납니다.
- 요리를 제때 제공받지 못해 인내심이 고갈되었다면 즉시 가게를 떠납니다.

**설비**
- 각 설비마다 가공 가능한 재료의 종류 및 숫자가 다릅니다.
- 모든 설비는 재료 배치 후 일정 시간 동안 가공해야 결과물을 획득할 수 있습니다.
- 수동 설비는 재료 배치 후 플레이어가 직접 가공 과정을 거쳐야 하지만 자동 설비는 재료만 배치하면 스스로 가공을 진행합니다.

</details>

<details>
<summary>&nbsp; 결산</summary>
<br>

<img width="2000" height="1125" alt="5" src="https://github.com/user-attachments/assets/47773524-b7ea-4330-9ed3-2dba2e96b50f" />


- 그날의 영업을 통해 벌어들인 금액, 방문 손님 숫자 등 정보를 확인할 수 있는 창입니다.
- 각 메뉴의 판매 숫자 및 수익, 손님 방문 및 대응 여부에 따른 숫자를 확인할 수 있습니다.

</details>


## 🧑🏻‍🍳 플레이 방법

* 빌드 - [PC버전 다운로드](https://drive.google.com/drive/u/0/folders/1d54WqSPr1tGAjxis93lwrhhfrGoD7TJJ)
* 웹 - [Unity Play 사이트에서 바로 플레이](https://play.unity.com/en/games/68b177a8-74d0-4380-8eb4-746b1c91d4fe/aero-diner)
* 에디터 - StartScene으로 시작



<img width="546" height="151" alt="Title" src="https://github.com/user-attachments/assets/906c4473-98e9-4fc7-896a-90bacb76c10f" />


<img width="1298" height="249" alt="ManagerSystem" src="https://github.com/user-attachments/assets/f87aac85-4160-4f54-bdec-7c84696c091f" />


<img width="1298" height="249" alt="CustomerAI" src="https://github.com/user-attachments/assets/20191318-3e27-4012-bd6b-886790fcabec" />


<img width="1298" height="249" alt="Component 20" src="https://github.com/user-attachments/assets/3443403f-6c69-44bb-949e-12a4a7666d1d" />


<img width="1298" height="249" alt="Component 21" src="https://github.com/user-attachments/assets/3bd5c652-2ec5-48a2-8823-beb27fb1d7ee" />



<img width="1298" height="249" alt="DataDrivenSystem" src="https://github.com/user-attachments/assets/f9a3019b-0a6a-4579-bd40-62cafa8d8c0b" />


<img width="1298" height="249" alt="IMGUISystem" src="https://github.com/user-attachments/assets/083919db-70f3-40f0-bda3-562c2ac59391" />

  

## 🏗️ 프로젝트 구조

| **구분** | **주요 스크립트** | **설명** |
|---|---|---|
| **Core & Managers** | `GameManager`, `UIManager`, `SaveLoadManager`, `PoolManager`, `EventBus`, `RecipeManager`, `StoryManager`, `QuestManager` | 게임의 핵심 로직, 데이터, 씬 관리 등 전반적인 시스템을 담당하는 매니저 그룹입니다. |
| **Player** | `PlayerController`, `PlayerInventory`, `PlayerInputActions.cs` | 플레이어의 입력 처리, 이동, 애니메이션, 아이템 소지 및 상호작용을 관리합니다. |
| **Customer** | `CustomerController`, `CustomerState`, `CustomerSpawner`, `TableManager`, `CustomerData.cs` | 손님의 상태 머신 기반 AI, 생성, 테이블 배치, 대기열 시스템 등 손님 관련 로직을 담당합니다. |
| **Station** | `BaseStation`, `IInteractable`, `PassiveStation`, `AutomaticStation`, `IngredientStation`, `StationData.cs` | 모든 조리대와 선반의 상호작용, 재료 조합, 요리 진행 과정을 처리하며, 모든 데이터는 `StationData` ScriptableObject 기반으로 관리됩니다. |
| **UI System** | `UIManager`, `OverSceneUIHandler`, `KeyRebindManager`, `Store.cs`, `Inventory.cs` | Addressable로 씬별 UI를 로드하고, `EventBus`로 이벤트를 처리합니다. 설정, 상점, 인벤토리 등 모든 UI 패널을 포함합니다. |
| **Data & Utils** | `SaveData.cs`, `CSVImporter.cs`, `StringNamespace.cs`, `VisualObjectFactory.cs` | 게임 저장 데이터 구조, CSV 데이터 임포터, 문자열 상수, 동적 오브젝트 생성 등 프로젝트 전반에서 사용되는 유틸리티와 데이터 구조를 정의합니다. |

## 🛠️ 기술 스택

* **Unity 2022.3.17f1**

* **C# 11**

* **Unity New Input System**: 유연한 키 입력 및 리바인딩 처리

* **Addressable Asset System**: UI 에셋의 비동기 로딩 및 메모리 관리

* **Newtonsoft.Json**: 게임 데이터의 안정적인 직렬화/역직렬화

* **DOTween**: UI 애니메이션 및 동적 연출

## ✨ 주요 기능

### 🧑‍🍳 요리 & 레시피 시스템

* **데이터 기반 레시피**: `ScriptableObject`로 구현된 `FoodData`와 `StationData`, `CustomerData`를 통해 재료, 레시피, 가격, 조리시간 등 모든 정보를 관리하여 확장 및 밸런싱이 용이합니다.
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

### 🔩 설비 배치 시스템

* **그리드 기반 배치**: `TilemapController`를 통해 관리되는 그리드 셀(`GridCell`) 위에 설비를 자유롭게 배치하고 재배치할 수 있습니다.
* **상태에 따른 상호작용**: 게임 페이즈가 `EditStation`일 때만 설비를 들고(`PlayerInventory`), 배치(`PlacementManager`)할 수 있도록 하여 게임 흐름에 맞는 상호작용을 구현했습니다.
* **배치 유효성 검사**: `GridCellStatus` 컴포넌트가 각 셀의 상태(비어있음/차있음)를 관리하여, 이미 다른 설비가 있는 곳에는 새로 배치할 수 없도록 제한합니다.

### 📔 인벤토리 (정보 열람)

* **탭 기반 UI**: `Inventory.cs`는 `TabController`를 통해 재료, 레시피, 설비, 퀘스트 정보를 각각의 패널에서 열람할 수 있는 통합 정보 UI의 역할을 합니다.
* **실시간 정보 연동**: 플레이어가 해금한 레시피(`RecipePanel`), 보유 및 배치한 설비의 수(`StationPanel`) 등 `MenuManager`와 `StationManager`의 데이터를 실시간으로 연동하여 UI에 표시합니다.
* **아이템 소지 시스템**: 플레이어는 전통적인 인벤토리 슬롯이 아닌, `PlayerInventory`를 통해 한 번에 하나의 아이템(`FoodDisplay` 또는 `IMovableStation`)만 들 수 있어 전략적인 동선 관리가 요구됩니다.

### 🏪 상점 시스템

* **데이터 기반 상품 목록**: `StoreDataManager`가 CSV 파일에서 상점 아이템 데이터(`StoreItemData`)를 로드하여, 코드 수정 없이 상품의 가격, 해금 조건 등을 관리할 수 있습니다.
* **구매 및 해금 로직**: `Store.cs`에서 플레이어의 소지금(`GameManager.TotalEarnings`)을 확인하여 구매 가능 여부를 판단하고, 구매 시 `MenuManager` 또는 `StationManager`에 해금 정보를 등록합니다.
* **동적 해금 조건**: 특정 퀘스트를 완료하거나 선행 레시피를 구매해야만 다음 상품이 해금되는(`UnlockType`) 시스템을 구현하여 플레이어에게 성장 목표를 제시합니다.


## 🍝 17조

| **역할** | **이름** | **개인 깃허브** |
|---|---|---|
| **팀장/기획** | 김민범 | - |
| **리드개발** | 고윤아 | https://github.com/happy124210 |
| **개발** | 김재영 | https://github.com/GUYoof |
| **개발** | 김동현 | https://github.com/StarCandy-D2 |
