using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;

namespace Techdept.FormFiller.Core
{
    public class DocumentSigner : IDocumentSigner
    {
        public Task Sign(
            Stream source,
            Stream destination,
            X509Certificate2 certificate,
            Stream? signatureImage,
            string signatureField,
            CancellationToken cancellationToken = default)
        {
            var reader = new PdfReader(source);
            var stampingProperties = new StampingProperties();
            stampingProperties.UseAppendMode();
            var signer = new PdfSigner(reader, destination, stampingProperties);

            signer.SetFieldName(signatureField);

            var appearance = signer.GetSignatureAppearance();
            appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.NAME_AND_DESCRIPTION);

            // TODO: to completely customise appearance
            // var layer = appearance.GetLayer2();
            // var canvas = new Canvas(layer, signer.GetDocument());

            if (signatureImage != null)
            {
                var imageMemoryStream = new MemoryStream();
                signatureImage.CopyTo(imageMemoryStream);
                var imageBytes = imageMemoryStream.ToArray();

                var signatureImageData = ImageDataFactory.CreatePng(imageBytes);
                appearance
                    .SetRenderingMode(PdfSignatureAppearance.RenderingMode.NAME_AND_DESCRIPTION)
                    .SetSignatureGraphic(signatureImageData);
            }

            var pks = new X509Certificate2Signature(certificate);
            var bcCert = DotNetUtilities.FromX509Certificate(certificate);
            var chain = new Org.BouncyCastle.X509.X509Certificate[1] { bcCert };

            // var tsaClient = new TSAClientBouncyCastle("http://timestamp.digicert.com");

            signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

            return Task.CompletedTask;
        }

        private void AddWatermark(PdfSignatureAppearance appearance)
        {
            var backgroundImage = ImageDataFactory.CreatePng(new Uri("https://international.growthco.uk/media/1001/gcreporting.png"));

            var r = appearance.GetPageRect();
            var w1 = r.GetWidth();
            var h1 = r.GetHeight();
            var w2 = backgroundImage.GetWidth();
            var h2 = backgroundImage.GetHeight();

            var s = w1 / h1 < w2 / h2 ? w1 / w2 : h1 / h2;

            appearance
                .SetImage(backgroundImage)
                .SetImageScale(s);
        }
    }
}