import type { UserConfig, ParserOptions, Commit } from '@commitlint/types'
import * as gitmoji from './gitmoji.json'

interface GitmojiCommit extends Commit {
  emoji: string
}

const types = [
  'feat',
  'fix',
  'docs',
  'style',
  'refactor',
  'perf',
  'test',
  'chore',
  'revert',
  'wip',
  'build',
  'ci',
  'conf',
  'deps'
]

const parserOpts: ParserOptions = {
  headerPattern: /^(:[\w:]+:) (\w+)(\([\w/*-]+\))?:? (.*)$/,
  headerCorrespondence: ['emoji', 'type', 'scope', 'subject']
}

const Configuration: UserConfig = {
  rules: {
    'emoji-enum': [2, 'always'],
    'emoji-empty': [2, 'never'],
    'type-enum': [2, 'always', types],
    'type-empty': [2, 'never']
  },
  parserPreset: { parserOpts: parserOpts },

  plugins: [
    {
      rules: {
        'emoji-empty': (parsed, when, value) => {
          const commit = parsed as GitmojiCommit
          return [!!commit.emoji, 'Emoji  may not be empty']
        },
        'emoji-enum': (parsed, when, value) => {
          return new Promise((resolve, reject) => {
            const commit = parsed as GitmojiCommit
            if (!commit.emoji) { resolve([true]) }

            const gitmojis = gitmoji.gitmojis.map(i => i.code)
            const emojis = commit.emoji.split('::').map((i:string) => {
              if (!i.startsWith(':')) { i = ':' + i }
              if (!i.endsWith(':')) { i = i + ':' }
              return i
            })

            emojis.forEach(emoji => {
              if (!gitmojis.includes(emoji)) {
                resolve([false, `Emoji type is not valid. "${emoji}"\nEmoji must be one of [${gitmojis.join(', ')}]\n`])
              }
            })

            resolve([true])
          })
        }
      }
    }
  ]
}
export default Configuration
