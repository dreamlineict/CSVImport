using CSVImport.DAL;
using CSVImport.DAL.edmx;
using CSVImport.DAL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CSVImport.Controllers
{
    public class ImportController : Controller
    {
        private IImportRepository importRepository;

        public ImportController()
        {
            this.importRepository = new ImportRepository(new ImportsContext());
        }

        public ImportController(IImportRepository importRepository)
        {
            this.importRepository = importRepository;
        }

        [HttpGet]
        public ActionResult ImportFile()
        {
            return View();
        }

        // GET: Import
        [HttpPost]
        public ActionResult ImportFile(HttpPostedFileBase postedFile)
        {
            try
            {
                string filePath = string.Empty;
                if (postedFile != null)
                {
                    string path = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    filePath = path + Path.GetFileName(postedFile.FileName);
                    string extension = Path.GetExtension(postedFile.FileName);
                    if (!extension.ToLower().Contains("csv"))
                    {
                        ViewBag.Result = "Invalid file format...allowed format is .csv";
                        return View();
                    }

                    postedFile.SaveAs(filePath);

                    //Read the contents of CSV file.
                    string csvData = System.IO.File.ReadAllText(filePath);

                    
                    foreach (string row in csvData.Split('\n').Skip(1))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            var context = new ImportsContext();
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    string TransDate = row.Split(',')[5];
                                    string EffectiveDate = row.Split(',')[8].Replace("\r", "");
                                    importRepository.InsertImport(new ImportData
                                    {
                                        ID = Guid.NewGuid(),
                                        PaymentID = int.Parse(row.Split(',')[0]),
                                        AccountHolder = row.Split(',')[1],
                                        BranchCode = int.Parse(row.Split(',')[2]),
                                        AccountNumber = (long)Convert.ToDouble(row.Split(',')[3]),
                                        AccountType = int.Parse(row.Split(',')[4]),
                                        TransactionDate = DateTime.ParseExact(TransDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                        Amount = Convert.ToDecimal(row.Split(',')[6]),
                                        Status = int.Parse(row.Split(',')[7]),
                                        EffectiveStatusDate = DateTime.ParseExact(EffectiveDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                    });
                                    importRepository.Save();
                                    Thread.Sleep(1000);
                                    transaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    ViewBag.Result = "ERROR: " + ex.Message;
                                    transaction.Rollback();
                                }
                            }
                        }
                    }
                    ViewBag.Result = "Import completed successfully.";
                }
            }
            catch(Exception ex) 
            {
                ViewBag.Result = "ERROR: " + ex.Message;
                return View(); 
            }
            return View();
        }

        [HttpGet]
        public ActionResult ImportView(string AccountNumber = "")
        {
            try
            {
                bool valid = false;
                string partialViewHtml = string.Empty;
                ImportMasterDetailViewModel importView = new ImportMasterDetailViewModel 
                { 
                    ImportMasters = importRepository.GetImportMasters().ToList() 
                };

                if (!string.IsNullOrEmpty(AccountNumber))
                {
                    ImportMasterDetailViewModel masterDetails = new ImportMasterDetailViewModel
                    {
                        ImportMasterDetails = importRepository.GetImportMasterDetails((long)Convert.ToDouble(AccountNumber)).ToList()
                    };
                    importView.AccountNumber = AccountNumber;
                    importView.ImportMasterDetails = masterDetails.ImportMasterDetails;
                }
                if (Request.IsAjaxRequest())
                {
                    valid = true;
                    partialViewHtml = RenderPartialViewToString("_ImportMasterDetailsGrid", importView);
                    return Json(new
                    {
                        Valid = valid,
                        Errors = GetErrorsFromModelState(),
                        PartialView = partialViewHtml
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View(importView);
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Report()
        {
            try
            {
                IEnumerable<ReportModel> report = importRepository.GetImportsReport();
                return View(report);
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult EditMaster(string AccountNumber)
        {
            try
            {
                ImportData importData = BuildMasterView(AccountNumber);
                return View(importData);
            }
            catch
            {
                return View();
            }
        }

        private ImportData BuildMasterView(string AccountNumber)
        {
            //Account Type list
            List<SelectListItem> AccTypeList = new List<SelectListItem>()
                {
                    new SelectListItem { Text = "Cheque", Value = "1" },
                    new SelectListItem { Text = "Savings", Value = "2" }
                };

            //Assigning generic list to ViewBag
            ViewBag.AccountTypes = AccTypeList;
            ImportData importData = importRepository.GetImportMaster((long)Convert.ToDouble(AccountNumber));
            return importData;
        }

        [HttpPost]
        public ActionResult EditMaster(ImportData ModelData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (ModelData.AccountNumber > 0 && ModelData.AccountType > 0)
                    {
                        importRepository.UpdateImport(ModelData);
                        ViewBag.Result = "Record Updated Successfully.";
                    }
                }
            }
            catch
            {

            }
            ImportData importData = BuildMasterView(ModelData.AccountNumber.ToString());
            return View(importData);
        }

        virtual public string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        virtual public Dictionary<string, object> GetErrorsFromModelState()
        {
            var errors = new Dictionary<string, object>();
            foreach (var key in ModelState.Keys)
            {
                // Only send the errors to the client.
                if (ModelState[key].Errors.Count > 0)
                {
                    errors[key] = ModelState[key].Errors;
                }
            }

            return errors;
        }
    }
}