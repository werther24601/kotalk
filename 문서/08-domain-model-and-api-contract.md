# 08. Domain Model And API Contract

이 문서는 도메인 범위와 전체 계약 초안을 담는다.

실제 `v0.1` 구현에 바로 쓰는 최소 계약은 아래 문서를 기준으로 한다.
- [13-v0.1-api-and-events-contract.md](13-v0.1-api-and-events-contract.md)

## 핵심 도메인 엔티티

### User

- `user_id`
- `display_name`
- `created_at`
- `status`

### Profile

- `user_id`
- `profile_image`
- `status_message`
- `locale`

### Device

- `device_id`
- `user_id`
- `device_name`
- `device_public_key`
- `trust_state`

### DeviceSession

- `session_id`
- `user_id`
- `device_id`
- `refresh_token_hash`
- `token_family_id`
- `last_seen_at`
- `expires_at`

### Invite

- `invite_token`
- `issued_by_user_id`
- `target_scope`
- `expires_at`
- `max_uses`
- `used_count`

### AuthMethod

- `auth_method_id`
- `user_id`
- `type`
- `value_hash_or_ref`
- `verified_at`

### Conversation

- `conversation_id`
- `type` (`self`, `dm`, `group`)
- `title`
- `created_by`
- `created_at`

### ConversationMember

- `conversation_id`
- `user_id`
- `role`
- `mute`
- `pin_order`
- `joined_at`

### Message

- `message_id`
- `conversation_id`
- `sender_user_id`
- `client_request_id`
- `server_sequence`
- `message_type`
- `body`
- `created_at`
- `edited_at`

### Attachment

- `attachment_id`
- `message_id`
- `object_key`
- `mime_type`
- `byte_size`
- `checksum`

### ReadCursor

- `conversation_id`
- `user_id`
- `last_read_sequence`
- `updated_at`

## REST 초안

### Alpha Auth

- `POST /v1/auth/device/bootstrap`
- `POST /v1/auth/register/alpha-quick`
- `POST /v1/auth/token/refresh`
- `POST /v1/auth/logout`
- `POST /v1/auth/logout-all`

### Beta Auth

- `POST /v1/auth/email/start`
- `POST /v1/auth/email/verify`
- `POST /v1/auth/link-email/request`
- `POST /v1/auth/link-email/verify`
- `POST /v1/auth/recovery-codes/issue`

### Session

- `GET /v1/me`
- `GET /v1/me/devices`
- `DELETE /v1/me/devices/{deviceId}`

### Invites

- `POST /v1/invites`
- `GET /v1/invites/{token}/preview`
- `POST /v1/invites/{token}/accept`

### Conversations

- `GET /v1/conversations`
- `POST /v1/conversations`
- `GET /v1/conversations/{id}`
- `GET /v1/conversations/{id}/messages?cursor=...`
- `POST /v1/conversations/{id}/members`

### Messages

- `POST /v1/conversations/{id}/messages`
- `PATCH /v1/messages/{id}`
- `DELETE /v1/messages/{id}`
- `POST /v1/messages/{id}/read`

### Attachments

- `POST /v1/attachments/presign`
- `POST /v1/attachments/complete`
- `GET /v1/files/{file_id}`

### Search

- `GET /v1/search?q=...`

## WSS 이벤트 초안

### Client -> Server

- `auth.connect`
- `session.resume`
- `message.send`
- `message.read`
- `typing.start`
- `typing.stop`
- `presence.update`
- `conversation.join`
- `conversation.leave`

### Server -> Client

- `auth.connected`
- `session.resumed`
- `session.invalidated`
- `account.created`
- `invite.accepted`
- `contact.added`
- `conversation.created`
- `sync.bootstrap`
- `message.created`
- `message.updated`
- `message.deleted`
- `message.read_updated`
- `presence.changed`
- `typing.changed`
- `conversation.updated`
- `sync.required`
- `error`

## 메시지 상태 머신

- `draft`
- `queued`
- `sending`
- `sent`
- `delivered`
- `read`
- `failed`

재시도는 `failed -> queued -> sending`으로만 허용한다.

## 동기화 원칙

- 모든 메시지는 `client_request_id`로 멱등 처리
- 서버는 `server_sequence`를 기준으로 정렬 책임
- 클라이언트는 대화방별 `last_synced_sequence` 저장
- 재연결 후 `sync.required`를 받으면 증분 동기화 수행

## 가입 단계별 계약

### Alpha 즉시 실행형

- 입력: `display_name`, `invite_token`
- 서버 처리: 계정, 프로필, 디바이스, 세션, 기본 대화 생성
- 결과: 바로 메인 진입

### Beta 기본형

- 입력: `email`
- 서버 처리: 링크 + 코드 발송
- 다음 입력: `verification_code` 또는 `magic_link`
- 결과: 계정/기기 세션 생성 후 메인 진입

## MVP 화면별 Definition Of Done

### 가입

- Alpha: 이름 + 초대코드만으로 진입 가능
- Beta: 이메일 1회 확인으로 진입 가능
- 세션 저장
- 재실행 후 자동 로그인
- 가입 직후 빈 화면 금지

### 대화 목록

- 최근 순 정렬
- 읽지 않음 표시
- 고정/음소거 반영
- 로컬 캐시 우선 렌더링

### 대화창

- 송수신
- 읽음 반영
- 실패 시 재시도
- 새 메시지 배너
- 긴 대화 스크롤 안정성

### 첨부

- 업로드
- 진행 상태
- 실패 복구
- 다운로드

## 구현 우선순위 백로그

1. Alpha quick register
2. Conversation list
3. Message send/receive
4. Local cache
5. Read cursor
6. Reconnect/sync
7. Attachment pipeline
8. Search
9. Beta email verify
10. Session management
11. Admin/report tools
