namespace PDMS.Domain.Models
{
    public class DocumentManagementServiceResult : BaseResult
    {
        public string Payload { get; set; }
        public string Filename { get; set; }
        public string Identifier { get; set; }
    }
}