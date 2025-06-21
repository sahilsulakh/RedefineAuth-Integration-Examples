using System;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YourWorkspaceNameHere                   // <<<--- REPLACE WITH YOUR WORKSPACE NAME 
{
    // Hardware ID Generator
    public static class HwidGenerator
    {
        public static string GetHardwareId()
        {
            try
            {
                string cpuId = string.Empty;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        cpuId = mo["ProcessorId"]?.ToString();
                        if (!string.IsNullOrEmpty(cpuId)) break;
                    }
                }

                string baseBoardSerial = string.Empty;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        baseBoardSerial = mo["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(baseBoardSerial)) break;
                    }
                }

                string combinedId = $"CPU:{cpuId ?? "N/A"}_BOARD:{baseBoardSerial ?? "N/A"}";
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedId));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString().ToUpperInvariant();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating HWID: " + ex.Message);
                return "HWID_GENERATION_ERROR";
            }
        }
    }

    // Data Models
    public class LicenseValidationRequest
    {
        [JsonProperty("productKey")]
        public string ProductKey { get; set; }

        [JsonProperty("hwid")]
        public string Hwid { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("redefineDeveloperId")]
        public string RedefineDeveloperId { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }
    }

    public class AppCredentialValidationRequest
    {
        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("hwid")]
        public string Hwid { get; set; }

        [JsonProperty("redefineDeveloperId")]
        public string RedefineDeveloperId { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }
    }

    public class ValidationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("username")]
        public string OwnerUsername { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }

    // Main Authentication Client
    public class RedefineAuthClient
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _redefineApiBaseUrl;
        private readonly string _redefineDeveloperId;

        public RedefineAuthClient(string apiBaseUrl, string redefineDeveloperId)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                throw new ArgumentNullException(nameof(apiBaseUrl), "Redefine API base URL cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(redefineDeveloperId))
            {
                throw new ArgumentNullException(nameof(redefineDeveloperId), "Redefine Developer ID cannot be null or empty.");
            }

            _redefineApiBaseUrl = apiBaseUrl.TrimEnd('/');
            _redefineDeveloperId = redefineDeveloperId;

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ValidationResponse> ValidateLicenseKeyAsync(string productKey, string appVersion = null)
        {
            string currentHwid = HwidGenerator.GetHardwareId();
            var requestPayload = new LicenseValidationRequest
            {
                ProductKey = productKey,
                Hwid = currentHwid,
                RedefineDeveloperId = _redefineDeveloperId,
                AppVersion = appVersion
            };

            string endpointUrl = $"{_redefineApiBaseUrl}/api/keys/validate";
            return await PostValidationRequestAsync<LicenseValidationRequest>(endpointUrl, requestPayload);
        }

        public async Task<ValidationResponse> ValidateAppCredentialAsync(string username, string password, string appVersion = null)
        {
            string currentHwid = HwidGenerator.GetHardwareId();
            var requestPayload = new AppCredentialValidationRequest
            {
                Username = username,
                Password = password,
                Hwid = currentHwid,
                RedefineDeveloperId = _redefineDeveloperId,
                AppVersion = appVersion
            };

            // IMPORTANT: The backend endpoint /api/app-credentials/validate needs to be implemented/updated
            // in your Redefine Next.js application to accept and use `redefineDeveloper Id`.
            string endpointUrl = $"{_redefineApiBaseUrl}/api/app-credentials/validate";
            return await PostValidationRequestAsync<AppCredentialValidationRequest>(endpointUrl, requestPayload);
        }

        private async Task<ValidationResponse> PostValidationRequestAsync<TRequest>(string endpointUrl, TRequest requestPayload)
        {
            string jsonPayload = JsonConvert.SerializeObject(requestPayload);
            HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(endpointUrl, httpContent);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ValidationResponse validationResponse = JsonConvert.DeserializeObject<ValidationResponse>(responseBody);
                    return validationResponse ?? new ValidationResponse { Success = false, Message = "Failed to parse successful server response.", Status = "response_parse_error" };
                }
                else
                {
                    try
                    {
                        ValidationResponse errorResponse = JsonConvert.DeserializeObject<ValidationResponse>(responseBody);
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            return errorResponse;
                        }
                    }
                    catch (JsonException) { /* Use raw body */ }

                    Console.WriteLine($"Redefine API Error: {response.StatusCode} - {responseBody}");
                    return new ValidationResponse
                    {
                        Success = false,
                        Message = $"Redefine server returned an error. Status: {response.StatusCode}. Details: {responseBody}",
                        Status = "api_error"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("HTTP Request Exception: " + ex.ToString());
                return new ValidationResponse { Success = false, Message = "Network error or Redefine server unreachable. " + ex.Message, Status = "network_error" };
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Request Timed Out: " + ex.Message);
                return new ValidationResponse { Success = false, Message = "The request to the Redefine server timed out. " + ex.Message, Status = "timeout_error" };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic Exception during validation: " + ex.ToString());
                return new ValidationResponse { Success = false, Message = "An unexpected error occurred on the client side: " + ex.Message, Status = "client_unexpected_error" };
            }
        }
    }
}
