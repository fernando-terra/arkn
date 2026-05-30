---
layout: home

hero:
  name: Arkn
  text: Conventions you can read.
  tagline: Patterns you can enforce. A zero-dependency .NET framework tailored for Clean Architecture and DDD, where failures are explicit and lock-in is eliminated.
  actions:
    - theme: brand
      text: Get Started
      link: /getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/fernando-terra/arkn

features:
  - icon: 🎯
    title: Failures as First-Class
    details: Stop using exceptions for control flow. Every operation that can fail returns Result<T>. No hidden nulls, no surprise exceptions, no ambiguous contracts.
  - icon: 🧩
    title: Agnostic Repositories
    details: Arkn.Repository provides a clean contract for your domain. Plug in EFCore, Dapper, or MongoDB via extensions without leaking infrastructure details into your business logic.
  - icon: ⚙️
    title: Zero Vendor Lock-in
    details: The core packages (Arkn.Core, Arkn.Results) have zero external NuGet dependencies. Your domain remains pure. No MediatR or EFCore in the center.
  - icon: 🔍
    title: Compile-Time Enforcement
    details: Arkn ships with Roslyn Analyzers (ARK001–ARK008) to enforce architectural patterns at build time, rejecting code smells before they reach production.
  - icon: 🤖
    title: AI-Native (MCP)
    details: The first .NET framework with a native Model Context Protocol server. Claude, Cursor, and Copilot scaffold correct DDD code on the first try, without hallucinations.
  - icon: 🧩
    title: Composable by Design
    details: Install only what you need. Need scheduling? Use Arkn.Jobs. Need a typed HTTP client? Use Arkn.Http. You are never forced to adopt the entire ecosystem.
  - icon: 📦
    title: Built for .NET 8 | 9 | 10
    details: Multi-targeted from the ground up to support the latest modern C# features while maintaining compatibility with your current LTS projects.
---
