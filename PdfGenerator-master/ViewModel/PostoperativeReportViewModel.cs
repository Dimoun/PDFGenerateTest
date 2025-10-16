using PdfGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator.ViewModel
{
    public class PostoperativeReportViewModel
    {
        public string Title { get; set; } = "术后评估报告";
        public PatientInfo Patient { get; set; }
        public SurgeryInfo Surgery { get; set; }
        public List<string> PreOpImagePaths { get; set; } // 术前图像路径
        public List<string> PostOpImagePaths { get; set; } // 术后图像路径
        public string Evaluation { get; set; } // 术后评估内容
        
        public PostoperativeReportViewModel()
        {
            Init();
        }


        public void Init()
        {
            Patient = new PatientInfo
            {
                Name = "张三",
                PatientId = "P123456",
                Age = "45"
            };
            Surgery = new SurgeryInfo
            {
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now,
                UltrasoundFrequency = "1.5 MHz"
            };
            PreOpImagePaths = new List<string>()
            {
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png"),
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png"),
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png")
            };

            PostOpImagePaths = new List<string>()
            {
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png"),
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png"),
                System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    "Assets", $"Ultrasound.png")
            };
            Evaluation = "术后患者恢复良好，未见明显不适。建议继续观察，定期复查。如有任何异常症状，请及时就医。中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文，1111111111111111111111111111111111111111111111111111111,中文" +
                         "中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文中文";
            //Evaluation = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        }
    }
}
