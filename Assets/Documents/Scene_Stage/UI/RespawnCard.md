# RespawnCard.cs 기술 문서

**개요**
`RespawnCard`는 플레이어가 사망 시 해당 플레이어가 리스폰될 월드 좌표(위치) 위에 떠오르는 UI 카드입니다. 리스폰까지의 대기 시간을 시각적으로 알려줍니다.

**주요 메서드 (Methods)**
- **`Activate(float time)`**: 지정된 시간 동안 카드를 활성화하고 카운트다운을 표시합니다.
- **`Deactivate`**: 카드를 비활성화하고 풀로 반환합니다.
- **`SetPos(Vector3 worldPos)`**: 월드 상의 리스폰 위치 좌표를 설정합니다.
