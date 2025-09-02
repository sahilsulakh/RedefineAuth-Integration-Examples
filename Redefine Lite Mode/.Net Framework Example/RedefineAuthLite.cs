using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace YourWorkspaceName
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class RedefineAuth
    {
        private readonly string _baseUrl;
        private readonly string _developerId;
        private readonly HttpClient _httpClient;

        public RedefineAuth(string baseUrl, string developerId)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _developerId = developerId;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Register a new user with username and license key
        /// </summary>
        public async Task<AuthResult> RegisterAsync(string username, string licenseKey, string hwid = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(licenseKey))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Username and license key are required"
                    };
                }

                var payload = new
                {
                    username = username.Trim(),
                    licenseKey = licenseKey.Trim(),
                    redefineDeveloperId = _developerId,
                    hwid = hwid ?? GetHardwareId()
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/client-users", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return new AuthResult
                    {
                        Success = true,
                        Message = result.message?.ToString() ?? "Registration successful",
                        Username = result.user?.username?.ToString(),
                        RegisteredAt = result.user?.registeredAt != null ?
                            DateTime.Parse(result.user.registeredAt.ToString()) : null
                    };
                }
                else
                {
                    var errorResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return new AuthResult
                    {
                        Success = false,
                        Message = errorResult?.message?.ToString() ?? "Registration failed"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Network error: Unable to connect to server"
                };
            }
            catch (TaskCanceledException)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Request timeout: Server took too long to respond"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Login with existing username
        /// </summary>
        public async Task<AuthResult> LoginAsync(string username, string hwid = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Username is required"
                    };
                }

                var payload = new
                {
                    username = username.Trim(),
                    redefineDeveloperId = _developerId,
                    hwid = hwid ?? GetHardwareId()
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/client-users/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return new AuthResult
                    {
                        Success = true,
                        Message = result.message?.ToString() ?? "Login successful",
                        Username = result.user?.username?.ToString(),
                        RegisteredAt = result.user?.registeredAt != null ?
                            DateTime.Parse(result.user.registeredAt.ToString()) : null,
                        LastLoginAt = result.user?.lastLoginAt != null ?
                            DateTime.Parse(result.user.lastLoginAt.ToString()) : null
                    };
                }
                else
                {
                    var errorResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return new AuthResult
                    {
                        Success = false,
                        Message = errorResult?.message?.ToString() ?? "Login failed"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Network error: Unable to connect to server"
                };
            }
            catch (TaskCanceledException)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Request timeout: Server took too long to respond"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get a simple hardware ID for the current machine
        /// </summary>
        private string GetHardwareId()
        {
            try
            {
                var computerName = Environment.MachineName;
                var userName = Environment.UserName;
                var combined = $"{computerName}-{userName}";

                // Simple hash to create consistent HWID
                var hash = combined.GetHashCode();
                return Math.Abs(hash).ToString("X8");
            }
            catch
            {
                return "DEFAULT-HWID";
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
