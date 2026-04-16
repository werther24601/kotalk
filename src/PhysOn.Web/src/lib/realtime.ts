import type {
  MessageItemDto,
  ReadCursorUpdatedDto,
  RealtimeEventEnvelope,
  SessionConnectedDto,
} from '../types'

export type RealtimeEvent =
  | { kind: 'session.connected'; payload: SessionConnectedDto }
  | { kind: 'message.created'; payload: MessageItemDto }
  | { kind: 'read_cursor.updated'; payload: ReadCursorUpdatedDto }
  | { kind: 'unknown'; payload: unknown }

export function buildBrowserWsUrl(rawUrl: string, ticket: string): string {
  const url = new URL(rawUrl)
  if (window.location.protocol === 'https:' && url.protocol === 'ws:') {
    url.protocol = 'wss:'
  }
  if (window.location.protocol === 'http:' && url.protocol === 'wss:') {
    url.protocol = 'ws:'
  }
  url.searchParams.set('access_token', ticket)
  return url.toString()
}

export function parseRealtimeEvent(message: string): RealtimeEvent | null {
  let envelope: RealtimeEventEnvelope
  try {
    envelope = JSON.parse(message) as RealtimeEventEnvelope
  } catch {
    return null
  }

  if (!envelope.event) {
    return null
  }

  switch (envelope.event) {
    case 'session.connected':
      return { kind: envelope.event, payload: envelope.data as SessionConnectedDto }
    case 'message.created':
      return { kind: envelope.event, payload: envelope.data as MessageItemDto }
    case 'read_cursor.updated':
      return { kind: envelope.event, payload: envelope.data as ReadCursorUpdatedDto }
    default:
      return { kind: 'unknown', payload: envelope.data }
  }
}
