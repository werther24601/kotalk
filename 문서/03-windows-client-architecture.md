# 03. Windows Client Architecture

## 현재 구현 스택

- UI: `Avalonia 12`
- Runtime: `.NET 8`
- Pattern: `MVVM + feature-first modules`
- Session persistence: 파일 기반 세션 저장에서 시작, 이후 `SQLite`로 확장
- Packaging: 1차는 `Windows x64 portable zip`
- 장기 검토: Windows 전용 고급 통합이 필요해질 경우 `MSIX`와 추가 Windows 네이티브 경로 평가

## 구현 현실 메모

`2026-04-16` 기준 실제 저장소 구현은 `Avalonia 12`를 사용한다.

이유:
- 현재 작업 환경에서 Windows 빌드 산출물을 지속적으로 재생성할 수 있다.
- 한국어 UI와 메신저 셸 UX를 우선 검증하기에 충분하다.
- WinUI 3 대비 플랫폼 통합은 줄지만, 1차 목표인 `실사용 가능한 Windows 빌드 반복 생성`에는 더 유리하다.

즉, 제품 목표는 여전히 `Windows-first UX`이고, 현재 런타임 선택은 `릴리즈 반복성`을 우선한 구현 결정이다.

## 왜 이 구현을 먼저 택했는가

- Windows 산출물을 이 워크스페이스에서 직접 만들 수 있다.
- 같은 `.NET 8` 기반으로 서버와 클라이언트의 개발 경험을 맞출 수 있다.
- 메신저 셸, 목록, 대화, 컴포저, 한국어 라이팅 품질을 빠르게 반복할 수 있다.

## 한국어-first 구조

- 앱 전역 언어 기본값은 한국어로 고정
- 문자열 시스템은 `리소스 기반`으로 설계하되 1차 카피는 모두 한국어 기준
- IME 조합 중 Enter 처리, 줄바꿈, 검색, 단축키 충돌을 별도 품질 축으로 관리
- 한국어 라벨은 짧게, 설명은 보조 텍스트로 분리

## 패키징 정책

- 내부 개발: 빠른 디버깅을 위해 unpackaged 프로필 병행 가능
- 공식 빌드: `MSIX` 고정
- 업데이트: `App Installer` 기반 업데이트 피드 사용

## 셸 구조

- 메인 윈도우 하나를 기본으로 둔다.
- 기본 화면은 `좌측 목록 - 중앙 대화 - 선택형 우측 패널`
- 보조 창은 아래만 허용한다.
  - 이미지 뷰어
  - 설정
  - 로그인/계정 관련 별도 창

## 앱 계층

- `Shell`
- `Feature Views`
- `ViewModels`
- `Stores`
- `Repositories`
- `Transport Clients`
- `Local DB`

## 권장 모듈 분리

- `App`
- `Shell`
- `Auth`
- `ChatList`
- `Conversation`
- `Search`
- `Attachments`
- `Settings`
- `Notifications`
- `Sync`
- `Common`

## 가입/로그인 구조

### 즉시 실행용 Alpha

- 앱 실행
- 유효 세션이 있으면 즉시 메인 진입
- 없으면 `이름 + 초대코드`
- 가입 직후 `나에게 메시지` 또는 inviter와의 기본 대화 생성

### Beta 기본형

- 앱 실행
- 유효 세션이 있으면 즉시 메인 진입
- 없으면 `이메일 1회 확인 + 이름`
- `이 PC에서 계속 로그인` 옵션 제공

### 자동 로그인

- `access token`은 메모리 유지
- `refresh token`은 Windows 보안 저장소 유지
- `SQLite`에는 계정/세션 메타만 저장
- 최근 계정 복귀 화면 지원

## 상태 관리

### ViewModel

- 화면 전용 상태만 관리
- 예: 선택된 대화, 검색어, 패널 열림 여부

### Store

- 기능 단위 세션 상태 관리
- 예: `ChatListStore`, `ConversationStore`, `PresenceStore`, `OnboardingStore`

### Repository

- 네트워크와 로컬 DB를 조합
- 예: 메시지 전송, 읽음 처리, 파일 업로드, 동기화, 가입/세션 갱신

## 로컬 캐시 전략

- 앱 시작 시 최근 대화 목록은 SQLite에서 먼저 렌더링
- 서버 응답으로 뒤에서 정합성 보정
- 메시지 전송 시 임시 메시지를 즉시 로컬에 추가
- 서버 확정 ACK로 상태를 `pending -> sent -> delivered -> read` 전환

### 로컬에 저장할 데이터

- 계정/세션 메타
- 최근 계정 목록
- 대화방 목록 요약
- 최근 메시지
- 읽음 커서
- 첨부 메타데이터
- Draft
- 검색 인덱스 일부
- UI 상태

## 오프라인/동기화 정책

- `offline-first shell`
- 앱 시작 시 로컬 대화 목록을 먼저 보여 줌
- 동기화는 cursor 기반 증분 모델
- 긴 오프라인 이후에는 대화방 단위 증분 복구
- 초안과 아웃박스는 대화방별 보존

### 오프라인 큐 대상

- 텍스트 전송
- 읽음 이벤트
- 반응
- 업로드 예약

## 실시간 연결

- REST는 조회/명령/업로드용
- WSS는 실시간 이벤트용
- 클라이언트는 재연결 backoff, session resume, sync-required 이벤트 처리 필수

## P0 편의 기능

- `Ctrl+K` 전역 빠른 이동기
- `Ctrl+N` 새 대화
- 작성중 텍스트 자동 보존
- 읽지 않음/고정/멘션 필터
- 이미지 붙여넣기 즉시 업로드
- 파일 드래그앤드롭
- `집중 모드`와 `조용히 보기`
- 정보 밀도 조절
- 글자 크기 조절
- 좌측 목록 폭 조절

## 알림과 트레이

- 포그라운드에서는 인앱 배너 중심
- 백그라운드에서는 Toast
- 트레이 아이콘은 unread 총합과 연결 상태 표현
- 클릭 시 앱 복귀와 대화방 포커싱 보장

## 미디어 처리

- 썸네일과 원본을 분리
- 리스트/타임라인은 경량 리소스만 사용
- 대용량 비디오는 자동 재생 금지
- 파일 열기는 기본 OS 연결 프로그램에 위임

## 보안 경계

- 관리자 키를 클라이언트에 절대 넣지 않음
- 토큰은 DPAPI/PasswordVault에 저장
- 로그와 크래시 리포트에 민감정보 금지
- 자동 로그인과 로컬 캐시는 별개 설정으로 분리
- 공용 PC 경고와 원격 로그아웃 제공

## 성능 기준

- 콜드 스타트 후 최근 대화 표시 `3초 이내`
- 가입 후 첫 대화 시작 `60초 이내`
- 대화 스크롤은 가상화 기반
- 긴 채팅방에서도 입력 지연 체감 없도록 유지

## 권장 폴더 구조

```text
src/
  Aster.App/
  Aster.Shell/
  Aster.Features.Auth/
  Aster.Features.ChatList/
  Aster.Features.Conversation/
  Aster.Features.Search/
  Aster.Features.Attachments/
  Aster.Features.Settings/
  Aster.Infrastructure/
  Aster.Domain/
  Aster.Persistence/
  Aster.Transport/
tests/
  Aster.UnitTests/
  Aster.IntegrationTests/
  Aster.UITests/
```

## 구현 순서

1. 셸, 최근 대화 목록, 대화 읽기 골격
2. Alpha용 초간단 가입
3. 텍스트 송수신과 로컬 캐시
4. 읽음 상태, 재연결, 알림
5. 파일/이미지 업로드
6. 통합 검색과 필터
7. Beta용 이메일 1회 확인 플로우
8. 세션 관리, 설정, 크래시 리포트

## 가장 먼저 미룰 기능

- 멀티 윈도우 대화 분리
- 스티커/테마 마켓
- 음성/영상 통화
- 고급 편집기
