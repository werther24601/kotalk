# Repository Surfaces

이 문서는 공개 저장소에서 무엇을 어떤 이름으로 노출하는지 정리합니다.

## Public Naming

- 제품 노출명: `KoTalk`
- 한글 표기: `코톡`
- 웹 진입 도메인: `vstalk.phy.kr`
- 다운로드 미러: `download-vstalk.phy.kr`

## Repository Structure

- `src/`, `tests/`: 제품 코드와 검증 코드
- `docs/`: 공개 보조 문서와 시각 자산
- `문서/`: 제품 마스터 플랜과 UX 아틀라스
- `deploy/`: 범용 배포 골격
- `release-assets/`: 릴리즈 메타데이터와 배포 자산 스테이징

## Public Writing Rules

- README는 첫 방문자가 30초 안에 판단할 수 있게 유지합니다.
- 공개 문서에는 실제 운영 힌트, 비밀값, 내부 메모를 적지 않습니다.
- 공식 서비스와 오픈소스 저장소는 같은 표면처럼 쓰지 않습니다.

## Current Technical Note

현재 저장소의 코드 네임스페이스와 프로젝트 파일은 아직 `PhysOn.*`를 사용합니다.
이 문서는 공개 브랜드 기준을 먼저 정리한 것이며, 소스 네임스페이스 정렬은 별도 작업입니다.
