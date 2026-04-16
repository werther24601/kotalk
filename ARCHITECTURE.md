# Architecture

## 시스템 구성도

```text
Windows Desktop (Avalonia 12)
        |
   REST / WebSocket
        |
ASP.NET Core 8 API
        |
SQLite (current local alpha)
        |
PostgreSQL / Redis / MinIO (target VPS stack)
```

## 핵심 컴포넌트 역할

| 컴포넌트 | 역할 |
|---|---|
| `VsMessenger.Desktop` | 한국어 Windows UX, 세션 보존, 대화 목록/대화창, 전송 흐름 |
| `VsMessenger.Api` | 인증, 부트스트랩, 대화/메시지 REST API, WebSocket 엔드포인트 |
| `VsMessenger.Application` | 유스케이스와 서비스 로직 |
| `VsMessenger.Domain` | 계정, 세션, 대화, 메시지 등 핵심 도메인 모델 |
| `VsMessenger.Infrastructure` | DB, 토큰, 시계, 실시간 연결 허브 등 인프라 구현 |
| `release-assets` | 릴리즈 메타데이터, 체크섬, 스크린샷 번들 |
| `deploy` | VPS용 Compose, Caddy, systemd, Dockerfile 초안 |

## 데이터 흐름

### 가입

1. 데스크톱 앱이 이름 + 초대코드를 보냅니다.
2. API가 초대코드를 검증하고 계정/세션을 생성합니다.
3. 앱은 반환된 세션을 저장하고 부트스트랩 데이터를 요청합니다.

### 메시지 전송

1. 사용자가 텍스트를 입력합니다.
2. 앱이 REST API로 메시지를 전송합니다.
3. API가 메시지를 저장하고 관련 사용자에게 WebSocket 이벤트를 보냅니다.
4. 앱은 읽기 상태와 목록을 갱신합니다.

## 현재 구조와 목표 구조

| 구분 | 현재 실행 구조 | 목표 배포 구조 |
|---|---|---|
| 클라이언트 | Avalonia 12 desktop | Windows x64 portable / 향후 설치형 |
| API 저장소 | SQLite | PostgreSQL |
| 실시간 | API 내 WebSocket | API + Redis 기반 팬아웃 보조 |
| 파일 저장 | 미구현 | MinIO |
| 리버스 프록시 | 로컬 직접 포트 | Caddy |
| 운영 환경 | 로컬/WSL 중심 | Rocky Linux VPS |

## 보안 경계

- 데스크톱 앱은 세션과 사용자 데이터를 OS 환경에 맞게 최소한으로 저장해야 합니다.
- API는 메시지 본문과 민감 정보를 로그에 남기지 않아야 합니다.
- 실사용 배포 전에는 `root + 비밀번호 SSH` 상태의 VPS를 그대로 사용하지 않습니다.
- 공개 다운로드 채널은 TLS와 체크섬 검증을 전제로 합니다.

보안 상세 정책은 [SECURITY.md](SECURITY.md)와 [문서/05-security-privacy-and-risk.md](문서/05-security-privacy-and-risk.md)를 참고하세요.

## 기술 선택 이유

- `Avalonia 12`: 현재 워크스페이스에서 빠르게 데스크톱 UI를 반복하고 Windows portable 산출물을 만들기 쉬움
- `.NET 8`: API와 데스크톱 모두에서 일관된 개발/배포 흐름 확보
- `ASP.NET Core 8`: REST + WebSocket 수직 슬라이스를 빠르게 구성 가능
- `SQLite`: Alpha 단계에서 복잡도와 운영 부담을 낮춤
- `PostgreSQL / Redis / MinIO`: VPS 운영 단계에서 필요한 데이터/캐시/파일 분리를 준비

## 관련 문서

- 제품 전략: [문서/01-product-strategy-and-mvp.md](문서/01-product-strategy-and-mvp.md)
- Windows 앱 구조: [문서/03-windows-client-architecture.md](문서/03-windows-client-architecture.md)
- 서버/VPS 구조: [문서/04-chat-server-vps-architecture.md](문서/04-chat-server-vps-architecture.md)
- API 계약: [문서/13-v0.1-api-and-events-contract.md](문서/13-v0.1-api-and-events-contract.md)
