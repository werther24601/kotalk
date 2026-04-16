# 릴리즈와 운영 신뢰

이 묶음은 릴리즈와 운영 신뢰 관점에서 사용자 체감 간편함을 세부 주제로 분해한 아틀라스다.

## 범위
- 주요 목적: 산출물, 다운로드, 관찰성, 복구를 하나의 공개 표면으로 묶는 구조
- 우선 사용자: 운영자 + 메인테이너
- 우선 채널: Windows + Web + Android + VPS

## 현재 산출물에 대한 비판적 요약
- 현재 릴리즈 표면은 정리 중이며, 다운로드 호스트와 원격 Releases의 정합성은 더 강화해야 한다.
- 문서상 원칙은 충분하지만 자동화와 무결성 검증, 공개 상태 표면은 아직 진행형이다.

## 문서 목록
- [버전 매니페스트](./01-version-manifest.md)
- [Windows 릴리즈 파이프라인](./02-windows-release-pipeline.md)
- [웹 릴리즈 파이프라인](./03-web-release-pipeline.md)
- [Android 릴리즈 파이프라인](./04-android-release-pipeline.md)
- [다운로드 호스트 라우팅](./05-download-host-routing.md)
- [릴리즈 노트 스타일](./06-release-notes-style.md)
- [스크린샷 규율](./07-screenshot-discipline.md)
- [체크섬 서명](./08-checksum-signature.md)
- [롤백 플레이북](./09-rollback-playbook.md)
- [관찰성 표면](./10-observability-surface.md)
- [백업 복구 훈련](./11-backup-drills.md)
- [경보 임계값](./12-alert-thresholds.md)
- [비밀값 취급](./13-secret-handling.md)
- [공개 상태 페이지](./14-public-status-page.md)
- [릴리즈 정직성](./15-release-truthfulness.md)
- [산출물 보존](./16-artifact-retention.md)
- [Gitea 릴리즈 플로우](./17-gitea-release-flow.md)
- [OS별 다운로드 라우트](./18-os-route-design.md)
- [릴리즈 회귀 추적](./19-release-regression.md)
- [운영 준비도 게이트](./20-ops-readiness-gate.md)
