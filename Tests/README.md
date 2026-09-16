# Testes de sincronização

Execute na pasta do projeto:

```powershell
dotnet run --project Tests/CursoAvalonia.Tests.csproj
```

O executável retorna código diferente de zero se alguma verificação falhar. Usa dois bancos SQLite descartáveis em `Tests/bin/Debug/net10.0/test-data`, sem ler a configuração nem acessar o SQL Server real.

Verifica envio com itens e auditoria, confirmação local, reenvio idempotente, alteração de itens, cancelamento, servidor indisponível, edição durante o envio, conflito de numeração e rollback remoto. A compilação também valida os bindings XAML. As particularidades do provedor SQL Server e a interação visual devem ser validadas no ambiente de destino.

Na aplicação, o envio ocorre ao iniciar e pelo botão **SINCRONIZAR**. O botão persiste os valores exibidos na configuração antes de sincronizar. Antes do envio, `Database.MigrateAsync()` aplica as migrations pendentes do `RemoteDbContext` no destino configurado, inclusive quando não há pedidos locais. A migration existente `InitialRemoteDb` cria Pedidos, ItensPedido e LogsAuditoria; ela não inclui produtos/clientes. Falhas na preparação interrompem o envio e preservam pendências. A numeração continua local: pedidos diferentes com o mesmo número são sinalizados para resolução, sem sobrescrever vendas.

As verificações adicionais conferem a seleção da migration remota, seu script SQL idempotente, a correspondência do snapshot com o modelo, a preparação de banco vazio e a interrupção do envio após falha na preparação. A execução real de migrations SQL Server não é simulada pelo SQLite e precisa de validação no destino.
