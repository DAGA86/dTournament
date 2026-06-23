# Segurança e operação

## Auditoria

As operações relevantes de gestão de jogo registam entradas em `AuditLogs`, incluindo utilizador, operação, entidade, valores anteriores/novos e data UTC.

## PWA

A aplicação inclui `manifest.webmanifest` e `service-worker.js` para instalação básica e cache de assets essenciais.

## Limites conhecidos

A rate limiting policy `Login` está registada para uso nas páginas de autenticação. Quando a UI Identity for personalizada, deve aplicar a policy aos endpoints de login.

## Primeiro administrador e registo

O primeiro administrador deve ser criado através de configuração segura (`InitialAdmin:Email` e `InitialAdmin:Password`) em User Secrets ou variáveis de ambiente. O registo público está desativado para impedir contas sem perfil; utilizadores sem `Administrator`, `Operator` ou `Viewer` não passam nas políticas da aplicação.
