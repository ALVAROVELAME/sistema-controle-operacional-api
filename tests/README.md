# Guia dos testes

## Organização

- `Infrastructure/ApiFactory.cs`: host isolado, configuração de teste, conexão SQLite e mensagens capturadas.
- `Infrastructure/IntegrationTest.cs`: criação do esquema, HttpClient, login real e descarte por caso de teste.
- `Integration/AuthTests.cs`: cadastro, confirmação, duplicidade, expiração, login, perfis, exclusão e autorização das rotas.
- `Integration/TarefasTests.cs`: CRUD, JSON, contador Pomodoro, validação, importação e isolamento entre usuários.
- `Integration/PomodorosTests.cs`: persistência de eventos e limites numéricos.
- `Unit/`: BCrypt, assinatura/configuração JWT e mapeamento de exceções.

## Princípios

1. Cada teste tem seu próprio host e banco. Não existe dependência de ordem, banco compartilhado ou limpeza de dados reais.
2. Use Arrange / Act / Assert e nomes que descrevam o comportamento esperado.
3. Verifique status, contrato e efeito persistido quando relevante. Um status 200 sozinho não comprova a regra de negócio.
4. Autenticação não é simulada: o helper cadastra um usuário no banco de teste e obtém JWT pelo endpoint real de login.
5. Use endereços fictícios `example.test`; a implementação `IEmailService` capturada nunca abre conexão SMTP.
6. Evite sleeps e relógios exatos: para expiração, prepare dados claramente vencidos; para timestamps gerados, use tolerância.
7. Não coloque credenciais nem leia `.env` nos testes.

## Adicionar um teste HTTP

Crie uma classe derivada de `IntegrationTest` no diretório `Integration`, adicione `[Fact]` ou `[Theory]` e use `Client`. Chame `LoginAsync()` para obter uma sessão autenticada. `Factory.WithDatabaseAsync(...)` permite preparar ou verificar dados com um escopo independente da requisição.

`IAsyncLifetime` cria e descarta a infraestrutura a cada caso, inclusive linhas de uma Theory. As classes podem executar em paralelo sem compartilhar estado mutável. O hash fixo da senha de fixture é calculado uma vez; a verificação BCrypt continua real.

## Executar e inspecionar

Na raiz:

```powershell
dotnet test SistemaControleOperacional.sln -c Release
dotnet test SistemaControleOperacional.sln --filter "FullyQualifiedName~TarefasTests"
dotnet test SistemaControleOperacional.sln -c Release --logger trx --collect:"XPlat Code Coverage" --results-directory TestResults
```

O TRX contém os resultados; `coverage.cobertura.xml` contém a cobertura. Não há meta percentual imposta: cobertura é uma ferramenta para localizar lacunas, não uma garantia de ausência de defeitos.

## Escopo das substituições

SQLite executa queries e gravações reais, incluindo índices únicos. Ele não executa migrations MySQL. O conversor temporal exclusivo do contexto de teste permite ordenar tarefas por data. O capturador de e-mail valida destinatário/token, mas não valida TLS, credenciais, template nem entrega SMTP.

Não use `EF Core InMemory` como substituto direto: ele não reproduz as mesmas restrições relacionais. Para validar detalhes do provedor de produção, acrescente uma suíte separada com MySQL/MariaDB descartável.
