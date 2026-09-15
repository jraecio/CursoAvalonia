using CursoAvalonia.Data;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class DadosLocaisService
{
    // =========================================================
    // PRODUTOS
    // =========================================================

    public async Task<List<Produto>> ListarProdutosAsync()
    {
        await using LocalDbContext db =
            new LocalDbContext();

        return await db.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }


    // =========================================================
    // CLIENTES
    // =========================================================

    public async Task<List<Cliente>> ListarClientesAsync()
    {
        await using LocalDbContext db =
            new LocalDbContext();

        return await db.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }
}