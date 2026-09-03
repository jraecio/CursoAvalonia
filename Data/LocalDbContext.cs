using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;

namespace CursoAvalonia.Data;

public class LocalDbContext : DbContext
{
    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<LogAuditoria> LogsAuditoria { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
    @"Data Source=E:\CURSO\Avalonia\CursoAvalonia\cursoavalonia.db"
);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);

        modelBuilder.Entity<LogAuditoria>()
            .HasOne(l => l.Pedido)
            .WithMany()
            .HasForeignKey(l => l.PedidoId);
    }
}