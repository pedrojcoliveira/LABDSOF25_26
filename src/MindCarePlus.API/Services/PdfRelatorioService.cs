using MindCarePlus.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MindCarePlus.API.Services;

public interface IPdfRelatorioService
{
    byte[] GerarPdfRelatorio(RelatorioIA relatorio, string nomePaciente);
}

public class PdfRelatorioService : IPdfRelatorioService
{
    public PdfRelatorioService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarPdfRelatorio(RelatorioIA relatorio, string nomePaciente)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                // Header
                page.Header().Element(ComposeHeader);

                // Content
                page.Content().Element(container => ComposeContent(container, relatorio, nomePaciente));

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Gerado em: ");
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
                    x.Span(" | MindCarePlus");
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("MindCarePlus").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                column.Item().Text("Relatório de Progresso").FontSize(14).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(QuestPDF.Infrastructure.IContainer container, RelatorioIA relatorio, string nomePaciente)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Spacing(15);

            // Informações do paciente e período
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Paciente:").SemiBold();
                    col.Item().Text(nomePaciente).FontSize(12);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Período:").SemiBold();
                    col.Item().Text($"{relatorio.PeriodoInicio:dd/MM/yyyy} - {relatorio.PeriodoFim:dd/MM/yyyy}").FontSize(12);
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Médias
            column.Item().Text("Resumo do Período").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Humor").SemiBold();
                    header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Energia").SemiBold();
                    header.Cell().Background(Colors.Red.Lighten3).Padding(5).Text("Stress").SemiBold();
                });

                // Values
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text($"{relatorio.MediaHumor:F1}/5").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text($"{relatorio.MediaEnergia:F1}/5").FontSize(18).SemiBold().FontColor(Colors.Orange.Medium);
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                    .Text($"{relatorio.MediaStress:F1}/5").FontSize(18).SemiBold().FontColor(Colors.Red.Medium);
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Análise Completa
            column.Item().Text("Análise do Período").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
            column.Item().Background(Colors.Grey.Lighten4).Padding(10)
                .Text(relatorio.RelatorioCompleto).FontSize(11).LineHeight(1.5f);

            // Tendências de Humor
            column.Item().Text("Tendências de Humor").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken1);
            column.Item().PaddingLeft(10).Text(relatorio.TendenciasHumor).FontSize(10).LineHeight(1.4f);

            // Tendências de Energia
            column.Item().Text("Tendências de Energia").FontSize(12).SemiBold().FontColor(Colors.Orange.Darken1);
            column.Item().PaddingLeft(10).Text(relatorio.TendenciasEnergia).FontSize(10).LineHeight(1.4f);

            // Tendências de Stress
            column.Item().Text("Tendências de Stress").FontSize(12).SemiBold().FontColor(Colors.Red.Darken1);
            column.Item().PaddingLeft(10).Text(relatorio.TendenciasStress).FontSize(10).LineHeight(1.4f);

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Recomendações
            column.Item().Text("Recomendações").FontSize(14).SemiBold().FontColor(Colors.Green.Darken1);
            column.Item().Background(Colors.Green.Lighten4).Padding(10)
                .Text(relatorio.Recomendacoes).FontSize(10).LineHeight(1.5f);
        });
    }
}
