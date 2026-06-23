# Tournament Manager

Aplicação web para gestão mobile-first de torneios de futebol de formação.

## Estado atual

O repositório estava vazio, contendo apenas `.gitkeep`. A Fase 1 criou a estrutura base da solução, autenticação com ASP.NET Core Identity, entidades iniciais de torneios e escalões, EF Core Code First, primeira migration manual, CRUD inicial de torneios, localização base para `pt-PT`, Bootstrap 5 e testes unitários iniciais. A Fase 2 acrescenta equipas, jogadores, campos, validações de idade/números, repositórios, serviços, controllers e views mobile-first para estes dados. A Fase 3 acrescenta a fundação de calendário, jogos agendados, geração todos-contra-todos e geração com número fixo de jogos por equipa. A Fase 4 acrescenta gestão operacional de jogos, golos, anulação de golos, melhores jogadores por equipa e notificações SignalR por jogo. A Fase 5 acrescenta classificações dinâmicas, estatísticas de equipas, melhores marcadores e ranking de votos de melhor jogador. A Fase 6 acrescenta fundação de fase final, criação idempotente de meias-finais, progressão para final/terceiro lugar e decisão por penáltis. A Fase 7 acrescenta auditoria, hardening de segurança inicial, PWA básico e documentação final de estado.

## Plano por fases

1. Estrutura da solução, autenticação, modelo base, DbContext, primeira migration, torneios e escalões.
2. Equipas, jogadores, campos e validações de idade/números.
3. Formatos de competição, geração e edição manual de calendário.
4. Gestão de jogos em tempo real, golos, cronómetro, melhores jogadores e SignalR.
5. Classificações, estatísticas e rankings.
6. Fase de grupos, meias-finais, final, terceiro lugar e penáltis.
7. Auditoria, concorrência, testes alargados, segurança, PWA e documentação final.

## Estrutura

- `src/TournamentManager.Domain`: entidades, enums e regras essenciais.
- `src/TournamentManager.Application`: DTOs, abstrações e serviços de aplicação.
- `src/TournamentManager.Infrastructure`: EF Core, Identity, repositórios e migrations.
- `src/TournamentManager.Web`: MVC, views, autenticação, SignalR e UI.
- `tests/TournamentManager.Tests`: testes xUnit.
- `docs`: documentação funcional.

## Requisitos

- .NET 8 SDK.
- SQL Server ou SQL Server LocalDB.

## Configuração da base de dados

Defina a connection string `DefaultConnection` em User Secrets ou variáveis de ambiente. Não guarde secrets no repositório.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=TournamentManager;..." --project src/TournamentManager.Web
```

## Primeiro administrador

Configure apenas por secrets/ambiente:

```bash
dotnet user-secrets set "InitialAdmin:Email" "admin@example.com" --project src/TournamentManager.Web
dotnet user-secrets set "InitialAdmin:Password" "<strong unique password>" --project src/TournamentManager.Web
```

O utilizador inicial é criado no arranque se ainda não existir. As funções `Administrator`, `Operator` e `Viewer` também são criadas.

## Migrations

```bash
dotnet ef database update --project src/TournamentManager.Infrastructure --startup-project src/TournamentManager.Web
```

## Executar

```bash
dotnet run --project src/TournamentManager.Web
```

## Testes

```bash
dotnet test TournamentManager.sln
```

## Decisões de arquitetura

- Controllers finos: regras ficam em serviços de aplicação/domínio.
- Identity usa hashing e lockout; passwords nunca são persistidas em texto simples.
- `pt-PT` é a cultura inicial; textos de UI foram escritos em português e a estrutura já suporta localização.
- Delete restritivo no relacionamento Tournament/AgeGroup para proteger histórico futuro.

## Limitações conhecidas da Fase 1

- A CLI `.NET` não está disponível neste contentor, por isso build, restore, migrations geradas automaticamente e testes não puderam ser executados localmente.
- A migration inicial foi criada manualmente e deve ser validada num ambiente com SDK .NET 8.
- As fases planeadas iniciais estão implementadas como fundação funcional; ainda requerem validação completa em ambiente .NET/SQL Server.
