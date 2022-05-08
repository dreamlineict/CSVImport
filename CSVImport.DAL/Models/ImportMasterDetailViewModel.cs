using System.Collections.Generic;

namespace CSVImport.DAL.Models
{
    public class ImportMasterDetailViewModel
    {
        public string AccountNumber { get; set; }
        public List<ImportMasterModel> ImportMasters { get; set; }
        public List<ImportMasterDetailModel> ImportMasterDetails { get; set; }
    }
}
