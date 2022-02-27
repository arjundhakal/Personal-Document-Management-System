using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PDMS.UI.Models
{
    public class Document

    {
        [DisplayName("Document File Name: ")]
        [Required(ErrorMessage = "Document File Name is required.")]
        public string DocumentFileName { get; set; }
        [DisplayName("Upload Document:")]
        public IFormFile FormFile { get; set; }
    }
}