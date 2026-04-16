# Security Response

이 문서는 보안 이슈 제보를 받았을 때의 기본 대응 방식을 설명합니다.

## Contact

- 보안 제보: [ian@physia.kr](mailto:ian@physia.kr)
- 운영 연계: [contact@physia.kr](mailto:contact@physia.kr)

공개 이슈에는 취약점 세부 내용을 남기지 않는 것을 권장합니다.

## Response Targets

| Step | Target |
|---|---|
| Receipt acknowledgement | 영업일 기준 3일 이내 |
| Initial triage | 영업일 기준 5일 이내 |
| Severity update | 재현 여부 확인 후 가능한 한 빠르게 |
| Fix or mitigation guidance | 심각도와 재현 범위에 따라 별도 판단 |

## Severity Guide

- `Critical`: 계정 탈취, 원격 실행, 대규모 데이터 노출
- `High`: 인증 우회, 장기 세션 탈취, 권한 상승, 릴리즈 변조
- `Medium`: 제한적 정보 노출, 우회 가능한 보호장치
- `Low`: 설명 부족, 완화 가능한 설정 실수
