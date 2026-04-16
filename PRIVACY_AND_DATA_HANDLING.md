# Privacy And Data Handling

이 문서는 현재 구현 기준에서 어떤 데이터가 어떤 경계에 놓이는지 설명합니다.

## Current Data Categories

| Category | Examples | Current purpose |
|---|---|---|
| Account identity | 표시 이름, 사용자 ID | 계정 식별 |
| Device identity | install ID, device name, app version | 세션 구분 |
| Session data | session ID, token family, refresh token hash | 인증과 세션 회전 |
| Conversation data | 대화 제목, 멤버, 메시지 본문, 읽기 커서 | 메시징 기능 |
| Operational logs | 오류, 상태, 제한적 진단 정보 | 장애 분석과 보안 대응 |

## Handling Principles

- refresh token 원문은 서버 저장소에 평문으로 남기지 않습니다.
- 메시지 본문과 민감정보는 로그 기본값으로 남기지 않습니다.
- 운영자가 접근할 수 있는 데이터 범위는 최소화하는 방향으로 설계합니다.

## Current Gaps

- 정식 개인정보 처리방침 수준의 법률 문서화는 아직 아닙니다.
- 보존 기간, 삭제 절차, 접근 감사는 더 구체화가 필요합니다.

문의: [help@physia.kr](mailto:help@physia.kr)
