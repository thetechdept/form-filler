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
            var documentFile = req.Form.Files["document"];
            if (documentFile == null)
            {
                return new BadRequestObjectResult(new { Message = "Document is required" });
            }
            var contentType = documentFile.ContentType;

            var certificateFile = req.Form.Files["certificate"];
            if (certificateFile == null)
            {
                return new BadRequestObjectResult(new { Message = "Certificate is required" });
            }
            using var certificateStream = certificateFile.OpenReadStream();
            var certificateBytes = new byte[certificateFile.Length];        
            certificateStream.Read(certificateBytes, 0, (int)certificateFile.Length);
            var certificate = new X509Certificate2(certificateBytes);

            req.Form.TryGetValue("signatureField", out var signatureField);

            using var source = documentFile.OpenReadStream();
            var dest = new MemoryStream();
            await _documentSigner.Sign(source, dest, certificate, null, signatureField);
            
            return new FileContentResult(dest.ToArray(), contentType);
        }
    }
}
