using PdfGenerator.Models;
using PdfGenerator.ViewModel;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator
{
    public class Export_QuestPDF
    {
        private static readonly string outputPath = System.IO.Path.Combine(
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"Export_QuestPDF.pdf");

        private PostoperativeReportViewModel model = new();

        public Export_QuestPDF()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            GeneratePDF();
        }


        public void GeneratePDF()
        {
            // 验证必要数据
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("输出路径不能为空", nameof(outputPath));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    // === 标题 ===
                    page.Header()
                        .AlignCenter()
                        .PaddingBottom(10)
                        .Text(model.Title)
                        .FontSize(18)
                        .Bold();

                    // === 内容区域 ===
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // 病人信息
                        AddSectionHeader(column, "病人信息");
                        AddPatientInfoTable(column);

                        // 手术信息
                        AddSectionHeader(column, "手术信息");
                        AddSurgeryInfoTable(column);

                        // 术前图像
                        if (model.PreOpImagePaths?.Any() == true)
                        {
                            AddSectionHeader(column, "术前图像");
                            AddImages(column, model.PreOpImagePaths);
                        }

                        // 术后图像
                        if (model.PostOpImagePaths?.Any() == true)
                        {
                            AddSectionHeader(column, "术后图像");
                            AddImages(column, model.PostOpImagePaths);
                        }

                        // 术后评估
                        AddSectionHeader(column, "术后评估");
                        column.Item()
                            .Text(model.Evaluation ?? "无")
                            .FontSize(11);
                    });
                });
            });

            document.GeneratePdf(outputPath);

            //document.ShowInCompanion();
        }

        // === 辅助方法：统一标题样式 ===
        private void AddSectionHeader(ColumnDescriptor container, string title)
        {
            container.Item()
                .ExtendHorizontal()
                .Background(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingLeft(10)
                .Text(title)
                .Bold()
                .FontSize(14);
        }

        // === 辅助方法：病人信息表格 ===
        private void AddPatientInfoTable(ColumnDescriptor container)
        {
            var cellStyle = (IContainer c) => c.PaddingVertical(4).PaddingHorizontal(8);

            container.Item().Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); });

                AddTableRow(table, cellStyle,
                    ("姓名：", model.Patient?.Name),
                    ("病人ID：", model.Patient?.PatientId),
                    ("年龄：", model.Patient?.Age),
                    ("性别：", model.Patient?.Gender),
                    ("检查日期：", model.Patient?.CheckDate.ToString("yyyy-MM-dd")),
                    ("主治医师：", model.Patient?.AttendingDoctor)
                );
            });
        }

        // === 辅助方法：手术信息表格 ===
        private void AddSurgeryInfoTable(ColumnDescriptor container)
        {
            var cellStyle = (IContainer c) => c.PaddingVertical(4).PaddingHorizontal(8);

            container.Item().Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); });

                AddTableRow(table, cellStyle,
                    ("开始时间：", model.Surgery?.StartTime.ToString("yyyy-MM-dd HH:mm")),
                    ("结束时间：", model.Surgery?.EndTime.ToString("yyyy-MM-dd HH:mm")),
                    ("超声频率：", model.Surgery?.UltrasoundFrequency),
                    ("治疗头电压：", model.Surgery?.TreatmentHeadVoltage),
                    ("手术医师：", model.Surgery?.Surgeon)
                );
            });
        }

        // === 通用表格行添加方法 ===
        private void AddTableRow(TableDescriptor table, Func<IContainer, IContainer> cellStyle, params (string label, string value)[] pairs)
        {
            foreach (var (label, value) in pairs)
            {
                table.Cell().Element(cellStyle).Text(label).Bold();
                table.Cell().Element(cellStyle).Text(value ?? "—");
            }
        }

        // === 图像插入方法 ===
        private void AddImages(ColumnDescriptor container, List<string> imagePaths)
        {
            foreach (var path in imagePaths.Where(p => !string.IsNullOrEmpty(p) && File.Exists(p)))
            {
                try
                {
                    container.Item().Row(row =>
                    {
                        row.RelativeItem().Image(path).FitWidth();
                    });
                }
                catch (Exception ex)
                {
                    // 可选：记录日志，或插入占位符
                    container.Item().Text($"[图像加载失败: {Path.GetFileName(path)}]").FontColor(Colors.Red.Lighten1);
                }
            }
        }
    }
}
