# Security Policy

KoTalk는 알파 단계의 오픈소스 프로젝트입니다.
완전무결함을 약속하지는 않지만, 공개 문서에서는 현재 통제와 남은 과제를 구분해 설명하는 방식을 유지합니다.

## Current Principles

- 기본 비밀값과 임시 접근값은 공개 문서에 실어두지 않습니다.
- 메시지 본문과 민감정보를 로그 기본값으로 남기지 않습니다.
- 세션과 인증 경로는 짧은 수명, 회전, 원격 폐기를 전제로 설계합니다.
- 공식 릴리즈 경로와 무결성 정보는 함께 제공합니다.

## What This Policy Covers

- 취약점 제보 경로
- 현재 적용 중인 기본 통제
- 아직 남아 있는 보강 과제

자세한 신뢰 표면은 [TRUST_CENTER.md](TRUST_CENTER.md), 제보 처리 방식은 [SECURITY_RESPONSE.md](SECURITY_RESPONSE.md)에서 확인할 수 있습니다.

## Reporting A Vulnerability

보안 이슈는 공개 이슈 대신 아래 경로로 먼저 알려 주세요.

- [ian@physia.kr](mailto:ian@physia.kr)
- [contact@physia.kr](mailto:contact@physia.kr)

가능하면 아래 정보를 포함해 주세요.

- 영향 범위
- 재현 절차
- 예상 시나리오
- 임시 완화책

## Current Areas Still In Progress

- 브라우저 세션 저장 경계 강화
- 취약점 advisory 공개 체계
- 키 회전, 백업/복구, 공급망 보안 증빙
- 규제 환경용 설치/운영 검증
