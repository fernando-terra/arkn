import { defineConfig } from 'vitepress'

const enNav = [
  { text: 'Guide', link: '/getting-started' },
  { text: 'MCP Server', link: '/mcp' },
  { text: 'Packages', link: '/packages/core' },
  {
    text: 'v0.3.0',
    items: [
      { text: 'Changelog', link: 'https://github.com/fernando-terra/arkn/blob/main/CHANGELOG.md' },
      { text: 'Contributing', link: '/contributing' },
    ]
  }
]

const ptBrNav = [
  { text: 'Guia', link: '/pt-br/getting-started' },
  { text: 'Servidor MCP', link: '/pt-br/mcp' },
  { text: 'Pacotes', link: '/pt-br/packages/core' },
  {
    text: 'v0.3.0',
    items: [
      { text: 'Changelog', link: 'https://github.com/fernando-terra/arkn/blob/main/CHANGELOG.md' },
      { text: 'Contribuindo', link: '/pt-br/contributing' },
    ]
  }
]

const enSidebar = [
  {
    text: 'Introduction',
    items: [
      { text: 'Getting Started', link: '/getting-started' },
      { text: 'MCP Server ✨', link: '/mcp' },
      { text: 'Contributing', link: '/contributing' },
    ]
  },
  {
    text: 'Packages',
    items: [
      { text: 'Arkn.Core', link: '/packages/core' },
      { text: 'Arkn.Results', link: '/packages/results' },
      { text: 'Arkn.Http', link: '/packages/http' },
      {
        text: 'Arkn.Logging',
        link: '/packages/logging',
        items: [
          { text: 'Seq', link: '/packages/seq' },
          { text: 'Elasticsearch', link: '/packages/elasticsearch' },
          { text: 'Application Insights', link: '/packages/appinsights' },
        ]
      },
      { text: 'Arkn.Jobs', link: '/packages/jobs' },
      {
        text: 'Arkn.Notifications',
        link: '/packages/notifications',
        items: [
          { text: 'Slack', link: '/packages/slack' },
          { text: 'Email', link: '/packages/email' },
          { text: 'Teams', link: '/packages/teams' },
          { text: 'Discord', link: '/packages/discord' },
        ]
      },
      { text: 'Arkn.Analyzers', link: '/packages/analyzers' },
      { text: 'Arkn.SourceGen', link: '/packages/sourcegen' },
      { text: 'Arkn.Templates', link: '/packages/templates' },
      { text: 'Arkn.MCP', link: '/packages/mcp' },
    ]
  }
]

const ptBrSidebar = [
  {
    text: 'Introdução',
    items: [
      { text: 'Primeiros Passos', link: '/pt-br/getting-started' },
      { text: 'Servidor MCP ✨', link: '/pt-br/mcp' },
      { text: 'Contribuindo', link: '/pt-br/contributing' },
    ]
  },
  {
    text: 'Pacotes',
    items: [
      { text: 'Arkn.Core', link: '/pt-br/packages/core' },
      { text: 'Arkn.Results', link: '/pt-br/packages/results' },
      { text: 'Arkn.Http', link: '/pt-br/packages/http' },
      {
        text: 'Arkn.Logging',
        link: '/pt-br/packages/logging',
        items: [
          { text: 'Seq', link: '/pt-br/packages/seq' },
          { text: 'Elasticsearch', link: '/pt-br/packages/elasticsearch' },
          { text: 'Application Insights', link: '/pt-br/packages/appinsights' },
        ]
      },
      { text: 'Arkn.Jobs', link: '/pt-br/packages/jobs' },
      {
        text: 'Arkn.Notifications',
        link: '/pt-br/packages/notifications',
        items: [
          { text: 'Slack', link: '/pt-br/packages/slack' },
          { text: 'Email', link: '/pt-br/packages/email' },
          { text: 'Teams', link: '/pt-br/packages/teams' },
          { text: 'Discord', link: '/pt-br/packages/discord' },
        ]
      },
      { text: 'Arkn.Analyzers', link: '/pt-br/packages/analyzers' },
      { text: 'Arkn.SourceGen', link: '/pt-br/packages/sourcegen' },
      { text: 'Arkn.Templates', link: '/pt-br/packages/templates' },
      { text: 'Arkn.MCP', link: '/pt-br/packages/mcp' },
    ]
  }
]

export default defineConfig({
  title: 'Arkn',
  description: 'Architecture Kernel for modern .NET — design patterns as composable packages',
  base: '/arkn/',
  appearance: { default: 'dark' },

  head: [
    ['link', { rel: 'icon', href: '/arkn/favicon.ico' }],
  ],

  locales: {
    root: {
      label: 'English',
      lang: 'en',
    },
    'pt-br': {
      label: 'Português',
      lang: 'pt-BR',
      themeConfig: {
        nav: ptBrNav,
        sidebar: ptBrSidebar,
        darkModeSwitchLabel: 'Aparência',
        lightModeSwitchTitle: 'Mudar para tema claro',
        darkModeSwitchTitle: 'Mudar para tema escuro',
      }
    }
  },

  themeConfig: {
    logo: '/logo.svg',
    siteTitle: 'Arkn',

    nav: enNav,
    sidebar: enSidebar,

    socialLinks: [
      { icon: 'github', link: 'https://github.com/fernando-terra/arkn' }
    ],

    footer: {
      message: 'Released under the Apache 2.0 License.',
      copyright: 'Copyright © 2026 Fernando Terra'
    },

    darkModeSwitchLabel: 'Appearance',
    lightModeSwitchTitle: 'Switch to light theme',
    darkModeSwitchTitle: 'Switch to dark theme',

    editLink: {
      pattern: 'https://github.com/fernando-terra/arkn/edit/main/docs/:path',
      text: 'Edit this page on GitHub'
    },

    search: {
      provider: 'local'
    }
  }
})
