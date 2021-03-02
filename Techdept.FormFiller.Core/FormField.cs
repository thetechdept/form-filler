using System.Collections.Generic;

namespace Techdept.FormFiller.Core
{
    public class FormField
    {
        public FieldType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ICollection<string> Choices { get; set; } = new List<string>();
        public FormFieldPosition? Position { get; set; }
    }
}
