using System.IO;
using Xunit;
using Techdept.FormFiller.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Techdept.FormFiller.UnitTests
{
    public class FormFillerTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILogger<PdfAcroFormFiller> logger = new NullLogger<PdfAcroFormFiller>();

        public FormFillerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GetFields()
        {
            // Arrange
            var service = new PdfAcroFormFiller(logger);
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
            var service = new PdfAcroFormFiller(logger);
            using var source = File.OpenRead("./Resources/Test Form.pdf");
            using var destination = new MemoryStream();

            // Act
            var values = new Dictionary<string, string>()
            {
                ["Check Box 1"] = "Name",
                ["Check Box 2"] = "Name",
                ["Radio Group.1"] = "Name",
                ["Radio Group.2"] = "Name",
                ["Choice 3"] = "Name",
                ["Button"] = "Name",
                ["Text Field"] = "Name",
                ["Date Field"] = "Name",
                ["Dropdown"] = "Name"
            };
            await service.SetValues(source, destination, values);

            // Assert
            const string key = "Text Field";
            destination.Position = 0;
            var fields = await service.GetFields(destination);                        
            Assert.Contains(key, fields);
        }

        [Fact]
        public async Task FlattenModeAll()
        {
            // Arrange
            var service = new PdfAcroFormFiller(logger);
            using var source = File.OpenRead("./Resources/Test Form.pdf");
            using var destination = new MemoryStream();

            // Act
            var values = new Dictionary<string, string>()
            {
                ["Text Field"] = "Test"
            };
            await service.SetValues(source, destination, values, FlattenMode.All);

            // Assert
            destination.Position = 0;
            var fields = await service.GetFields(destination);                        
            Assert.DoesNotContain("Text Field", fields);
            Assert.DoesNotContain("Digital Signature", fields);
        }

        [Fact]
        public async Task FlattenModeFilled()
        {
            // Arrange
            var service = new PdfAcroFormFiller(logger);
            using var source = File.OpenRead("./Resources/Test Form.pdf");
            using var destination = new MemoryStream();

            // Act
            var values = new Dictionary<string, string>()
            {
                ["Text Field"] = "Test"
            };
            await service.SetValues(source, destination, values, FlattenMode.Filled);

            // Assert
            destination.Position = 0;
            var fields = await service.GetFields(destination);
            Assert.DoesNotContain("Text Field", fields);
            Assert.Contains("Check Box 1", fields);
        }
        
        [Fact]
        public async Task FlattenModeExcludeSignature()
        {
            // Arrange
            var service = new PdfAcroFormFiller(logger);
            using var source = File.OpenRead("./Resources/Test Form.pdf");
            using var destination = new MemoryStream();

            // Act
            var values = new Dictionary<string, string>()
            {
                ["Text Field"] = "Test"
            };
            await service.SetValues(source, destination, values, FlattenMode.ExcludeSignature);

            // Assert
            destination.Position = 0;
            var fields = await service.GetFields(destination);
            Assert.DoesNotContain("Text Field", fields);
            Assert.DoesNotContain("Check Box 1", fields);
            Assert.Contains("Digital Signature", fields);
        }
    }
}
