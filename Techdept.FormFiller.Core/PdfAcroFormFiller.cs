using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;

namespace Techdept.FormFiller.Core
{
    public class PdfAcroFormFiller : IFormFiller
    {
        public Task<IDictionary<string, FormField>> GetFields(Stream source)
        {
            var reader = new PdfReader(source);
            var doc = new PdfDocument(reader);
            var form = PdfAcroForm.GetAcroForm(doc, false);
            var fields = form.GetFormFields();

            doc.SetCloseReader(false);
            doc.Close();

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

            FormFieldPosition? GetFieldPosition(PdfFormField field, PdfDocument doc)
            {
                var firstWidget = field.GetWidgets().FirstOrDefault();

                if (firstWidget == null)
                {
                    return null;
                }
                var bounds = firstWidget.GetRectangle();
                var page = doc.GetPageNumber(firstWidget.GetPage());
                var rectangle = Rectangle.CreateBoundingRectangleFromQuadPoint(bounds);
                return new FormFieldPosition
                {
                    Top = (int)Math.Round(rectangle.GetTop()),
                    Left = (int)Math.Round(rectangle.GetLeft()),
                    Width = (int)Math.Round(rectangle.GetWidth()),
                    Height = (int)Math.Round(rectangle.GetHeight()),
                    Page = page
                };
            }

            var dictionary = fields.ToDictionary(x => x.Key, x =>
            {
                var field = x.Value;
                var type = GetFieldType(field);
                var position = GetFieldPosition(field, doc);

                return new FormField
                {
                    Name = x.Key,
                    Type = type,
                    Value = field.GetValueAsString(),
                    Position = position
                };
            });

            return Task.FromResult(dictionary as IDictionary<string, FormField>);
        }

        public Task SetValues(Stream source, Stream destination, IDictionary<string, string> values)
        {
            var reader = new PdfReader(source);
            var writer = new PdfWriter(destination);
            var doc = new PdfDocument(reader, writer);

            var form = PdfAcroForm.GetAcroForm(doc, false);

            var fields = values
                .Select(x => new
                {
                    Key = x.Key,
                    Field = form.GetField(x.Key),
                    Value = x.Value
                })
                .Where(f => f.Field != null)
                .ToList();

            fields.ForEach(f => f.Field.SetValue(f.Value));

            doc.SetCloseWriter(false);
            doc.SetCloseReader(false);
            doc.Close();

            return Task.CompletedTask;
        }
    }
}