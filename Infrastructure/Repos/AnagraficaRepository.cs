using Application.Contracts;
using Application.DTOs.Request.Anagrafica;
using Application.DTOs.Response.Anagrafica;
using Application.DTOs.Response;
using Domain.Entities.Anagrafica;
using Infrastructure.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    public class AnagraficaRepository(AppDbContext context) : IAnagrafica
    {
        public async Task<GeneralResponse> CreateAsync(CreateCustomerDTO dto)
        {
            context.Customers.Add(dto.Adapt<Customer>());
            await context.SaveChangesAsync();
            return new(true, "Created");
        }

        public async Task<GeneralResponse> DeleteAsync(int id)
        {
            var entity = await context.Customers.FindAsync(id);
            if (entity == null) return new(false, "Not found");
            context.Customers.Remove(entity);
            await context.SaveChangesAsync();
            return new(true, "Deleted");
        }

        public async Task<IEnumerable<CustomerDTO>> GetAsync()
            => (await context.Customers.ToListAsync()).Adapt<IEnumerable<CustomerDTO>>();

        public async Task<CustomerDTO?> GetByIdAsync(int id)
        {
            var entity = await context.Customers.FindAsync(id);
            return entity?.Adapt<CustomerDTO>();
        }

        public async Task<GeneralResponse> UpdateAsync(UpdateCustomerDTO dto)
        {
            var entity = await context.Customers.FindAsync(dto.Id);
            if (entity == null) return new(false, "Not found");
            dto.Adapt(entity);
            await context.SaveChangesAsync();
            return new(true, "Updated");
        }

        public Task<GeneralResponse> ExportAsync()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500); // simulate export
            });
            return Task.FromResult(new GeneralResponse(true, "Export started"));
        }
    }
}
