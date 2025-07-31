using System;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedefineApp                   // <<<--- REPLACE WITH YOUR WORKSPACE NAME 
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

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AuthType { get; set; }
        public ValidationResponse ValidationResponse { get; set; }
    }

    // Stored Credentials Model
    public class StoredCredentials
    {
        public string AuthType { get; set; }
        public string LicenseKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    // Credential Storage Manager
    public static class CredentialStorage
    {
        private static readonly string CredentialsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RedefineAuth",
            "credentials.dat"
        );

        public static void SaveCredentials(string authType, string licenseKey, string username, string password)
        {
            try
            {
                var credentials = new StoredCredentials
                {
                    AuthType = authType,
                    LicenseKey = licenseKey,
                    Username = username,
                    Password = password,
                    RegisteredAt = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(credentials);
                string encrypted = EncryptString(json);

                Directory.CreateDirectory(Path.GetDirectoryName(CredentialsFile));
                File.WriteAllText(CredentialsFile, encrypted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save credentials: {ex.Message}");
            }
        }

        public static StoredCredentials LoadCredentials()
        {
            try
            {
                if (!File.Exists(CredentialsFile))
                    return null;

                string encrypted = File.ReadAllText(CredentialsFile);
                string json = DecryptString(encrypted);
                return JsonConvert.DeserializeObject<StoredCredentials>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load credentials: {ex.Message}");
                return null;
            }
        }

        public static void ClearCredentials()
        {
            try
            {
                if (File.Exists(CredentialsFile))
                    File.Delete(CredentialsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear credentials: {ex.Message}");
            }
        }

        public static bool HasStoredCredentials()
        {
            return File.Exists(CredentialsFile);
        }

        private static string EncryptString(string text)
        {
            try
            {
                // Use simple Base64 encoding with basic obfuscation for .NET Framework compatibility
                byte[] data = Encoding.UTF8.GetBytes(text);

                // Simple XOR encryption with a key derived from machine/user info
                string key = Environment.MachineName + Environment.UserName;
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
                }

                return Convert.ToBase64String(data);
            }
            catch
            {
                // Fallback to simple Base64 if encryption fails
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            }
        }

        private static string DecryptString(string encryptedText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encryptedText);

                // Simple XOR decryption with the same key
                string key = Environment.MachineName + Environment.UserName;
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
                }

                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                // Fallback to simple Base64 decode if decryption fails
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }

    // Authentication Session Manager
    public class AuthenticationSession
    {
        public bool IsRegistered { get; set; }
        public ValidationResponse LastValidationResult { get; set; }
        public string RegisteredAuthType { get; set; }
        public DateTime? LastLoginTime { get; set; }

        public void Reset()
        {
            IsRegistered = false;
            LastValidationResult = null;
            RegisteredAuthType = null;
            LastLoginTime = null;
        }

        public void SetRegistered(ValidationResponse result, string authType)
        {
            IsRegistered = true;
            LastValidationResult = result;
            RegisteredAuthType = authType;
            LastLoginTime = DateTime.Now;
        }
    }

    // Main Authentication Client
    public class RedefineAuthClient
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _redefineApiBaseUrl;
        private readonly string _redefineDeveloperId;
        private readonly AuthenticationSession _session;

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
            _session = new AuthenticationSession();

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Session management properties
        public bool IsRegistered => _session.IsRegistered || CredentialStorage.HasStoredCredentials();
        public string RegisteredAuthType => _session.RegisteredAuthType;
        public ValidationResponse LastValidationResult => _session.LastValidationResult;

        // Credential management
        public StoredCredentials GetStoredCredentials() => CredentialStorage.LoadCredentials();

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

        /// <summary>
        /// Tests connectivity to the Redefine server
        /// </summary>
        /// <returns>True if server is reachable, false otherwise</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                string testUrl = $"{_redefineApiBaseUrl}/api/health";
                HttpResponseMessage response = await httpClient.GetAsync(testUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Registers user with license key
        /// </summary>
        public async Task<AuthenticationResult> RegisterWithLicenseKeyAsync(string licenseKey, string appVersion = null)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Please enter a valid license key.",
                    AuthType = "License Key"
                };
            }

            try
            {
                var result = await ValidateLicenseKeyAsync(licenseKey, appVersion);
                if (result.Success)
                {
                    _session.SetRegistered(result, "License Key");
                    // Save credentials for future use
                    CredentialStorage.SaveCredentials("License Key", licenseKey, null, null);
                }

                return new AuthenticationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    AuthType = "License Key",
                    ValidationResponse = result
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = ex.Message,
                    AuthType = "License Key"
                };
            }
        }

        /// <summary>
        /// Registers user with username and password
        /// </summary>
        public async Task<AuthenticationResult> RegisterWithCredentialsAsync(string username, string password, string appVersion = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Please enter both username and password.",
                    AuthType = "Username/Password"
                };
            }

            try
            {
                var result = await ValidateAppCredentialAsync(username, password, appVersion);
                if (result.Success)
                {
                    _session.SetRegistered(result, "Username/Password");
                    // Save credentials for future use
                    CredentialStorage.SaveCredentials("Username/Password", null, username, password);
                }

                return new AuthenticationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    AuthType = "Username/Password",
                    ValidationResponse = result
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = ex.Message,
                    AuthType = "Username/Password"
                };
            }
        }

        /// <summary>
        /// Attempts login with provided credentials
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string licenseKey, string username, string password, string appVersion = null)
        {
            // Determine which credentials to use
            bool hasLicenseKey = !string.IsNullOrWhiteSpace(licenseKey);
            bool hasUserPass = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);

            if (!hasLicenseKey && !hasUserPass)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Please enter either a license key OR username and password.",
                    AuthType = "Unknown"
                };
            }

            if (hasLicenseKey && hasUserPass)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Please use either license key OR username/password, not both.",
                    AuthType = "Mixed"
                };
            }

            try
            {
                ValidationResponse result;
                string authType;

                if (hasLicenseKey)
                {
                    result = await ValidateLicenseKeyAsync(licenseKey, appVersion);
                    authType = "License Key";
                }
                else
                {
                    result = await ValidateAppCredentialAsync(username, password, appVersion);
                    authType = "Username/Password";
                }

                if (result.Success)
                {
                    _session.SetRegistered(result, authType);
                }

                return new AuthenticationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    AuthType = authType,
                    ValidationResponse = result
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = ex.Message,
                    AuthType = hasLicenseKey ? "License Key" : "Username/Password"
                };
            }
        }

        /// <summary>
        /// Attempts login with stored credentials
        /// </summary>
        public async Task<AuthenticationResult> LoginWithStoredCredentialsAsync(string appVersion = null)
        {
            var storedCredentials = CredentialStorage.LoadCredentials();
            if (storedCredentials == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "No stored credentials found. Please register first.",
                    AuthType = "Stored Credentials"
                };
            }

            try
            {
                ValidationResponse result;
                string authType = storedCredentials.AuthType;

                if (authType == "License Key")
                {
                    result = await ValidateLicenseKeyAsync(storedCredentials.LicenseKey, appVersion);
                }
                else // Username/Password
                {
                    result = await ValidateAppCredentialAsync(storedCredentials.Username, storedCredentials.Password, appVersion);
                }

                if (result.Success)
                {
                    _session.SetRegistered(result, authType);
                }

                return new AuthenticationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    AuthType = authType,
                    ValidationResponse = result
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = ex.Message,
                    AuthType = storedCredentials.AuthType
                };
            }
        }

        /// <summary>
        /// Resets the authentication session and clears stored credentials
        /// </summary>
        public void ResetSession()
        {
            _session.Reset();
        }

        /// <summary>
        /// Clears stored credentials (for logout)
        /// </summary>
        public void ClearStoredCredentials()
        {
            CredentialStorage.ClearCredentials();
            _session.Reset();
        }

        private async Task<ValidationResponse> PostValidationRequestAsync<TRequest>(string endpointUrl, TRequest requestPayload)
        {
            string jsonPayload = JsonConvert.SerializeObject(requestPayload);
            HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Retry logic for better resilience
            int maxRetries = 3;
            int retryDelay = 1000; // 1 second

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
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

                        Console.WriteLine($"Redefine API Error (Attempt {attempt}): {response.StatusCode} - {responseBody}");

                        // Don't retry on client errors (4xx), only on server errors (5xx)
                        if ((int)response.StatusCode < 500)
                        {
                            return new ValidationResponse
                            {
                                Success = false,
                                Message = $"Redefine server returned an error. Status: {response.StatusCode}. Details: {responseBody}",
                                Status = "api_error"
                            };
                        }

                        // If it's a server error and we have retries left, continue to retry
                        if (attempt < maxRetries)
                        {
                            Console.WriteLine($"Server error detected. Retrying in {retryDelay}ms... (Attempt {attempt + 1}/{maxRetries})");
                            await Task.Delay(retryDelay);
                            retryDelay *= 2; // Exponential backoff
                            continue;
                        }

                        return new ValidationResponse
                        {
                            Success = false,
                            Message = $"Redefine server returned an error after {maxRetries} attempts. Status: {response.StatusCode}. Details: {responseBody}",
                            Status = "api_error_max_retries"
                        };
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Request Exception (Attempt {attempt}): " + ex.ToString());
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Network error detected. Retrying in {retryDelay}ms... (Attempt {attempt + 1}/{maxRetries})");
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }
                    return new ValidationResponse { Success = false, Message = "Network error or Redefine server unreachable after multiple attempts. " + ex.Message, Status = "network_error" };
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Request Timed Out (Attempt {attempt}): " + ex.Message);
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Timeout detected. Retrying in {retryDelay}ms... (Attempt {attempt + 1}/{maxRetries})");
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }
                    return new ValidationResponse { Success = false, Message = "The request to the Redefine server timed out after multiple attempts. " + ex.Message, Status = "timeout_error" };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Generic Exception during validation: " + ex.ToString());
                    return new ValidationResponse { Success = false, Message = "An unexpected error occurred on the client side: " + ex.Message, Status = "client_unexpected_error" };
                }
            }

            // This should never be reached, but just in case
            return new ValidationResponse { Success = false, Message = "Unexpected error in retry logic.", Status = "retry_logic_error" };
        }
    }
}
