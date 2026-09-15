using System.Text.Json;
using System.Text.Json.Serialization;

namespace CursoAvalonia.DTOs;

public class ClienteApiDto
{
    // =========================================================
    // IDENTIFICAÇÃO
    // =========================================================

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("pessoa")]
    public string? Pessoa { get; set; }

    [JsonPropertyName("cpf_cnpj")]
    public string? CpfCnpj { get; set; }

    [JsonPropertyName("inscricao_estadual")]
    public string? InscricaoEstadual { get; set; }

    [JsonPropertyName("inscricao_municipal")]
    public string? InscricaoMunicipal { get; set; }

    [JsonPropertyName("rg")]
    public string? Rg { get; set; }


    // =========================================================
    // DADOS DO CLIENTE
    // =========================================================

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("razao_social")]
    public string? RazaoSocial { get; set; }

    [JsonPropertyName("data_fundacao")]
    public string? DataFundacao { get; set; }

    [JsonPropertyName("data_nascimento")]
    public string? DataNascimento { get; set; }

    [JsonPropertyName("contribuinte_icms")]
    public string? ContribuinteIcms { get; set; }

    [JsonPropertyName("indicador_finalidade")]
    public JsonElement? IndicadorFinalidade { get; set; }

    [JsonPropertyName("bloqueado")]
    public JsonElement? Bloqueado { get; set; }


    // =========================================================
    // ENDEREÇO
    // =========================================================

    [JsonPropertyName("endereco")]
    public string? Endereco { get; set; }

    [JsonPropertyName("numero")]
    public JsonElement? Numero { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("ponto_referencia")]
    public string? PontoReferencia { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("cidade")]
    public string? Cidade { get; set; }

    [JsonPropertyName("codigo_cidade")]
    public JsonElement? CodigoCidade { get; set; }

    [JsonPropertyName("cidade_id")]
    public JsonElement? CidadeId { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("cep")]
    public string? Cep { get; set; }


    // =========================================================
    // CONTATO
    // =========================================================

    [JsonPropertyName("contato_nome")]
    public string? ContatoNome { get; set; }

    [JsonPropertyName("contato_ddd")]
    public JsonElement? ContatoDdd { get; set; }

    [JsonPropertyName("contato_telefone")]
    public JsonElement? ContatoTelefone { get; set; }

    [JsonPropertyName("contato_email")]
    public string? ContatoEmail { get; set; }

    [JsonPropertyName("contato_nascimento")]
    public JsonElement? ContatoNascimento { get; set; }

    [JsonPropertyName("contato_outros")]
    public JsonElement? ContatoOutros { get; set; }


    // =========================================================
    // INFORMAÇÕES ADICIONAIS
    // =========================================================

    [JsonPropertyName("observacao")]
    public string? Observacao { get; set; }

    [JsonPropertyName("area_id")]
    public JsonElement? AreaId { get; set; }

    [JsonPropertyName("area_nome")]
    public string? AreaNome { get; set; }


    // =========================================================
    // TIPO DO CLIENTE
    // =========================================================

    [JsonPropertyName("tipo_cliente_id")]
    public JsonElement? TipoClienteId { get; set; }

    [JsonPropertyName("tipo_cliente_nome")]
    public string? TipoClienteNome { get; set; }


    // =========================================================
    // FUNCIONÁRIO
    // =========================================================

    [JsonPropertyName("funcionario_id")]
    public JsonElement? FuncionarioId { get; set; }

    [JsonPropertyName("funcionario_nome")]
    public string? FuncionarioNome { get; set; }


    // =========================================================
    // FINANCEIRO
    // =========================================================

    [JsonPropertyName("detalhe_financeiro")]
    public JsonElement? DetalheFinanceiro { get; set; }

    [JsonPropertyName("limite_credito")]
    public JsonElement? LimiteCredito { get; set; }

    [JsonPropertyName("credito_saldo_disponivel")]
    public JsonElement? CreditoSaldoDisponivel { get; set; }


    // =========================================================
    // TABELA DE PREÇO
    // =========================================================

    [JsonPropertyName("tabela_preco")]
    public JsonElement? TabelaPreco { get; set; }
}