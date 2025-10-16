using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator.Models
{
    public class SurgeryInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UltrasoundFrequency { get; set; }
        public string TreatmentHeadVoltage { get; set; }
        public string Surgeon { get; set; }
    }
}
