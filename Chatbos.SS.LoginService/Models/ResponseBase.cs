namespace Chatbos.SS.LoginService.Models
{
    public class ResponseBase<T>
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
