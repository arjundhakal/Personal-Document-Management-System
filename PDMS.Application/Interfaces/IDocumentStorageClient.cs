using PDMS.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
    public interface IDocumentStorageClient
    {
        Task<DocumentStorageResult> Get(string identifier);
        Task<DocumentStorageResult> Post(string payload);
    }
}
