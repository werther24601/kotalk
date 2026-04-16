export interface ApiEnvelope<T> {
  data: T
}

export interface ApiError {
  code: string
  message: string
  retryable?: boolean
  fieldErrors?: Record<string, string>
}

export interface ApiErrorEnvelope {
  error: ApiError
}

export interface ListEnvelope<T> {
  items: T[]
  nextCursor: string | null
}

export interface MeDto {
  userId: string
  displayName: string
  profileImageUrl: string | null
  statusMessage: string | null
}

export interface SessionDto {
  sessionId: string
  deviceId: string
  deviceName: string
  createdAt: string
}

export interface TokenPairDto {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export interface BootstrapWsDto {
  url: string
  ticket: string
  ticketExpiresAt: string
}

export interface MessagePreviewDto {
  messageId: string
  text: string
  createdAt: string
  senderUserId: string
}

export interface ConversationSummaryDto {
  conversationId: string
  type: string
  title: string
  avatarUrl: string | null
  subtitle: string
  memberCount: number
  isMuted: boolean
  isPinned: boolean
  sortKey: string
  unreadCount: number
  lastReadSequence: number
  lastMessage: MessagePreviewDto | null
}

export interface MessageSenderDto {
  userId: string
  displayName: string
  profileImageUrl: string | null
}

export interface MessageItemDto {
  messageId: string
  conversationId: string
  clientMessageId: string
  kind: string
  text: string
  createdAt: string
  editedAt: string | null
  sender: MessageSenderDto
  isMine: boolean
  serverSequence: number
}

export interface BootstrapResponse {
  me: MeDto
  session: SessionDto
  ws: BootstrapWsDto
  conversations: ListEnvelope<ConversationSummaryDto>
}

export interface RegisterAlphaQuickResponse {
  account: MeDto
  session: SessionDto
  tokens: TokenPairDto
  bootstrap: BootstrapResponse
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface RefreshTokenResponse {
  tokens: TokenPairDto
}

export interface DeviceRegistrationDto {
  installId: string
  platform: string
  deviceName: string
  appVersion: string
}

export interface RegisterAlphaQuickRequest {
  displayName: string
  inviteCode: string
  device: DeviceRegistrationDto
}

export interface PostTextMessageRequest {
  clientRequestId: string
  body: string
}

export interface UpdateReadCursorRequest {
  lastReadSequence: number
}

export interface ReadCursorUpdatedDto {
  conversationId: string
  accountId: string
  lastReadSequence: number
  updatedAt: string
}

export interface StoredSession {
  apiBaseUrl: string
  tokens: TokenPairDto
  bootstrap: BootstrapResponse
  savedAt: string
}

export interface RealtimeEventEnvelope<T = unknown> {
  event: string
  eventId: string
  occurredAt: string
  data: T
}

export interface SessionConnectedDto {
  sessionId: string
}
