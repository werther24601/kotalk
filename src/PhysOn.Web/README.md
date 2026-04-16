# vstalk Web

`src/PhysOn.Web`는 `vstalk.phy.kr`에 배포할 모바일 퍼스트 웹앱 채널입니다.

현재 범위:
- 이름 + 초대코드 기반 초간단 가입
- 최근 대화 목록
- 대화 진입과 메시지 읽기
- 텍스트 메시지 전송
- 모바일 브라우저용 PWA 메타데이터

개발 명령:

```bash
npm install
npm run dev
```

기본 개발 프록시:
- 웹앱: `http://127.0.0.1:4173`
- API 프록시 대상: `http://127.0.0.1:5082`

환경 변수:
- `VITE_API_BASE_URL`
- `VITE_DEV_PROXY_TARGET`

프로덕션 배포는 루트 문서의 [deploy/README.md](../../deploy/README.md)와 [문서/17-vstalk-webapp-mvp-and-rollout-plan.md](../../문서/17-vstalk-webapp-mvp-and-rollout-plan.md)를 따른다.
