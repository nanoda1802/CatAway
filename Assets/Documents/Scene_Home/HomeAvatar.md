# HomeAvatar.cs 기술 문서

**개요**
`HomeAvatar`는 홈 씬에서 플레이어 아바타의 외형을 관리하고 시각화하는 컴포넌트입니다. `AvatarData`를 사용하여 아바타 메시(`SkinnedMeshRenderer`)의 재질 속성을 업데이트합니다.

**필드 (Fields)**
- `_renderer`: 아바타 모델의 `SkinnedMeshRenderer`.
- `_avatarData`: 아바타 데이터를 관리하는 `AvatarData` 인스턴스.
- `_playerStatus`: 현재 플레이어 상태(`PlayerStatus`) 정보.
- `_matPropBlock`: 재질의 속성을 효율적으로 변경하기 위한 `MaterialPropertyBlock`.
- `_subs`: `MessagePipe` 구독 관리를 위한 `IDisposable`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 데이터와 메시지 구독을 초기화하며, 현재 상태에 맞춰 초기 아바타를 적용합니다.
- **`SetAvatar`**: `AvatarMessage`를 구독하여 아바타 정보가 변경될 때마다 외형을 갱신합니다.
- **`OnDestroy`**: 메시지 구독을 해제하여 리소스를 관리합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체입니다.*
- **AvatarData (ScriptableObject)**: 아바타 관련 데이터 관리.
- **PlayerStatus (Class)**: 플레이어 상태 관리.
- **AvatarMessage (Struct/Class)**: 아바타 변경 메시지.
