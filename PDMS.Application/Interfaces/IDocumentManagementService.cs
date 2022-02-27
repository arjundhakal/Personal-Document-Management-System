using PDMS.Domain.Models;

namespace PDMS.Application.Interfaces
{
    public interface IDocumentManagementService
    {
        Task<DocumentManagementServiceResult> AddNewDocument(string documentFileName, string FormFile, string fileExtension);
        Task<DocumentManagementServiceResult> GetDocument(int documentId);
    }
}