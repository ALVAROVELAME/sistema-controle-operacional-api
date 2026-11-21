<div align="center">

# 📋 CtOperacional API

### Autenticação, tarefas e Pomodoro para o controle operacional

API REST em **ASP.NET Core 8** que dá suporte ao cadastro de usuários, ao quadro de tarefas e ao registro de eventos Pomodoro.

[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/banco-MySQL%20%2F%20MariaDB-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Docker](https://img.shields.io/badge/deploy-Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Nginx](https://img.shields.io/badge/proxy-Nginx-009639?logo=nginx&logoColor=white)](https://nginx.org/)
[![GitHub Actions](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?logo=githubactions&logoColor=white)](https://github.com/features/actions)
[![Testes](https://img.shields.io/badge/testes-xUnit-512BD4)](tests/README.md)

[🌐 Aplicação web](https://ctoperacional.vercel.app) · [📚 Guia de testes](tests/README.md) · [📋 Análise técnica](docs/ANALISE.md)

</div>

---

## 📖 Sobre o projeto

A API do **CtOperacional** oferece os recursos de autenticação e persistência usados pelo frontend de controle operacional. As tarefas são associadas ao usuário autenticado e podem acompanhar prioridade, prazo, status e contador de Pomodoros.

---

## ✨ Funcionalidades

- 🔐 Cadastro com confirmação por e-mail, recuperação de senha, login JWT e exclusão de conta com senha.
- 📋 Criação, listagem, atualização, importação e exclusão de tarefas.
- 🍅 Registro de eventos Pomodoro e incremento do contador de uma tarefa.
- 🧪 Testes unitários e de integração HTTP com banco isolado.
- 🚀 Publicação em container com fluxo de testes e deploy no GitHub Actions.

---

## Sumário

- [Tecnologias e arquitetura](#tecnologias-e-arquitetura)
- [Executar localmente](#executar-localmente)
- [Configuração](#configuração)
- [Docker](#docker)
- [Infraestrutura e deploy](#infraestrutura-e-deploy)
- [Endpoints](#endpoints)
- [Exemplos](#exemplos)
- [Testes automatizados](#testes-automatizados)
- [Integração contínua](#integração-contínua)
- [Banco e migrations](#banco-e-migrations)
- [Limitações e próximos passos](#limitações-e-próximos-passos)

---

## 🛠 Tecnologias e arquitetura

| Componente | Tecnologia / responsabilidade |
| --- | --- |
| API | .NET 8, ASP.NET Core Controllers |
| Banco | EF Core 8, Pomelo, MySQL/MariaDB |
| Autenticação | JWT Bearer assinado com HMAC SHA-256 |
| Senhas | BCrypt.Net-Next |
| E-mail | MailKit, SMTP com STARTTLS |
| Documentação HTTP | Swagger / OpenAPI |
| Testes | xUnit, Microsoft.AspNetCore.Mvc.Testing, HttpClient, FluentAssertions 7.2 |
| Persistência de teste | SQLite em memória, isolado por caso |
| Cobertura | Coverlet, formato Cobertura |
| Infraestrutura | VM compartilhada, Nginx, Docker Compose e GitHub Actions |

### 📁 Estrutura do projeto

```text
Controllers/       Rotas, autorização e respostas HTTP
DTOs/              Contratos de entrada/saída e validações
Services/          Regras de negócio, hash e envio de e-mail
Repositories/      Consultas e persistência via EF Core
Models/            Entidades de domínio
Data/              AppDbContext e mapeamentos
Migrations/        Histórico do esquema MySQL
Config/            Configuração JWT
Security/          Geração de tokens
Extensions/        Leitura de claims
Exceptions/        Exceções de negócio e tratamento global
Enums/             Estados de tarefas e eventos Pomodoro
tests/
  SistemaControleOperacionalApi.Tests/
    Infrastructure/  WebApplicationFactory, SQLite e e-mail capturado
    Integration/     Fluxos HTTP de autenticação, tarefas e Pomodoro
    Unit/            Hash, JWT e tratamento de exceções
.github/workflows/  Testes e deploy
SistemaControleOperacional.sln
```

O fluxo principal é `HTTP → Controller → Service → Repository → AppDbContext`. O controller de Pomodoro usa diretamente o repositório. `Cliente` existe no modelo, mas não possui endpoints.

---

## 🚀 Executar localmente

Pré-requisitos: SDK .NET 8 (inclui runtime ASP.NET Core 8), MySQL/MariaDB acessível e credenciais SMTP para o cadastro. Para executar somente os testes, banco externo e SMTP não são necessários.

```powershell
dotnet restore SistemaControleOperacional.sln
dotnet build SistemaControleOperacional.sln -c Release
Copy-Item .env.example .env  # Execute apenas se ainda não houver um .env configurado
```

Edite `.env` com suas configurações. O .NET **não importa esse arquivo automaticamente**. No PowerShell, carregue os pares `CHAVE=VALOR` no processo atual, sem executar seu conteúdo como código:

```powershell
Get-Content .env | ForEach-Object {
    $line = $_.Trim()
    if ($line -and !$line.StartsWith('#')) {
        $pair = $line -split '=', 2
        if ($pair.Length -eq 2) {
            [Environment]::SetEnvironmentVariable($pair[0].Trim(), $pair[1], 'Process')
        }
    }
}
dotnet run --project sistema-controle-operacional-api.csproj
```

Esse carregador aceita o formato simples de `.env.example`, sem aspas nem comentários no fim dos valores. O perfil local usa `http://localhost:8080`; não há rota de health check HTTP na raiz. Swagger: `http://localhost:8080/swagger` em Development. As migrations são aplicadas na inicialização, portanto o banco deve estar disponível.

---

## ⚙️ Configuração

| Variável | Uso / padrão |
| --- | --- |
| `DB_CONNECTION_STRING` | String completa; tem precedência sobre as variáveis individuais |
| `DB_SERVER` | Host do MySQL/MariaDB |
| `DB_PORT` | Porta, padrão `3306` |
| `DB_DATABASE` | Nome do banco |
| `DB_USER` / `DB_PASSWORD` | Credenciais do banco |
| `JWT_SECRET` | Segredo de assinatura; mínimo de 32 caracteres |
| `JWT_EXPIRATION` | Validade em **milissegundos**, padrão `86400000` (24 h) |
| `JWT_ISSUER` | Padrão `sistema-controle-operacional-api` |
| `JWT_AUDIENCE` | Padrão igual ao emissor |
| `MAIL_HOST` / `MAIL_PORT` | Servidor SMTP e porta STARTTLS, geralmente `587` |
| `MAIL_USERNAME` / `MAIL_PASSWORD` | Autenticação SMTP |
| `MAIL_FROM` | Endereço remetente |
| `APP_FRONTEND_URL` | Base do link de confirmação, padrão `http://localhost:5173` |
| `SWAGGER_ENABLED` | `true` habilita Swagger fora de Development |
| `Database__ApplyMigrations` | Padrão `true`; `false` desabilita aplicação automática de migrations |

Não versione `.env`. O arquivo `.env.example` contém somente exemplos. Os testes fornecem configuração própria pelo host e não carregam `.env` nem alteram variáveis globais do processo.

CORS permite atualmente `https://ctoperacional.vercel.app`, `http://localhost:5173` e `http://localhost:4173`, com credenciais. Essas origens estão em `Program.cs`.

---

## 🐳 Docker

```powershell
docker compose up -d --build
docker compose logs -f sistema_controle_operacional_api
docker compose down
```

O Compose carrega `.env` e publica `127.0.0.1:3001 → 8080`. Ele contém **somente a API**: o banco e o SMTP precisam estar acessíveis pelo container. Dentro do container, `localhost` é o próprio container; use o endereço do banco apropriado ao ambiente (por exemplo, `host.docker.internal` no Docker Desktop quando o banco está no host).

O Dockerfile usa build em múltiplos estágios e publica explicitamente o projeto da API. Testes, resultados e ferramentas locais ficam fora do contexto Docker.

---

## 🚢 Infraestrutura e deploy

Esta API está **em produção**, na mesma máquina virtual e infraestrutura compartilhada com a outra API. O código é enviado pelo GitHub e o GitHub Actions executa o deploy na VM, onde a aplicação é construída e executada em um container Docker. Os detalhes abaixo descrevem a configuração deste serviço no repositório.

```text
Deploy: push para main → GitHub Actions → runner na VM → Docker Compose

Requisições:
Frontend CtOperacional → Nginx na VM → 127.0.0.1:3001
                                            │
                                            ▼
                                  Container da API :8080
                                      │           │
                                      ▼           ▼
                               MySQL/MariaDB     SMTP
```

O README de referência informa `https://controle-operacional-api.duckdns.org` como endereço configurado no frontend. O Nginx faz parte da infraestrutura compartilhada da VM. Seus arquivos de configuração, assim como as configurações de DNS e TLS, não estão neste repositório; o vínculo desse endereço com este container não foi verificado a partir do código.

### Serviço nesta VM

| Item | Configuração neste repositório |
| --- | --- |
| Diretório de deploy | `/srv/apps/sistema-controle-operacional-api` |
| Proxy reverso | Nginx na infraestrutura compartilhada da VM |
| Orquestração | Docker Compose; o arquivo define somente esta API |
| Container | `sistema_controle_operacional_api` |
| Porta | `127.0.0.1:3001` na VM para `8080` no container |
| Ambiente | `.env` no diretório do projeto na VM |
| Recursos | Limites configurados de 350 MB e 0,50 CPU |
| Reinício | `unless-stopped` |
| Logs | Driver `json-file`, até 3 arquivos de 10 MB |
| Healthcheck | Verificação TCP configurada; compatibilidade com o shell da imagem ainda precisa ser validada |

A porta está vinculada ao loopback da VM. O Nginx faz parte do acesso externo na infraestrutura compartilhada; o Compose deste projeto não configura o Nginx. Como a máquina é compartilhada, confira portas, nomes de containers, recursos e configuração do serviço existente antes de alterar a implantação.

### Fluxo de publicação

Um push na branch `main` aciona o workflow de deploy. Primeiro o GitHub Actions executa os testes; se passarem, um runner `self-hosted` atualiza o código em `/srv/apps/sistema-controle-operacional-api`, reconstrói a imagem e reinicia o serviço com Docker Compose. O workflow também executa `docker image prune -f`, que pode afetar imagens não utilizadas por outros projetos da VM.

Para operação na VM, a partir do diretório deste projeto:

```bash
cd /srv/apps/sistema-controle-operacional-api
docker compose ps
docker compose logs --tail=100 -f sistema_controle_operacional_api
```

O arquivo `.env` é necessário no servidor e deve conter as credenciais do banco, JWT e SMTP. O banco e o servidor SMTP são serviços externos ao Compose; suas localizações e acessos devem ser conferidos na VM.

---

---

## 🌐 Endpoints

Rotas protegidas exigem `Authorization: Bearer <token>`. O Swagger também permite informar esse cabeçalho pelo botão **Authorize**; como o esquema é do tipo API key, informe `Bearer ` seguido do token.

| Método | Rota | JWT | Resultado principal |
| --- | --- | --- | --- |
| POST | `/api/usuarios` | Não | 200, inicia cadastro e envia confirmação |
| GET | `/api/auth/confirmar?token=...` | Não | 200, ativa conta; 400 para token inválido/expirado |
| POST | `/api/auth/recuperar-senha` | Não | 200 com resposta genérica; envia link para contas ativas |
| POST | `/api/auth/redefinir-senha` | Não | 200 ao alterar senha; 400 para token inválido ou expirado |
| POST | `/api/auth/login` | Não | 200 com JWT; 401 para credenciais/conta inválidas |
| GET | `/api/auth/me` | Sim | 200 com perfil; 404 se usuário ausente |
| GET | `/api/usuarios/me` | Sim | 200 com perfil; 404 se usuário ausente |
| POST | `/api/usuarios/me/excluir` | Sim | 200, exige senha no corpo |
| GET | `/api/tarefas` | Sim | 200, tarefas do usuário em ordem de criação |
| POST | `/api/tarefas` | Sim | 201, cria tarefa |
| PUT | `/api/tarefas/{id}` | Sim | 200, substitui campos editáveis; 404 se ausente/de outro usuário |
| PATCH | `/api/tarefas/{id}/status` | Sim | 204, muda status; 404 se ausente/de outro usuário |
| POST | `/api/tarefas/{id}/pomodoros` | Sim | 204, incrementa contador; 404 se ausente/de outro usuário |
| DELETE | `/api/tarefas/{id}` | Sim | 204, exclui tarefa; 404 se ausente/de outro usuário |
| DELETE | `/api/tarefas` | Sim | 204, remove todas as tarefas do usuário |
| POST | `/api/tarefas/importar` | Sim | 200, recebe array de tarefas |
| POST | `/api/pomodoros/eventos` | Sim | 204, registra evento |

JSON usa propriedades `camelCase`, enums textuais em `snake_case` e omite propriedades nulas. A configuração também permite valores numéricos para enums; valores fora dos membros declarados não são explicitamente validados. IDs de tarefas são strings; IDs de usuários são números. Datas de prazo usam `YYYY-MM-DD`; timestamps usam ISO 8601.

| Enum | Valores textuais |
| --- | --- |
| Prioridade | `baixa`, `media`, `alta` |
| Status | `a_fazer`, `em_progresso`, `revisao`, `concluido` |
| Modo Pomodoro | `foco`, `pausa_curta`, `pausa_longa` |
| Tipo de evento | `foco_iniciado`, `foco_pausado`, `foco_retomado`, `foco_completado`, `foco_interrompido`, `pausa_iniciada`, `pausa_pausada`, `pausa_retomada`, `pausa_completada`, `pausa_interrompida`, `modo_alterado` |

Validações de DTO retornam `400` com `ValidationProblemDetails` (`errors`). Exceções de negócio usam `{ "sucesso": false, "mensagem": "..." }`. Falhas de autenticação do middleware e alguns `404` não usam esse envelope. Erros inesperados retornam mensagem genérica com status `500`.

---

## 📝 Exemplos

Cadastro (`POST /api/usuarios`):

```json
{ "nome": "Maria Silva", "email": "maria@example.com", "senha": "SenhaExemplo123!" }
```

A recuperação recebe `{"email":"..."}` e envia um link para `/redefinir-senha?token=...`; a redefinição recebe `{"token":"...","senha":"..."}`. O token de recuperação expira em 30 minutos, é armazenado como hash e aceita apenas um uso. Uma nova solicitação invalida a anterior. As duas rotas limitam, em conjunto, 10 requisições por IP a cada 15 minutos, por instância. A alteração de senha não revoga JWTs já emitidos.

O token de confirmação expira após 24 horas; a API não remove automaticamente o cadastro pendente expirado. O link enviado aponta para o frontend em `/confirmar?token=...`. O frontend precisa chamar a rota de confirmação da API. Após confirmar, faça login (`POST /api/auth/login`):

```json
{ "email": "maria@example.com", "senha": "SenhaExemplo123!" }
```

Use o campo `token` da resposta nas próximas requisições. Criação/atualização de tarefa:

```json
{
  "titulo": "Revisar relatório",
  "descricao": "Conferir os indicadores do mês",
  "prioridade": "alta",
  "status": "a_fazer",
  "prazo": "2027-01-20",
  "pomodorosPlanejados": 3
}
```

Título é obrigatório (até 200 caracteres), descrição aceita até 5000 e estimativa aceita 1–20 ou null. O contador `pomodoros` começa em zero e só aumenta pela rota própria; PUT não altera esse contador.

Evento Pomodoro:

```json
{
  "tipo": "foco_completado",
  "modo": "foco",
  "ciclo": 1,
  "minutosPlanejados": 25,
  "minutosReais": 25,
  "segundosReais": 1500,
  "ocorridoEm": "2027-01-20T12:00:00Z",
  "tarefaId": "1",
  "tarefaTitulo": "Revisar relatório",
  "statusTarefa": "em_progresso"
}
```

Eventos aceitam ciclos de 1–8, minutos de 0–120 e segundos de 0–7200. Registrar evento não incrementa automaticamente o contador da tarefa. O `tarefaId` é opcional: se não puder ser convertido para número, é salvo como nulo; a API não verifica a existência ou a propriedade da tarefa informada.

---

## 🧪 Testes automatizados

```powershell
# Toda a suíte
dotnet test SistemaControleOperacional.sln -c Release

# Somente testes unitários
dotnet test SistemaControleOperacional.sln --filter "FullyQualifiedName~.Unit."

# Somente integração HTTP
dotnet test SistemaControleOperacional.sln --filter "FullyQualifiedName~.Integration."

# Resultados TRX e cobertura Cobertura
dotnet test SistemaControleOperacional.sln -c Release --logger trx --collect:"XPlat Code Coverage" --results-directory TestResults
```

Abra a solução no Visual Studio e use o **Test Explorer**, ou execute pelo terminal. Resultados gerados ficam em `TestResults/`, ignorado pelo Git. Consulte [tests/README.md](tests/README.md) para a estratégia e como ampliar a suíte.

Na execução documentada em 15/11/2026, **67 testes foram aprovados** e o build Release terminou sem avisos ou erros. Consulte [docs/VALIDACAO.md](docs/VALIDACAO.md) para cobertura, limites e comandos com o runtime local preparado neste computador.

Cada caso de integração inicia uma API com `WebApplicationFactory<Program>` e envia requisições com `HttpClient` para o servidor em memória. Controllers, serialização, validação, autenticação JWT, serviços e repositórios são os reais. Substituímos apenas MySQL por SQLite e SMTP por um capturador de mensagens. Nenhum serviço externo precisa estar rodando.

O SQLite é relacional e aplica índices únicos, mas **não comprova compatibilidade das migrations ou do SQL com MySQL/MariaDB**. Um conversor exclusivo dos testes persiste `DateTimeOffset` como ticks UTC para permitir ordenação no SQLite.

FluentAssertions está fixado em 7.2, linha anterior à mudança de licença comercial da versão 8. Referências: [testes de integração no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) e [licenciamento do FluentAssertions](https://fluentassertions.com/releases/).

---

## 🔄 Integração contínua

`tests.yml` restaura, compila em Release, executa testes e publica TRX/cobertura como artefato em pushes para `main` e pull requests. Não exige secrets, MySQL ou SMTP.

`deploy.yml` chama o workflow de testes e só inicia o job de deploy após sucesso. O deploy de produção utiliza runner próprio, Docker e o diretório `/srv/apps/sistema-controle-operacional-api`; o `.env` precisa existir nesse servidor. Como os workflows também rodam separadamente em `main`, haverá uma execução de teste independente e outra vinculada ao deploy.

---

## 🗄️ Banco e migrations

O projeto contém a migration `InitialCreate`. Na execução normal, `Database.Migrate()` aplica migrations pendentes, salvo quando `Database__ApplyMigrations=false`. O usuário de banco precisa das permissões necessárias para isso. Em testes usamos `EnsureCreated()` somente no SQLite isolado.

Com a ferramenta `dotnet-ef` 8 instalada e configuração de banco disponível:

```powershell
dotnet ef migrations add NomeDaAlteracao --project sistema-controle-operacional-api.csproj
dotnet ef database update --project sistema-controle-operacional-api.csproj --connection "<conexao-do-banco-alvo>"
```

A inicialização de design-time usa conexão fictícia `design_time` e MySQL 8.0.36 para gerar o modelo sem autodetecção. Para aplicar migrations pela CLI, informe explicitamente a conexão correta. Não coloque credenciais reais em scripts versionados.

---

## 🗺️ Limitações e próximos passos

O envio SMTP ocorre depois de salvar o cadastro pendente: uma falha de envio pode deixar esse cadastro no banco. A exclusão da conta não revoga imediatamente o JWT nem remove automaticamente tarefas e eventos associados. A análise técnica e os limites da validação estão em [docs/ANALISE.md](docs/ANALISE.md). A suíte cobre os fluxos descritos, mas não substitui testes com MySQL real, entrega SMTP, infraestrutura Docker, carga ou segurança especializada.

---

## 🤝 Contribuição

1. Crie uma branch para a mudança.
2. Implemente e execute `dotnet test SistemaControleOperacional.sln -c Release`.
3. Atualize a documentação quando mudar contratos ou configuração.
4. Abra um pull request com a descrição da alteração e das verificações realizadas.

<div align="center">

**CtOperacional API** · Backend para tarefas e foco

[⬆ Voltar ao topo](#-ctoperacional-api)

</div>
