using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Techdept.FormFiller.Core;

namespace Techdept.FormFiller.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormController : ControllerBase
    {
        private readonly ILogger<FormController> _logger;
        private readonly IFormFiller _formFiller;

        public FormController(ILogger<FormController> logger, IFormFiller formFiller)
        {
            _formFiller = formFiller;
            _logger = logger;
        }
        
        [HttpPost("Fields")]
        public async Task<IActionResult> GetFields(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var fields = await _formFiller.GetFields(stream);
            return Ok(fields);
        }

        [HttpPost("Fill")]
        public async Task<IActionResult> UpdateFields(IFormFile file, IDictionary<string, string> fields)
        {
            using var destination = new MemoryStream();
            using var source = file.OpenReadStream();

            await _formFiller.SetValues(source, destination, fields);
            return File(destination, file.ContentType);
        }
    }
}
