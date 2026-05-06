# Contribuindo

Consulte o [CONTRIBUTING.md](https://github.com/fernando-terra/arkn/blob/main/CONTRIBUTING.md) para o guia completo.

## Referência rápida

### Nomenclatura de branches

| Prefixo | Finalidade |
|---|---|
| `feat/` | Nova funcionalidade |
| `fix/` | Correção de bug |
| `docs/` | Documentação |
| `chore/` | Ferramental, build |
| `test/` | Apenas testes |

### Formato de commit (Conventional Commits)

```
feat(results): add TapAsync extension method
fix(jobs): prevent double-fire within the same minute
docs(logging): add Elasticsearch sink example
```

### Regras

- Abra uma issue antes de implementar funcionalidades não triviais
- Todo PR deve incluir testes
- `Arkn.Core` e `Arkn.Results` devem permanecer **sem dependências externas**
- Mínimo de 1 aprovação de code review antes do merge
