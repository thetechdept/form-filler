using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Techdept.FormFiller.Core;
using System.IO;
using System.Linq;
using System;

namespace Techdept.FormFiller.Functions
{
    public class SetValues
    {
        private readonly IFormFiller formFiller;

        private readonly ILogger logger;

        public SetValues(IFormFiller formFiller, ILogger<SetValues> logger)
        {
            this.formFiller = formFiller;
            this.logger = logger;
        }

        [FunctionName("SetValues")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequest req)
        {
            var source = req.Form.Files["source"];
            var fields = req.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            req.Form.TryGetValue("flattenMode", out var flattenModes);
            Enum.TryParse<FlattenMode>(flattenModes.FirstOrDefault(), true, out var flattenMode);

            if (source == null)
            {
                return new BadRequestObjectResult(new { Message = "Source file is required" });
            }

            using var src = source.OpenReadStream();
            var dest = new MemoryStream();

            await formFiller.SetValues(src, dest, fields, flattenMode);
            dest.Position = 0;

            return new FileStreamResult(dest, source.ContentType);
        }
    }
}
