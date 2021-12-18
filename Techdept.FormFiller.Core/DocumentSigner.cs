using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;

namespace Techdept.FormFiller.Core
{
    public class DocumentSigner : IDocumentSigner
    {
        public Task Sign(Stream source, Stream destination, X509Certificate2 certificate)
        {
            var image = ImageDataFactory.Create(new byte[0]);

            var reader = new PdfReader(source);
            var signer = new PdfSigner(reader, destination, new StampingProperties());
            signer.SetFieldName("fieldName");

            var appearance = signer.GetSignatureAppearance();
            appearance
                .SetReason("Test reason")
                .SetLocation("Test location");
                //.SetImage(image);

            var pks = new X509Certificate2Signature(certificate);
            var bcCert = DotNetUtilities.FromX509Certificate(certificate);
            var chain = new Org.BouncyCastle.X509.X509Certificate[1] { bcCert };

            signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

            return Task.CompletedTask;
        }
    }
}