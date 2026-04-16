import type { StoredSession } from '../types'

const SESSION_KEY = 'vs-talk.session'
const INSTALL_ID_KEY = 'vs-talk.install-id'
const INVITE_CODE_KEY = 'vs-talk.invite-code'
const DRAFTS_KEY = 'vs-talk.drafts'
const FOLLOW_UP_KEY = 'vs-talk.follow-up'
const RECENT_CONVERSATIONS_KEY = 'vs-talk.recent-conversations'
const RECENT_SEARCHES_KEY = 'vs-talk.recent-searches'

function fallbackRandomId(): string {
  return `web-${Date.now()}-${Math.random().toString(16).slice(2, 10)}`
}

export function getInstallId(): string {
  const existing = window.localStorage.getItem(INSTALL_ID_KEY)
  if (existing) {
    return existing
  }

  const next = typeof crypto.randomUUID === 'function' ? crypto.randomUUID() : fallbackRandomId()
  window.localStorage.setItem(INSTALL_ID_KEY, next)
  return next
}

export function readStoredSession(): StoredSession | null {
  const raw = window.sessionStorage.getItem(SESSION_KEY) ?? window.localStorage.getItem(SESSION_KEY)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as StoredSession
    if (window.localStorage.getItem(SESSION_KEY)) {
      window.localStorage.removeItem(SESSION_KEY)
      window.sessionStorage.setItem(SESSION_KEY, raw)
    }
    return parsed
  } catch {
    window.sessionStorage.removeItem(SESSION_KEY)
    window.localStorage.removeItem(SESSION_KEY)
    return null
  }
}

export function writeStoredSession(session: StoredSession): void {
  window.sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
  window.localStorage.removeItem(SESSION_KEY)
}

export function clearStoredSession(): void {
  window.sessionStorage.removeItem(SESSION_KEY)
  window.localStorage.removeItem(SESSION_KEY)
}

export function readSavedInviteCode(): string {
  return window.localStorage.getItem(INVITE_CODE_KEY) ?? ''
}

export function writeSavedInviteCode(value: string): void {
  window.localStorage.setItem(INVITE_CODE_KEY, value)
}

export function clearSavedInviteCode(): void {
  window.localStorage.removeItem(INVITE_CODE_KEY)
}

function readDraftMap(): Record<string, string> {
  const raw = window.localStorage.getItem(DRAFTS_KEY)
  if (!raw) {
    return {}
  }

  try {
    return JSON.parse(raw) as Record<string, string>
  } catch {
    window.localStorage.removeItem(DRAFTS_KEY)
    return {}
  }
}

export function readConversationDraft(conversationId: string): string {
  return readDraftMap()[conversationId] ?? ''
}

export function writeConversationDraft(conversationId: string, value: string): void {
  const next = readDraftMap()
  if (value.trim()) {
    next[conversationId] = value
  } else {
    delete next[conversationId]
  }

  window.localStorage.setItem(DRAFTS_KEY, JSON.stringify(next))
}

export function clearConversationDraft(conversationId: string): void {
  const next = readDraftMap()
  delete next[conversationId]
  window.localStorage.setItem(DRAFTS_KEY, JSON.stringify(next))
}

export function clearConversationDrafts(): void {
  window.localStorage.removeItem(DRAFTS_KEY)
}

function readFollowUpMap(): Record<string, string> {
  const raw = window.localStorage.getItem(FOLLOW_UP_KEY)
  if (!raw) {
    return {}
  }

  try {
    return JSON.parse(raw) as Record<string, string>
  } catch {
    window.localStorage.removeItem(FOLLOW_UP_KEY)
    return {}
  }
}

export function readConversationFollowUps(): Record<string, string> {
  return readFollowUpMap()
}

export function writeConversationFollowUp(conversationId: string, bucket: string | null): void {
  const next = readFollowUpMap()
  if (bucket) {
    next[conversationId] = bucket
  } else {
    delete next[conversationId]
  }

  window.localStorage.setItem(FOLLOW_UP_KEY, JSON.stringify(next))
}

export function clearConversationFollowUps(): void {
  window.localStorage.removeItem(FOLLOW_UP_KEY)
}

export function readRecentConversationIds(): string[] {
  const raw = window.localStorage.getItem(RECENT_CONVERSATIONS_KEY)
  if (!raw) {
    return []
  }

  try {
    const parsed = JSON.parse(raw) as string[]
    return Array.isArray(parsed) ? parsed.filter((item) => typeof item === 'string') : []
  } catch {
    window.localStorage.removeItem(RECENT_CONVERSATIONS_KEY)
    return []
  }
}

export function pushRecentConversationId(conversationId: string, limit = 6): string[] {
  const next = [conversationId, ...readRecentConversationIds().filter((item) => item !== conversationId)].slice(0, limit)
  window.localStorage.setItem(RECENT_CONVERSATIONS_KEY, JSON.stringify(next))
  return next
}

export function clearRecentConversationIds(): void {
  window.localStorage.removeItem(RECENT_CONVERSATIONS_KEY)
}

export function readRecentSearchQueries(): string[] {
  const raw = window.localStorage.getItem(RECENT_SEARCHES_KEY)
  if (!raw) {
    return []
  }

  try {
    const parsed = JSON.parse(raw) as string[]
    return Array.isArray(parsed)
      ? parsed.filter((item) => typeof item === 'string' && item.trim().length > 0)
      : []
  } catch {
    window.localStorage.removeItem(RECENT_SEARCHES_KEY)
    return []
  }
}

export function pushRecentSearchQuery(value: string, limit = 6): string[] {
  const trimmed = value.trim()
  if (!trimmed) {
    return readRecentSearchQueries()
  }

  const next = [trimmed, ...readRecentSearchQueries().filter((item) => item !== trimmed)].slice(0, limit)
  window.localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(next))
  return next
}

export function clearRecentSearchQueries(): void {
  window.localStorage.removeItem(RECENT_SEARCHES_KEY)
}
