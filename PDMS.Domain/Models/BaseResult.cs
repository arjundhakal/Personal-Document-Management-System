namespace PDMS.Domain.Models
{
    public class BaseResult
    {
        public string ErrorMessage { get; set; }
        public bool IsSuccessful() => string.IsNullOrEmpty(ErrorMessage);
    }
}