using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.IO.Image;

class Program
{
    static void Main()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string inputTxt = System.IO.Path.Combine(baseDir, "etiqueta.txt");
            string outputPdf = System.IO.Path.Combine(baseDir, "etiquetas.pdf");
            string logoPath = System.IO.Path.Combine(baseDir, "logof.png");

            if (!File.Exists(inputTxt))
            {
                Console.WriteLine("Falha: arquivo etiqueta.txt não encontrado.");
                return;
            }

            if (!File.Exists(logoPath))
            {
                Console.WriteLine("Falha: arquivo logof.png não encontrado.");
                return;
            }

            string conteudo = File.ReadAllText(inputTxt, Encoding.UTF8);
            string[] linhas = conteudo.Replace("\r", "")
                                      .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (linhas.Length < 2)
            {
                Console.WriteLine("Arquivo etiqueta.txt não contém dados suficientes.");
                return;
            }

            // Detecta pelo primeiro campo do cabeçalho
            string[] cabecalhoCampos = linhas[0].Split(';');
            string primeiroCampo = cabecalhoCampos[0].Trim();

            bool formatoFuncionario = primeiroCampo.Equals("CodEmpresa", StringComparison.OrdinalIgnoreCase);
            bool formatoEmpresa = primeiroCampo.Equals("Cod", StringComparison.OrdinalIgnoreCase);

            var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

            PageSize primeiraPaginaSize = formatoFuncionario
                ? new PageSize(425.25f, 283.5f) // landscape 15x10 cm
                : new PageSize(283.5f, 425.25f); // portrait 10x15 cm

            using (PdfWriter writer = new PdfWriter(outputPdf))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf, primeiraPaginaSize))
            {
                document.SetMargins(20, 20, 20, 20);

                for (int i = 1; i < linhas.Length; i++)
                {
                    string[] campos = linhas[i].Split(';');

                    if (formatoFuncionario && campos.Length < 11) continue;
                    if (formatoEmpresa && campos.Length < 12) continue;

                    if (i > 1)
                    {
                        PageSize etiquetaSize = formatoFuncionario
                            ? new PageSize(425.25f, 283.5f)
                            : new PageSize(283.5f, 425.25f);

                        document.Add(new AreaBreak(etiquetaSize));
                    }

                    Div etiqueta = new Div()
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT);

                    // Logo no topo
                    Image logo = new Image(ImageDataFactory.Create(logoPath))
                        .ScaleToFit(120, 60);
                    etiqueta.Add(logo);

                    if (formatoFuncionario)
                    {
                        string codEmpresa = campos[0].Trim();
                        string nomeEmpresa = campos[2].Trim();
                        string nomeFuncionario = campos[3].Trim();
                        string Cod = campos[4].Trim(); // Cod
                        string codigoNaEmpresa = campos[5].Trim();

                        etiqueta.Add(new Paragraph($"NOME - {nomeFuncionario}")
                            .SetFont(fontBold).SetFontSize(14));
                        etiqueta.Add(new Paragraph($"MATRÍCULA - {codigoNaEmpresa}")
                            .SetFont(fontBold).SetFontSize(14));
                        etiqueta.Add(new Paragraph($"{codEmpresa} - {nomeEmpresa}")
                            .SetFont(fontBold).SetFontSize(12));
                        etiqueta.Add(new Paragraph("\n"));
                        etiqueta.Add(new Paragraph(
@"ATENÇÃO
NÃO JOGAR ESSE ENVELOPE FORA, VOCÊ VAI PRECISAR
VOCÊ DEVE BAIXAR O APLICATIVO FÁCILCARD
USAR A MATRÍCULA ACIMA COMO SENHA
PARA CADASTRAR O APLICATIVO")
                            .SetFont(fontBold).SetFontSize(11)
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                            .SetTextAlignment(TextAlignment.LEFT));
                    }
                    else if (formatoEmpresa)
                    {
                        string fantasia = campos[2].Trim();
                        string endereco = campos[7].Trim();
                        string bairro = campos[8].Trim();
                        string cidade = campos[9].Trim();
                        string estado = campos[10].Trim();
                        string cep = campos[11].Trim();

                        etiqueta.Add(new Paragraph("Remetente: Facilcard Serviços")
                            .SetFont(fontBold).SetFontSize(12));
                        etiqueta.Add(new Paragraph("Rua Ipiranga 957, Carmo Mogi das Cruzes")
                            .SetFont(fontBold).SetFontSize(12));
                        etiqueta.Add(new Paragraph("CEP 08730-000")
                            .SetFont(fontBold).SetFontSize(12));

                        etiqueta.Add(new Paragraph("\n"));

                        // Moldura apenas no bloco do destinatário
                        Div destinatario = new Div()
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                            .SetPadding(5);

                        string codEmpresa = campos[0].Trim(); // o campo "Cod"
                        destinatario.Add(new Paragraph($"Destinatário: {codEmpresa}")
                            .SetFont(fontBold).SetFontSize(12));
                        destinatario.Add(new Paragraph($"{fantasia}")
                            .SetFont(fontBold).SetFontSize(14));
                        destinatario.Add(new Paragraph($"{endereco}, {bairro}")
                            .SetFont(fontBold).SetFontSize(12));
                        destinatario.Add(new Paragraph($"{cidade} - {estado}")
                            .SetFont(fontBold).SetFontSize(12));
                        destinatario.Add(new Paragraph($"CEP {cep}")
                            .SetFont(fontBold).SetFontSize(12));
                        destinatario.Add(new Paragraph("\n"));
                        destinatario.Add(new Paragraph("A/Departamento de RH")
                            .SetFont(fontBold).SetFontSize(12));

                        etiqueta.Add(destinatario);
                    }

                    document.Add(etiqueta);
                }
            }

            Console.WriteLine("PDF gerado com sucesso!");
            Process.Start(new ProcessStartInfo(outputPdf) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.Message);
        }
    }
}