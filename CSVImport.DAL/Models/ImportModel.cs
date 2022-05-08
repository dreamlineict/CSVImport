using System;

namespace CSVImport.DAL.Models
{
    public class ImportModel
    {
        public int PaymentID { get; set; }
        public string AccountHolder { get; set; }
        public int BranchCode { get; set; }
        public long AccountNumber { get; set; }
        public int AccountType { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public int Status { get; set; }
        public DateTime? EffectiveStatusDate { get; set; }
    }
}
