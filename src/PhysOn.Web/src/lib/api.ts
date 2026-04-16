import type {
  ApiEnvelope,
  ApiErrorEnvelope,
  BootstrapResponse,
  ListEnvelope,
  MessageItemDto,
  ReadCursorUpdatedDto,
  RefreshTokenRequest,
  RefreshTokenResponse,
  RegisterAlphaQuickRequest,
  RegisterAlphaQuickResponse,
  UpdateReadCursorRequest,
} from '../types'

export class ApiRequestError extends Error {
  readonly code?: string
  readonly status?: number

  constructor(message: string, code?: string, status?: number) {
    super(message)
    this.name = 'ApiRequestError'
    this.code = code
    this.status = status
  }
}

function resolveErrorMessage(status: number, code?: string, fallback?: string): string {
  if (status === 401) {
    return '연결이 잠시 만료되었습니다. 다시 이어서 들어와 주세요.'
  }

  if (status === 403) {
    return '이 화면은 아직 사용할 수 없습니다. 초대 상태를 확인해 주세요.'
  }

  if (status === 404) {
    return '대화 정보를 다시 불러오는 중입니다. 잠시 후 다시 시도해 주세요.'
  }

  if (status === 429) {
    return '요청이 많습니다. 잠시 후 다시 시도해 주세요.'
  }

  if (status >= 500) {
    return '지금은 연결이 고르지 않습니다. 잠시 후 다시 시도해 주세요.'
  }

  if (code === 'invite_code_invalid' || code === 'invite_invalid') {
    return '초대코드를 다시 확인해 주세요.'
  }

  return fallback ?? '요청을 처리하지 못했습니다. 잠시 후 다시 시도해 주세요.'
}

function ensureTrailingSlash(value: string): string {
  if (!value) {
    return `${window.location.origin}/`
  }

  return value.endsWith('/') ? value : `${value}/`
}

function buildUrl(apiBaseUrl: string, path: string): string {
  return new URL(path.replace(/^\//, ''), ensureTrailingSlash(apiBaseUrl)).toString()
}

function parsePayload<T>(text: string): ApiEnvelope<T> | ApiErrorEnvelope | null {
  if (!text) {
    return null
  }

  try {
    return JSON.parse(text) as ApiEnvelope<T> | ApiErrorEnvelope
  } catch {
    return null
  }
}

async function request<T>(
  apiBaseUrl: string,
  path: string,
  init: RequestInit,
  accessToken?: string,
): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')

  if (init.body) {
    headers.set('Content-Type', 'application/json')
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  let response: Response
  try {
    response = await fetch(buildUrl(apiBaseUrl, path), {
      ...init,
      headers,
    })
  } catch {
    throw new ApiRequestError('네트워크 연결을 확인한 뒤 다시 시도해 주세요.')
  }

  const text = await response.text()
  const payload = parsePayload<T>(text)

  if (!response.ok) {
    const error = (payload as ApiErrorEnvelope | null)?.error
    throw new ApiRequestError(
      resolveErrorMessage(response.status, error?.code, error?.message ?? undefined),
      error?.code,
      response.status,
    )
  }

  if (!payload || !('data' in payload)) {
    throw new ApiRequestError('응답을 다시 확인하는 중입니다. 잠시 후 다시 시도해 주세요.')
  }

  return payload.data
}

export function registerAlphaQuick(
  apiBaseUrl: string,
  body: RegisterAlphaQuickRequest,
): Promise<RegisterAlphaQuickResponse> {
  return request<RegisterAlphaQuickResponse>(apiBaseUrl, '/v1/auth/register/alpha-quick', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function refreshToken(
  apiBaseUrl: string,
  body: RefreshTokenRequest,
): Promise<RefreshTokenResponse> {
  return request<RefreshTokenResponse>(apiBaseUrl, '/v1/auth/token/refresh', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function getBootstrap(apiBaseUrl: string, accessToken: string): Promise<BootstrapResponse> {
  return request<BootstrapResponse>(apiBaseUrl, '/v1/bootstrap', { method: 'GET' }, accessToken)
}

export function getMessages(
  apiBaseUrl: string,
  accessToken: string,
  conversationId: string,
  beforeSequence?: number,
): Promise<ListEnvelope<MessageItemDto>> {
  const query = new URLSearchParams()
  if (beforeSequence) {
    query.set('beforeSequence', String(beforeSequence))
  }
  query.set('limit', '60')

  const suffix = query.toString()
  return request<ListEnvelope<MessageItemDto>>(
    apiBaseUrl,
    `/v1/conversations/${conversationId}/messages${suffix ? `?${suffix}` : ''}`,
    { method: 'GET' },
    accessToken,
  )
}

export function sendTextMessage(
  apiBaseUrl: string,
  accessToken: string,
  conversationId: string,
  body: { clientRequestId: string; body: string },
): Promise<MessageItemDto> {
  return request<MessageItemDto>(
    apiBaseUrl,
    `/v1/conversations/${conversationId}/messages`,
    {
      method: 'POST',
      body: JSON.stringify(body),
    },
    accessToken,
  )
}

export function updateReadCursor(
  apiBaseUrl: string,
  accessToken: string,
  conversationId: string,
  body: UpdateReadCursorRequest,
): Promise<ReadCursorUpdatedDto> {
  return request<ReadCursorUpdatedDto>(
    apiBaseUrl,
    `/v1/conversations/${conversationId}/read-cursor`,
    {
      method: 'POST',
      body: JSON.stringify(body),
    },
    accessToken,
  )
}
