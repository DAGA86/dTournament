# Segurança e operação

## Auditoria

As operações relevantes de gestão de jogo registam entradas em `AuditLogs`, incluindo utilizador, operação, entidade, valores anteriores/novos e data UTC.

## PWA

A aplicação inclui `manifest.webmanifest` e `service-worker.js` para instalação básica e cache de assets essenciais.

## Limites conhecidos

A rate limiting policy `Login` está registada para uso nas páginas de autenticação. Quando a UI Identity for personalizada, deve aplicar a policy aos endpoints de login.
