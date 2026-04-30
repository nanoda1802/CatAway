## Overview

- 최대 4인 멀티플레이어
	- Server Auth
	- 참여자 중 한 명이 Host가 되고, 다른 참여자들은 Client가 되는 구조
- PC와 Android 플랫폼 호환
- Overcooked2 모작
- 플레이 목표
	- 제한된 스테이지 시간 내에,
	- 가능한 한 신속히 음식을 제조해,
	- 최대한 많은 주문을 해결해,
	- 최고 점수를 획득
- 협동 모드와 경쟁 모드 존재
	- 협동은 최소 1인에서 최대 4인 플레이
	- 참여자 공통 주문 리스트를 해결해 제한 시간 내 고득점 기록하는 것이 목표
	- 경쟁은 최소 2인에서 최대 4인 플레이
	- 팀별 주문 리스트를 해결해 상대 팀보다 고득점 기록하는 것이 목표

## Package In Project

### Network

- **Netcode for GameObjects :** 멀티플레이 구현용, 추후 Relay 등 유니티의 부가적인 네트워크 패키지와의 호환성 고려
- **Multiplayer Samples Utilities :** ClientNetworkTransform, ClientNetworkAnimator 등, 클라이언트 주도 동기화에 필요한 Component 활용 위해 설치 (네트워크 지연에 의한 플레이 불편 줄이기 위함)
- **Multiplayer Play Mode :** 개발 과정에서의 네트워크 기능 테스트 위해 설치, Project Build 또는 PararellSync 등 다른 테스트 방식과 달리, 프로젝트 클론 방식이 아니라서 별도의 용량 차지하지 않는다는 점, 각 가상 플레이어에서 별도의 Hierarchy, Inspectorm, Console 창 등을 확인 가능

### Third Party? (명칭 점검 필요)

- **VContainer :** GetComponent, Find 등 MonoBehaviour 의존적인 참조 방식 극복, 오브젝트 간 의존성의 가시성 향상, 디버깅 경로 일원화 등등의 이유로 설치
- **MessagePipe :** 스크립트 간 결합도 낮추기, Publisher와 Subscriber 기능 활용한 메세지 기반 반응형 프로그래밍 의도 (특히 View와 Logic 간)
- **UniTask :** Coroutine 및 async/await 대체. 구조체 기반 작동 방식에 의거해 GC call 최소화, 메모리 효율성 향상 의도
- **Prime Tween :** View 오브젝트들의 생동감 향상 연출, 구조체 기반 방식으로 다른 Tween 라이브러리들에 비해 성능적 우위

### Unity Registry

- **Input System :** PC와 Mobile 전환 간 입력 방식의 효율적인 스위칭 의도
- **2D Tilemap Editor :** Stage의 Level 생성 작업 시 능률 향상 의도
- **2D Tilemap Extras :** 3D 오브젝트의 Tilemap 패키지 호환 위해 설치 (Tilemap Pallete의 GameObject Brush 활용)
- **FBX Exporter :** Blender에서의 간단한 Mesh 병합 및 Mesh 단순화 목적으로 fbx 모델 추출하기 위해 설치

## Play Loop

- Home → Room → Stage → Result → Room
	- Home을 제외한 모든 단계에서 Home으로 복귀 가능
- **Home :** 애플리케이션 실행 시 처음 조우하는 장면
	- 플레이어는 방 생성 / 방 참가 선택 가능
	- 방 생성 시 Host로, 방 참가 시 Client로 네트워크 접속
- **Room :** 참여자들이 스테이지 입장 전 대기하는 장면
	- Host 주관으로 모드(협동/경쟁)과 스테이지 선택 가능
	- Client 들은 준비 상태 전환을 통해 의사 표현 가능
	- 스테이지 입장 조건 달성되면 Host 주관으로 스테이지 입장 가능 (모든 참여자가 준비되었는지, 경쟁 모드라면 각 팀에 최소 1명 이상의 참여자가 있는지)
- **Stage :** 게임의 핵심 콘텐츠를 플레이하는 장면
	- Stage System 목차에서 별도로 설명
- **Result :** 스테이지의 결과를 확인하는 장면
	- 획득 점수, 주문 성공률 등 표시
	- 점수 획득 최대 기여자 표시
	- 경쟁 모드라면 승리한 팀 표시

## Extra Functions

- 플레이어 닉네임 설정
- 플레이어 아바타 외형 설정
- 음량 조절

## Stage System

### 핵심 요소

- **Player :** 스테이지 내 행동의 주체, 아이템들을 운반, 테이블들과 상호작용
	- Carry : 아이템 들기, 내려놓기, 던지기 등 운반 기능의 명칭
	- Contact : 단발성 입력 통한 상호작용 (Press)
	- Interact : 지속성 입력 통한 상호작용 (Hold), 상호작용 완료 전에 입력 중단 시 취소됨
- **Item :** 음식 제조 과정에서 활용되는 객체들
	- Ingredient : 레시피에서 요구하는 재료, 조합하기 위해선 각 재료에 맞는 가공(Prep) 필요함
	- Dish : 가공된 재료들을 보관, 보관된 재료 조합와 일치하는 레시피의 주문 타겟 가능
	- Cookware : 특정 재료 가공에 필요한 임시 보관 아이템
- **Table :** 음식 제조 과정에서 재료 획득, 재료 가공, 음식 제출 등을 위해 플레이어의 상호작용 대상이 되는 객체들

### 규칙

- **제한 시간 :** 스테이지 별 제한 시간 180초
- **주문**
	- 일정 시간 마다 신규 주문 추가
	- 만약 현재 활성화 주문 수가 cap에 도달했다면 추가 X
		- 협동 모드 Coop : 최대 활성화 주문 수 5개
		- 경쟁 모드 Comp : 팀별 최대 활성화 주문 수 3개씩
	- 주문 개체 별 규칙
		- 제한 시간 : 남은 시간에 비례한 점수 배율
		- 레시피 Recipe : 필요 재료 명시, 요구 재료 종류에 비례한 기본 점수
	- 플레이어가 음식 제출 시, 해당 음식의 재료 조합과 일치하는 레시피의 활성화 주문이 있다면, 해당 주문을 제거하고 성공 처리
		- 만약 중복 레시피의 주문이 있다면, 먼저 활성화된 주문을 제거하고, 성공 처리
	- 만약 제한 시간 내에 활성화 주문 처리 실패 시, 해당 주문을 제거하고 실패 처리
- **점수**
	- 총점과 콤보로 구성
	- 총점 Score
		- 주문 제출 성공 : 해당 주문의 남은 시간 배율, 해당 레시피의 기본 점수, 현재 콤보 수로 산정한 득점
		- 주문 제출 실패 : 해당 레시피의 기본 점수로 산정한 감점
	- 콤보 Combo
		- 주문 제출 성공 : 연속 성공마다 1씩 증가 (1회 성공 시 콤보 수 1, 2회 성공 시 콤보 수 2, …)
		- 주문 제출 실패 : 0으로 초기화



