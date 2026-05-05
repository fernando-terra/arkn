# Arkn.Templates

`dotnet new` templates for starting Arkn-powered projects.

```bash
dotnet new install Arkn.Templates
```

## Available templates

### `arkn-api` — Minimal API

```bash
dotnet new arkn-api -n MyApi
```

Includes: `Arkn.Results`, global usings, error→HTTP mapping, GitHub Actions CI.

### `arkn-job` — Worker with background jobs

```bash
dotnet new arkn-job -n MyWorker
```

Includes: `Arkn.Jobs`, `Arkn.Logging`, a ready-to-use `SampleJob`.

### `arkn-lib` — Class library

```bash
dotnet new arkn-lib -n MyLibrary
```

Includes: `Arkn.Core`, `Arkn.Results`, `Arkn.Analyzers`, a starter `SampleEntity` with Result pattern, README.
