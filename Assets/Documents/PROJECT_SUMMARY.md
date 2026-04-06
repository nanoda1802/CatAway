# Project Summary: PartTimeCat Technical Documentation

## 1. 개요
본 프로젝트(PartTimeCat)의 `Assets/_Scripts` 하위 주요 스크립트들에 대한 기술 문서화 작업을 완료하였습니다. 이 문서는 프로젝트의 아키텍처 및 시스템 구성을 보존하기 위한 요약본입니다.

## 2. 완료된 디렉토리별 문서화 범위
기술 문서는 각 `.cs` 파일에 대응하는 `.md` 파일로 `Assets/Documents/` 하위 디렉토리에 생성되었습니다.

- **_Shared**: RootScope, SceneChanger, SessionManager (DI, 씬 전환, 네트워크 세션 관리)
- **Scene_Home**: HomeAvatar, HomeScope, RoomConnector, HomeView (홈 씬 UI 및 네트워크 연결)
- **Scene_Result**: ResultInitiator, ResultMember, ResultMemberSyncer, ResultScope (결과 씬 로직 및 동기화)
- **Scene_Room**: MemberPoint, PointSwapper, RoomDisconnector, RoomMember, RoomSyncer, RoomScope, ButtonSection, CodeSection, RoomMemberCard, RoomMemberCardProvider, RoomView, SectionBase, SelectionSection, StageThumbnailBoard, ToastSection
- **Scene_Stage**: 
    - **직계**: DespawnZone, LevelInitiator, LevelScope, RespawnHandler, StageDisconnector, StageHub, StageInitiator, StageScope
    - **Player**: CarrierBehaviour, CollisionBehaviour, EmotionBehaviour, InteractionBehaviour, MovementBehaviour, CarryStatus, DetectStatus, InteractStatus, MoveStatus
    - **Table**: ContactBroker, PlacementBroker, BinTable, PantryTable_BackUp, ServingTable, SinkTable, BoxTable, ChoppingTable, PantryTable, PlateRackTable, PlateReturnTable, StoveTable
    - **Item**: Carriable, Cookware, Ingredient, Plate, CookwareProvider, IngredientProvider, PlateProvider
    - **UI**: OrderBoard, OrderPresenter, ScoreBoard, ScorePresenter, TimerBoard, TimerPresenter, RespawnCard, ProgressBarWidget, PlatingIconWidget, TableAlertWidget, ToastWidget

## 3. 제외 대상 (문서화 미수행)
- 인터페이스 및 추상 기반 클래스 (IContactable, IInteractable, IPlacable, IDespawnable, IProvider 등)
- NetworkPrefabInstanceHandler 구현체들 (TablePrefabHandler, CookwarePrefabHandler 등)
- 데이터 객체들 (ScriptableObject, Enum, Struct 등)

## 4. 향후 작업 제안
- 새로운 작업을 시작할 때, 이전 세션의 컨텍스트를 새로 고침하려면 이 요약 파일을 참조하십시오.
- 특정 파일의 세부 로직 수정이 필요할 경우, 해당 `.md` 파일을 먼저 확인하여 구조를 파악한 뒤 작업을 진행하십시오.
