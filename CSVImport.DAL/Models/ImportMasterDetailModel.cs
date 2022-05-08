using System;

namespace CSVImport.DAL.Models
{
    public class ImportMasterDetailModel
    {
        public DateTime? TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? EffectiveStatusDate { get; set; }
        public string TimeBreached { get; set; }
    }
}
