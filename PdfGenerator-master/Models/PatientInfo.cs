using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator.Models
{
    public class PatientInfo
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public string PatientId { get; set; }
        public string Gender { get; set; }
        public DateTime CheckDate { get; set; }
        public string AttendingDoctor { get; set; }

    }
}
