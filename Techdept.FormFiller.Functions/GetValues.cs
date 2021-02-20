using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Techdept.FormFiller.Core;

namespace Techdept.FormFiller.Functions
{
    public class GetValues
    {
        private readonly IFormFiller _formFiller;

        private readonly ILogger _log;

        public GetValues(IFormFiller formFiller, ILogger<GetValues> log)
        {
            _formFiller = formFiller;
            _log = log;
        }

        [FunctionName("GetValues")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequest req)
        {
            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            using var source = file.OpenReadStream();
            var fields = await _formFiller.GetFields(source);
            
            return new JsonResult(fields, Defaults.JsonSerializerSettings);
        }
    }
}
