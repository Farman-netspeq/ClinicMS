namespace ClinicMS.Shared.Common
{
    // This class wraps every service method return value.
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        // Private constructor — you can't do "new Result<T>()"
        // You MUST use Ok() or Fail() methods below. Forces correct usage.
        private Result() { }

        // Call this when operation succeeded: Result<Patient>.Ok(patientObject)
        public static Result<T> Ok(T data)
        {
            return new Result<T> { IsSuccess = true, Data = data };
        }

        // Call this when operation failed: Result<Patient>.Fail("Patient not found")
        public static Result<T> Fail(string errorMessage)
        {
            return new Result<T> { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }

    // Non-generic version — for operations that don't return data
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        private Result() { }

        public static Result Ok()
        {
            return new Result { IsSuccess = true };
        }

        public static Result Fail(string errorMessage)
        {
            return new Result { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
}