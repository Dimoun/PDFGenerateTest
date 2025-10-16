using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator.FontResolver;

public class BaseFontResolver : IFontResolver
{
    public string DefaultFontName => "simhei";

    public byte[] GetFont(string faceName)
    {
        return faceName.ToLower() switch
        {
            "simsun" => LoadFontFile("simsun.ttc"), //宋体
            "simhei" => LoadFontFile("simhei.ttf"), //黑体
            "microsoft yahei" => LoadFontFile("msyh.ttc"),
        };
    }
    private static byte[] LoadFontFile(string fileName)
    {
        string fontPath = Path.Combine("Assets/Fonts", fileName.ToUpper());
        if (!File.Exists(fontPath))
            throw new FileNotFoundException($"中文字体文件未找到: {fontPath}\n请将 {fileName} 放入项目 Fonts 文件夹并设置“始终复制”。");

        return File.ReadAllBytes(fontPath);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return familyName?.ToLowerInvariant() switch
        {
            "microsoft yahei" or "yahei" or "微软雅黑" => new FontResolverInfo("Microsoft YaHei"),
            "simsun" or "宋体" => new FontResolverInfo("SimSun"),
            "simhei" or "黑体" => new FontResolverInfo("SimHei"),
            _ => new FontResolverInfo("SimSun") // 默认宋体
        };
    }
}

