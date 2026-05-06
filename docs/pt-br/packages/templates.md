# Arkn.Templates

Templates `dotnet new` para iniciar projetos com Arkn.

```bash
dotnet new install Arkn.Templates
```

## Templates disponíveis

### `arkn-api` — Minimal API

```bash
dotnet new arkn-api -n MyApi
```

Inclui: `Arkn.Results`, usings globais, mapeamento error→HTTP, CI com GitHub Actions.

### `arkn-job` — Worker com background jobs

```bash
dotnet new arkn-job -n MyWorker
```

Inclui: `Arkn.Jobs`, `Arkn.Logging`, um `SampleJob` pronto para uso.

### `arkn-lib` — Class library

```bash
dotnet new arkn-lib -n MyLibrary
```

Inclui: `Arkn.Core`, `Arkn.Results`, `Arkn.Analyzers`, uma `SampleEntity` inicial com o padrão Result, README.
