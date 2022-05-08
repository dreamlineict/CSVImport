using CSVImport.DAL.edmx;
using CSVImport.DAL.Models;
using System;
using System.Collections.Generic;

namespace CSVImport.DAL
{
    public interface IImportRepository : IDisposable
    {
        IEnumerable<ReportModel> GetImportsReport();
        IEnumerable<ImportMasterModel> GetImportMasters();
        ImportData GetImportMaster(long AccountNumber);
        IEnumerable<ImportMasterDetailModel> GetImportMasterDetails(long accountNumber);
        void InsertImport(ImportData import);
        void UpdateImport(ImportData import);
        void Save();
    }
}
