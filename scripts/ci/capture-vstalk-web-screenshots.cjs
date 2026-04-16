const fs = require('node:fs/promises')
const path = require('node:path')
const process = require('node:process')
const { createRequire } = require('node:module')

const webPackageRequire = createRequire(path.resolve(process.cwd(), 'src/PhysOn.Web/package.json'))
const puppeteer = webPackageRequire('puppeteer-core')

const baseUrl = process.env.VSTALK_CAPTURE_URL ?? 'http://127.0.0.1:4174/'
const outputDir = process.env.VSTALK_CAPTURE_OUTPUT_DIR
  ?? path.resolve(process.cwd(), 'docs/assets/latest')
const executablePath = process.env.CHROME_BIN ?? '/usr/bin/google-chrome'

const bootstrapPayload = {
  me: {
    userId: 'me-1',
    displayName: '이안',
    profileImageUrl: null,
    statusMessage: '업무와 일상을 가볍게 잇는 중',
  },
  session: {
    sessionId: 'session-alpha-web',
    deviceId: 'device-web-alpha',
    deviceName: 'Mobile Web',
    createdAt: '2026-04-16T04:50:00Z',
  },
  ws: {
    url: 'wss://vstalk.phy.kr/v1/realtime/ws',
  },
  conversations: {
    items: [
      {
        conversationId: 'conv-team',
        type: 'group',
        title: '제품 운영',
        avatarUrl: null,
        subtitle: '10시 전에 공유안만 확인해 주세요.',
        memberCount: 4,
        isMuted: false,
        isPinned: true,
        sortKey: '2026-04-16T05:04:00Z',
        unreadCount: 2,
        lastReadSequence: 10,
        lastMessage: {
          messageId: 'msg-11',
          text: '10시 전에 공유안만 확인해 주세요.',
          createdAt: '2026-04-16T05:04:00Z',
          senderUserId: 'u-2',
        },
      },
      {
        conversationId: 'conv-friends',
        type: 'group',
        title: '주말 약속',
        avatarUrl: null,
        subtitle: '토요일 2시에 브런치 어때?',
        memberCount: 3,
        isMuted: false,
        isPinned: false,
        sortKey: '2026-04-16T04:48:00Z',
        unreadCount: 0,
        lastReadSequence: 5,
        lastMessage: {
          messageId: 'msg-22',
          text: '토요일 2시에 브런치 어때?',
          createdAt: '2026-04-16T04:48:00Z',
          senderUserId: 'u-3',
        },
      },
    ],
    nextCursor: null,
  },
}

const messageMap = {
  'conv-team': {
    items: [
      {
        messageId: 'msg-8',
        conversationId: 'conv-team',
        clientMessageId: 'client-8',
        kind: 'text',
        text: '회의 전에 이슈만 짧게 정리해 주세요.',
        createdAt: '2026-04-16T04:40:00Z',
        editedAt: null,
        sender: {
          userId: 'u-2',
          displayName: '민지',
          profileImageUrl: null,
        },
        isMine: false,
        serverSequence: 8,
      },
      {
        messageId: 'msg-9',
        conversationId: 'conv-team',
        clientMessageId: 'client-9',
        kind: 'text',
        text: '공유안은 정리해 두었습니다. 바로 올릴게요.',
        createdAt: '2026-04-16T04:47:00Z',
        editedAt: null,
        sender: {
          userId: 'me-1',
          displayName: '이안',
          profileImageUrl: null,
        },
        isMine: true,
        serverSequence: 9,
      },
      {
        messageId: 'msg-10',
        conversationId: 'conv-team',
        clientMessageId: 'client-10',
        kind: 'text',
        text: '좋아요. 10시 전에 공유안만 확인해 주세요.',
        createdAt: '2026-04-16T05:04:00Z',
        editedAt: null,
        sender: {
          userId: 'u-2',
          displayName: '민지',
          profileImageUrl: null,
        },
        isMine: false,
        serverSequence: 10,
      },
      {
        messageId: 'msg-11',
        conversationId: 'conv-team',
        clientMessageId: 'client-11',
        kind: 'text',
        text: '10시 전에 공유안만 확인해 주세요.',
        createdAt: '2026-04-16T05:04:00Z',
        editedAt: null,
        sender: {
          userId: 'u-2',
          displayName: '민지',
          profileImageUrl: null,
        },
        isMine: false,
        serverSequence: 11,
      },
    ],
    nextCursor: null,
  },
  'conv-friends': {
    items: [
      {
        messageId: 'msg-20',
        conversationId: 'conv-friends',
        clientMessageId: 'client-20',
        kind: 'text',
        text: '이번 주말에 시간 괜찮아?',
        createdAt: '2026-04-16T04:32:00Z',
        editedAt: null,
        sender: {
          userId: 'u-3',
          displayName: '수아',
          profileImageUrl: null,
        },
        isMine: false,
        serverSequence: 4,
      },
      {
        messageId: 'msg-21',
        conversationId: 'conv-friends',
        clientMessageId: 'client-21',
        kind: 'text',
        text: '토요일 2시에 브런치 어때?',
        createdAt: '2026-04-16T04:48:00Z',
        editedAt: null,
        sender: {
          userId: 'u-3',
          displayName: '수아',
          profileImageUrl: null,
        },
        isMine: false,
        serverSequence: 5,
      },
    ],
    nextCursor: null,
  },
}

const storedSession = {
  apiBaseUrl: '',
  tokens: {
    accessToken: 'access-token-alpha',
    accessTokenExpiresAt: '2026-04-16T06:00:00Z',
    refreshToken: 'refresh-token-alpha',
    refreshTokenExpiresAt: '2026-05-16T06:00:00Z',
  },
  bootstrap: bootstrapPayload,
  savedAt: '2026-04-16T05:04:00Z',
}

async function ensureOutputDir() {
  await fs.mkdir(outputDir, { recursive: true })
}

async function createBrowser() {
  return puppeteer.launch({
    executablePath,
    headless: 'new',
    args: ['--no-sandbox', '--disable-setuid-sandbox'],
    defaultViewport: {
      width: 390,
      height: 844,
      isMobile: true,
      hasTouch: true,
      deviceScaleFactor: 2,
    },
  })
}

async function installSessionMocks(page) {
  await page.evaluateOnNewDocument((session) => {
    class FakeWebSocket {
      constructor(url) {
        this.url = url
        this.readyState = 0
        this.onopen = null
        this.onclose = null
        this.onerror = null
        this.onmessage = null
        window.setTimeout(() => {
          this.readyState = 1
          if (this.onopen) {
            this.onopen({ type: 'open' })
          }
        }, 80)
      }

      close() {
        this.readyState = 3
        if (this.onclose) {
          this.onclose({ type: 'close' })
        }
      }

      send() {}
    }

    window.localStorage.setItem('vs-talk.session', JSON.stringify(session))
    window.localStorage.setItem('vs-talk.invite-code', 'ALPHA')
    window.localStorage.setItem('vs-talk.recent-conversations', JSON.stringify(['conv-team', 'conv-friends']))
    window.localStorage.setItem(
      'vs-talk.follow-up',
      JSON.stringify({
        'conv-team': 'today',
        'conv-friends': 'later',
      }),
    )
    window.WebSocket = FakeWebSocket
  }, storedSession)

  await page.setRequestInterception(true)
  page.on('request', (request) => {
    const url = new URL(request.url())

    if (url.pathname === '/v1/bootstrap') {
      request.respond({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: bootstrapPayload }),
      })
      return
    }

    if (/\/v1\/conversations\/[^/]+\/messages$/.test(url.pathname)) {
      const match = url.pathname.match(/\/v1\/conversations\/([^/]+)\/messages/)
      const conversationId = match ? match[1] : ''
      const payload = messageMap[conversationId] ?? { items: [], nextCursor: null }

      request.respond({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: payload }),
      })
      return
    }

    if (/\/v1\/conversations\/[^/]+\/read-cursor$/.test(url.pathname)) {
      const match = url.pathname.match(/\/v1\/conversations\/([^/]+)\/read-cursor/)
      const conversationId = match ? match[1] : ''
      const body = JSON.parse(request.postData() ?? '{}')

      request.respond({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            conversationId,
            accountId: 'me-1',
            lastReadSequence: body.lastReadSequence ?? 0,
            updatedAt: '2026-04-16T05:05:00Z',
          },
        }),
      })
      return
    }

    if (url.pathname === '/v1/auth/token/refresh') {
      request.respond({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            tokens: storedSession.tokens,
          },
        }),
      })
      return
    }

    request.continue()
  })
}

async function captureOnboarding(browser) {
  const page = await browser.newPage()
  await page.goto(baseUrl, { waitUntil: 'networkidle2' })
  await page.evaluate(() => {
    window.localStorage.clear()
  })
  await page.reload({ waitUntil: 'networkidle2' })
  const app = await page.waitForSelector('.onboarding')
  await app.screenshot({
    path: path.join(outputDir, 'vstalk-web-onboarding.png'),
  })
  await page.close()
}

async function captureConversationList(browser) {
  const page = await browser.newPage()
  await installSessionMocks(page)
  await page.goto(baseUrl, { waitUntil: 'networkidle2' })
  await page.waitForSelector('.conversation-row')
  const app = await page.waitForSelector('.shell')
  await app.screenshot({
    path: path.join(outputDir, 'vstalk-web-list.png'),
  })
  await page.close()
}

async function captureConversation(browser) {
  const page = await browser.newPage()
  await installSessionMocks(page)
  await page.goto(baseUrl, { waitUntil: 'networkidle2' })
  await page.waitForSelector('.conversation-row')
  await page.click('.conversation-row')
  await page.waitForSelector('.message-bubble')
  const app = await page.waitForSelector('.shell')
  await app.screenshot({
    path: path.join(outputDir, 'vstalk-web-chat.png'),
  })
  await page.close()
}

async function captureSearch(browser) {
  const page = await browser.newPage()
  await installSessionMocks(page)
  await page.goto(baseUrl, { waitUntil: 'networkidle2' })
  await page.waitForSelector('.bottom-bar')
  await page.click('.bottom-bar .nav-button:nth-child(2)')
  await page.waitForSelector('.search-field')
  await page.type('.search-field input', '공유안')
  await page.waitForSelector('.search-result')
  const app = await page.waitForSelector('.shell')
  await app.screenshot({
    path: path.join(outputDir, 'vstalk-web-search.png'),
  })
  await page.close()
}

async function captureSaved(browser) {
  const page = await browser.newPage()
  await installSessionMocks(page)
  await page.goto(baseUrl, { waitUntil: 'networkidle2' })
  await page.waitForSelector('.bottom-bar')
  await page.click('.bottom-bar .nav-button:nth-child(3)')
  await page.waitForSelector('.saved-section')
  const app = await page.waitForSelector('.shell')
  await app.screenshot({
    path: path.join(outputDir, 'vstalk-web-saved.png'),
  })
  await page.close()
}

async function main() {
  await ensureOutputDir()
  const browser = await createBrowser()

  try {
    await captureOnboarding(browser)
    await captureConversationList(browser)
    await captureSearch(browser)
    await captureSaved(browser)
    await captureConversation(browser)
  } finally {
    await browser.close()
  }
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
