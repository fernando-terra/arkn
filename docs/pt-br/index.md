---
layout: home

hero:
  name: Arkn
  text: Convenções que você lê.
  tagline: Padrões que você impõe. Um framework .NET composável onde toda falha é explícita, todo padrão é verificado em tempo de compilação e assistentes de IA geram código correto na primeira tentativa.
  actions:
    - theme: brand
      text: Primeiros Passos
      link: /pt-br/getting-started
    - theme: alt
      text: Ver no GitHub
      link: https://github.com/fernando-terra/arkn

features:
  - icon: 🎯
    title: Result<T> em todo lugar
    details: Toda operação que pode falhar retorna Result<T>. Sem nulls escondidos, sem exceções surpresa, sem contratos ambíguos.
  - icon: ⚙️
    title: Zero dependências externas
    details: Os pacotes principais dependem apenas do .NET BCL. Sem Polly, sem Serilog, sem Hangfire. Você escolhe suas ferramentas.
  - icon: 🔍
    title: Verificação em tempo de compilação
    details: Os analyzers Roslyn ARK001–ARK008 impõem os padrões do Arkn na build, antes de o código chegar à produção.
  - icon: 🤖
    title: Nativo para MCP
    details: O primeiro framework .NET com servidor Model Context Protocol nativo. Claude, Cursor e Copilot geram código correto na primeira tentativa.
  - icon: 🧩
    title: Composável
    details: Instale apenas o que precisar. Cada pacote é útil de forma independente e se integra naturalmente com os demais.
  - icon: 📦
    title: 16 pacotes, um ecossistema
    details: Results, HTTP, Jobs, Logging, Notifications, Analyzers, SourceGen, MCP — todos seguindo as mesmas convenções.
---
