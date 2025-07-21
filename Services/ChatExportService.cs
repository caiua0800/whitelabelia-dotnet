using System.Text;
using OfficeOpenXml;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;
using backend.Services;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

public interface IChatExportService
{
    Task<byte[]> ExportChatsToExcelAsync();
    Task<byte[]> ExportChatsToExcelAdvancedAsync(); // Novo método
    Task<byte[]> ExportChatsToPdfAsync();
}

public class ChatExportService : IChatExportService
{
    private readonly IChatService _chatService;
    private readonly IConverter _pdfConverter;

    public ChatExportService(IChatService chatService, IConverter pdfConverter)
    {
        _chatService = chatService;
        _pdfConverter = pdfConverter;
    }

    public async Task<byte[]> ExportChatsToExcelAdvancedAsync()
    {
        var chats = await _chatService.GetAllChatsWithLastMessageAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Clientes");

            // Estilos
            var headerStyle = workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Fill.BackgroundColor = XLColor.LightGray;
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Cabeçalhos (agora com 6 colunas, sem Tags)
            worksheet.Cell(1, 1).Value = "Nome";
            worksheet.Cell(1, 2).Value = "Contato";
            worksheet.Cell(1, 3).Value = "Data";
            worksheet.Cell(1, 4).Value = "Cidade";
            worksheet.Cell(1, 5).Value = "Estado";
            worksheet.Cell(1, 6).Value = "Status";

            // Aplicar estilo ao cabeçalho
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style = headerStyle;

            // Dados
            int row = 2;
            foreach (var chat in chats)
            {
                worksheet.Cell(row, 1).Value = chat.ClientName ?? "Não cadastrado";
                worksheet.Cell(row, 2).Value = chat.Id;
                worksheet.Cell(row, 3).Value = chat.DateCreated.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = chat.City ?? "Não cadastrado";
                worksheet.Cell(row, 5).Value = chat.State ?? "Não cadastrado";
                worksheet.Cell(row, 6).Value = chat.Status == 1 ? "Ativo" : "Inativo";

                // Formata condicional para status
                if (chat.Status == 1)
                    worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
                else
                    worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Red;

                row++;
            }

            // Autoajustar colunas
            worksheet.Columns().AdjustToContents();

            // Adicionar tabela formatada (sem conflito com AutoFilter)
            var tableRange = worksheet.Range(1, 1, row - 1, 6);
            var table = tableRange.CreateTable();
            table.ShowAutoFilter = true; // Habilita filtros diretamente na tabela

            // Salvar em MemoryStream
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    public async Task<byte[]> ExportChatsToExcelAsync()
    {
        var chats = await _chatService.GetAllChatsWithLastMessageAsync();
        var csv = new StringBuilder();

        // Cabeçalhos
        csv.AppendLine("Nome,Contato,Data,Cidade,Estado,Status");

        // Dados
        foreach (var chat in chats)
        {
            csv.AppendLine(
                $"\"{chat.ClientName ?? "Não cadastrado"}\"," +
                $"\"{chat.Id}\"," +
                $"\"{(chat.DateCreated != default ? chat.DateCreated.ToString("dd/MM/yyyy") : "Não cadastrado")}\"," +
                $"\"{chat.City ?? "Não cadastrado"}\"," +
                $"\"{chat.State ?? "Não cadastrado"}\"," +
                $"\"{(chat.Status == 1 ? "Ativo" : "Inativo")}\""
            );
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportChatsToPdfAsync()
    {
        var chats = await _chatService.GetAllChatsWithLastMessageAsync();

        using (var ms = new MemoryStream())
        {
            // Usa construtor básico sem WriterProperties
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            document.SetFont(font);

            document.Add(new Paragraph("Tabela de Clientes")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
            );

            Table table = new Table(6, true);
            table.AddHeaderCell("Nome");
            table.AddHeaderCell("Contato");
            table.AddHeaderCell("Data");
            table.AddHeaderCell("Cidade");
            table.AddHeaderCell("Estado");
            table.AddHeaderCell("Status");

            foreach (var chat in chats)
            {
                table.AddCell(chat.ClientName ?? "Não cadastrado");
                table.AddCell(chat.Id ?? "Não cadastrado");
                table.AddCell(chat.DateCreated != default ? chat.DateCreated.ToString("dd/MM/yyyy") : "Não cadastrado");
                table.AddCell(chat.City ?? "Não cadastrado");
                table.AddCell(chat.State ?? "Não cadastrado");
                table.AddCell(chat.Status == 1 ? "Ativo" : "Inativo");
            }

            document.Add(table);
            document.Close();

            return ms.ToArray();
        }
    }
}
