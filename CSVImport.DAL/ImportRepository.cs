using CSVImport.DAL.edmx;
using CSVImport.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSVImport.DAL
{
    public class ImportRepository : IImportRepository, IDisposable
    {
        public ImportsContext context;

        public ImportRepository(ImportsContext context)
        {
            this.context = context;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<ReportModel> GetImportsReport()
        {
            return context.usp_ImportReport().Select(x =>
            new ReportModel
            {
                AccountType = x.AccountType,
                BranchCode = x.BranchCode,
                Status = x.Status,
                TotalAmount = (decimal)x.TotalAmount,
                TotalCount = (int)x.TotalCount
            }).ToList();
        }

        public IEnumerable<ImportMasterModel> GetImportMasters()
        {
            return context.usp_ImportMaster().Select(x =>
            new ImportMasterModel
            {
                AccountHolder = x.AccountHolder,
                AccountNumber = x.AccountNumber,
                AccountType = x.AccountType,
                BranchCode = x.BranchCode
            }).ToList();
        }

        public ImportData GetImportMaster(long AccountNumber)
        {
            return context.ImportDatas.Where(x => x.AccountNumber == AccountNumber).FirstOrDefault();
        }

        public IEnumerable<ImportMasterDetailModel> GetImportMasterDetails(long accountNumber)
        {
            return context.usp_ImportMaster_Details(accountNumber).Select(x =>
            new ImportMasterDetailModel
            {
                Amount = (decimal)x.Amount,
                EffectiveStatusDate = x.EffectiveStatusDate,
                Status = x.Status,
                TimeBreached = x.TimeBreached,
                TransactionDate = x.TransactionDate
            }).ToList();
        }

        public void InsertImport(ImportData import)
        {
            context.ImportDatas.Add(import);
        }

        public void UpdateImport(ImportData import)
        {
            context.usp_UpdateAccountTypeForAccount(import.AccountNumber, import.AccountType);
        }
    }
}
