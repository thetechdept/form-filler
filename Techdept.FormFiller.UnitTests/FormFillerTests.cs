using System.IO;
using Xunit;
using Techdept.FormFiller.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Techdept.FormFiller.IntegrationTests
{
    public class FormFillerTests
    {
        private readonly ITestOutputHelper output;

        public FormFillerTests(ITestOutputHelper output)
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

        [Fact]
        public async Task FillFields()
        {
            // Arrange
            var service = new PdfAcroFormFiller();
            using var source = File.OpenRead("./Resources/Test Form.pdf");
            using var destination = new MemoryStream();

            // Act
            var values = new Dictionary<string, string>()
            {
                ["Text Field"] = "Test"
            };
            await service.SetValues(source, destination, values);

            // Assert
            const string key = "Text Field";
            destination.Position = 0;
            var fields = await service.GetFields(destination);                        
            Assert.DoesNotContain(key, fields);
        }

    }
}
