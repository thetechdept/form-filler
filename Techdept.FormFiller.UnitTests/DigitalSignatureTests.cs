using System.IO;
using Xunit;
using Techdept.FormFiller.Core;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Techdept.FormFiller.UnitTests
{
    public class DigitalSignatureTests
    {
        private readonly ITestOutputHelper output;

        public DigitalSignatureTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GetFields()
        {
            // Arrange
            var service = new PdfAcroFormFiller();
            using var source = File.OpenRead("./Resources/Test Form.pdf");

            // Act
            var fields = await service.GetFields(source);

            // Assert
            Assert.Contains("Text Field", fields);
        }

    }
}
