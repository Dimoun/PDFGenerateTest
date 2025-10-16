using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.Rendering;
using PdfGenerator.FontResolver;
using PdfGenerator.ViewModel;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Drawing.Layout.enums;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfGenerator;

public class Export_MigraDocCore
{
    private static readonly string outputPath = System.IO.Path.Combine(
        new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"Export_MigraDocCore.pdf");

    private PostoperativeReportViewModel model = new();
    private double contentWidth = 0;
    private const double A4_WIDTH = 595; // A4 宽度（points）
    public Export_MigraDocCore()
    {
        GeneratePDF();
    }

    public void GeneratePDF()
    {
        if (string.IsNullOrEmpty(outputPath))
            throw new ArgumentException("输出路径不能为空", nameof(outputPath));

        // 设置中文字体解析器（关键！）
        GlobalFontSettings.FontResolver = new BaseFontResolver();

        var doc = new Document();

        // 全局字体设置
        doc.Styles["Normal"].Font.Name = "simhei";
        doc.Styles["Normal"].Font.Size = 12;
        doc.Styles["Normal"].Font.Bold = true;

        var section = doc.AddSection();

        // 设置 A4 页面 + 页边距（20 points ≈ 7mm）
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = Unit.FromPoint(20);
        section.PageSetup.RightMargin = Unit.FromPoint(20);
        section.PageSetup.TopMargin = Unit.FromPoint(20);
        section.PageSetup.BottomMargin = Unit.FromPoint(20);

        contentWidth = A4_WIDTH
                       - section.PageSetup.LeftMargin.Point
                       - section.PageSetup.RightMargin.Point;
        // === 标题 ===
        var titlePara = section.AddParagraph(model.Title);
        titlePara.Format.Alignment = ParagraphAlignment.Center;
        titlePara.Format.Font.Size = 24;
        titlePara.Format.Font.Bold = true;
        titlePara.Format.SpaceAfter = Unit.FromPoint(15);

        // === 病人信息 ===
        AddSectionHeader(section, "病人信息");
        AddPatientInfoTable(section);

        // === 手术信息 ===
        AddSectionHeader(section, "手术信息");
        AddSurgeryInfoTable(section);

        // === 术前图像 ===
        if (model.PreOpImagePaths?.Any() == true)
        {
            AddSectionHeader(section, "术前图像");
            AddImages(section, model.PreOpImagePaths);
        }

        // === 术后图像 ===
        if (model.PostOpImagePaths?.Any() == true)
        {
            AddSectionHeader(section, "术后图像");
            AddImages(section, model.PostOpImagePaths);
            //}

            // === 术后评估 ===
            AddSectionHeader(section, "术后评估");

            string text = model.Evaluation ?? "无";
            var evalPara = section.AddParagraph();
            evalPara.AddFormattedText(text);
            evalPara.Format.Font.Size = 14;
            evalPara.Format.Font.Bold = true;
            evalPara.Format.LeftIndent = Unit.FromPoint(10);
            evalPara.Format.SpaceAfter = Unit.FromPoint(15);

            // 渲染为 PDF
            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = doc;
            renderer.RenderDocument();

            //var pdfDocument = renderer.PdfDocument;
            //Evaluate(pdfDocument, doc);

            //string text = model.Evaluation ?? "无";
            //var evalPara = section.AddParagraph(text);
            //evalPara.Format.Font.Size = 14;
            //evalPara.Format.Font.Bold = true;
            //evalPara.Format.FirstLineIndent = Unit.FromPoint(10);
            //evalPara.Format.RightIndent = Unit.FromPoint(10);
            //evalPara.Format.SpaceAfter = Unit.FromPoint(15);

            renderer.PdfDocument.Save(outputPath);

            // 可选：自动打开 PDF（仅 Windows）
            Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
        }
    }

    private void AddSectionHeader(Section section, string title)
    {
        var para = section.AddParagraph();
        para.Format.Shading.Color = Colors.LightGray;
        para.Format.Font.Bold = true;
        para.Format.Font.Size = 18;
        para.Format.TabStops.AddTabStop(Unit.FromPoint(10), TabAlignment.Left);

        //添加换行符创建多行
        //para.AddLineBreak();
        para.AddTab();
        para.AddText(title);
        para.AddLineBreak();

        para.Format.SpaceBefore = Unit.FromPoint(15);
        para.Format.SpaceAfter = Unit.FromPoint(15);

        //const double A4_WIDTH = 595; // A4 宽度（points）
        //double leftPadding = 10;
        //double contentWidth = A4_WIDTH
        //                      - section.PageSetup.LeftMargin.Point
        //                      - section.PageSetup.RightMargin.Point
        //                      - leftPadding;

        //var table = section.AddTable();
        //table.AddColumn(Unit.FromPoint(leftPadding));   // 左空白列
        //table.AddColumn(Unit.FromPoint(contentWidth));  // 内容列
        //table.Format.SpaceAfter = Unit.FromPoint(10);
        //table.Format.SpaceBefore = Unit.FromPoint(10);
        //table.Borders.Visible = true;

        //var row = table.AddRow();
        //row.Cells[0].Shading.Color = Colors.LightGray;
        //row.Cells[1].Shading.Color = Colors.LightGray;

        //var para = row.Cells[1].AddParagraph(title);
        //para.Format.Font.Bold = true;
        //para.Format.Font.Size = 16;

    }

    private void AddPatientInfoTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromPoint(contentWidth * 0.2));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.3));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.2));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.3));
        table.Format.Font.Size = 14;
        table.Format.Alignment = ParagraphAlignment.Left;

        table.Format.LeftIndent = Unit.FromPoint(10);

        table.Format.SpaceBefore = Unit.FromPoint(5);
        table.Format.SpaceAfter = Unit.FromPoint(5);

        var pairs = new (string label, string? value)[]
        {
            ("姓名：", model.Patient?.Name),
            ("病人ID：", model.Patient?.PatientId),
            ("年龄：", model.Patient?.Age?.ToString()),
            ("性别：", model.Patient?.Gender),
            ("检查日期：", model.Patient?.CheckDate.ToString("yyyy-MM-dd")),
            ("主治医师：", model.Patient?.AttendingDoctor)
        };

        for (int i = 0; i < pairs.Length; i += 2)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(pairs[i].label).Format.Font.Bold = true;
            row.Cells[1].AddParagraph(pairs[i].value ?? "—");

            if (i + 1 < pairs.Length)
            {
                row.Cells[2].AddParagraph(pairs[i + 1].label).Format.Font.Bold = true;
                row.Cells[3].AddParagraph(pairs[i + 1].value ?? "—");
            }
            else
            {
                row.Cells[2].MergeRight = 1; // 合并剩余单元格
            }
        }
    }

    private void AddSurgeryInfoTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromPoint(contentWidth * 0.2));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.3));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.2));
        table.AddColumn(Unit.FromPoint(contentWidth * 0.3));
        table.Format.Font.Size = 14;
        table.Format.Alignment = ParagraphAlignment.Left;

        table.Format.LeftIndent = Unit.FromPoint(10);
        table.Format.SpaceBefore = Unit.FromPoint(5);
        table.Format.SpaceAfter = Unit.FromPoint(5);

        var pairs = new (string label, string? value)[]
        {
            ("开始时间：", model.Surgery?.StartTime.ToString("yyyy-MM-dd HH:mm")),
            ("结束时间：", model.Surgery?.EndTime.ToString("yyyy-MM-dd HH:mm")),
            ("超声频率：", model.Surgery?.UltrasoundFrequency),
            ("治疗头电压：", model.Surgery?.TreatmentHeadVoltage),
            ("手术医师：", model.Surgery?.Surgeon)
        };

        for (int i = 0; i < pairs.Length; i += 2)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(pairs[i].label).Format.Font.Bold = true;
            row.Cells[1].AddParagraph(pairs[i].value ?? "—");

            if (i + 1 < pairs.Length)
            {
                row.Cells[2].AddParagraph(pairs[i + 1].label).Format.Font.Bold = true;
                row.Cells[3].AddParagraph(pairs[i + 1].value ?? "—");
            }
            else
            {
                row.Cells[2].MergeRight = 1;
            }
        }
    }

    private void AddImages(Section section, List<string> imagePaths)
    {
        if (ImageSource.ImageSourceImpl == null)
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();

        foreach (var path in imagePaths.Where(p => !string.IsNullOrEmpty(p) && File.Exists(p)))
        {
            try
            {
                var para = section.AddParagraph();

                Image image = para.AddImage(ImageSource.FromFile(path, 100));
                image.Width = Unit.FromPoint(contentWidth * 0.95); // 限制宽度
                image.LockAspectRatio = true;
                para.Format.SpaceAfter = Unit.FromPoint(5);
                para.Format.Alignment = ParagraphAlignment.Center;
            }
            catch (Exception ex)
            {
                section.AddParagraph($"[图像加载失败: {Path.GetFileName(path)}]")
                    .Format.Font.Color = Colors.Red;
            }
        }
    }

    public void Evaluate(PdfDocument document, Document doc)
    {
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        // HACK
        gfx.MUH = PdfFontEncoding.Unicode;
        //gfx.MFEH = PdfFontEmbedding.Default;

        XFont font = new XFont("simhei", 12, XFontStyle.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode));

        //gfx.MeasureString(model.Evaluation, font);
        var tf = new XTextFormatter(gfx);
        TextFormatAlignment format = new TextFormatAlignment();
        format.Horizontal = XParagraphAlignment.Left;
        format.Vertical = XVerticalAlignment.Middle;

        tf.DrawString("中文中文 中文中文中文中文 中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中" +
                      " 文中文中文中文中文中文:", font, XBrushes.Red,
            new XRect(100, 100, 300, 300), format);
        // You always need a MigraDocCore document for rendering.
        Section sec = doc.AddSection();
        // Add a single paragraph with some text and format information.
        Paragraph para = sec.AddParagraph();
        para.Format.Alignment = ParagraphAlignment.Justify;
        para.Format.Font.Name = "simhei";
        para.Format.Font.Size = 12;
        para.Format.Font.Color = Colors.DarkGray;
        para.Format.Font.Color = Colors.DarkGray;
        para.AddText("Duisism odigna acipsum delesenisl ");
        para.AddFormattedText("ullum in velenit", TextFormat.Bold);
        para.AddText(" ipit iurero dolum zzriliquisis nit wis dolore vel et nonsequipit, velendigna " +
        "auguercilit lor se dipisl duismod tatem zzrit at laore magna feummod oloborting ea con vel " +
        "essit augiati onsequat luptat nos diatum vel ullum illummy nonsent nit ipis et nonsequis " +
        "niation utpat. Odolobor augait et non etueril landre min ut ulla feugiam commodo lortie ex " +
        "essent augait el ing 中文中文中文中文中文中文 英语 中文中文中文中文中文中文中文中文中文中文" +
        "eumsan hendre feugait prat augiatem amconul laoreet. ≤≥≈≠");
        para.AddFormattedText("中文 中文 中文 中文 中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文");
        para.Format.Borders.Distance = "5pt";
        para.Format.Borders.Color = Colors.Gold;

        // Create a renderer and prepare (=layout) the document
        DocumentRenderer docRenderer = new DocumentRenderer(doc);
        docRenderer.PrepareDocument();

        // Render the paragraph. You can render tables or shapes the same way.
        docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
    }
}

