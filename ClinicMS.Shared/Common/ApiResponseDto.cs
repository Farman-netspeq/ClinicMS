namespace ClinicMS.Shared.Common
{
    // Every API controller method returns this wrapper.
    // Example: instead of returning just a Patient object,
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        // Static helpers so controller code is clean one-liners

        public static ApiResponseDto<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponseDto<T> { Success = true, Message = message, Data = data };
        }

        // Use when operation succeeded but no data needed (e.g. delete)
        public static ApiResponseDto<T> SuccessResponse(string message = "Success")
        {
            return new ApiResponseDto<T> { Success = true, Message = message };
        }

        public static ApiResponseDto<T> ErrorResponse(string message)
        {
            return new ApiResponseDto<T> { Success = false, Message = message };
        }
    }
}