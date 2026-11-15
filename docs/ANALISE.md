# Análise técnica

## Estrutura encontrada

API .NET 8 de porte pequeno, organizada em controllers, DTOs, serviços e repositórios. Há quatro controllers e cinco entidades persistidas. Autenticação usa JWT e BCrypt; confirmação de cadastro usa token de uso único armazenado em `CadastroPendente`. Tarefas filtram operações por `UsuarioId`. Eventos Pomodoro são gravados sem consulta posterior exposta pela API.

## Ajustes para testabilidade

- Ponto de entrada `public partial Program` para `WebApplicationFactory`.
- Banco, JWT e Swagger consultam `IConfiguration`, preservando os nomes existentes das variáveis de ambiente.
- `Database:ApplyMigrations` mantém padrão `true`; o host de teste desativa migrations MySQL e cria somente seu SQLite.
- `IEmailService` permite substituir SMTP por captura em memória sem simular as regras de cadastro.
- Projeto de testes separado, excluído da compilação e publicação da API.
- Solução, CI, cobertura, exemplo de ambiente e documentação adicionados.
- Dockerfile publica explicitamente o `.csproj`, evitando ambiguidade após a criação da solução.

## Pontos encontrados para evolução

Estes pontos não foram tratados como uma reescrita funcional nesta tarefa:

| Ponto | Consequência / evolução sugerida |
| --- | --- |
| `UsuarioId` e `TarefaId` sem relacionamentos/FKs no modelo | Exclusão de conta não remove automaticamente tarefas/eventos. Definir política de retenção e integridade antes de alterar migrations. |
| JWT não consulta existência/atividade em todas as rotas | Um token emitido antes da exclusão continua criptograficamente válido até expirar; considerar revogação/validação de sessão. |
| Evento aceita ID de tarefa sem verificar proprietário | Definir se o evento é um snapshot independente ou se deve validar vínculo e propriedade. |
| Enums aceitam inteiros; campos de valor usam `[Required]` | Alguns valores ausentes ou inteiros fora do enum podem ser aceitos. Definir validação explícita com atenção ao contrato do frontend. |
| `GetUsuarioId` usa `long.Parse` | Claim assinada mas malformada pode gerar erro 500; considerar `TryParse` com rejeição de autenticação. |
| Confirmação salva usuário e remove pendência em operações separadas | Considerar transação para falhas entre operações e concorrência de confirmação. |
| Cadastro persiste antes de enviar SMTP | Falha de envio deixa cadastro pendente; considerar reenvio e outbox. |
| Importação salva item por item | Validação de DTO ocorre antes do lote, mas falha de persistência no meio pode deixar importação parcial; considerar transação. |
| Incremento Pomodoro por leitura/modificação/gravação | Requisições concorrentes podem perder incrementos; considerar operação atômica. |
| Healthcheck Compose usa `/dev/tcp` via shell | Depende de suporte do shell da imagem; validar em Docker ou substituir por verificação HTTP com ferramenta disponível. |
| Autodetecção MySQL e migrations no startup | Inicialização depende da disponibilidade e permissões de banco; avaliar migrations como etapa de implantação. |

## Limites da verificação

Os testes de integração verificam o pipeline real da API com SQLite e JWT. Eles não usam o banco configurado em `.env`, não enviam e-mails e não executam deploy. A compatibilidade das migrations com MySQL/MariaDB, a entrega SMTP e a execução do container exigem validação própria nesses ambientes. Não se deve interpretar a aprovação da suíte como comprovação de todos esses componentes externos.
