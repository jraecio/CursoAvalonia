using System;

namespace CursoAvalonia.Models;

public class Cliente
{
    // =========================================================
    // IDENTIFICAÇÃO
    // =========================================================

    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string CPF { get; set; } = string.Empty;

    public string Pessoa { get; set; } = string.Empty;

    public string RazaoSocial { get; set; } = string.Empty;

    public string InscricaoEstadual { get; set; } = string.Empty;

    public string Rg { get; set; } = string.Empty;


    // =========================================================
    // ENDEREÇO
    // =========================================================

    public string Endereco { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Complemento { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public string Cep { get; set; } = string.Empty;


    // =========================================================
    // CONTATO
    // =========================================================

    public string Telefone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;


    // =========================================================
    // CLIENTE
    // =========================================================

    public string TipoCliente { get; set; } = string.Empty;

    public bool Bloqueado { get; set; }


    // =========================================================
    // FINANCEIRO
    // =========================================================

    public decimal LimiteCredito { get; set; }

    public decimal CreditoSaldoDisponivel { get; set; }


    // =========================================================
    // CONTROLE LOCAL / SINCRONIZAÇÃO
    // =========================================================

    public DateTime AtualizadoEm { get; set; } = DateTime.Now;
}