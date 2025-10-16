using Flurl.Http;
using PdfGenerator.Models;
using PdfGenerator.ViewModel;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator;

public class Export_PdfSharpCore
{
    private PostoperativeReportViewModel model = new ();
    //黑体
    private readonly XFont defaultFont = new XFont("simhei", 10, XFontStyle.Regular);
    public Export_PdfSharpCore()
    {
        var outputPath = System.IO.Path.Combine(
            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"Export_PdfSharpCore.pdf");

        GeneratePDF(outputPath);
    }
    public void GeneratePDF(string outputPath)
    {
        // 创建新 PDF 文档
        var document = new PdfDocument();
        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        
        // 创建绘图对象
        var gfx = XGraphics.FromPdfPage(page);
        
        // === 标题 ===
        DrawHeader(gfx,page, model.Title);

        // === 病人信息 ===
        DrawSection(gfx, page, "病人信息", new List<RowData>
    {
        new RowData { Label = "姓名：", Value = model.Patient.Name },
        new RowData { Label = "病人ID：", Value = model.Patient.PatientId },
        new RowData { Label = "年龄：", Value = model.Patient.Age },
        new RowData { Label = "性别：", Value = model.Patient.Gender },
        new RowData { Label = "检查日期：", Value = model.Patient.CheckDate.ToString("yyyy-MM-dd") },
        new RowData { Label = "主治医师：", Value = model.Patient.AttendingDoctor }
    });

    //    // === 手术信息 ===
    //    DrawSection(gfx,page, "手术信息", new List<RowData>
    //{
    //    new RowData { Label = "开始时间：", Value = model.Surgery.StartTime.ToString("yyyy-MM-dd HH:mm") },
    //    new RowData { Label = "结束时间：", Value = model.Surgery.EndTime.ToString("yyyy-MM-dd HH:mm") },
    //    new RowData { Label = "超声频率：", Value = model.Surgery.UltrasoundFrequency },
    //    new RowData { Label = "治疗头电压：", Value = model.Surgery.TreatmentHeadVoltage },
    //    new RowData { Label = "手术医师：", Value = model.Surgery.Surgeon }
    //});

    //    // === 术前图像 ===
    //    DrawImageSection(gfx,page, "术前图像", model.PreOpImagePaths);

    //    // === 术后图像 ===
    //    DrawImageSection(gfx,page, "术后图像", model.PostOpImagePaths);

    //    // === 术后评估 ===
    //    DrawSection(gfx,page, "术后评估", new List<RowData>
    //{
    //    new RowData { Label = "", Value = model.Evaluation }
    //});

        // 保存 PDF
        document.Save(outputPath);
    }

    // === 辅助类 ===
    private class RowData
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    // === 标题绘制方法 ===
    private void DrawHeader(XGraphics gfx,PdfPage page, string title)
    {
        var font = new XFont("simhei", 18, XFontStyle.Bold);
        var textFormat = new XStringFormat
        {
            Alignment = XStringAlignment.Center,
            LineAlignment = XLineAlignment.Near
        };

        // 标题区域
        var rect = new XRect(0, 0, page.Width, 50);
        gfx.DrawString(title, font, XBrushes.Black, rect, textFormat);
    }

    // === 段落绘制方法（表格） ===
    private void DrawSection(XGraphics gfx,PdfPage page, string sectionTitle, List<RowData> rows)
    {
        // 计算布局参数
        double leftMargin = 20;
        double topMargin = 20;
        double sectionSpacing = 10;
        double rowHeight = 20;
        double columnWidth = 150;

        // 绘制标题
        var sectionY = page.Height - gfx.PageSize.Height + topMargin; // 从顶部开始
        DrawSectionHeader(gfx, page, sectionTitle, sectionY);
        sectionY += 30; // 标题高度

        var textFormat = new XStringFormat
        {
            Alignment = XStringAlignment.Near,
            LineAlignment = XLineAlignment.Near // 或其他对齐方式
        };

        // 绘制表格
        foreach (var row in rows)
        {
            if (row.Value == null)
            {
                row.Value = " - ";
            }
            // 标签列（左对齐）
            gfx.DrawString(row.Label, defaultFont, XBrushes.Black,
                new XRect(leftMargin, sectionY, columnWidth, rowHeight), textFormat);

            // 值列（右对齐）
            gfx.DrawString(row.Value, defaultFont, XBrushes.Black,
                new XRect(leftMargin + columnWidth, sectionY, page.Width - 2 * leftMargin, rowHeight),
                textFormat);

            sectionY += rowHeight + 2; // 行间距
        }

        // 更新顶部边距
        topMargin = sectionY + sectionSpacing;
    }

    // === 段落标题绘制 ===
    private void DrawSectionHeader(XGraphics gfx,PdfPage page, string title, double y)
    {
        const double headerHeight = 30;
        const double leftMargin = 20;

        // 背景矩形
        var rect = new XRect(0, y, page.Width, headerHeight);
        gfx.DrawRectangle(XBrushes.LightGray, rect);

        // 标题文本
        var textFormat = new XStringFormat
        {
            Alignment = XStringAlignment.Near,
            LineAlignment = XLineAlignment.Center
        };
        gfx.DrawString(title, new XFont("simhei", 14, XFontStyle.Bold),
            XBrushes.Black, rect, textFormat);
    }

    // === 图像绘制方法 ===
    private void DrawImageSection(XGraphics gfx, PdfPage page, string sectionTitle, List<string> imagePaths)
    {
        const double imagePadding = 10;
        const double imageMaxWidth = 400; // 图像最大宽度
        const double imageMaxHeight = 300; // 图像最大高度

        // 计算当前位置
        var currentY = page.Height - gfx.PageSize.Height + 20; // 从顶部开始

        // 绘制标题
        DrawSectionHeader(gfx, page, sectionTitle, currentY);
        currentY += 30; // 标题高度

        foreach (var path in imagePaths.Where(p => File.Exists(p)))
        {
            try
            {
                // 加载并缩放图像
                using var image = Image.Load(path);
                double scale = Math.Min(
                    imageMaxWidth / image.Width,
                    imageMaxHeight / image.Height);
                double scaledWidth = image.Width * scale;
                double scaledHeight = image.Height * scale;

                // 计算位置（居中）
                double x = (page.Width - scaledWidth) / 2;
                double y = currentY;

                // 插入图像
                using var ms = new MemoryStream();
                var xImage = XImage.FromStream(() => ms);
                //using var xImage = XImage.FromGdiPlusBitmap(image.ToGdiBitmap());
                gfx.DrawImage(xImage, x, y, scaledWidth, scaledHeight);

                currentY += scaledHeight + imagePadding;
            }
            catch
            {
                // 处理错误（可添加红色文本提示）
                gfx.DrawString($"[图像加载失败: {Path.GetFileName(path)}]",
                    new XFont("simhei", 10, XFontStyle.Italic),
                    XBrushes.Red, new XPoint(20, currentY));
                currentY += 20;
            }
        }
    }

}