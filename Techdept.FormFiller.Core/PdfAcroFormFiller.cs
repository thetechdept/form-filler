using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Techdept.FormFiller.Core
{
    public class PdfAcroFormFiller : IFormFiller
    {
        public Task<IDictionary<string, FormField>> GetFields(Stream source)
        {
            var reader = new PdfReader(source);
            var doc = new PdfDocument(reader);
            var form = PdfAcroForm.GetAcroForm(doc, false);

            var fields = form?.GetFormFields() ?? new Dictionary<string, PdfFormField>();

            FieldType GetFieldType(PdfFormField field)
            {
                var pdfName = field.GetFormType();
                if (Equals(pdfName, PdfName.Btn))
                {
                    return FieldType.Radiobutton;
                }

                if (Equals(pdfName, PdfName.Ch))
                {
                    return FieldType.Checkbox;
                }

                if (Equals(pdfName, PdfName.Sig))
                {
                    return FieldType.Signature;
                }

                if (Equals(pdfName, PdfName.Tx))
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
            var document = new Document(doc);

            var form = PdfAcroForm.GetAcroForm(doc, false);

            if (form == null)
            {
                doc.Close();
                return Task.CompletedTask;
            }

            var fields = values
                .Select(x => new
                {
                    x.Key,
                    Field = form.GetField(x.Key),
                    x.Value
                })
                .Where(f => f.Field != null)
                .ToList();

            fields.ForEach(f =>
            {
                try
                {
                    var states = f.Field.GetAppearanceStates();
                    if (states.Length > 1)
                    {
                        var state = states.FirstOrDefault(state => f.Value.Equals(state, StringComparison.InvariantCultureIgnoreCase));
                        f.Field.SetValue(state ?? states.First());
                    }
                    else
                    {
                        if (f.Field is PdfButtonFormField pdfButtonFormField)
                        {
                            if (pdfButtonFormField.IsPushButton())
                            {
                                pdfButtonFormField.SetValue(f.Value);
                                pdfButtonFormField.SetReadOnly(true);
                            }
                        }

                        if (f.Key.EndsWith("_af_image"))
                        {
                            var bytes = Convert.FromBase64String(f.Value);
                            var imageData = ImageDataFactory.Create(bytes);
                            var rectangle = f.Field.GetPdfObject().GetAsRectangle(PdfName.Rect);
                            var image = new Image(imageData);

                            // best-fit image to form field
                            var rWidth = rectangle.GetWidth();
                            var rHeight = rectangle.GetHeight();

                            if (Math.Abs(image.GetImageWidth() - rWidth) > 0 || Math.Abs(image.GetImageHeight() - rHeight) > 0)
                            {
                                image = image.ScaleToFit(rWidth, rHeight);
                            }

                            var rLeft = Math.Round(rectangle.GetLeft());
                            var rBottom = Math.Round(rectangle.GetBottom());
                            image.SetFixedPosition((float)rLeft, (float)rBottom);

                            document.Add(image);

                            form.RemoveField(f.Key);
                        }
                        else
                        {
                            f.Field.SetValue(f.Value);
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO: log
                    // this.logger.LogError(ex, $"Error setting value - {f.Key}:{f.Value}");
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

            document.Close();
            doc.Close();

            return Task.CompletedTask;
        }
    }
}