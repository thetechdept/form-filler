using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Techdept.FormFiller.Core;

[assembly: FunctionsStartup(typeof(Techdept.FormFiller.Functions.Startup))]
namespace Techdept.FormFiller.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;

            builder.Services.AddTransient<IFormFiller, PdfAcroFormFiller>();
        }
    }
}