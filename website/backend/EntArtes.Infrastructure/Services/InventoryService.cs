using Microsoft.EntityFrameworkCore;
using EntArtes.Core.DTOs;
using EntArtes.Core.Entities;
using EntArtes.Core.Interfaces;
using EntArtes.Infrastructure.Data;

namespace EntArtes.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Item> SubmitItemAsync(int contribuidorId, ItemSubmissionDto dto)
    {
        var item = new Item
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Categoria = dto.Categoria,
            EstadoConservacao = dto.EstadoConservacao,
            FotoUrl = dto.FotoUrl,
            Disponivel = false,
            TaxaSimbolica = dto.TaxaSimbolica,
            Estado = EstadoItem.Pendente,
            ContribuidorId = contribuidorId
        };
        _context.Itens.Add(item);
        await _context.SaveChangesAsync();
        // TODO: notify Direção
        return item;
    }

    public async Task<Item?> GetItemByIdAsync(int id) => await _context.Itens.FindAsync(id);

    public async Task ApproveItemAsync(int itemId)
    {
        var item = await _context.Itens.FindAsync(itemId);
        if (item == null) throw new Exception("Item not found");
        if (item.Estado != EstadoItem.Pendente) throw new Exception("Item already processed");
        item.Estado = EstadoItem.Aprovado;
        item.Disponivel = true;
        await _context.SaveChangesAsync();
        // TODO: notify contributor
    }

    public async Task RejectItemAsync(int itemId)
    {
        var item = await _context.Itens.FindAsync(itemId);
        if (item == null) throw new Exception("Item not found");
        item.Estado = EstadoItem.Rejeitado;
        item.Disponivel = false;
        await _context.SaveChangesAsync();
        // TODO: notify contributor
    }

    public async Task<IEnumerable<Item>> GetPendingItemsAsync()
    {
        return await _context.Itens
            .Where(i => i.Estado == EstadoItem.Pendente)
            .Include(i => i.Contribuidor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetAvailableItemsAsync()
    {
        return await _context.Itens
            .Where(i => i.Estado == EstadoItem.Aprovado && i.Disponivel)
            .ToListAsync();
    }
}