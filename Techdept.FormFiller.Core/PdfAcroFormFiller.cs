using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;

namespace Techdept.FormFiller.Core
{
    public class PdfAcroFormFiller : IFormFiller
    {
        private readonly ILogger logger;

        public PdfAcroFormFiller(ILogger<PdfAcroFormFiller> logger)
        {
            this.logger = logger;
        }

        public Task<IDictionary<string, FormField>> GetFields(Stream source)
        {
            var reader = new PdfReader(source);
            var doc = new PdfDocument(reader);
            var form = PdfAcroForm.GetAcroForm(doc, false);

            var fields = form?.GetFormFields() ?? new Dictionary<string, PdfFormField>();

            FieldType GetFieldType(PdfFormField field)
            {
                var pdfName = field.GetFormType();
                if (pdfName == PdfName.Btn)
                {
                    return FieldType.Radiobutton;
                }
                else if (pdfName == PdfName.Ch)
                {
                    return FieldType.Checkbox;
                }
                else if (pdfName == PdfName.Sig)
                {
                    return FieldType.Signature;
                }
                else if (pdfName == PdfName.Tx)
                {
                    return FieldType.Text;
                }
                return FieldType.Unknown;
            }

            var dictionary = fields.ToDictionary(x => x.Key, x =>
            {
                var field = x.Value;
                var type = GetFieldType(field);

                return new FormField
                {
                    Name = x.Key,
                    Type = type,
                    Value = field.GetValueAsString()
                };
            });

            doc.SetCloseReader(false);
            doc.Close();

            return Task.FromResult(dictionary as IDictionary<string, FormField>);
        }

        public Task SetValues(Stream source, Stream destination, IDictionary<string, string> values, FlattenMode flattenMode = FlattenMode.None)
        {
            var reader = new PdfReader(source);
            var writer = new PdfWriter(destination);
            var doc = new PdfDocument(reader, writer);
            doc.SetCloseWriter(false);
            doc.SetCloseReader(false);

            var form = PdfAcroForm.GetAcroForm(doc, false);

            if (form == null)
            {
                doc.Close();
                return Task.CompletedTask;
            }

            var fields = values
                .Select(x => new
                {
                    Key = x.Key,
                    Field = form.GetField(x.Key),
                    Value = x.Value
                })
                .Where(f => f.Field != null)
                .ToList();


            fields.ForEach(f =>
            {
                try
                {
                    f.Field.SetValue(f.Value);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Error setting value - {f.Key}:{f.Value}");
                }
            });

            if (flattenMode != FlattenMode.None)
            {
                var partialFlattenFields = new List<string>();
                switch (flattenMode)
                {
                    case FlattenMode.Filled:
                        partialFlattenFields.AddRange(fields.Select(x => x.Key));
                        break;
                    case FlattenMode.ExcludeSignature:
                        partialFlattenFields.AddRange(form.GetFormFields().Where(field => field.Value.GetFormType() != PdfName.Sig).Select(x => x.Key));
                        break;
                }

                partialFlattenFields.ForEach(k => form.PartialFormFlattening(k));

                form.FlattenFields();
            }

            doc.Close();

            return Task.CompletedTask;
        }
    }
}