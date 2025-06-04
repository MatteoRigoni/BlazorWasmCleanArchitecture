using Application.DTOs.Request.Anagrafica;
using Application.DTOs.Response.Anagrafica;
using Application.DTOs.Response;

namespace Application.Services
{
    public interface IAnagraficaService
    {
        Task<IEnumerable<CustomerDTO>> GetAsync();
        Task<CustomerDTO?> GetByIdAsync(int id);
        Task<GeneralResponse> CreateAsync(CreateCustomerDTO dto);
        Task<GeneralResponse> UpdateAsync(UpdateCustomerDTO dto);
        Task<GeneralResponse> DeleteAsync(int id);
        Task<GeneralResponse> ExportAsync();
    }
}
