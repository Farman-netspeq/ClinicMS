namespace ClinicMS.Web.Helpers
{
    public static class ApiErrorHelper
    {
        public static string ExtractApiMessage(string exceptionMessage)
        {
            try
            {
                var jsonStart = exceptionMessage.IndexOf('{');
                if (jsonStart >= 0)
                {
                    var json = exceptionMessage.Substring(jsonStart);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        return msg.GetString() ?? "Save failed.";
                }
            }
            catch { }
            return "Save failed.";
        }
    }
}