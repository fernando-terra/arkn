import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Arkn',
  description: 'Architecture Kernel for modern .NET — design patterns as composable packages',
  base: '/arkn/',
  appearance: 'dark',

  head: [
    ['link', { rel: 'icon', href: '/arkn/favicon.ico' }],
  ],

  themeConfig: {
    logo: '/logo.svg',
    siteTitle: 'Arkn',

    nav: [
      { text: 'Guide', link: '/getting-started' },
      { text: 'MCP Server', link: '/mcp' },
      { text: 'Packages', link: '/packages/core' },
      { text: 'Roadmap', link: '/roadmap' },
      {
        text: 'v0.2.0',
        items: [
          { text: 'Changelog', link: 'https://github.com/fernando-terra/arkn/blob/main/CHANGELOG.md' },
          { text: 'Contributing', link: '/contributing' },
        ]
      }
    ],

    sidebar: [
      {
        text: 'Introduction',
        items: [
          { text: 'Getting Started', link: '/getting-started' },
          { text: 'MCP Server ✨', link: '/mcp' },
          { text: 'Contributing', link: '/contributing' },
        ]
      },
      {
        text: 'Core Packages',
        items: [
          { text: 'Arkn.Core', link: '/packages/core' },
          { text: 'Arkn.Results', link: '/packages/results' },
          { text: 'Arkn.Http', link: '/packages/http' },
          { text: 'Arkn.Logging', link: '/packages/logging' },
          { text: 'Arkn.Jobs', link: '/packages/jobs' },
          { text: 'Arkn.Notifications', link: '/packages/notifications' },
          { text: 'Arkn.Analyzers', link: '/packages/analyzers' },
          { text: 'Arkn.SourceGen', link: '/packages/sourcegen' },
          { text: 'Arkn.Templates', link: '/packages/templates' },
          { text: 'Arkn.MCP', link: '/packages/mcp' },
        ]
      },
      {
        text: 'Extensions',
        items: [
          { text: 'Application Insights', link: '/packages/appinsights' },
          { text: 'Seq', link: '/packages/seq' },
          { text: 'Elasticsearch', link: '/packages/elasticsearch' },
          { text: 'Email', link: '/packages/email' },
          { text: 'Slack', link: '/packages/slack' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/fernando-terra/arkn' }
    ],

    footer: {
      message: 'Released under the Apache 2.0 License.',
      copyright: 'Copyright © 2026 Fernando Terra'
    },

    editLink: {
      pattern: 'https://github.com/fernando-terra/arkn/edit/main/docs/:path',
      text: 'Edit this page on GitHub'
    },

    search: {
      provider: 'local'
    }
  }
})
