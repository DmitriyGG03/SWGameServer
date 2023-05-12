namespace Server.Domain
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public ServiceResult()
        {
            Success = true;
            ErrorMessage = string.Empty;
        }
        public ServiceResult(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
    
    public class ServiceResult<T> : ServiceResult
    {
        public T? Value { get; set; }

        public ServiceResult(T value) : base()
        {
            Value = value;
        }
        public ServiceResult(string errorMessage) : base(errorMessage)
        {
            Value = default(T);
        }
    }
}
