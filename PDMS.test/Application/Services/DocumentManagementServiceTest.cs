using PDMS.Application.Repositories;
using PDMS.Application.Services;
using PDMS.Domain.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using PDMS.Infrastructure;
using PDMS.Application.Interfaces;
using System.Threading.Tasks;

namespace PDMS.Test.Application.Services
{
    public class DocumentManagementServiceTest
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContex;
        private DatabaseRepository<DocumentDetail> _documentRepo;
        private string _dbName;
        private DocumentManagementService _documentManagementService;
        private DocumentManagementService _documentManagementService2;
        private DocumentManagementService _documentManagementService3;

        public DocumentManagementServiceTest()
        {
            _dbName = Guid.NewGuid().ToString();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_dbName).Options;
            _dbContex = new ApplicationDbContext(_options);
            _documentRepo = new DatabaseRepository<DocumentDetail>(_dbContex);
            _documentManagementService = new DocumentManagementService(_documentRepo, new FakeNimbusDocumentStorageClient());
            _documentManagementService2 = new DocumentManagementService(_documentRepo, new NegFirstFakeNimbusDocumentStorageClient());
            _documentManagementService3 = new DocumentManagementService(_documentRepo, new NegSecondFakeNimbusDocumentStorageClient());
        }


        [Fact]
        public async void CanSuccessfullyAddNewDocument()
        {
            var documentFileName = Guid.NewGuid().ToString();
            var fileExtension = Guid.NewGuid().ToString();
            var payload = Guid.NewGuid().ToString();

            var documentManagementServiceResult = await _documentManagementService.AddNewDocument(documentFileName, payload, fileExtension);

            documentManagementServiceResult.Should().NotBeNull();
            documentManagementServiceResult.ErrorMessage.Should().BeNullOrEmpty();
            documentManagementServiceResult.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void UploadingDocumentWithSameNameMultipleTimes()
        {
            var documentFileName = Guid.NewGuid().ToString();
            var fileExtension = Guid.NewGuid().ToString();
            var payload = Guid.NewGuid().ToString();

            var documentManagementServiceResult = await _documentManagementService.AddNewDocument(documentFileName, payload, fileExtension);

            documentManagementServiceResult.Should().NotBeNull();
            documentManagementServiceResult.ErrorMessage.Should().BeNullOrEmpty();
            documentManagementServiceResult.IsSuccessful().Should().BeTrue();

            var payload2 = Guid.NewGuid().ToString();
            var documentManagementServiceResult2 = await _documentManagementService.AddNewDocument(documentFileName, payload, fileExtension);

            documentManagementServiceResult2.Should().NotBeNull();
            documentManagementServiceResult2.ErrorMessage.Should().NotBeNullOrEmpty();
            documentManagementServiceResult2.IsSuccessful().Should().BeFalse();
        }

        [Fact]
        public async void ServerProblemInUploadingDocument()
        {
            var documentFileName = Guid.NewGuid().ToString();
            var fileExtension = Guid.NewGuid().ToString();
            var payload = Guid.NewGuid().ToString();

            var documentManagementServiceResult = await _documentManagementService2.AddNewDocument(documentFileName, payload, fileExtension);

            documentManagementServiceResult.Should().NotBeNull();
            documentManagementServiceResult.ErrorMessage.Should().NotBeNullOrEmpty();
            documentManagementServiceResult.IsSuccessful().Should().BeFalse();
        }

        [Fact]
        public async void CanSuccessfullyFetchDocument()
        {
            var documentFileName = Guid.NewGuid().ToString();
            var fileExtension = Guid.NewGuid().ToString();
            var payload = Guid.NewGuid().ToString();
            var documentManagementServiceResultPost = await _documentManagementService.AddNewDocument(documentFileName, payload, fileExtension);
            var fileDetail = await _documentRepo.SingleOrDefaultAsync(x => x.Identifier == documentManagementServiceResultPost.Identifier);

            var documentManagementServiceResultGet = await _documentManagementService.GetDocument(fileDetail.Id);

            documentManagementServiceResultGet.Should().NotBeNull();
            documentManagementServiceResultGet.ErrorMessage.Should().BeNullOrEmpty();
            documentManagementServiceResultGet.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void CanNotFetchDocumentFromServer()
        {
            var documentFileName = Guid.NewGuid().ToString();
            var fileExtension = Guid.NewGuid().ToString();
            var payload = Guid.NewGuid().ToString();
            var documentManagementServiceResultPost = await _documentManagementService3.AddNewDocument(documentFileName, payload, fileExtension);
            var fileDetail = await _documentRepo.SingleOrDefaultAsync(x => x.Identifier == documentManagementServiceResultPost.Identifier);

            var documentManagementServiceResultGet = await _documentManagementService3.GetDocument(fileDetail.Id);

            documentManagementServiceResultGet.Should().NotBeNull();
            documentManagementServiceResultGet.ErrorMessage.Should().NotBeNullOrEmpty();
            documentManagementServiceResultGet.IsSuccessful().Should().BeFalse();
        }
    }

    #region FakeInterfaces

    public class FakeNimbusDocumentStorageClient : IDocumentStorageClient
    {
        public Task<DocumentStorageResult> Get(string identifier)
        {
            return Task.FromResult(new DocumentStorageResult(){ Payload = Guid.NewGuid().ToString() });
        }

        public Task<DocumentStorageResult> Post(string payload)
        {
            return Task.FromResult(new DocumentStorageResult(){ Identifier = Guid.NewGuid().ToString() });
        }

    }

    public class NegFirstFakeNimbusDocumentStorageClient : IDocumentStorageClient
    {
        public Task<DocumentStorageResult> Get(string identifier)
        {
            return Task.FromResult(new DocumentStorageResult() { ErrorMessage = "Error : cannot fetch the document" });
        }

        public Task<DocumentStorageResult> Post(string payload)
        {
            return Task.FromResult(new DocumentStorageResult() { ErrorMessage = "Error: cannot upload the document." });
        }
    }

    public class NegSecondFakeNimbusDocumentStorageClient : IDocumentStorageClient
    {
        public Task<DocumentStorageResult> Get(string identifier)
        {
            return Task.FromResult(new DocumentStorageResult() { ErrorMessage = "Error : cannot fetch the document" });
        }

        public Task<DocumentStorageResult> Post(string payload)
        {
            return Task.FromResult(new DocumentStorageResult() { Identifier = Guid.NewGuid().ToString() });
        }
    }

    #endregion
}
