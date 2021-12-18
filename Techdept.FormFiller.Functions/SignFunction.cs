using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Techdept.FormFiller.Core;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Techdept.FormFiller.Functions
{
    public class SignFunction
    {
        private readonly IDocumentSigner _documentSigner;

        private readonly ILogger _log;

        public SignFunction(IDocumentSigner documentSigner, ILogger<GetFields> log)
        {
            _documentSigner = documentSigner;
            _log = log;
        }

        [FunctionName("Sign")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequest req)
        {
            var sourceFile = req.Form.Files["source"];
            if (sourceFile == null)
            {
                return new BadRequestObjectResult(new { Message = "Source file is required" });
            }

            var certificateFile = req.Form.Files["certificate"];
            if (certificateFile == null)
            {
                return new BadRequestObjectResult(new { Message = "Certificate file is required" });
            }
            using var fileStream = certificateFile.OpenReadStream();
            var certificateBytes = new byte[certificateFile.Length];        
            fileStream.Read(certificateBytes, 0, (int)certificateFile.Length);
            var certificate = new X509Certificate2(certificateBytes);


            using var src = sourceFile.OpenReadStream();
            var dest = new MemoryStream();

            await _documentSigner.Sign(src, dest, certificate);
            dest.Position = 0;

            return new FileStreamResult(dest, sourceFile.ContentType);
        }
    }
}
