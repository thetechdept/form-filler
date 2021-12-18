using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using iText.Signatures;

namespace Techdept.FormFiller.Core
{
    internal class X509Certificate2Signature : IExternalSignature
    {
        private readonly X509Certificate2 certificate;

        public X509Certificate2Signature(X509Certificate2 certificate)
        {
            if (certificate.GetRSAPrivateKey() == null)
            {
                throw new ArgumentException("Certificate has no RSA private key");
            }

            this.certificate = certificate;
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        public string GetHashAlgorithm()
        {
            return HashAlgorithmName.SHA256.Name!;
        }

        public byte[] Sign(byte[] message)
        {
            var rsa = certificate.GetRSAPrivateKey();
            return rsa!.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}