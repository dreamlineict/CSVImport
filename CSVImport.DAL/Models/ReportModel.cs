namespace CSVImport.DAL.Models
{
    public class ReportModel
    {
        public int BranchCode { get; set; }
        public string AccountType { get; set; }
        public string Status { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
