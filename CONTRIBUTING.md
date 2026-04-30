# Contributing to Arkn

Thank you for your interest in contributing! This document outlines everything
you need to get started.

---

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).
By participating, you agree to abide by its terms.

---

## Branches

| Branch pattern      | Purpose                              |
|---------------------|--------------------------------------|
| `main`              | Always releasable. Protected.        |
| `feat/<name>`       | New features                         |
| `fix/<name>`        | Bug fixes                            |
| `docs/<name>`       | Documentation changes only           |
| `chore/<name>`      | Tooling, CI, dependency updates      |
| `refactor/<name>`   | Internal restructuring, no behavior change |

Always branch from `main` and open a PR back to `main`.

---

## Commit Messages — Conventional Commits

All commits must follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short description>

[optional body]

[optional footer: BREAKING CHANGE or issue refs]
```

### Types

| Type       | When to use                                          |
|------------|------------------------------------------------------|
| `feat`     | A new feature visible to consumers                   |
| `fix`      | A bug fix                                            |
| `docs`     | Documentation only                                   |
| `test`     | Adding or fixing tests                               |
| `refactor` | Code restructuring without behavior change           |
| `chore`    | Build, CI, dependency, or tooling changes            |
| `perf`     | Performance improvements                             |

### Examples

```
feat(results): add MapAsync / BindAsync overloads
fix(results): throw on null value in Result<T>.Ok
docs(results): document Bind chain short-circuit behavior
chore(ci): upgrade to actions/setup-dotnet@v4
```

---

## Development Setup

```bash
# Requires .NET 10 SDK
dotnet --version   # should be 10.x.x

# Clone
git clone https://github.com/fernando-terra/arkn.git
cd arkn

# Restore & build
dotnet restore Arkn.sln
dotnet build Arkn.sln

# Run all tests
dotnet test Arkn.sln
```

---

## Pull Request Requirements

Every PR **must**:

1. **Include tests** — new behavior = new tests; changed behavior = updated tests.
   CI will fail if test coverage drops on changed code paths.
2. **Pass CI** — both Ubuntu and Windows builds must be green.
3. **Follow commit conventions** — squash or rebase before requesting review.
4. **Not add external NuGet dependencies** to `Arkn.Core` or `Arkn.Results`.
   These packages are dependency-free by design.
5. **Include XML doc comments** for any new public API.

### Code Review

- At least **one approval** is required before merging.
- Reviewer may request changes; address them with new commits (do not force-push
  after review has started).
- Use **Squash and Merge** for feature branches to keep `main` history clean.

---

## Reporting Issues

Use the GitHub issue templates:

- [Bug report](.github/ISSUE_TEMPLATE/bug_report.md)
- [Feature request](.github/ISSUE_TEMPLATE/feature_request.md)

For security vulnerabilities, **do not** open a public issue.
Email `security@bluezee.io` instead.
