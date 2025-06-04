using Application.Contracts;
using Application.DTOs.Request.Anagrafica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnagraficaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnagraficaController(IAnagrafica service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await service.GetAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) => Ok(await service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Post(CreateCustomerDTO dto) => Ok(await service.CreateAsync(dto));

        [HttpPut]
        public async Task<IActionResult> Put(UpdateCustomerDTO dto) => Ok(await service.UpdateAsync(dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) => Ok(await service.DeleteAsync(id));

        [HttpPost("export")]
        public async Task<IActionResult> Export() => Ok(await service.ExportAsync());
    }
}
