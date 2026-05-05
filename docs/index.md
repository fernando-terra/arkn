---
layout: home

hero:
  name: "Arkn"
  text: "Architecture Kernel for modern .NET"
  tagline: Design patterns as composable, dependency-free packages.
  image:
    src: /logo.svg
    alt: Arkn
  actions:
    - theme: brand
      text: Get Started
      link: /getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/fernando-terra/arkn

features:
  - icon: 🔒
    title: Zero Lock-in
    details: Core packages have no external NuGet dependencies. No MediatR, no Serilog, no EF Core unless you choose.
  - icon: 🧩
    title: Composable
    details: Each pattern lives in its own package. Take one, take all. Mix and match as your project evolves.
  - icon: 🔍
    title: Explicit
    details: No magic. Every behavior is visible, testable, and overridable. Arkn enforces patterns at compile time via Roslyn analyzers.
  - icon: ⚡
    title: Modern .NET
    details: Built for .NET 10. Takes advantage of primary constructors, collection expressions, and the latest C# features.
---
