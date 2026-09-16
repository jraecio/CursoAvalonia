using CursoAvalonia.Models;
using CursoAvalonia.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using CursoAvalonia.Data;

// Bancos isolados: não usa config.json nem os bancos da aplicação.
var folder = Path.Combine(AppContext.BaseDirectory, "test-data", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
string localPath = Path.Combine(folder, "local.db");
string remotePath = Path.Combine(folder, "remote.db");
TestDb Local() => new(localPath);
TestDb Remote() => new(remotePath);
Task Preparar(DbContext db) => db.Database.EnsureCreatedAsync();
var sync = new SincronizacaoPedidosService(Local, () => Task.FromResult<DbContext>(Remote()), Preparar);
await using (var local = Local()) await local.Database.EnsureCreatedAsync();
await using (var remote = Remote()) await remote.Database.EnsureCreatedAsync();

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("OK: " + message);
}

var pedido = new Pedido { NumeroPedido = 1, Total = 30, SubTotal = 30, ValorPago = 30 };
pedido.Itens.Add(new ItemPedido { ProdutoId = 10, DescricaoProduto = "Teste", Quantidade = 3, ValorUnitario = 10, Total = 30 });
await using (var db = Local())
{
    db.Add(pedido);
    db.Add(new LogAuditoria { PedidoId = pedido.Id, Acao = "PEDIDO_FINALIZADO" });
    await db.SaveChangesAsync();
}
var result = await sync.SincronizarAsync();
Check(result.Enviados == 1 && result.Erros.Count == 0, "primeiro envio concluído");
await using (var db = Remote())
{
    var saved = await db.Set<Pedido>().Include(p => p.Itens).SingleAsync();
    Check(saved.Id == pedido.Id && saved.Itens.Count == 1 && await db.Set<LogAuditoria>().CountAsync() == 1,
        "pedido, itens e auditoria gravados juntos");
}
await using (var db = Local()) Check((await db.Set<Pedido>().SingleAsync()).Sincronizado, "confirmação local após envio");

// Simula falha entre o commit remoto e a confirmação local.
await using (var db = Local()) await db.Set<Pedido>().ExecuteUpdateAsync(s => s.SetProperty(p => p.Sincronizado, false));
result = await sync.SincronizarAsync();
await using (var db = Remote()) Check(result.Enviados == 1 && await db.Set<Pedido>().CountAsync() == 1 &&
    await db.Set<ItemPedido>().CountAsync() == 1 && await db.Set<LogAuditoria>().CountAsync() == 1,
    "reenvio não duplica pedidos, itens nem logs");

await using (var db = Local())
{
    var saved = await db.Set<Pedido>().Include(p => p.Itens).SingleAsync();
    db.RemoveRange(saved.Itens);
    saved.Itens.Clear();
    saved.Itens.Add(new ItemPedido { ProdutoId = 20, DescricaoProduto = "Alterado", Quantidade = 2, ValorUnitario = 20, Total = 40 });
    db.Set<ItemPedido>().Add(saved.Itens[0]);
    saved.Total = saved.SubTotal = 40;
    saved.Sincronizado = false;
    saved.AtualizadoEm = saved.AtualizadoEm.AddSeconds(1);
    db.Add(new LogAuditoria { PedidoId = saved.Id, Acao = "PEDIDO_ALTERADO" });
    await db.SaveChangesAsync();
}
result = await sync.SincronizarAsync();
await using (var db = Remote()) Check(result.Erros.Count == 0 && (await db.Set<Pedido>().SingleAsync()).Total == 40 &&
    (await db.Set<ItemPedido>().SingleAsync()).ProdutoId == 20 && await db.Set<LogAuditoria>().CountAsync() == 2,
    "alteração substitui itens e preserva auditoria");

await using (var db = Local())
{
    var saved = await db.Set<Pedido>().SingleAsync();
    saved.Cancelado = true;
    saved.Sincronizado = false;
    saved.AtualizadoEm = saved.AtualizadoEm.AddSeconds(1);
    db.Add(new LogAuditoria { PedidoId = saved.Id, Acao = "PEDIDO_CANCELADO" });
    await db.SaveChangesAsync();
}
result = await sync.SincronizarAsync();
await using (var db = Remote()) Check(result.Erros.Count == 0 && (await db.Set<Pedido>().SingleAsync()).Cancelado &&
    await db.Set<LogAuditoria>().CountAsync() == 3, "cancelamento enviado com auditoria");

await using (var db = Local()) await db.Set<Pedido>().ExecuteUpdateAsync(s => s.SetProperty(p => p.Sincronizado, false));
var offline = new SincronizacaoPedidosService(Local, () => throw new IOException("Servidor indisponível"), Preparar);
result = await offline.SincronizarAsync();
await using (var db = Local()) Check(result.Erros.Count == 1 && !(await db.Set<Pedido>().SingleAsync()).Sincronizado,
    "falha de conexão mantém pedido pendente");

// Edição acontece depois da leitura local, antes do commit remoto.
var concorrente = new SincronizacaoPedidosService(Local, () => Task.FromResult<DbContext>(new TestDb(remotePath)
{
    AntesSalvar = async () =>
    {
    await using var db = Local();
    var saved = await db.Set<Pedido>().SingleAsync();
    saved.AtualizadoEm = saved.AtualizadoEm.AddSeconds(1);
    saved.Total = 50;
    await db.SaveChangesAsync();
    }
}), Preparar);
await concorrente.SincronizarAsync();
await using (var db = Local()) Check(!(await db.Set<Pedido>().SingleAsync()).Sincronizado,
    "edição durante envio permanece pendente");
await sync.SincronizarAsync();

var conflito = new Pedido { NumeroPedido = 2 };
var outro = new Pedido { NumeroPedido = 3 };
await using (var db = Local()) { db.AddRange(conflito, outro); await db.SaveChangesAsync(); }
await using (var db = Remote()) { db.Add(new Pedido { NumeroPedido = 2 }); await db.SaveChangesAsync(); }
result = await sync.SincronizarAsync();
await using (var db = Local()) Check(result.Erros.Count == 1 && result.Enviados == 1 &&
    !(await db.Set<Pedido>().SingleAsync(p => p.Id == conflito.Id)).Sincronizado &&
    (await db.Set<Pedido>().SingleAsync(p => p.Id == outro.Id)).Sincronizado,
    "número em conflito não sobrescreve outra venda nem bloqueia outros pedidos");

// Falha na gravação dos filhos reverte também o cabeçalho remoto.
await using (var db = Local()) await db.Set<Pedido>().Where(p => p.Id == conflito.Id)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Sincronizado, true));
var rollback = new Pedido { NumeroPedido = 4 };
rollback.Itens.Add(new ItemPedido { ProdutoId = -1, DescricaoProduto = "Inválido" });
await using (var db = Local()) { db.Add(rollback); await db.SaveChangesAsync(); }
await using (var db = Remote()) await db.Database.ExecuteSqlRawAsync(
    "CREATE TRIGGER FalharItem BEFORE INSERT ON ItensPedido WHEN NEW.ProdutoId = -1 BEGIN SELECT RAISE(ABORT, 'Falha de teste'); END;");
result = await sync.SincronizarAsync();
await using (var db = Remote()) Check(result.Erros.Count == 1 && !await db.Set<Pedido>().AnyAsync(p => p.Id == rollback.Id),
    "falha de item reverte transação remota inteira");
await using (var db = Local()) Check(!(await db.Set<Pedido>().SingleAsync(p => p.Id == rollback.Id)).Sincronizado,
    "rollback remoto não confirma envio local");
// Verifica as migrations reais do SQL Server sem conexão com o banco do usuário.
await using (var sql = new RemoteDbContext("Server=localhost;Database=TesteMigration;Integrated Security=True;TrustServerCertificate=True"))
{
    var migrations = sql.Database.GetMigrations().ToArray();
    Check(migrations.SequenceEqual(new[] { "20260915233222_InitialRemoteDb" }),
        "contexto remoto seleciona somente a migration SQL Server");
    var script = sql.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
    Check(script.Contains("CREATE TABLE [Pedidos]") && script.Contains("CREATE TABLE [ItensPedido]") &&
        script.Contains("CREATE TABLE [LogsAuditoria]") && script.Contains("__EFMigrationsHistory"),
        "migration gera tabelas remotas e controle idempotente");
    Check(!sql.Database.HasPendingModelChanges(), "modelo remoto corresponde à migration existente");
}

string vazioLocal = Path.Combine(folder, "vazio-local.db");
string vazioRemoto = Path.Combine(folder, "vazio-remoto.db");
await using (var db = new TestDb(vazioLocal)) await db.Database.EnsureCreatedAsync();
int preparacoes = 0;
var vazio = new SincronizacaoPedidosService(() => new TestDb(vazioLocal),
    () => Task.FromResult<DbContext>(new TestDb(vazioRemoto)), async db =>
    {
        preparacoes++;
        await db.Database.EnsureCreatedAsync();
    });
await vazio.SincronizarAsync();
await vazio.SincronizarAsync();
await using (var db = new TestDb(vazioRemoto)) Check(preparacoes == 2 && await db.Set<Pedido>().CountAsync() == 0,
    "prepara banco vazio mesmo sem pedidos e permite repetir sincronização");

var falhaMigration = new SincronizacaoPedidosService(Local,
    () => Task.FromResult<DbContext>(Remote()), _ => throw new InvalidOperationException("Falha migration"));
result = await falhaMigration.SincronizarAsync();
await using (var db = Local()) Check(result.Enviados == 0 && result.Erros.Single().Contains("preparar as tabelas") &&
    !(await db.Set<Pedido>().SingleAsync(p => p.Id == rollback.Id)).Sincronizado,
    "falha ao preparar banco interrompe envio e preserva pendências");
Console.WriteLine("Todos os testes passaram.");

sealed class TestDb(string path) : DbContext
{
    public Func<Task>? AntesSalvar { get; init; }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (AntesSalvar != null) await AntesSalvar();
        return await base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path}");
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Pedido>().ToTable("Pedidos").HasIndex(p => p.NumeroPedido).IsUnique();
        model.Entity<Pedido>().HasMany(p => p.Itens).WithOne(i => i.Pedido).HasForeignKey(i => i.PedidoId);
        model.Entity<ItemPedido>().ToTable("ItensPedido");
        model.Entity<LogAuditoria>().ToTable("LogsAuditoria").HasOne(l => l.Pedido).WithMany().HasForeignKey(l => l.PedidoId);
    }
}
