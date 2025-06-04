using Application.DTOs.Request.Anagrafica;
using Application.DTOs.Response.Anagrafica;
using Application.DTOs.Response;
using Application.Extensions;
using Application.Utils;
using System.Net.Http.Json;

namespace Application.Services
{
    public class AnagraficaService(HttpClientService httpClientService) : IAnagraficaService
    {
        public async Task<GeneralResponse> CreateAsync(CreateCustomerDTO dto)
        {
            var client = await httpClientService.GetPrivateClient();
            var response = await client.PostAsJsonAsync(Constant.CreateCustomerRoute, dto);
            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new(false, "error");
        }

        public async Task<GeneralResponse> DeleteAsync(int id)
        {
            var client = await httpClientService.GetPrivateClient();
            var response = await client.DeleteAsync($"{Constant.CustomerRoute}/{id}");
            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new(false, "error");
        }

        public async Task<GeneralResponse> UpdateAsync(UpdateCustomerDTO dto)
        {
            var client = await httpClientService.GetPrivateClient();
            var response = await client.PutAsJsonAsync(Constant.CustomerRoute, dto);
            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new(false, "error");
        }

        public async Task<IEnumerable<CustomerDTO>> GetAsync()
        {
            var client = await httpClientService.GetPrivateClient();
            return await client.GetFromJsonAsync<IEnumerable<CustomerDTO>>(Constant.CustomerRoute) ?? new List<CustomerDTO>();
        }

        public async Task<CustomerDTO?> GetByIdAsync(int id)
        {
            var client = await httpClientService.GetPrivateClient();
            return await client.GetFromJsonAsync<CustomerDTO>($"{Constant.CustomerRoute}/{id}");
        }

        public async Task<GeneralResponse> ExportAsync()
        {
            var client = await httpClientService.GetPrivateClient();
            var response = await client.PostAsync(Constant.ExportCustomerRoute, null);
            return await response.Content.ReadFromJsonAsync<GeneralResponse>() ?? new(false, "error");
        }
    }
}
