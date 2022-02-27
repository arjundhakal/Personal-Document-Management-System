using Microsoft.AspNetCore.Mvc;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using PDMS.UI.Models;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using PDMS.UI.Attributes;
using Serilog;

namespace PDMS.UI.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentManagementService _documentManagementService;
        private readonly IDatabaseRepository<DocumentDetail> _documentDetailDatabase;

        public DocumentController(IDocumentManagementService documentManagementService,IDatabaseRepository<DocumentDetail> documentDetailDatabase)
        {
            _documentManagementService = documentManagementService;
            _documentDetailDatabase = documentDetailDatabase;
        }

        [HttpGet]
        [RequireSession]
        public IActionResult DocumentCreate()
        {
            return View("documentCreateForm");
        }
      
        [HttpPost]
        [RequireSession]
        public async Task<IActionResult> DocumentCreate(Document document)
        {
            if (!ModelState.IsValid)
            {
                return View("documentCreateForm");
            }
            if (document.FormFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    document.FormFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    string payload = Convert.ToBase64String(fileBytes);
                    int lastIndexOfDot = document.FormFile.FileName.LastIndexOf(".");
                    var fileExtension = document.FormFile.FileName.Substring(lastIndexOfDot + 1);
                    var documentManagementResult = await _documentManagementService.AddNewDocument(document.DocumentFileName, payload, fileExtension);
                    
                    if (!documentManagementResult.IsSuccessful())
                    {
                        ModelState.AddModelError("DocumentFileName", documentManagementResult.ErrorMessage);
                        return View("documentCreateForm");
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [RequireSession]
        public async Task<IActionResult> DocumentView()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.AllRecords = await _documentDetailDatabase.ReadAsync();
            return View("documentView");
        }

        [HttpPost]
        [RequireSession]
        public async Task<IActionResult> DocumentView(DocumentSearch documentSearch)
        {
            ViewBag.Message = TempData["Message"];

            if (String.IsNullOrEmpty(documentSearch.DocumentFileName.Trim()))
            {
                ViewBag.AllRecords = null;
                return View("documentView");
            }
            var searchDocumentFilename = documentSearch.DocumentFileName.ToUpper().Trim();
            ViewBag.AllRecords = await _documentDetailDatabase.ReadAsync(x => x.FileName.ToUpper() == searchDocumentFilename);
            return View("documentView");
        }

        [RequireSession]
        public async Task<IActionResult> DownloadFileFromFileSystem(int Id)
        {
            try
            {
                var documentManagementResult = await _documentManagementService.GetDocument(Id);
                
                if (!documentManagementResult.IsSuccessful())
                {
                    TempData["Message"] = documentManagementResult.ErrorMessage;
                    return RedirectToAction("DocumentView");
                }

                Log.Information("File with fileid: {0} successfully downloaded.", Id);
                byte[] byteArray = Convert.FromBase64String(documentManagementResult.Payload);
                return File(byteArray, MediaTypeNames.Application.Octet, documentManagementResult.Filename);
            }
            catch (Exception ex)
            {
                Log.Information("File with fileid: {0} couldnot be downloaded.", Id);
                TempData["Message"] = "File Could not be downloaded. Please Try Again!!";
                return RedirectToAction("DocumentView");
            }
        }
    }
}