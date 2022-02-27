using PDMS.Application.Interfaces;
using PDMS.Domain.Models;
using PDMS.Domain.Entity;
using Serilog;

namespace PDMS.Application.Services
{
    public class DocumentManagementService : IDocumentManagementService
    {
        private const string FilenameAlreadlyInUse = "File with the provided filename already exists.";
        private const string FileNotUploadedInServer = "Could not upload the new document in server.";
        private const string FileNotDownloadedFromServer = "Could not dwonload the document from server.";


        private readonly IDatabaseRepository<DocumentDetail> _documentDetailDatabase;
        private readonly IDocumentStorageClient _documentStorageClient;

        public DocumentManagementService(IDatabaseRepository<DocumentDetail> documentDetailDatabase,
                                     IDocumentStorageClient documentStorageClient)
        {
            _documentDetailDatabase = documentDetailDatabase;
            _documentStorageClient = documentStorageClient;
        }

        public async Task<DocumentManagementServiceResult> AddNewDocument(string documentFileName, string payload, string fileExtension)
        {
            if (await IsFileNameAlreadlyInUse(documentFileName + "." + fileExtension))
            {
                Log.Information("Could not add the new document. File with filename: {0} already exists.", documentFileName);
                return new DocumentManagementServiceResult()
                {
                    ErrorMessage = FilenameAlreadlyInUse
                };
            }

            var DocumentStorageResponse = await _documentStorageClient.Post(payload);

            if (!DocumentStorageResponse.IsSuccessful())
            {
                Log.Information("Could not upload the new document '{0}' in server.", documentFileName);
                return new DocumentManagementServiceResult()
                {
                    ErrorMessage = FileNotUploadedInServer
                };
            }

            var documentDetails = GenerateDocument(documentFileName, fileExtension, DocumentStorageResponse.Identifier);
            await _documentDetailDatabase.Create(documentDetails);
            Log.Information("File '{0}' successfully stored in server and database.", documentFileName);
            return new DocumentManagementServiceResult() { Identifier = DocumentStorageResponse.Identifier };
        }

        private DocumentDetail GenerateDocument(string documentFileName, string Extension, string identifier)
        {
            var documentDetail = new DocumentDetail()
            {
                FileName = documentFileName + "." + Extension,
                Identifier = identifier,
                CreatedAt = DateTime.Now,
                LastModified = null,
                LastModifiedBy = ""
            };
            return documentDetail;
        }

        private async Task<bool> IsFileNameAlreadlyInUse(string documentFileName)
        {
            var RegisteredDocument = await _documentDetailDatabase.SingleOrDefaultAsync(x => x.FileName.ToUpper() == documentFileName.ToUpper());

            if (RegisteredDocument == null)
                return false;
            else
                return true;
        }

        public async Task<DocumentManagementServiceResult> GetDocument(int documentId)
        {
            var fileDetail = await _documentDetailDatabase.SingleOrDefaultAsync(x => x.Id == documentId);
            var DocumentStorageResult = await _documentStorageClient.Get(fileDetail.Identifier);

            if (!DocumentStorageResult.IsSuccessful())
            {
                Log.Information("Could not download the document '{0}' from server.", fileDetail.FileName);
                return new DocumentManagementServiceResult()
                {
                    ErrorMessage = FileNotDownloadedFromServer
                };
            }

            Log.Information("Payload request successfully fulfilled.");
            return new DocumentManagementServiceResult()
            {
                ErrorMessage = null,
                Payload = DocumentStorageResult.Payload,
                Filename = fileDetail.FileName,
                Identifier = fileDetail.Identifier
            };
        }
    }
}