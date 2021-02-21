using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Techdept.FormFiller.Core;

namespace Techdept.FormFiller.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetFields(IFormFile source)
        {
            using var src = source.OpenReadStream();
            var fields = await _formFiller.GetFields(src);
            return Ok(fields);
        }

        [HttpPost("Fill")]
        public async Task<FileResult> UpdateFields(IFormFile source)
        {
            var fields = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            using var src = source.OpenReadStream();
            var dest = new MemoryStream();

            await _formFiller.SetValues(src, dest, fields);
            dest.Position = 0;

            return File(dest, source.ContentType);
        }
    }
}
