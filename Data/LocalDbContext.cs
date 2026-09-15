using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace CursoAvalonia.Data;

public class LocalDbContext : DbContext
{
    // =====================================================
    // TABELAS
    // =====================================================

    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<LogAuditoria> LogsAuditoria { get; set; }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Cliente> Clientes { get; set; }


    // =====================================================
    // CONFIGURAÇÃO SQLITE
    // =====================================================

    protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
    {
        string pastaBanco = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "CursoAvalonia",
            "Data"
        );

        Directory.CreateDirectory(
            pastaBanco
        );

        string caminhoBanco = Path.Combine(
            pastaBanco,
            "cursoavalonia.db"
        );

        optionsBuilder.UseSqlite(
            $"Data Source={caminhoBanco}"
        );
    }


    // =====================================================
    // CONFIGURAÇÃO DOS MODELOS
    // =====================================================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =================================================
        // PEDIDO / ITENS
        // =================================================

        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);


        // =================================================
        // LOG AUDITORIA
        // =================================================

        modelBuilder.Entity<LogAuditoria>()
            .HasOne(l => l.Pedido)
            .WithMany()
            .HasForeignKey(l => l.PedidoId);


        // =================================================
        // PEDIDO
        // =================================================

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();


        // =================================================
        // PRODUTO
        // =================================================

        modelBuilder.Entity<Produto>()
            .HasKey(p => p.Codigo);


        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.ProdutoEmpresaId);


        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.CodigoBarras);


        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.Descricao);


        // =================================================
        // CLIENTE
        // =================================================

        modelBuilder.Entity<Cliente>()
            .HasKey(c => c.Id);


        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Nome);


        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.CPF);
    }
}