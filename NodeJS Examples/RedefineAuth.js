const https = require('https');
const http = require('http');
const crypto = require('crypto');
const os = require('os');

class AuthResult {
    constructor(success, message) {
        this.success = success;
        this.message = message;
    }
}

class RedefineAuthClient {
    /**
     * Initialize the RedefineAuth client.
     * 
     * @param {string} redefineId - Your Redefine ID
     * @param {string} baseUrl - Base URL of your RedefineAuth deployment (default: "https://redefine-auth-v2.vercel.app/")
     * @param {string} appVersion - Version of your application (default: "1.0.0.0")
     */
    constructor(redefineId, baseUrl = "https://redefine-auth-v2.vercel.app/", appVersion = "1.0.0.0") {
        this.redefineId = redefineId;
        this.baseUrl = baseUrl.replace(/\/$/, ''); // Remove trailing slash if present
        this.appVersion = appVersion;
    }

    /**
     * Register a new user with username and license key.
     * 
     * @param {string} username - The username to register
     * @param {string} licenseKey - The license key provided by the admin
     * @returns {Promise<AuthResult>} AuthResult with success status and message
     */
    async register(username, licenseKey) {
        try {
            // Generate HWID based on machine info
            const hwid = this._getHardwareId();
            
            // Prepare request data
            const requestData = {
                username: username,
                licenseKey: licenseKey,
                hwid: hwid,
                redefineDeveloperId: this.redefineId,
                appVersion: this.appVersion
            };
            
            // Make HTTP POST request
            const response = await this._makeRequest('/api/client-users', requestData);
            
            // Handle response
            if (response.statusCode === 200) {
                return new AuthResult(true, "User registered successfully");
            } else {
                return this._handleErrorResponse(response);
            }
        } catch (error) {
            return new AuthResult(false, `Registration error: ${error.message}`);
        }
    }

    /**
     * Login an existing user with username.
     * 
     * @param {string} username - The username to login
     * @returns {Promise<AuthResult>} AuthResult with success status and message
     */
    async login(username) {
        try {
            // Generate HWID based on machine info
            const hwid = this._getHardwareId();
            
            // Prepare request data
            const requestData = {
                username: username,
                hwid: hwid,
                redefineDeveloperId: this.redefineId,
                appVersion: this.appVersion
            };
            
            // Make HTTP POST request
            const response = await this._makeRequest('/api/client-users/login', requestData);
            
            // Handle response
            if (response.statusCode === 200) {
                return new AuthResult(true, "Login successful");
            } else {
                return this._handleErrorResponse(response);
            }
        } catch (error) {
            return new AuthResult(false, `Login error: ${error.message}`);
        }
    }

    /**
     * Generate a simple HWID based on machine name and platform info.
     * 
     * @returns {string} Base64 encoded HWID string
     */
    _getHardwareId() {
        // Get machine info
        const machineInfo = `${os.hostname()}${os.platform()}${os.arch()}${os.cpus()[0].model}`;
        
        // Create SHA256 hash and encode as hex
        return crypto.createHash('sha256').update(machineInfo).digest('hex');
    }

    /**
     * Make HTTP POST request to the API.
     * 
     * @param {string} endpoint - API endpoint
     * @param {Object} data - Request data
     * @returns {Promise<Object>} Response object with statusCode and data
     */
    _makeRequest(endpoint, data) {
        return new Promise((resolve, reject) => {
            const url = new URL(this.baseUrl + endpoint);
            const jsonData = JSON.stringify(data);
            
            const options = {
                hostname: url.hostname,
                port: url.port,
                path: url.pathname,
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(jsonData)
                }
            };
            
            // Choose http or https based on protocol
            const client = url.protocol === 'https:' ? https : http;
            
            const req = client.request(options, (res) => {
                let responseData = '';
                
                res.on('data', (chunk) => {
                    responseData += chunk;
                });
                
                res.on('end', () => {
                    resolve({
                        statusCode: res.statusCode,
                        data: responseData
                    });
                });
            });
            
            req.on('error', (error) => {
                reject(error);
            });
            
            // Write data to request body
            req.write(jsonData);
            req.end();
        });
    }

    /**
     * Handle different HTTP error responses.
     * 
     * @param {Object} response - HTTP response object
     * @returns {AuthResult} AuthResult with appropriate error message
     */
    _handleErrorResponse(response) {
        const responseContent = response.data || "Unknown error";
        
        // Handle different status codes
        switch (response.statusCode) {
            case 503:
                return new AuthResult(false, `Application Paused: ${responseContent}`);
            case 400:
                return new AuthResult(false, `Invalid Request: ${responseContent}`);
            case 404:
                return new AuthResult(false, `Resource Not Found: ${responseContent}`);
            case 403:
                return new AuthResult(false, `Access Denied: ${responseContent}`);
            default:
                return new AuthResult(false, `HTTP ${response.statusCode}: ${responseContent}`);
        }
    }
}

// Example usage
if (require.main === module) {
    // Initialize the client
    const client = new RedefineAuthClient(
        "YOUR_REDEFINE_ID_HERE",           // Change this to your Redefine ID
        "https://redefine-auth-v2.vercel.app/",          // Change this to your deployed RedefineAuth URL
        "1.0.0.0"                         // Change this to match your application version
    );
    
    // Example registration
    console.log("Registering user...");
    client.register("testuser", "LICENSE-KEY-123")
        .then(result => {
            console.log(`Registration result: ${result.success} - ${result.message}`);
        })
        .catch(error => {
            console.error(`Registration error: ${error.message}`);
        });
    
    // Example login
    console.log("\nLogging in user...");
    client.login("testuser")
        .then(result => {
            console.log(`Login result: ${result.success} - ${result.message}`);
        })
        .catch(error => {
            console.error(`Login error: ${error.message}`);
        });
}

module.exports = { RedefineAuthClient, AuthResult };
