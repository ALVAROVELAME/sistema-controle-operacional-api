# Resultado da validação

Execução local em 15/11/2026, Windows, SDK .NET 10.0.302 compilando o alvo `net8.0`, com runtime ASP.NET Core/.NET 8.0.34 instalado localmente em `.artifacts/dotnet`.

| Verificação | Resultado |
| --- | --- |
| Restore da solução | Concluído |
| Build Release | Aprovado, zero avisos e zero erros |
| Testes xUnit | **67 aprovados, 0 falhas, 0 ignorados** |
| Tempo informado pelo executor | 28 segundos |
| Publicação Release da API | Concluída em `.artifacts/publish` |
| `git diff --check` | Sem problemas |
| `docker compose config --quiet` | Configuração válida |
| Build/execução Docker | Não executados: daemon Docker Desktop indisponível |

## Cobertura coletada

- Linhas: **46,86%** (576 de 1229).
- Ramificações: **69,11%**.
- Escopo: assembly completo da API, incluindo migrations, inicialização e SMTP. Não foram excluídos arquivos para elevar o percentual.

Os relatórios locais da execução aprovada estão em `TestResults/verified/`: arquivo `.trx` na pasta e `coverage.cobertura.xml` na subpasta da execução. Esses artefatos não são versionados. Execuções intermediárias de desenvolvimento podem existir em outras subpastas de `TestResults`; o resultado acima corresponde à execução `verified`.

## Reexecutar neste computador

O ambiente original tinha SDK 10, mas não o runtime ASP.NET Core 8. Para usar a instalação local preparada durante a tarefa, execute na raiz:

```powershell
$env:DOTNET_ROOT = Join-Path $PWD '.artifacts/dotnet'
$env:DOTNET_ROOT_X64 = $env:DOTNET_ROOT
dotnet test SistemaControleOperacional.sln -c Release --logger trx --collect:"XPlat Code Coverage" --results-directory TestResults
```

Em máquinas com SDK/runtime ASP.NET Core 8 instalados normalmente, basta `dotnet test SistemaControleOperacional.sln -c Release`. A CI instala o SDK 8 automaticamente. A pasta `.artifacts` é local e não entra no Git nem na imagem Docker.

## O que não foi validado

Não houve conexão ao MySQL/MariaDB configurado em `.env`, aplicação de migrations em banco externo, envio SMTP real, deploy ou teste de carga. Os testes HTTP rodaram no servidor em memória com SQLite relacional isolado. Consulte [ANALISE.md](ANALISE.md) para os pontos funcionais e de integridade ainda existentes.
