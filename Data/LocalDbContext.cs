using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace CursoAvalonia.Data;

public class LocalDbContext : DbContext
{
    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<LogAuditoria> LogsAuditoria { get; set; }

    public DbSet<Produto> Produtos { get; set; }


    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        string pastaBanco = Path.Combine(
            AppContext.BaseDirectory,
            "Data"
        );

        Directory.CreateDirectory(pastaBanco);

        string caminhoBanco = Path.Combine(
            pastaBanco,
            "cursoavalonia.db"
        );

        optionsBuilder.UseSqlite(
            $"Data Source={caminhoBanco}"
        );
    }


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        // =====================================================
        // PEDIDO / ITENS
        // =====================================================

        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);


        // =====================================================
        // LOG AUDITORIA
        // =====================================================

        modelBuilder.Entity<LogAuditoria>()
            .HasOne(l => l.Pedido)
            .WithMany()
            .HasForeignKey(l => l.PedidoId);


        // =====================================================
        // NÚMERO DO PEDIDO
        // =====================================================

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();


        // =====================================================
        // PRODUTOS
        // =====================================================

        modelBuilder.Entity<Produto>()
            .HasKey(p => p.Codigo);


        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.ProdutoEmpresaId);
    }
}