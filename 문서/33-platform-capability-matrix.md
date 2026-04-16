# Platform Capability Matrix

## 목적

Windows, Mobile Web, Android를 병렬로 운영하면 언제든지 `문서상 가능`, `부분 가능`, `실제 완성`이 섞여 혼란이 생긴다.  
이 문서는 플랫폼별 기능 상태를 솔직하게 나누고, 어디에 무엇을 먼저 넣어야 하는지 판단 기준을 제공한다.

## 상태 정의

- `Live`: 실제 사용자에게 사용 가능
- `Buildable`: 빌드되지만 실사용 검증은 부족
- `Partial`: 일부만 구현
- `Planned`: 설계만 존재
- `Blocked`: 구현보다 선행 리스크가 큼

## 현재 기능 매트릭스

| 기능 | Windows | Mobile Web | Android | 비고 |
|---|---|---|---|---|
| 초간단 가입 | Buildable | Live | Buildable | Android는 WebView 셸 기준 |
| 자기 자신과의 대화 | Buildable | Live | Buildable | 첫 진입 루프 |
| 텍스트 메시지 전송 | Buildable | Live | Buildable | 기본 루프 |
| 읽음 커서 갱신 | Partial | Partial | Partial | 복귀 정확도 보강 필요 |
| 실시간 수신 | Partial | Partial | Partial | 경계 조건 검증 부족 |
| 로컬 검색 | Partial | Partial | Partial | 제목/기본 검색 수준 |
| 전역 검색 | Planned | Planned | Planned | 핵심 우선순위 |
| 드래프트 보존 | Partial | Partial | Partial | 실제 복원 체감 부족 |
| 세션 자동 갱신 | Planned | Planned | Partial | WebView 셸 기준 |
| 파일 첨부 | Planned | Planned | Planned | 다음 대규모 과제 |
| 링크 프리뷰 | Planned | Planned | Planned | 제품성 핵심 |
| 알림 묶음 정책 | Planned | Planned | Planned | 정책 문서화 완료 |
| 팝아웃 창 | Planned | N/A | N/A | 데스크톱 핵심 |
| 다중 창 | Planned | N/A | N/A | 데스크톱 핵심 |
| Android APK | N/A | N/A | Buildable | alpha 기준선 확보 |
| Releases 배포 | Partial | Partial | Buildable | 표면 정리 진행 중 |

## 제품 메시지 규칙

- README는 `Live`와 `Buildable`을 혼동하지 않는다.
- 문서에서 `지원`이라는 표현은 `실제 사용자 기준 사용 가능`일 때만 쓴다.
- `Planned` 기능은 과장된 홍보 문구에 쓰지 않는다.

## 플랫폼별 역할 재정리

## Windows

- 최종적으로 업무 효율 중심의 대표 채널
- 다중 창, 검색, 파일, 지식 재발견, 장시간 세션에 강해야 한다
- 현재는 UI 방향은 좋지만 생산성 장치 구현이 부족하다

## Mobile Web

- 지금 당장 체험과 빠른 배포의 핵심 채널
- 가입과 첫 대화 루프의 검증에 적합
- 그러나 안정감과 레이아웃 품질을 먼저 끌어올려야 한다

## Android

- 장기적으로 일상 주사용 모바일 채널
- 푸시/미디어/백그라운드 안정성으로 모바일 웹을 보완
- 현재는 WebView 기반 APK 셸을 확보했고, 장기적으로는 공용 네이티브 UI 축으로 옮길지 판단 중이다

## iOS / Linux

- iOS는 저장소 Assets 직접 배포가 아니라 Apple 채널 기준으로 준비한다
- Linux는 Windows와 같은 장기 네이티브 데스크톱 축으로 본다
- 두 채널 모두 공용 UI 프레임워크 선택에 직접 연결되므로, Android 단독 최적화와 분리해 판단하면 안 된다

## 기능 우선순위 매핑

### 모든 플랫폼 공통 우선

- 세션 연속성
- 드래프트 복원
- 기본 검색
- 알림 정확성

### Windows 우선

- 팝아웃
- 다중 창
- 단축키
- 파일/링크 허브

### Mobile Web 우선

- 레이아웃 안정화
- 세션 오류 제거
- 한 손 조작 최적화
- 홈 진입 후 복귀

### Android 우선

- 로그인/가입 플로우
- 푸시 채널
- 첨부/카메라
- 오프라인 재개

## 사용자가 기대하는 일관성

- 용어는 같아야 한다
- 대화 목록 구조는 유사해야 한다
- 읽지 않은 상태 표시와 고정/무음 개념이 일치해야 한다
- 파일/링크/보관 개념도 채널마다 다르게 이름 붙이지 않는다

## 개발 운영 원칙

- 기능이 한 플랫폼에만 있으면 문서에 명확히 적는다
- 공통 도메인 개념은 서버/문서/클라이언트 이름이 같아야 한다
- 테스트 없는 대규모 주장 금지
- 라이브 채널 기준으로 문제를 발견하면 해당 기능 상태를 즉시 재평가한다

## 완료 기준

- 기능 상태를 누구나 같은 언어로 이해할 수 있어야 한다.
- 사용자는 README를 보고 기대한 것과 실제 제품 사이의 괴리가 적어야 한다.
