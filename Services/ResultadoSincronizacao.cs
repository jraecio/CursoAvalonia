using System.Collections.Generic;

namespace CursoAvalonia.Services;

public class ResultadoSincronizacao
{
    public int Produtos { get; set; }
    public int Clientes { get; set; }
    public int Pedidos { get; set; }
    public List<string> Erros { get; } = new();
    public bool Executada { get; set; }
    public string Mensagem =>
        $"Produtos: {Produtos} | Clientes: {Clientes} | Pedidos enviados: {Pedidos}" +
        (Erros.Count == 0 ? string.Empty : " | " + string.Join(" | ", Erros));
}
