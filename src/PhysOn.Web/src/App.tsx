import { FormEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react'
import './App.css'
import { ApiRequestError, getBootstrap, getMessages, refreshToken, registerAlphaQuick, sendTextMessage, updateReadCursor } from './lib/api'
import { buildBrowserWsUrl, parseRealtimeEvent } from './lib/realtime'
import {
  clearConversationDraft,
  clearConversationDrafts,
  clearStoredSession,
  getInstallId,
  readConversationDraft,
  readStoredSession,
  writeConversationDraft,
  writeSavedInviteCode,
  writeStoredSession,
} from './lib/storage'
import type {
  BootstrapResponse,
  ConversationSummaryDto,
  MessageItemDto,
  ReadCursorUpdatedDto,
  RefreshTokenResponse,
  RegisterAlphaQuickResponse,
  StoredSession,
} from './types'

type ConnectionState = 'idle' | 'connecting' | 'connected' | 'fallback'
type ConversationFilter = 'all' | 'unread' | 'pinned'
type MobileView = 'list' | 'chat'
type BottomDestination = 'inbox' | 'search' | 'saved' | 'me'
type SearchResultItem = {
  key: string
  kind: 'conversation' | 'message'
  conversationId: string
  title: string
  excerpt: string
  meta: string
  timestamp: string
}
type IconName =
  | 'mark'
  | 'refresh'
  | 'chat'
  | 'session'
  | 'back'
  | 'send'
  | 'search'
  | 'pin'
  | 'archive'
  | 'me'
  | 'spark'
  | 'check'
  | 'clock'
  | 'group'

const DEFAULT_API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.trim() ?? ''
const APP_VERSION = 'web-0.1.0-alpha.4'

const CONNECTION_LABEL: Record<ConnectionState, string> = {
  idle: '준비 중',
  connecting: '불러오는 중',
  connected: '대화 준비됨',
  fallback: '다시 맞추는 중',
}

const CONNECTION_DESCRIPTION: Record<ConnectionState, string> = {
  idle: '준비',
  connecting: '동기화',
  connected: '연결',
  fallback: '복구',
}

function compareConversations(left: ConversationSummaryDto, right: ConversationSummaryDto): number {
  if (left.isPinned !== right.isPinned) {
    return left.isPinned ? -1 : 1
  }

  return new Date(right.sortKey).getTime() - new Date(left.sortKey).getTime()
}

function sortConversations(items: ConversationSummaryDto[]): ConversationSummaryDto[] {
  return [...items].sort(compareConversations)
}

function dedupeConversationsById(items: ConversationSummaryDto[]): ConversationSummaryDto[] {
  const seen = new Set<string>()
  return items.filter((item) => {
    if (seen.has(item.conversationId)) {
      return false
    }

    seen.add(item.conversationId)
    return true
  })
}

function upsertConversation(
  items: ConversationSummaryDto[],
  nextConversation: ConversationSummaryDto,
): ConversationSummaryDto[] {
  const filtered = items.filter((item) => item.conversationId !== nextConversation.conversationId)
  return sortConversations([nextConversation, ...filtered])
}

function mergeMessages(existing: MessageItemDto[] | undefined, incoming: MessageItemDto[]): MessageItemDto[] {
  const map = new Map<string, MessageItemDto>()
  for (const item of existing ?? []) {
    map.set(item.messageId, item)
  }
  for (const item of incoming) {
    map.set(item.messageId, item)
  }

  return [...map.values()].sort((left, right) => left.serverSequence - right.serverSequence)
}

function buildConversationPreview(conversation: ConversationSummaryDto, message: MessageItemDto): ConversationSummaryDto {
  return {
    ...conversation,
    subtitle: message.text,
    sortKey: message.createdAt,
    lastMessage: {
      messageId: message.messageId,
      text: message.text,
      createdAt: message.createdAt,
      senderUserId: message.sender.userId,
    },
    unreadCount: message.isMine ? 0 : Math.max(conversation.unreadCount + 1, 1),
  }
}

function applyReadCursor(
  conversation: ConversationSummaryDto,
  payload: ReadCursorUpdatedDto | { lastReadSequence: number; unreadCount?: number },
): ConversationSummaryDto {
  return {
    ...conversation,
    lastReadSequence: payload.lastReadSequence,
    unreadCount: 'unreadCount' in payload ? payload.unreadCount ?? 0 : 0,
  }
}

function getConversationInitials(title: string): string {
  const compact = title.trim()
  if (!compact) {
    return 'KO'
  }

  return compact.slice(0, 2).toUpperCase()
}

function formatClock(value: string): string {
  return new Intl.DateTimeFormat('ko-KR', {
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(value))
}

function formatListTime(value: string): string {
  const date = new Date(value)
  const now = new Date()
  const diff = now.getTime() - date.getTime()

  if (diff < 1000 * 60 * 60 * 20 && now.getDate() === date.getDate()) {
    return formatClock(value)
  }

  return new Intl.DateTimeFormat('ko-KR', {
    month: 'numeric',
    day: 'numeric',
  }).format(date)
}

function formatRelativeConnection(savedAt: string | null): string {
  if (!savedAt) {
    return '방금 시작'
  }

  const minutes = Math.max(1, Math.floor((Date.now() - new Date(savedAt).getTime()) / 60000))
  if (minutes < 60) {
    return `${minutes}분 전`
  }

  const hours = Math.floor(minutes / 60)
  return `${hours}시간 전`
}

function getFriendlyStatusMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiRequestError) {
    return error.message
  }

  if (error instanceof Error) {
    if (/401/.test(error.message)) {
      return '연결이 잠시 흔들렸지만 현재 화면과 입력은 그대로 두고 다시 맞추고 있어요.'
    }

    return error.message
  }

  return fallback
}

function isUnauthorizedError(error: unknown): boolean {
  return error instanceof ApiRequestError && error.status === 401
}

function createClientId(): string {
  return typeof crypto.randomUUID === 'function'
    ? crypto.randomUUID()
    : `client-${Date.now()}-${Math.random().toString(16).slice(2, 10)}`
}

function getDefaultDeviceName(): string {
  const platform = /android/i.test(window.navigator.userAgent) ? 'Android' : 'Mobile Web'
  return `${platform} 브라우저`
}

function Icon({ name }: { name: IconName }) {
  switch (name) {
    case 'mark':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M5 6.5a2.5 2.5 0 0 1 2.5-2.5h9A2.5 2.5 0 0 1 19 6.5v6A2.5 2.5 0 0 1 16.5 15H10l-3.5 3v-3H7.5A2.5 2.5 0 0 1 5 12.5z" />
        </svg>
      )
    case 'refresh':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M18.2 8.3A7 7 0 1 0 19 12h-2a5 5 0 1 1-1.3-3.4L13 11h7V4z" />
        </svg>
      )
    case 'chat':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M4 6.5A2.5 2.5 0 0 1 6.5 4h11A2.5 2.5 0 0 1 20 6.5v7a2.5 2.5 0 0 1-2.5 2.5H9l-4 3v-3.4A2.5 2.5 0 0 1 4 13.5z" />
        </svg>
      )
    case 'session':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M12 3a7 7 0 0 0-7 7v2.2A2.8 2.8 0 0 0 3 15v3a3 3 0 0 0 3 3h12a3 3 0 0 0 3-3v-3a2.8 2.8 0 0 0-2-2.8V10a7 7 0 0 0-7-7m0 2a5 5 0 0 1 5 5v2H7v-2a5 5 0 0 1 5-5" />
        </svg>
      )
    case 'back':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="m14.8 5.8 1.4 1.4L11.4 12l4.8 4.8-1.4 1.4L8.6 12z" />
        </svg>
      )
    case 'send':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M4 19.5 20 12 4 4.5v5.2l8.3 2.3L4 14.3z" />
        </svg>
      )
    case 'search':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M10.5 4a6.5 6.5 0 1 0 4.1 11.5l4 4 1.4-1.4-4-4A6.5 6.5 0 0 0 10.5 4m0 2a4.5 4.5 0 1 1 0 9 4.5 4.5 0 0 1 0-9" />
        </svg>
      )
    case 'pin':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="m15.9 4.5 3.6 3.6-2.4 1.2-1.8 4.6-2.4-2.4-4.9 4.9-1.4-1.4 4.9-4.9-2.4-2.4 4.6-1.8z" />
        </svg>
      )
    case 'archive':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M4 5.5A1.5 1.5 0 0 1 5.5 4h13A1.5 1.5 0 0 1 20 5.5v2A1.5 1.5 0 0 1 18.5 9h-13A1.5 1.5 0 0 1 4 7.5zm1 5.5h14v7.5A2.5 2.5 0 0 1 16.5 21h-9A2.5 2.5 0 0 1 5 18.5zm4 2v2h6v-2z" />
        </svg>
      )
    case 'me':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M12 4a4 4 0 1 1 0 8 4 4 0 0 1 0-8m0 10c4.4 0 8 2.2 8 5v1H4v-1c0-2.8 3.6-5 8-5" />
        </svg>
      )
    case 'spark':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="m12 3 1.9 5.1L19 10l-5.1 1.9L12 17l-1.9-5.1L5 10l5.1-1.9zm6.5 11 1 2.5L22 17l-2.5 1-1 2.5-1-2.5-2.5-1 2.5-1zm-13 1 1.2 3L10 19.2l-3.3 1.2L5.5 23l-1.2-2.6L1 19.2 4.3 18z" />
        </svg>
      )
    case 'check':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="m9.6 16.6-4.2-4.2L4 13.8l5.6 5.6L20 9l-1.4-1.4z" />
        </svg>
      )
    case 'clock':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M12 4a8 8 0 1 0 8 8 8 8 0 0 0-8-8m1 4h-2v4.4l3.5 2.1 1-1.7-2.5-1.5z" />
        </svg>
      )
    case 'group':
      return (
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M9 5a3 3 0 1 1 0 6 3 3 0 0 1 0-6m6 1a2.5 2.5 0 1 1 0 5 2.5 2.5 0 0 1 0-5M4 18c0-2.3 2.8-4 5-4s5 1.7 5 4v1H4zm11 1c0-1.6-.8-2.9-2.1-3.8.7-.1 1.4-.2 2.1-.2 1.9 0 5 1 5 3v1z" />
        </svg>
      )
  }
}

function App() {
  const initialSession = useMemo(() => readStoredSession(), [])
  const [storedSession, setStoredSession] = useState<StoredSession | null>(initialSession)
  const [apiBaseUrl, setApiBaseUrl] = useState(initialSession?.apiBaseUrl ?? DEFAULT_API_BASE_URL)
  const [displayName, setDisplayName] = useState(initialSession?.bootstrap.me.displayName ?? '')
  const [inviteCode, setInviteCode] = useState('')
  const [bootstrap, setBootstrap] = useState<BootstrapResponse | null>(initialSession?.bootstrap ?? null)
  const [conversations, setConversations] = useState<ConversationSummaryDto[]>(
    sortConversations(initialSession?.bootstrap.conversations.items ?? []),
  )
  const [messagesByConversation, setMessagesByConversation] = useState<Record<string, MessageItemDto[]>>({})
  const [selectedConversationId, setSelectedConversationId] = useState<string | null>(
    initialSession?.bootstrap.conversations.items[0]?.conversationId ?? null,
  )
  const [mobileView, setMobileView] = useState<MobileView>('list')
  const [bottomDestination, setBottomDestination] = useState<BottomDestination>('inbox')
  const [listFilter, setListFilter] = useState<ConversationFilter>('all')
  const [connectionState, setConnectionState] = useState<ConnectionState>('idle')
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [registering, setRegistering] = useState(false)
  const [bootstrapping, setBootstrapping] = useState(false)
  const [loadingConversationId, setLoadingConversationId] = useState<string | null>(null)
  const [sending, setSending] = useState(false)
  const [composerText, setComposerText] = useState('')
  const [showAdvanced, setShowAdvanced] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [refreshTick, setRefreshTick] = useState(0)
  const storedSessionRef = useRef(storedSession)
  const refreshSessionPromiseRef = useRef<Promise<StoredSession> | null>(null)
  const pendingReadCursorRef = useRef<Record<string, number>>({})
  const pendingAutoScrollRef = useRef(true)
  const messageStreamRef = useRef<HTMLDivElement | null>(null)
  const messageEndRef = useRef<HTMLDivElement | null>(null)
  const searchInputRef = useRef<HTMLInputElement | null>(null)
  const composerFieldRef = useRef<HTMLTextAreaElement | null>(null)

  const accessToken = storedSession?.tokens.accessToken ?? null
  const selectedConversation = conversations.find((item) => item.conversationId === selectedConversationId) ?? null
  const selectedMessages = useMemo(
    () => (selectedConversationId ? messagesByConversation[selectedConversationId] ?? [] : []),
    [messagesByConversation, selectedConversationId],
  )
  const me = bootstrap?.me ?? storedSession?.bootstrap.me ?? null
  const hasSession = Boolean(storedSession && accessToken)
  const unreadTotal = useMemo(
    () => conversations.reduce((sum, conversation) => sum + conversation.unreadCount, 0),
    [conversations],
  )
  const pinnedTotal = useMemo(
    () => conversations.reduce((sum, conversation) => sum + (conversation.isPinned ? 1 : 0), 0),
    [conversations],
  )
  const normalizedSearchQuery = searchQuery.trim()
  const filteredConversations = useMemo(() => {
    return conversations.filter((conversation) => {
      return (
        listFilter === 'all' ||
        (listFilter === 'unread' && conversation.unreadCount > 0) ||
        (listFilter === 'pinned' && conversation.isPinned)
      )
    })
  }, [conversations, listFilter])
  const searchResults = useMemo(() => {
    const query = normalizedSearchQuery.toLocaleLowerCase('ko-KR')
    if (!query) {
      return {
        conversations: [] as SearchResultItem[],
        messages: [] as SearchResultItem[],
      }
    }

    const conversationMatches: SearchResultItem[] = []
    const messageMatches: SearchResultItem[] = []

    for (const conversation of conversations) {
      const matchMeta: string[] = []
      if (conversation.title.toLocaleLowerCase('ko-KR').includes(query)) {
        matchMeta.push('방')
      }
      if (conversation.subtitle.toLocaleLowerCase('ko-KR').includes(query)) {
        matchMeta.push('요약')
      }
      if ((conversation.lastMessage?.text ?? '').toLocaleLowerCase('ko-KR').includes(query)) {
        matchMeta.push('최근')
      }

      if (matchMeta.length > 0) {
        conversationMatches.push({
          key: `conversation-${conversation.conversationId}`,
          kind: 'conversation',
          conversationId: conversation.conversationId,
          title: conversation.title,
          excerpt: conversation.lastMessage?.text ?? conversation.subtitle,
          meta: matchMeta.join(' · '),
          timestamp: conversation.lastMessage?.createdAt ?? conversation.sortKey,
        })
      }

      const loadedMessages = messagesByConversation[conversation.conversationId] ?? []
      for (const message of loadedMessages) {
        if (!message.text.toLocaleLowerCase('ko-KR').includes(query)) {
          continue
        }

        messageMatches.push({
          key: `message-${message.messageId}`,
          kind: 'message',
          conversationId: conversation.conversationId,
          title: conversation.title,
          excerpt: message.text,
          meta: message.isMine ? '내 메시지' : message.sender.displayName,
          timestamp: message.createdAt,
        })
      }
    }

    messageMatches.sort((left, right) => new Date(right.timestamp).getTime() - new Date(left.timestamp).getTime())

    return {
      conversations: conversationMatches.slice(0, 8),
      messages: messageMatches.slice(0, 8),
    }
  }, [conversations, messagesByConversation, normalizedSearchQuery])
  const replyNeededConversations = useMemo(
    () => conversations.filter((conversation) => conversation.unreadCount > 0).slice(0, 4),
    [conversations],
  )
  const pinnedConversations = useMemo(
    () => conversations.filter((conversation) => conversation.isPinned).slice(0, 4),
    [conversations],
  )
  const recentConversations = useMemo(
    () => conversations.slice(0, 4),
    [conversations],
  )
  const savedReplyQueue = replyNeededConversations
  const savedPinnedQueue = useMemo(
    () => dedupeConversationsById(pinnedConversations.filter((conversation) => !replyNeededConversations.some((item) => item.conversationId === conversation.conversationId))),
    [pinnedConversations, replyNeededConversations],
  )
  const savedRecentQueue = useMemo(
    () => dedupeConversationsById(
      recentConversations.filter((conversation) =>
        !replyNeededConversations.some((item) => item.conversationId === conversation.conversationId) &&
        !pinnedConversations.some((item) => item.conversationId === conversation.conversationId),
      ),
    ),
    [pinnedConversations, recentConversations, replyNeededConversations],
  )
  const savedConversations = useMemo(
    () => dedupeConversationsById([...savedReplyQueue, ...savedPinnedQueue, ...savedRecentQueue]),
    [savedPinnedQueue, savedRecentQueue, savedReplyQueue],
  )
  const searchResultTotal = searchResults.conversations.length + searchResults.messages.length
  const primaryResumeConversation = selectedConversation ?? conversations[0] ?? null

  const persistSession = useCallback((nextSession: StoredSession) => {
    writeStoredSession(nextSession)
    setStoredSession(nextSession)
    setBootstrap(nextSession.bootstrap)
    setConversations(sortConversations(nextSession.bootstrap.conversations.items))
  }, [])

  const persistSessionTokens = useCallback((
    session: StoredSession,
    tokens: RefreshTokenResponse['tokens'],
  ): StoredSession => {
    const nextSession: StoredSession = {
      apiBaseUrl: session.apiBaseUrl,
      tokens,
      bootstrap: session.bootstrap,
      savedAt: new Date().toISOString(),
    }

    writeStoredSession(nextSession)
    setStoredSession(nextSession)
    return nextSession
  }, [])

  const refreshSessionTokens = useCallback(async (session: StoredSession): Promise<StoredSession> => {
    if (!refreshSessionPromiseRef.current) {
      refreshSessionPromiseRef.current = refreshToken(session.apiBaseUrl, {
        refreshToken: session.tokens.refreshToken,
      })
        .then((refreshed) => persistSessionTokens(session, refreshed.tokens))
        .finally(() => {
          refreshSessionPromiseRef.current = null
        })
    }

    return refreshSessionPromiseRef.current
  }, [persistSessionTokens])

  const withRecoveredSession = useCallback(async <T,>(
    operation: (session: StoredSession) => Promise<T>,
    baseSession?: StoredSession | null,
  ): Promise<{ result: T; session: StoredSession }> => {
    const currentSession = baseSession ?? storedSessionRef.current
    if (!currentSession) {
      throw new ApiRequestError('세션이 필요합니다.', 'unauthorized', 401)
    }

    try {
      const result = await operation(currentSession)
      return { result, session: currentSession }
    } catch (error: unknown) {
      if (!isUnauthorizedError(error) || !currentSession.tokens.refreshToken) {
        throw error
      }

      setStatusMessage('연결을 다시 확인하고 있어요.')
      const refreshedSession = await refreshSessionTokens(currentSession)
      const result = await operation(refreshedSession)
      return { result, session: refreshedSession }
    }
  }, [refreshSessionTokens])

  const selectConversation = useCallback((conversationId: string, nextMobileView?: MobileView) => {
    setComposerText(readConversationDraft(conversationId))
    pendingAutoScrollRef.current = true
    setSelectedConversationId(conversationId)
    if (nextMobileView) {
      setMobileView(nextMobileView)
    }
  }, [])

  const openDestination = useCallback((nextDestination: BottomDestination) => {
    setBottomDestination(nextDestination)
    setMobileView('list')
  }, [])

  const handleReconnect = useCallback(() => {
    setStatusMessage('현재 화면은 그대로 두고 대화와 연결 상태를 다시 확인하고 있어요.')
    setRefreshTick((value) => value + 1)
  }, [])

  const applyQuickDraft = useCallback((value: string) => {
    setComposerText(value)
    window.requestAnimationFrame(() => {
      composerFieldRef.current?.focus()
      const nextLength = value.length
      composerFieldRef.current?.setSelectionRange(nextLength, nextLength)
    })
  }, [])

  useEffect(() => {
    storedSessionRef.current = storedSession
  }, [storedSession])

  useEffect(() => {
    if (!selectedConversationId && conversations.length > 0) {
      selectConversation(conversations[0].conversationId)
    }
  }, [conversations, selectedConversationId, selectConversation])

  useEffect(() => {
    if (mobileView !== 'list' || bottomDestination !== 'search') {
      return
    }

    const frame = window.requestAnimationFrame(() => {
      searchInputRef.current?.focus()
    })

    return () => window.cancelAnimationFrame(frame)
  }, [bottomDestination, mobileView])

  useEffect(() => {
    if (selectedConversationId && !conversations.some((item) => item.conversationId === selectedConversationId)) {
      const fallbackConversationId = conversations[0]?.conversationId ?? null
      if (fallbackConversationId) {
        selectConversation(fallbackConversationId, 'list')
        return
      }

      setComposerText('')
      setSelectedConversationId(null)
      setMobileView('list')
    }
  }, [conversations, selectedConversationId, selectConversation])

  useEffect(() => {
    const stream = messageStreamRef.current
    const shouldScroll =
      pendingAutoScrollRef.current ||
      !stream ||
      stream.scrollHeight - stream.scrollTop - stream.clientHeight < 72

    if (shouldScroll) {
      messageEndRef.current?.scrollIntoView({ block: 'end' })
    }

    pendingAutoScrollRef.current = false
  }, [selectedConversationId, selectedMessages])

  useEffect(() => {
    if (!accessToken) {
      return
    }

    let disposed = false
    setBootstrapping(true)

    withRecoveredSession((session) => getBootstrap(session.apiBaseUrl, session.tokens.accessToken), storedSessionRef.current)
      .then(({ result: nextBootstrap, session }) => {
        if (disposed) {
          return
        }

        const nextSession: StoredSession = {
          apiBaseUrl: session.apiBaseUrl,
          tokens: session.tokens,
          bootstrap: nextBootstrap,
          savedAt: new Date().toISOString(),
        }

        persistSession(nextSession)
        setStatusMessage(null)
      })
      .catch((error: unknown) => {
        if (disposed) {
          return
        }

        if (storedSessionRef.current && !isUnauthorizedError(error)) {
          setConnectionState('fallback')
          setStatusMessage(getFriendlyStatusMessage(error, '최근 화면을 유지한 채 다시 연결하고 있어요.'))
          return
        }

        clearStoredSession()
        setStoredSession(null)
        setBootstrap(null)
        setConversations([])
        setSelectedConversationId(null)
        setStatusMessage(getFriendlyStatusMessage(error, '잠시 후 자동으로 다시 이어지지 않으면 한 번 더 들어와 주세요.'))
      })
      .finally(() => {
        if (!disposed) {
          setBootstrapping(false)
        }
      })

    return () => {
      disposed = true
    }
  }, [apiBaseUrl, accessToken, persistSession, refreshTick, storedSession?.tokens.refreshToken, withRecoveredSession])

  useEffect(() => {
    if (!bootstrap || !accessToken) {
      setConnectionState('idle')
      return
    }

    let socket: WebSocket | null = null
    let opened = false
    let reconnectTimer: number | null = null
    let disposed = false

    const connect = () => {
      if (disposed) {
        return
      }

      setConnectionState('connecting')

      try {
        socket = new WebSocket(buildBrowserWsUrl(bootstrap.ws.url, bootstrap.ws.ticket))
      } catch {
        setConnectionState('fallback')
        return
      }

      socket.onopen = () => {
        opened = true
        setConnectionState('connected')
      }

      socket.onmessage = (event) => {
        const parsed = parseRealtimeEvent(String(event.data))
        if (!parsed) {
          return
        }

        switch (parsed.kind) {
          case 'session.connected':
            break
          case 'message.created':
            setMessagesByConversation((current) => ({
              ...current,
              [parsed.payload.conversationId]: mergeMessages(
                current[parsed.payload.conversationId],
                [parsed.payload],
              ),
            }))
            setConversations((current) => {
              const source = current.find((item) => item.conversationId === parsed.payload.conversationId)
              return source ? upsertConversation(current, buildConversationPreview(source, parsed.payload)) : current
            })
            break
          case 'read_cursor.updated':
            setConversations((current) =>
              current.map((item) =>
                item.conversationId === parsed.payload.conversationId
                  ? applyReadCursor(item, parsed.payload)
                  : item,
              ),
            )
            break
          case 'unknown':
            break
        }
      }

      socket.onclose = () => {
        if (disposed) {
          return
        }

        if (!opened) {
          setConnectionState('fallback')
          reconnectTimer = window.setTimeout(connect, 1500)
          return
        }

        setConnectionState('fallback')
        reconnectTimer = window.setTimeout(connect, 5000)
      }

      socket.onerror = () => {
        setConnectionState('fallback')
      }
    }

    connect()

    return () => {
      disposed = true
      if (reconnectTimer) {
        window.clearTimeout(reconnectTimer)
      }
      socket?.close()
    }
  }, [accessToken, bootstrap])

  useEffect(() => {
    if (!accessToken || connectionState !== 'fallback') {
      return
    }

    const timer = window.setInterval(() => {
      setRefreshTick((value) => value + 1)
    }, 30000)

    return () => window.clearInterval(timer)
  }, [accessToken, connectionState])

  useEffect(() => {
    if (!accessToken || !selectedConversationId || messagesByConversation[selectedConversationId]) {
      return
    }

    let disposed = false
    setLoadingConversationId(selectedConversationId)

    withRecoveredSession((session) =>
      getMessages(session.apiBaseUrl, session.tokens.accessToken, selectedConversationId),
    )
      .then(({ result: response }) => {
        if (disposed) {
          return
        }

        setMessagesByConversation((current) => ({
          ...current,
          [selectedConversationId]: mergeMessages(current[selectedConversationId], response.items),
        }))
        setStatusMessage(null)
      })
      .catch((error: unknown) => {
        if (!disposed) {
          setStatusMessage(getFriendlyStatusMessage(error, '대화를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.'))
        }
      })
      .finally(() => {
        if (!disposed) {
          setLoadingConversationId(null)
        }
      })

    return () => {
      disposed = true
    }
  }, [accessToken, messagesByConversation, selectedConversationId, storedSession, withRecoveredSession])

  useEffect(() => {
    if (!accessToken || !selectedConversation || selectedMessages.length === 0) {
      return
    }

    const lastMessage = selectedMessages[selectedMessages.length - 1]
    if (lastMessage.serverSequence <= selectedConversation.lastReadSequence) {
      return
    }

    if (pendingReadCursorRef.current[selectedConversation.conversationId] === lastMessage.serverSequence) {
      return
    }

    pendingReadCursorRef.current[selectedConversation.conversationId] = lastMessage.serverSequence

    withRecoveredSession((session) =>
      updateReadCursor(session.apiBaseUrl, session.tokens.accessToken, selectedConversation.conversationId, {
        lastReadSequence: lastMessage.serverSequence,
      }),
    )
      .then(({ result: response }) => {
        setConversations((current) =>
          current.map((item) =>
            item.conversationId === response.conversationId ? applyReadCursor(item, response) : item,
          ),
        )
        setStatusMessage(null)
      })
      .catch(() => {
        delete pendingReadCursorRef.current[selectedConversation.conversationId]
      })
  }, [accessToken, selectedConversation, selectedMessages, storedSession, withRecoveredSession])

  useEffect(() => {
    if (!accessToken || connectionState !== 'connected' || !selectedConversationId) {
      return
    }

    let disposed = false

    withRecoveredSession((session) =>
      getMessages(session.apiBaseUrl, session.tokens.accessToken, selectedConversationId),
    )
      .then(({ result: response }) => {
        if (disposed) {
          return
        }

        setMessagesByConversation((current) => ({
          ...current,
          [selectedConversationId]: mergeMessages(current[selectedConversationId], response.items),
        }))
      })
      .catch(() => undefined)

    return () => {
      disposed = true
    }
  }, [accessToken, connectionState, selectedConversationId, withRecoveredSession])

  const handleRegister = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setRegistering(true)
    setStatusMessage(null)

    try {
      const response: RegisterAlphaQuickResponse = await registerAlphaQuick(apiBaseUrl, {
        displayName: displayName.trim(),
        inviteCode: inviteCode.trim(),
        device: {
          installId: getInstallId(),
          platform: 'web-mobile',
          deviceName: getDefaultDeviceName(),
          appVersion: APP_VERSION,
        },
      })

      const nextSession: StoredSession = {
        apiBaseUrl,
        tokens: response.tokens,
        bootstrap: response.bootstrap,
        savedAt: new Date().toISOString(),
      }

      writeSavedInviteCode(inviteCode.trim())
      persistSession(nextSession)
      const firstConversationId = response.bootstrap.conversations.items[0]?.conversationId ?? null
      if (firstConversationId) {
        selectConversation(firstConversationId, 'chat')
      } else {
        setComposerText('')
        setSelectedConversationId(null)
      }
      setBottomDestination('inbox')
      setMessagesByConversation({})
      setSearchQuery('')
      setListFilter('all')
    } catch (error: unknown) {
      setStatusMessage(getFriendlyStatusMessage(error, '입장하지 못했습니다. 입력 내용을 다시 확인해 주세요.'))
    } finally {
      setRegistering(false)
    }
  }

  const handleSendMessage = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!selectedConversation || !storedSession || !accessToken || !composerText.trim()) {
      return
    }

    const draft = composerText.trim()
    setSending(true)
    pendingAutoScrollRef.current = true

    try {
      const { result: message } = await withRecoveredSession((session) =>
        sendTextMessage(session.apiBaseUrl, session.tokens.accessToken, selectedConversation.conversationId, {
          clientRequestId: createClientId(),
          body: draft,
        }),
      )

      setMessagesByConversation((current) => ({
        ...current,
        [selectedConversation.conversationId]: mergeMessages(current[selectedConversation.conversationId], [message]),
      }))

      setConversations((current) => {
        const source = current.find((item) => item.conversationId === selectedConversation.conversationId)
        return source ? upsertConversation(current, buildConversationPreview(source, message)) : current
      })

      setComposerText('')
      clearConversationDraft(selectedConversation.conversationId)
      setStatusMessage(null)
    } catch (error: unknown) {
      setComposerText(draft)
      setStatusMessage(getFriendlyStatusMessage(error, '메시지를 보내지 못했습니다. 잠시 후 다시 시도해 주세요.'))
    } finally {
      setSending(false)
    }
  }

  const handleLogout = () => {
    const shouldClear = window.confirm('이 기기의 세션과 임시 입력을 정리할까요?')
    if (!shouldClear) {
      return
    }

    clearStoredSession()
    setStoredSession(null)
    setBootstrap(null)
    setConversations([])
    setMessagesByConversation({})
    setSelectedConversationId(null)
    setSearchQuery('')
    setListFilter('all')
    setBottomDestination('inbox')
    setMobileView('list')
    clearConversationDrafts()
    setStatusMessage('세션을 정리했습니다.')
  }

  useEffect(() => {
    if (!selectedConversationId) {
      setComposerText('')
      return
    }

    setComposerText(readConversationDraft(selectedConversationId))
  }, [selectedConversationId])

  useEffect(() => {
    if (!selectedConversationId) {
      return
    }

    writeConversationDraft(selectedConversationId, composerText)
  }, [composerText, selectedConversationId])

  const shellStateClass = hasSession ? 'app app--ready' : 'app app--onboarding'
  const destinationMeta: Record<BottomDestination, { title: string; subtitle: string }> = {
    inbox: {
      title: '받은함',
      subtitle: me?.displayName ? `${me.displayName}` : '최근',
    },
    search: {
      title: '검색',
      subtitle: '다시 찾기',
    },
    saved: {
      title: '보관',
      subtitle: '후속 작업',
    },
    me: {
      title: '내 공간',
      subtitle: '세션',
    },
  }
  const activeDestinationMeta = destinationMeta[bottomDestination]
  const renderConversationRows = (items: ConversationSummaryDto[]) =>
    items.map((conversation) => {
      const active = conversation.conversationId === selectedConversationId

      return (
        <button
          key={conversation.conversationId}
          className={`conversation-row ${active ? 'conversation-row--active' : ''}`}
          type="button"
          onClick={() => {
            selectConversation(conversation.conversationId, 'chat')
          }}
        >
          <div className="avatar">{getConversationInitials(conversation.title)}</div>

          <div className="conversation-row__body">
            <div className="conversation-row__topline">
              <strong>{conversation.title}</strong>
              <time>{formatListTime(conversation.lastMessage?.createdAt ?? conversation.sortKey)}</time>
            </div>

            <div className="conversation-row__meta">
              <span>{conversation.lastMessage?.text ?? conversation.subtitle}</span>
              <div className="conversation-row__tail">
                {conversation.isPinned ? <i className="row-pin" aria-hidden="true" /> : null}
                {conversation.unreadCount > 0 ? <em>{conversation.unreadCount}</em> : null}
              </div>
            </div>
          </div>
        </button>
      )
    })

  const renderSearchResultRows = (items: SearchResultItem[]) =>
    items.map((item) => (
      <button
        key={item.key}
        className="search-result"
        type="button"
        onClick={() => {
          selectConversation(item.conversationId, 'chat')
          setBottomDestination('inbox')
        }}
      >
        <div className="search-result__topline">
          <strong>{item.title}</strong>
          <time>{formatListTime(item.timestamp)}</time>
        </div>
        <p>{item.excerpt}</p>
        <span>{item.meta}</span>
      </button>
    ))

  return (
    <div className={shellStateClass} data-mobile-view={mobileView}>
      {!hasSession ? (
        <main className="onboarding">
          <header className="onboarding__chrome">
            <div className="brand-lockup">
              <span className="brand-mark" aria-hidden="true">
                <Icon name="mark" />
              </span>
              <div className="brand-lockup__text">
                <strong>KoTalk</strong>
              </div>
            </div>
            <span className="surface-badge">WEB</span>
          </header>

          <section className="onboarding__panel">
            <form className="onboarding__form" onSubmit={handleRegister}>
              <label className="field">
                <input
                  aria-label="표시 이름"
                  autoComplete="nickname"
                  maxLength={20}
                  placeholder="표시 이름"
                  required
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                />
              </label>

              <label className="field">
                <input
                  aria-label="참여 키"
                  autoCapitalize="characters"
                  placeholder="참여 키"
                  required
                  value={inviteCode}
                  onChange={(event) => setInviteCode(event.target.value.toUpperCase())}
                />
              </label>

              <button className="text-action" type="button" onClick={() => setShowAdvanced((value) => !value)}>
                {showAdvanced ? '기본 보기' : '고급'}
              </button>

              {showAdvanced ? (
                <label className="field">
                  <input
                    aria-label="서버 주소"
                    inputMode="url"
                    placeholder="서버 주소"
                    value={apiBaseUrl}
                    onChange={(event) => setApiBaseUrl(event.target.value)}
                  />
                </label>
              ) : null}

              <button className="primary-button" disabled={registering} type="submit">
                {registering ? '입장 중...' : '열기'}
              </button>
            </form>

            {statusMessage ? <p className="status-text">{statusMessage}</p> : null}
          </section>
        </main>
      ) : (
        <main className="shell">
          <aside className="bottom-bar bottom-bar--shell" aria-label="목적지">
            <button
              className={`nav-button ${bottomDestination === 'inbox' ? 'nav-button--active' : ''}`}
              type="button"
              onClick={() => openDestination('inbox')}
            >
              <Icon name="chat" />
              <span>받은함</span>
            </button>
            <button
              className={`nav-button ${bottomDestination === 'search' ? 'nav-button--active' : ''}`}
              type="button"
              onClick={() => openDestination('search')}
            >
              <Icon name="search" />
              <span>검색</span>
            </button>
            <button
              className={`nav-button ${bottomDestination === 'saved' ? 'nav-button--active' : ''}`}
              type="button"
              onClick={() => openDestination('saved')}
            >
              <Icon name="archive" />
              <span>보관</span>
            </button>
            <button
              className={`nav-button ${bottomDestination === 'me' ? 'nav-button--active' : ''}`}
              type="button"
              onClick={() => openDestination('me')}
            >
              <Icon name="me" />
              <span>내 공간</span>
            </button>
          </aside>

          <section className={`pane pane--list pane--${bottomDestination} ${mobileView === 'chat' ? 'pane--hidden' : ''}`}>
            <header className="appbar">
              <div className="appbar__leading">
                <span className="brand-mark brand-mark--small" aria-hidden="true">
                  <Icon name="mark" />
                </span>
                <div className="appbar__title">
                  <h2>{activeDestinationMeta.title}</h2>
                  <span>{activeDestinationMeta.subtitle}</span>
                </div>
              </div>

              <div className="appbar__actions">
                {bottomDestination === 'search' && normalizedSearchQuery ? (
                  <button className="text-action text-action--quiet" type="button" onClick={() => setSearchQuery('')}>
                    지우기
                  </button>
                ) : null}
                {bottomDestination !== 'me' ? (
                  <button
                    className="icon-button"
                    type="button"
                    aria-label="대화 목록 새로고침"
                    onClick={handleReconnect}
                  >
                    <Icon name="refresh" />
                  </button>
                ) : null}
              </div>
            </header>

            {bottomDestination === 'inbox' ? (
              <>
                <div className="toolbar-strip" aria-label="받은함 빠른 막대">
                  <button
                    className={`summary-chip ${listFilter === 'all' ? 'summary-chip--active' : ''}`}
                    type="button"
                    onClick={() => setListFilter('all')}
                  >
                    <Icon name="chat" /> {conversations.length}
                  </button>
                  <button
                    className={`summary-chip ${listFilter === 'unread' ? 'summary-chip--active' : ''}`}
                    type="button"
                    onClick={() => setListFilter('unread')}
                  >
                    <Icon name="spark" /> {unreadTotal}
                  </button>
                  <button
                    className={`summary-chip ${listFilter === 'pinned' ? 'summary-chip--active' : ''}`}
                    type="button"
                    onClick={() => setListFilter('pinned')}
                  >
                    <Icon name="pin" /> {pinnedTotal}
                  </button>
                  <div className="toolbar-strip__group toolbar-strip__group--actions">
                    <button
                      className="icon-button icon-button--soft"
                      type="button"
                      aria-label="검색"
                      onClick={() => openDestination('search')}
                    >
                      <Icon name="search" />
                    </button>
                    <button
                      className="icon-button icon-button--soft"
                      type="button"
                      aria-label="새로고침"
                      onClick={handleReconnect}
                    >
                      <Icon name="refresh" />
                    </button>
                  </div>
                </div>

                <div className="conversation-list">
                  {bootstrapping ? <p className="empty-state">동기화 중</p> : null}
                  {!bootstrapping && conversations.length === 0 ? (
                    <div className="empty-panel">
                      <strong>대화가 아직 없습니다</strong>
                      <p>첫 메시지를 남기거나 다시 동기화해 보세요.</p>
                      <div className="empty-panel__actions">
                        <button className="secondary-button" type="button" onClick={handleReconnect}>
                          새로고침
                        </button>
                        <button className="ghost-button" type="button" onClick={() => openDestination('me')}>
                          프로필
                        </button>
                      </div>
                    </div>
                  ) : null}
                  {!bootstrapping && conversations.length > 0 && filteredConversations.length === 0 ? (
                    <div className="empty-panel">
                      <strong>맞는 대화가 없습니다</strong>
                      <div className="empty-panel__actions">
                        <button className="secondary-button" type="button" onClick={() => setListFilter('all')}>
                          전체 보기
                        </button>
                      </div>
                    </div>
                  ) : null}

                  {renderConversationRows(filteredConversations)}
                </div>
              </>
            ) : null}

            {bottomDestination === 'search' ? (
              <>
                <label className="search-field" aria-label="대화 검색">
                  <span className="search-field__icon">
                    <Icon name="search" />
                  </span>
                  <input
                    ref={searchInputRef}
                    type="search"
                    placeholder="검색"
                    value={searchQuery}
                    onChange={(event) => setSearchQuery(event.target.value)}
                  />
                </label>

                <div className="toolbar-strip toolbar-strip--utility" aria-label="검색 빠른 이동">
                  <button
                    className="icon-button icon-button--soft"
                    type="button"
                    aria-label="최근 대화"
                    onClick={() => {
                      if (primaryResumeConversation) {
                        selectConversation(primaryResumeConversation.conversationId, 'chat')
                        setBottomDestination('inbox')
                      }
                    }}
                  >
                    <Icon name="clock" />
                  </button>
                  <button
                    className="icon-button icon-button--soft"
                    type="button"
                    aria-label="안읽음 보기"
                    onClick={() => {
                      setListFilter('unread')
                      openDestination('inbox')
                    }}
                  >
                    <Icon name="spark" />
                  </button>
                  <button
                    className="icon-button icon-button--soft"
                    type="button"
                    aria-label="고정 대화 보기"
                    onClick={() => {
                      setListFilter('pinned')
                      openDestination('inbox')
                    }}
                  >
                    <Icon name="pin" />
                  </button>
                </div>

                <div className="conversation-list">
                  {!normalizedSearchQuery ? (
                    <div className="search-results search-results--discovery">
                      {recentConversations.length > 0 ? (
                        <section className="saved-section">
                          <div className="saved-section__header">
                            <strong>최근</strong>
                            <span>{recentConversations.length}개</span>
                          </div>
                          <div className="saved-section__body">{renderConversationRows(recentConversations)}</div>
                        </section>
                      ) : null}
                      {replyNeededConversations.length > 0 ? (
                        <section className="saved-section">
                          <div className="saved-section__header">
                            <strong>안읽음</strong>
                            <span>{replyNeededConversations.length}개</span>
                          </div>
                          <div className="saved-section__body">{renderConversationRows(replyNeededConversations)}</div>
                        </section>
                      ) : null}
                      {pinnedConversations.length > 0 ? (
                        <section className="saved-section">
                          <div className="saved-section__header">
                            <strong>고정</strong>
                            <span>{pinnedConversations.length}개</span>
                          </div>
                          <div className="saved-section__body">{renderConversationRows(pinnedConversations)}</div>
                        </section>
                      ) : null}
                    </div>
                  ) : null}
                  {normalizedSearchQuery && searchResultTotal === 0 ? (
                    <p className="empty-state empty-state--inline">결과가 없습니다. 다른 단어로 다시 찾아보세요.</p>
                  ) : null}
                  {normalizedSearchQuery && searchResultTotal > 0 ? (
                    <div className="search-results search-results--matches">
                      {searchResults.messages.length > 0 ? (
                        <section className="saved-section">
                          <div className="saved-section__header">
                            <strong>메시지</strong>
                            <span>{searchResults.messages.length}개</span>
                          </div>
                          <div className="saved-section__body">
                            {renderSearchResultRows(searchResults.messages)}
                          </div>
                        </section>
                      ) : null}

                      {searchResults.conversations.length > 0 ? (
                        <section className="saved-section">
                          <div className="saved-section__header">
                            <strong>대화</strong>
                            <span>{searchResults.conversations.length}개</span>
                          </div>
                          <div className="saved-section__body">
                            {renderSearchResultRows(searchResults.conversations)}
                          </div>
                        </section>
                      ) : null}
                    </div>
                  ) : null}
                </div>
              </>
            ) : null}

            {bottomDestination === 'saved' ? (
              <>
                <div className="toolbar-strip toolbar-strip--utility" aria-label="보관함 요약">
                  <span className="status-chip"><Icon name="spark" /> {savedReplyQueue.length}</span>
                  <span className="status-chip"><Icon name="pin" /> {savedPinnedQueue.length}</span>
                </div>

                <div className="conversation-list conversation-list--saved">
                  {savedConversations.length === 0 ? (
                    <p className="empty-state empty-state--inline">지금 보관된 후속 작업이 없습니다.</p>
                  ) : null}

                  {savedReplyQueue.length > 0 ? (
                    <section className="saved-section">
                      <div className="saved-section__header">
                        <strong>답장</strong>
                        <span>{savedReplyQueue.length}개</span>
                      </div>
                      <div className="saved-section__body">{renderConversationRows(savedReplyQueue)}</div>
                    </section>
                  ) : null}

                  {savedPinnedQueue.length > 0 ? (
                    <section className="saved-section">
                      <div className="saved-section__header">
                        <strong>중요</strong>
                        <span>{savedPinnedQueue.length}개</span>
                      </div>
                      <div className="saved-section__body">{renderConversationRows(savedPinnedQueue)}</div>
                    </section>
                  ) : null}

                  {savedRecentQueue.length > 0 ? (
                    <section className="saved-section">
                      <div className="saved-section__header">
                        <strong>최근</strong>
                        <span>{savedRecentQueue.length}개</span>
                      </div>
                      <div className="saved-section__body">{renderConversationRows(savedRecentQueue)}</div>
                    </section>
                  ) : null}
                </div>
              </>
            ) : null}

            {bottomDestination === 'me' ? (
              <>
                <div className="profile-card">
                  <div className="profile-card__hero">
                    <div className="avatar avatar--profile">{getConversationInitials(me?.displayName ?? 'KO')}</div>
                    <div className="profile-card__body">
                      <strong>{me?.displayName ?? '게스트'}</strong>
                      <span>{CONNECTION_DESCRIPTION[connectionState]}</span>
                    </div>
                  </div>

                  <dl className="profile-meta">
                    <div>
                      <dt>연결</dt>
                      <dd>{CONNECTION_DESCRIPTION[connectionState]}</dd>
                    </div>
                    <div>
                      <dt>최근</dt>
                      <dd>{formatRelativeConnection(storedSession?.savedAt ?? null)}</dd>
                    </div>
                    <div>
                      <dt>앱 버전</dt>
                      <dd>{APP_VERSION}</dd>
                    </div>
                  </dl>
                </div>

                <div className="stack-actions">
                  <button className="secondary-button" type="button" onClick={handleReconnect}>
                    다시 확인
                  </button>
                  <button className="ghost-button" type="button" onClick={handleLogout}>
                    로그아웃
                  </button>
                </div>
              </>
            ) : null}

          </section>

          <section className={`pane pane--chat ${mobileView === 'list' ? 'pane--hidden' : ''}`}>
            {selectedConversation ? (
              <>
                <header className="chat-appbar">
                  <div className="chat-appbar__leading">
                    <button
                      className="icon-button icon-button--ghost back-button"
                      type="button"
                      aria-label="대화 목록으로 돌아가기"
                      onClick={() => setMobileView('list')}
                    >
                      <Icon name="back" />
                    </button>

                    <div className="avatar avatar--header">{getConversationInitials(selectedConversation.title)}</div>

                    <div className="chat-appbar__title">
                      <strong>{selectedConversation.title}</strong>
                      <span>
                        {selectedConversation.memberCount}명 · {selectedConversation.unreadCount}
                      </span>
                    </div>
                  </div>

                  <span className="mini-pill">{CONNECTION_LABEL[connectionState]}</span>
                </header>

                <div className="message-stream" ref={messageStreamRef}>
                  {loadingConversationId === selectedConversation.conversationId ? (
                    <p className="empty-state">불러오는 중</p>
                  ) : null}

                  {loadingConversationId !== selectedConversation.conversationId && selectedMessages.length === 0 ? (
                    <div className="empty-panel empty-panel--chat">
                      <strong>{selectedConversation.memberCount <= 1 ? '첫 메모' : '첫 메시지'}</strong>
                      <div className="empty-panel__actions">
                        <button
                          className="secondary-button"
                          type="button"
                          onClick={() =>
                            applyQuickDraft(
                              selectedConversation.memberCount <= 1 ? '오늘 확인할 일\n- ' : '짧게 먼저 공유드릴게요.\n',
                            )
                          }
                        >
                          시작
                        </button>
                        <button className="ghost-button" type="button" onClick={() => openDestination('saved')}>
                          보관
                        </button>
                      </div>
                    </div>
                  ) : null}

                  {selectedMessages.map((message, index) => {
                    const previous = selectedMessages[index - 1]
                    const showSender =
                      !message.isMine &&
                      (!previous || previous.isMine || previous.sender.userId !== message.sender.userId)

                    return (
                      <article
                        key={message.messageId}
                        className={`message-bubble ${message.isMine ? 'message-bubble--mine' : ''}`}
                      >
                        {showSender ? <p className="message-bubble__sender">{message.sender.displayName}</p> : null}
                        <div className="message-bubble__body">
                          <p>{message.text}</p>
                        </div>
                        <time>{formatClock(message.createdAt)}</time>
                      </article>
                    )
                  })}
                  <div ref={messageEndRef} />
                </div>

                <form className="composer" onSubmit={handleSendMessage}>
                  <div className="composer-shortcuts" aria-label="빠른 입력">
                    <button className="ghost-button" type="button" onClick={() => applyQuickDraft('확인 후 답드릴게요.')}>
                      확인
                    </button>
                    <button className="ghost-button" type="button" onClick={() => applyQuickDraft('자료 공유드립니다.\n- ')}>
                      공유
                    </button>
                    <button className="ghost-button" type="button" onClick={() => applyQuickDraft('잠시 뒤 다시 말씀드릴게요.')}>
                      나중
                    </button>
                  </div>
                  <label className="composer__field">
                    <textarea
                      ref={composerFieldRef}
                      maxLength={1200}
                      placeholder={selectedMessages.length === 0 ? '메모 또는 메시지' : '메시지'}
                      rows={1}
                      value={composerText}
                      onChange={(event) => setComposerText(event.target.value)}
                    />
                  </label>
                  <button
                    className="send-button"
                    disabled={sending || !composerText.trim()}
                    type="submit"
                    aria-label="메시지 보내기"
                  >
                    <Icon name="send" />
                  </button>
                </form>
              </>
            ) : (
              <div className="message-stream message-stream--empty">
                <p className="empty-state empty-state--inline">대화를 고르세요.</p>
              </div>
            )}
          </section>
        </main>
      )}

      {hasSession && statusMessage ? <aside className="toast">{statusMessage}</aside> : null}
    </div>
  )
}

export default App
