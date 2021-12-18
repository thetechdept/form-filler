using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Techdept.FormFiller.Core
{
    public interface IDocumentSigner
    {
        Task Sign(Stream source, Stream destination, X509Certificate2 certificate);
    }
}
