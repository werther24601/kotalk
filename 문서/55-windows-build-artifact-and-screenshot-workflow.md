# Windows Build Artifact And Screenshot Workflow

## 목적

Windows 채널은 실제 산출물과 스크린샷이 꾸준히 남아야 신뢰를 만든다.  
이 문서는 빌드 버전 관리, 목업/실제 스크린샷, 공개 링크 운영 기준을 정리한다.

## 기본 원칙

- 버전별 빌드 보존
- latest 링크 별도 유지
- 스크린샷은 가능한 실제 빌드 기준
- 목업일 경우 명시

## 저장 구조

- `releases/windows/<version>/`
- `docs/assets/latest/`
- 원격 Releases

## 완료 기준

- 사용자가 과거 버전과 최신 버전을 혼동하지 않도록, 산출물과 시각 자산이 함께 관리되어야 한다.
