using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Techdept.FormFiller.Core
{
    public class FormField
    {
        public FieldType Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public string[] Choices { get; set; }
    }

    public enum FieldType
    {
        Text,
        Checkbox,
        Radiobutton,
        Signature,
        Unknown
    }

    public interface IFormFiller
    {
        Task<IDictionary<string, FormField>> GetFields(Stream source);

        Task SetValues(Stream source, Stream destination, IDictionary<string, string> values);
    }
}
