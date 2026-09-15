namespace CursoAvalonia.Services;

public class SoftcomShopRoutes
{

    public string Clientes =>
    $"{_baseUrl}/softauth/api/clientes/clientes";

    public string ClientesPagina(int pagina) =>
        $"{_baseUrl}/softauth/api/clientes/clientes/page/{pagina}";

    private readonly string _baseUrl;

    public SoftcomShopRoutes(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public string Device =>
        $"{_baseUrl}/softauth/device/add";

    public string Token =>
        $"{_baseUrl}/softauth/authentication/token";

    public string Empresa =>
        $"{_baseUrl}/softauth/api/empresa";

    public string Produtos =>
        $"{_baseUrl}/softauth/api/produtos/produtos";

    public string ProdutosV2 =>
        $"{_baseUrl}/softauth/api/v2/produtos/produtos";

    public string Promocao =>
        $"{_baseUrl}/softauth/api/produtos/promocao";

    public string AtualizacaoPreco =>
        $"{_baseUrl}/softauth/api/produtos/produtos/data_atualizacao_preco/";

    public string EntradaNotaFiscal =>
        $"{_baseUrl}/softauth/api/produtos/produtos/data_entrada/";

    public string EntradaNotaFiscalV2 =>
        $"{_baseUrl}/softauth/api/v2/produtos/compras?data_hora_entrada=";

    public string Vendas =>
        $"{_baseUrl}/softauth/api/vendas/vendas/completa/";
}