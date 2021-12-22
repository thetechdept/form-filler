using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Techdept.FormFiller.Core
{
    public interface IDocumentSigner
    {
        Task Sign(
            Stream source, 
            Stream destination, 
            X509Certificate2 certificate, 
            Stream? signatureImage, 
            string signatureField, 
            CancellationToken cancellationToken = default);
    }
}
