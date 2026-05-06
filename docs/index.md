---
layout: home

hero:
  name: Arkn
  text: Conventions you can read.
  tagline: Patterns you can enforce. A composable .NET framework where every failure is explicit, every pattern is enforced at compile time, and AI assistants generate correct code on the first try.
  actions:
    - theme: brand
      text: Get Started
      link: /getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/fernando-terra/arkn

features:
  - icon: 🎯
    title: Result<T> everywhere
    details: Every operation that can fail returns Result<T>. No hidden nulls, no surprise exceptions, no ambiguous contracts.
  - icon: ⚙️
    title: Zero external dependencies
    details: Core packages depend only on the .NET BCL. No Polly, no Serilog, no Hangfire. You bring your own tools.
  - icon: 🔍
    title: Compile-time enforcement
    details: ARK001–ARK008 Roslyn analyzers enforce Arkn patterns at build time, before code reaches production.
  - icon: 🤖
    title: MCP-native
    details: The first .NET framework with a native Model Context Protocol server. Claude, Cursor, and Copilot scaffold correct code on the first try.
  - icon: 🧩
    title: Composable
    details: Install only what you need. Every package is independently useful and integrates naturally with the others.
  - icon: 📦
    title: 16 packages, one ecosystem
    details: Results, HTTP, Jobs, Logging, Notifications, Analyzers, SourceGen, MCP — all following the same conventions.
---
