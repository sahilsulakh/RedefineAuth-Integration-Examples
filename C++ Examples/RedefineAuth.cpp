#include "RedefineAuth.h"
#include <curl/curl.h>
#include <sstream>
#include <cstring>
#include <iostream>
#include <iomanip>
#include <openssl/sha.h>

// Callback function to write response data
static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* response) {
    size_t totalSize = size * nmemb;
    response->append((char*)contents, totalSize);
    return totalSize;
}

// Implementation class
class RedefineAuthClientImpl {
public:
    std::string redefineId;
    std::string baseUrl;
    std::string appVersion;
    
    RedefineAuthClientImpl(const std::string& id, const std::string& url, const std::string& version)
        : redefineId(id), baseUrl(url), appVersion(version) {
        // Initialize curl globally (should be done once in the application)
        curl_global_init(CURL_GLOBAL_DEFAULT);
    }
    
    ~RedefineAuthClientImpl() {
        // Cleanup curl globally (should be done once in the application)
        curl_global_cleanup();
    }
    
    // Generate HWID based on machine info
    std::string getHardwareId() {
        // Get machine info (simplified for example)
        std::string machineInfo = "machine_info_placeholder"; // In real implementation, get actual machine info
        
        // Create SHA256 hash
        unsigned char hash[SHA256_DIGEST_LENGTH];
        SHA256_CTX sha256;
        SHA256_Init(&sha256);
        SHA256_Update(&sha256, machineInfo.c_str(), machineInfo.size());
        SHA256_Final(hash, &sha256);
        
        // Convert to hex string
        std::stringstream ss;
        for(int i = 0; i < SHA256_DIGEST_LENGTH; i++) {
            ss << std::hex << std::setw(2) << std::setfill('0') << (int)hash[i];
        }
        
        return ss.str();
    }
    
    // Make HTTP POST request
    AuthResult makeRequest(const std::string& endpoint, const std::string& jsonData) {
        CURL* curl;
        CURLcode res;
        std::string response;
        
        curl = curl_easy_init();
        if(curl) {
            std::string url = baseUrl + endpoint;
            
            // Set URL
            curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
            
            // Set headers
            struct curl_slist* headers = NULL;
            headers = curl_slist_append(headers, "Content-Type: application/json");
            curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
            
            // Set POST data
            curl_easy_setopt(curl, CURLOPT_POSTFIELDS, jsonData.c_str());
            
            // Set callback function to capture response
            curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
            curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);
            
            // Perform the request
            res = curl_easy_perform(curl);
            
            // Get HTTP response code
            long responseCode;
            curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &responseCode);
            
            // Cleanup
            curl_slist_free_all(headers);
            curl_easy_cleanup(curl);
            
            // Handle response
            if(res == CURLE_OK) {
                if(responseCode == 200) {
                    return AuthResult(true, "Operation successful");
                } else {
                    return handleErrorResponse(responseCode, response);
                }
            } else {
                return AuthResult(false, "Network error: " + std::string(curl_easy_strerror(res)));
            }
        }
        
        return AuthResult(false, "Failed to initialize CURL");
    }
    
    // Handle different HTTP error responses
    AuthResult handleErrorResponse(long responseCode, const std::string& responseContent) {
        switch(responseCode) {
            case 503:
                return AuthResult(false, "Application Paused: " + responseContent);
            case 400:
                return AuthResult(false, "Invalid Request: " + responseContent);
            case 404:
                return AuthResult(false, "Resource Not Found: " + responseContent);
            case 403:
                return AuthResult(false, "Access Denied: " + responseContent);
            default:
                return AuthResult(false, "HTTP " + std::to_string(responseCode) + ": " + responseContent);
        }
    }
};

// RedefineAuthClient implementation
RedefineAuthClient::RedefineAuthClient(const std::string& redefineId, 
                                     const std::string& baseUrl,
                                     const std::string& appVersion)
    : impl(new RedefineAuthClientImpl(redefineId, baseUrl, appVersion)) {}

RedefineAuthClient::~RedefineAuthClient() = default;

AuthResult RedefineAuthClient::registerUser(const std::string& username, const std::string& licenseKey) {
    try {
        // Generate HWID
        std::string hwid = impl->getHardwareId();
        
        // Prepare JSON data
        std::string jsonData = "{"
            "\"username\":\"" + username + "\","
            "\"licenseKey\":\"" + licenseKey + "\","
            "\"hwid\":\"" + hwid + "\","
            "\"redefineDeveloperId\":\"" + impl->redefineId + "\","
            "\"appVersion\":\"" + impl->appVersion + "\""
            "}";
        
        // Make request
        return impl->makeRequest("/api/client-users", jsonData);
    } catch(const std::exception& e) {
        return AuthResult(false, "Registration error: " + std::string(e.what()));
    }
}

AuthResult RedefineAuthClient::loginUser(const std::string& username) {
    try {
        // Generate HWID
        std::string hwid = impl->getHardwareId();
        
        // Prepare JSON data
        std::string jsonData = "{"
            "\"username\":\"" + username + "\","
            "\"hwid\":\"" + hwid + "\","
            "\"redefineDeveloperId\":\"" + impl->redefineId + "\","
            "\"appVersion\":\"" + impl->appVersion + "\""
            "}";
        
        // Make request
        return impl->makeRequest("/api/client-users/login", jsonData);
    } catch(const std::exception& e) {
        return AuthResult(false, "Login error: " + std::string(e.what()));
    }
}
