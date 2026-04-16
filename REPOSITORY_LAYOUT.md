# Repository Layout

이 저장소는 코드, 공개 보조 문서, 상세 기획 문서를 역할별로 나눠 관리합니다.

## Top Level

- `src/`: 제품 코드
- `tests/`: 자동 테스트
- `docs/`: 공개 저장소 보조 문서와 시각 자산
- `문서/`: 제품 마스터 플랜과 UX 아틀라스
- `deploy/`: 범용 배포 골격
- `release-assets/`: 릴리즈 메타데이터와 스테이징 자산
- `scripts/release/`: 릴리즈 준비와 게시 스크립트
- `scripts/deploy/`: 서버와 웹앱 배포 스크립트
- `scripts/ci/`: 캡처와 검증 보조 스크립트
- `.workspace-*`: 워크스페이스 전용 비공개 정책과 시크릿

## Public Naming

- 제품 노출명: `KoTalk`
- 한글 표기: `코톡`
- 웹 진입점: `vstalk.phy.kr`
- 다운로드 미러: `download-vstalk.phy.kr`

## Technical Note

현재 코드 네임스페이스와 프로젝트 파일은 여전히 `PhysOn.*`를 사용합니다.
공개 브랜드와 소스 네이밍은 단계적으로 정렬합니다.

## Documentation Roles

- `docs/assets/`: README와 공개 표면에 직접 쓰는 이미지와 SVG
- `docs/archive/`: 보관용 초안
- `docs/repository-surfaces.md`: 공개 표면 규칙
- `문서/`: 제품 전략, UX 기준, 실행 계획

## House Rules

- 공개 문서에는 비밀값, 운영 힌트, 내부 메모를 남기지 않습니다.
- 공개 자산과 보관용 초안은 같은 폴더에 섞지 않습니다.
- 공식 스크립트 경로는 `scripts/release`, `scripts/deploy`, `scripts/ci`입니다.
