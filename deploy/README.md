# Deployment Guide

이 디렉터리는 KoTalk의 범용 배포 골격을 담습니다.

## Public Endpoints

- 모바일 웹: [vstalk.phy.kr](https://vstalk.phy.kr)
- 공식 다운로드 미러: [download-vstalk.phy.kr](https://download-vstalk.phy.kr)
- 버전 메타데이터: [download-vstalk.phy.kr/latest/version.json](https://download-vstalk.phy.kr/latest/version.json)

## Intended Shape

- `Caddyfile`: 웹 진입점, 다운로드 미러, API 프록시
- `compose*.yml`: API, 정적 웹, 보조 서비스 구성
- `docker/`: 이미지 빌드 정의

## Public Rules

- 실서비스 호스트 주소, 관리자 계정, 비밀값은 공개 문서에 적지 않습니다.
- 운영 중인 컨테이너명과 네트워크명은 공개 표면의 필수 정보가 아닙니다.
- 배포 예시는 범용 구조 중심으로 유지합니다.

## Download Layout

- `/windows/latest`
- `/android/latest`
- `/latest/version.json`

실제 공개 릴리즈 경로는 [RELEASING.md](../RELEASING.md)와 함께 봐야 합니다.
