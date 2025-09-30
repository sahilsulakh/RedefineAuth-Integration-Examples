// JUST CREATE THE CLASS AND COPY-PASTE THE BELOW CODE

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;

namespace RedefineAuth
{
    public class RedefineAuthClient
    {
        private readonly string _redefineId;
        private readonly string _baseUrl;
        private readonly string _appVersion;

        public RedefineAuthClient(string redefineId, string baseUrl = "https://redefine-auth-v2.vercel.app/", string appVersion = "1.0.0.0")
        {
            _redefineId = redefineId;
            _baseUrl = baseUrl.TrimEnd('/'); // Remove trailing slash if present
            _appVersion = appVersion;
        }

        /// <summary>
        /// Register a new user with username and license key
        /// </summary>
        /// <param name="username">The username to register</param>
        /// <param name="licenseKey">The license key provided by the admin</param>
        /// <returns>Registration result with success status and message</returns>
        public async Task<AuthResult> RegisterAsync(string username, string licenseKey)
        {
            using (var httpClient = new HttpClient())
            {
                // Generate a simple HWID based on machine name
                string hwid = GetHardwareId();
                
                var requestData = new
                {
                    username = username,
                    licenseKey = licenseKey,
                    hwid = hwid,
                    redefineDeveloperId = _redefineId,
                    appVersion = _appVersion
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_baseUrl}/api/client-users", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new AuthResult { Success = true, Message = "User registered successfully" };
                }
                else
                {
                    // Handle different status codes
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                            return new AuthResult { Success = false, Message = $"Application Paused: {responseContent}" };
                        case System.Net.HttpStatusCode.BadRequest:
                            return new AuthResult { Success = false, Message = $"Invalid Request: {responseContent}" };
                        case System.Net.HttpStatusCode.NotFound:
                            return new AuthResult { Success = false, Message = $"Resource Not Found: {responseContent}" };
                        default:
                            return new AuthResult { Success = false, Message = $"HTTP {response.StatusCode}: {responseContent}" };
                    }
                }
            }
        }

        /// <summary>
        /// Login an existing user with username
        /// </summary>
        /// <param name="username">The username to login</param>
        /// <returns>Login result with success status and message</returns>
        public async Task<AuthResult> LoginAsync(string username)
        {
            using (var httpClient = new HttpClient())
            {
                // Generate a simple HWID based on machine name
                string hwid = GetHardwareId();
                
                var requestData = new
                {
                    username = username,
                    hwid = hwid,
                    redefineDeveloperId = _redefineId,
                    appVersion = _appVersion
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_baseUrl}/api/client-users/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new AuthResult { Success = true, Message = "Login successful" };
                }
                else
                {
                    // Handle different status codes
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                            return new AuthResult { Success = false, Message = $"Application Paused: {responseContent}" };
                        case System.Net.HttpStatusCode.BadRequest:
                            return new AuthResult { Success = false, Message = $"Invalid Request: {responseContent}" };
                        case System.Net.HttpStatusCode.NotFound:
                            return new AuthResult { Success = false, Message = $"User Not Found: {responseContent}" };
                        case System.Net.HttpStatusCode.Forbidden:
                            return new AuthResult { Success = false, Message = $"Access Denied: {responseContent}" };
                        default:
                            return new AuthResult { Success = false, Message = $"HTTP {response.StatusCode}: {responseContent}" };
                    }
                }
            }
        }

        private string GetHardwareId()
        {
            // Generate a simple HWID based on machine name and user name
            // In a real application, you might want to use more sophisticated methods
            string machineInfo = Environment.MachineName + Environment.UserName;
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
