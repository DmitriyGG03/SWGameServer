namespace Server.Domain
{
    public class ServiceResult<T>
    {
        public T? Value { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public ServiceResult(T value)
        {
            Success = true;
            Value = value;
            ErrorMessage = string.Empty;
        }
        public ServiceResult(string errorMessage)
        {
            Success = false;
            Value = default(T);
            ErrorMessage = errorMessage;
        }
    }
}
