# StageThumbnailBoard.cs 기술 문서

**개요**
`StageThumbnailBoard`는 로비 씬에서 스테이지 선택을 위한 UI 썸네일 보드를 관리하는 컴포넌트입니다. 드래그(스와이프) 입력을 통해 스테이지 변경을 요청하거나, 메시지 수신을 통해 썸네일을 시각적으로 슬라이드 전환하는 기능을 제공합니다.

**필드 (Fields)**
- `_thumbnails`: 화면에 표시되는 `Thumbnail` 컴포넌트 목록.
- `_stageList`: 스테이지 설정 데이터(`StageListData`).
- `_isDragActive`: 호스트 여부에 따른 드래그 활성화 상태.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 스테이지 데이터와 메시지 발행기를 초기화하고, 스테이지 변경 응답(`SelectStageRespond`)을 구독합니다.
- **`OnBeginDrag` / `OnEndDrag`**: 사용자의 드래그 입력을 감지하고, 스와이프 거리와 방향을 판단하여 스테이지 변경 요청(`SelectStageRequest`)을 발행합니다.
- **`InitThumbnails`**: 현재 모드와 선택된 스테이지에 따라 썸네일 이미지를 로드하고 배치합니다.
- **`UpdateThumbnail`**: 스테이지 변경 응답을 받아 썸네일 리스트를 갱신하고 `SlideThumbnails`로 슬라이드 애니메이션을 처리합니다.
- **`SlideThumbnails`**: 비동기 슬라이드 애니메이션을 수행하여 썸네일 위치를 조정합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **StageListData (ScriptableObject)**: 스테이지 설정 데이터.
- **Thumbnail (Class)**: 썸네일 UI 컴포넌트.
- **StageMode (Enum)**: 게임 모드 정의.
- **다양한 메시지 클래스 (Struct/Class)**: 통신용 메시지.
