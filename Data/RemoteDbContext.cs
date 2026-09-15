using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;

namespace CursoAvalonia.Data;

public class RemoteDbContext : DbContext
{
    private readonly string _connectionString;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public RemoteDbContext(
        string connectionString)
    {
        _connectionString =
            connectionString;
    }


    // =========================================================
    // TABELAS
    // =========================================================

    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<LogAuditoria> LogsAuditoria { get; set; }


    // =========================================================
    // CONFIGURAÇÃO SQL SERVER
    // =========================================================

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            _connectionString
        );
    }


    // =========================================================
    // MODELO
    // =========================================================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(
            modelBuilder
        );


        // =====================================================
        // PEDIDO -> ITENS
        // =====================================================

        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);


        // =====================================================
        // PEDIDO -> LOG AUDITORIA
        // =====================================================

        modelBuilder.Entity<LogAuditoria>()
            .HasOne(l => l.Pedido)
            .WithMany()
            .HasForeignKey(l => l.PedidoId);


        // =====================================================
        // NÚMERO DO PEDIDO ÚNICO
        // =====================================================

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();
    }
}