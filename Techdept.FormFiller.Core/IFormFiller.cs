using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Techdept.FormFiller.Core
{
    public interface IFormFiller
    {
        Task<IDictionary<string, FormField>> GetFields(Stream source);
        Task SetValues(Stream source, Stream destination, IDictionary<string, string> values);
    }
}
