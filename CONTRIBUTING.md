# Contributing to Inride Fair

Thanks for contributing to Inride Fair.

This repository contains a Windows WPF application for local cheat and suspicious artifact detection. Contributions should favor correctness, explainability, and low false-positive risk.

## Ways to Help

- Report reproducible bugs
- Improve scan coverage or heuristics
- Add tests for existing behavior
- Improve logging, diagnostics, or documentation
- Refine UI/UX without breaking established workflows

## Development Setup

Requirements:

- .NET 11 SDK or newer
- Windows 10/11 x64
- Git

Clone and restore:

```powershell
git clone https://github.com/InrideSoftware/InrideFair.git
cd InrideFair
dotnet restore InrideFair.sln
```

Build:

```powershell
dotnet build InrideFair.sln -c Release
```

Run tests:

```powershell
dotnet test InrideFair.sln -c Release
```

Run the application:

```powershell
dotnet run --project InrideFair/InrideFair.csproj -c Release
```

## Branching

Create feature branches from `main`.

```powershell
git checkout main
git pull origin main
git checkout -b feature/my-change
```

## Coding Rules

- Follow the existing `.editorconfig`
- Use `PascalCase` for public types and members
- Use `camelCase` for locals and parameters
- Use `_camelCase` for private fields
- Keep nullable reference types enabled
- Prefer typed models over weakly typed dictionaries
- Fix root causes instead of layering UI-only or logging-only patches

## Logging and Diagnostics

- Use the existing logging service abstractions
- Keep log messages actionable and specific
- Avoid noisy duplicate logs for the same operation

## Tests

Any non-trivial change should include one of the following:

- automated test coverage
- a strong reason why the change is not practical to automate
- manual verification steps in the PR description

Before opening a PR, run:

```powershell
dotnet build InrideFair.sln -c Release
dotnet test InrideFair.sln -c Release
```

## Pull Requests

PRs should include:

- what changed
- why it changed
- risk or regression notes
- how it was tested
- screenshots for visible UI changes

Recommended checklist:

- [ ] Builds successfully in Release
- [ ] Tests pass locally
- [ ] No unrelated files were modified
- [ ] Documentation updated if behavior changed
- [ ] Release scripts updated if packaging changed

## Areas That Need Extra Care

- detection heuristics and signatures
- archive scanning behavior
- browser and registry access
- report generation output formats
- release packaging and GitHub release automation

## Security Expectations

- Do not add automatic destructive remediation
- Treat detections as indicators, not proof
- Minimize false positives where possible

## Questions and Issues

Use GitHub Issues for bug reports and improvement proposals:

https://github.com/InrideSoftware/InrideFair/issues

---

## 📞 Контакты

| | |
|---|---|
| **Email** | inridesoftware@gmail.com |
| **GitHub** | https://github.com/InrideSoftware |
| **Website** | in progress... |

---

**Спасибо за ваш вклад!** 🙏

Ваш код поможет сделать Inride Fair лучше для всех пользователей.
