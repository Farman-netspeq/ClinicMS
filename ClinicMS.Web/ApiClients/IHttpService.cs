namespace ClinicMS.Web.ApiClients
{
    // IHttpService = Web project's HTTP client wrapper.
    public interface IHttpService
    {
        // GET request — fetch data from Api
        Task<T?> GetAsync<T>(string endpoint);

        // POST request — send new data to Api
        Task<T?> PostAsync<T>(string endpoint, object data);

        // PUT request — update existing data
        Task<T?> PutAsync<T>(string endpoint, object data);

        // DELETE request
        Task<T?> DeleteAsync<T>(string endpoint);
    }
}