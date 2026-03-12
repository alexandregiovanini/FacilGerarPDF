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

class Program
{
    static void Main()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string inputTxt = System.IO.Path.Combine(baseDir, "etiqueta.txt");
            string outputPdf = System.IO.Path.Combine(baseDir, "etiquetas.pdf");

            if (!File.Exists(inputTxt))
            {
                Console.WriteLine("Falha: arquivo etiqueta.txt não encontrado.");
                return;
            }

            // Lê o conteúdo do arquivo TXT em UTF-8
            string conteudo = File.ReadAllText(inputTxt, Encoding.UTF8);

            // Normaliza quebras de linha
            string[] linhas = conteudo.Replace("\r", "")
                                      .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Carrega fonte em negrito
            var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

            // Define tamanho da etiqueta: 10cm x 15cm
            float largura = 425.25f;  
            float altura = 283.5f; 
            PageSize etiquetaSize = new PageSize(largura, altura);

            using (PdfWriter writer = new PdfWriter(outputPdf))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf, etiquetaSize))
            {
                // Margens pequenas para caber bem na etiqueta
                document.SetMargins(20, 20, 20, 20);

                for (int i = 1; i < linhas.Length; i++) // pula cabeçalho
                {
                    string[] campos = linhas[i].Split(';');
                    if (campos.Length < 11) continue;

                    string nome = campos[3].Trim();
                    string matricula = campos[5].Trim(); // CodigoNaEmpresa
                    string codEmpresa = campos[0].Trim();
                    string nomeEmpresa = campos[1].Trim();

                    if (i > 1) document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    // Container centralizado
                    Div etiqueta = new Div()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    etiqueta.Add(new Paragraph($"NOME - {nome}")
                        .SetFont(fontBold).SetFontSize(14));

                    etiqueta.Add(new Paragraph($"MATRÍCULA - {matricula}")
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
                        .SetTextAlignment(TextAlignment.CENTER));

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