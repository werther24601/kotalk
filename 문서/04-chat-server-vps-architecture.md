# 04. Chat Server And VPS Architecture

## 목표

현재 보유한 Rocky Linux VPS 위에 메신저 백엔드를 올리되, MVP는 단일 서버로 단순하게, 이후 성장은 구조 변경 없이 따라갈 수 있게 설계한다.

## 고정 아키텍처

- Reverse proxy: `Caddy`
- API/WebSocket: `ASP.NET Core 8`
- Worker: `ASP.NET Core Hosted Service` 또는 별도 worker container
- Database: `PostgreSQL`
- Cache/ephemeral state: `Redis`
- Object storage: `MinIO`
- Metrics/Logs: `Prometheus + Grafana + Loki`
- Backup: 별도 backup job container + 외부 저장소

## 외부 프로토콜

### REST

사용처:
- 가입/로그인/세션
- 대화방/메시지 조회
- 파일 업로드 초기화
- 설정 변경
- 초대 발급/수락

### WSS

사용처:
- 실시간 메시지
- 읽음 이벤트
- 타이핑
- presence
- sync required
- 가입 직후 bootstrap 동기화

## 서버 코드베이스 전략

- 초기에는 `단일 코드베이스`
- 배포 역할만 `api`와 `worker`로 분리
- 혼자 운영하기 쉽고 도메인 모델을 일관되게 유지할 수 있어야 한다.

## 메시지 처리 흐름

1. 클라이언트가 `client_request_id` 포함 메시지 전송
2. 서버가 인증과 멤버십 확인
3. PostgreSQL에 메시지 저장
4. 같은 트랜잭션에서 outbox 이벤트 저장
5. 송신자에 ACK 반환
6. Worker가 outbox를 읽어 fan-out
7. 수신자 전달/읽음 상태 업데이트

## 가입/인증 구조

### Alpha 즉시 실행형

- `이름 + 초대코드`
- invite-only
- 메일 인프라 없이 가능
- 계정, 프로필, 디바이스, 세션을 한 번에 생성

### Beta 기본형

- `이메일 1회 확인 + 표시 이름`
- 매직링크와 6~8자리 코드 병행
- 필요 시 invite gate 유지
- 자동 로그인과 기기 세션 유지

### 장기형

- `Windows Hello Passkey` 추가

## 핵심 저장소 정책

### PostgreSQL

진실의 원천:
- 사용자
- 프로필
- 디바이스
- 세션
- 인증수단
- 초대
- 대화방
- 멤버십
- 메시지
- 첨부 메타데이터
- 읽음 커서
- 반응
- 차단/뮤트/고정
- 감사 로그

### Redis

역할 제한:
- Presence TTL
- 세션 라우팅 인덱스
- Rate limit
- Pub/sub fan-out 보조

메시지 원본 저장소로 쓰지 않는다.

### MinIO

- 첨부파일
- 프로필 이미지
- 썸네일

주의:
- MinIO는 서비스 저장소이지 백업 저장소가 아니다.
- 백업본은 외부 S3 호환 스토리지나 별도 원격 저장소로 보낸다.

## 현재 VPS에 올릴 컨테이너 권장 구성

- `aster-caddy`
- `aster-api`
- `aster-worker`
- `aster-postgres`
- `aster-redis`
- `aster-minio`
- `aster-backup`
- `aster-prometheus`
- `aster-grafana`
- `aster-loki`

## 네트워크 정책

외부 공개:
- `80`, `443`, `22`

외부 비공개:
- PostgreSQL
- Redis
- MinIO API/Console
- Grafana
- Loki

## 도메인 전략

현재 `fulda-renewal.phy.kr`는 다른 서비스에 쓰고 있으므로 메신저는 별도 도메인 또는 별도 서브도메인 군을 사용한다.

권장 예시:
- `app.<messenger-domain>`
- `api.<messenger-domain>`
- `ws.<messenger-domain>`
- `files.<messenger-domain>`
- `admin.<messenger-domain>`
- `download-vstalk.phy.kr`

Windows 빌드 산출물 배포 원칙:
- 최신 Windows 빌드는 갱신될 때마다 `https://download-vstalk.phy.kr`에서 직접 다운로드 가능해야 한다.
- 이 서브도메인은 메신저 운영용 VPS IP를 가리키는 A 레코드로 관리한다.
- 정적 빌드 파일, MSIX/App Installer 피드, 릴리즈 노트 파일이 필요하면 이 배포 호스트 아래에 함께 둔다.
- HTTPS와 인증서 갱신은 Caddy 또는 동등한 프록시 계층에서 책임진다.

멀티 OS 다운로드 호스트 운영 원칙:

- 같은 버전 번호 아래에 Windows와 Android 산출물을 함께 게시한다.
- 원격 Forge Releases는 버전별 원본 저장소, 다운로드 호스트는 최종 사용자용 최신 미러 역할을 맡는다.
- 사용자는 운영체제에 따라 아래 진입 경로를 사용한다.
  - `https://download-vstalk.phy.kr/windows/latest`
  - `https://download-vstalk.phy.kr/android/latest`
  - `https://download-vstalk.phy.kr/latest/version.json`
- 버전별 이력은 `releases/<version>/windows/x64/...`, `releases/<version>/android/universal/...` 경로로 고정한다.
- APK는 공개 호스트에 둘 때 무결성 체크섬과 코드 서명 정책을 함께 유지한다.

## VPS 운영 원칙

- 메신저 전용 Linux 사용자
- 메신저 전용 Compose project
- 메신저 전용 볼륨 디렉터리
- 기존 서비스와 비밀값 파일 분리
- systemd에서 별도 서비스로 기동

## 세션과 인증수단 설계

- `accounts`
- `profiles`
- `devices`
- `sessions`
- `invites`
- `account_auth_methods`

핵심 원칙:
- 초대 코드는 `입장 제어용`
- 이메일 확인은 `계정 부트스트랩용`
- 지속 로그인은 `기기 세션용`

## 백업 정책

- PostgreSQL: 일일 전체 + 증분 복구 전략
- MinIO 메타/버킷: 주기적 외부 동기화
- 비밀정보: 별도 암호화 보관
- 복구 연습: 최소 월 1회

## 관측성

대시보드 4종은 처음부터 만든다.
- 앱/API 안정성
- 메시징 전달 지표
- VPS 자원 지표
- 백업 성공/실패

추가 인증 대시보드:
- 가입 완료율
- 초대코드 실패율
- 이메일 확인 성공률
- 세션 재발급 실패율

## 스케일링 경로

### 단계 1. MVP

- 단일 VPS
- 단일 API 인스턴스
- 단일 Worker
- 초대 기반 또는 소규모 이메일 확인

### 단계 2. 초기 성장

- API 2개 이상 복제
- Redis fan-out 활용
- PostgreSQL 튜닝과 인덱스 강화
- 이메일 발송 신뢰성 강화

### 단계 3. 베타 이후

- 별도 DB/스토리지 관리형 서비스 이관 검토
- 파일 스토리지를 외부 S3로 이동
- 백업과 관측성 완전 분리

## 기술 선택 이유

### 자가 구현 서버를 택하는 이유

- 개인 사이드 프로젝트에 맞게 도메인 모델을 단순화할 수 있다.
- Windows-first UX에 맞는 이벤트 계약을 깔끔하게 설계할 수 있다.
- Matrix/XMPP를 붙일 때 생기는 복잡한 개념과 운영부담을 피할 수 있다.

### 왜 Matrix가 아닌가

- 강력하지만 MVP에 비해 무겁다.
- 브리지, federation, 클라이언트 호환성까지 생각하면 범위가 커진다.
- 이번 프로젝트의 차별화는 프로토콜이 아니라 Windows 클라이언트 경험이다.

## 실제 구축 순서

1. VPS 하드닝
2. 메신저 전용 도메인/DNS
3. 다운로드 호스트에 Windows/Android latest 라우트 반영
4. Forge Releases와 다운로드 미러의 같은 버전 자산 정합성 검증
3. Docker Compose skeleton
4. PostgreSQL/Redis/MinIO/Caddy
5. Alpha용 초대 가입 API
6. Conversation list API
7. WSS 게이트웨이
8. Message outbox/worker
9. File upload pipeline
10. Beta용 이메일 1회 확인 플로우
11. Monitoring/backup
