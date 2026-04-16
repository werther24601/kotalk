# Business Model

KoTalk는 오픈소스 코어를 유지하면서, 운영 서비스와 지원을 별도 가치로 만드는 방식을 지향합니다.

## Core Position

- 저장소 코드는 읽고, 수정하고, 자체 배포할 수 있어야 합니다.
- 공식 운영 서비스는 호스팅, 운영 책임, 배포 지원, 조직 기능, 보안·감사성 같은 영역에서 가치를 만들어야 합니다.
- 공개 저장소와 운영 서비스는 같은 문장으로 뭉개지지 않게 설명합니다.

## Why This Structure

메신저는 기능만으로 끝나지 않습니다. 실제 도입에서는 운영 책임, 업데이트 경로, 지원, 보안 설명 가능성도 중요합니다.
KoTalk는 이 차이를 숨기지 않고, 코어와 서비스 경계를 분리해 설명하는 방식을 택합니다.

## Three Surfaces

| Surface | Meaning |
|---|---|
| Open-source core | 코드, 문서, 기본 배포 골격, 자체 호스팅 가능한 경로 |
| Official service | PHYSIA가 운영하는 서비스와 다운로드 미러 |
| Support and deployment packages | 운영 지원, 규제 환경 대응, 설치·운영 지원 |

## What The Official Service Should Add

- 관리형 배포와 업데이트 운영
- 운영 책임과 장애 대응
- 조직용 정책과 감사성
- 도입 및 전환 지원

## What KoTalk Should Avoid

- 코어 기능을 의도적으로 훼손해 유료 전환을 강제하는 구조
- 구현보다 앞서는 과장된 문구
- 검증 전 항목을 “즉시 도입 가능”처럼 말하는 방식

라이선스와 상표 경계는 [LICENSE-FAQ.md](LICENSE-FAQ.md), [TRADEMARKS.md](TRADEMARKS.md)를 참고하세요.
