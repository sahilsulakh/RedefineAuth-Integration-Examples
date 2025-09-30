import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.concurrent.CompletableFuture;
import java.io.IOException;
import java.net.InetAddress;
import java.net.UnknownHostException;
import com.google.gson.Gson;
import com.google.gson.JsonObject;

public class RedefineAuth {
    private final String redefineId;
    private final String baseUrl;
    private final String appVersion;
    private final HttpClient httpClient;
    private final Gson gson;

    /**
     * Initialize the RedefineAuth client.
     * 
     * @param redefineId Your Redefine ID
     * @param baseUrl Base URL of your RedefineAuth deployment (default: "https://redefine-auth-v2.vercel.app/")
     * @param appVersion Version of your application (default: "1.0.0.0")
     */
    public RedefineAuth(String redefineId, String baseUrl, String appVersion) {
        this.redefineId = redefineId;
        this.baseUrl = baseUrl.replaceAll("/$", ""); // Remove trailing slash if present
        this.appVersion = appVersion;
        this.httpClient = HttpClient.newHttpClient();
        this.gson = new Gson();
    }

    /**
     * Constructor with default base URL and app version.
     * 
     * @param redefineId Your Redefine ID
     */
    public RedefineAuth(String redefineId) {
        this(redefineId, "https://redefine-auth-v2.vercel.app/", "1.0.0.0");
    }

    /**
     * Register a new user with username and license key.
     * 
     * @param username The username to register
     * @param licenseKey The license key provided by the admin
     * @return AuthResult with success status and message
     */
    public AuthResult register(String username, String licenseKey) {
        try {
            // Generate HWID based on machine info
            String hwid = getHardwareId();
            
            // Prepare request data
            JsonObject requestData = new JsonObject();
            requestData.addProperty("username", username);
            requestData.addProperty("licenseKey", licenseKey);
            requestData.addProperty("hwid", hwid);
            requestData.addProperty("redefineDeveloperId", redefineId);
            requestData.addProperty("appVersion", appVersion);
            
            // Make HTTP POST request
            HttpResponse<String> response = makeRequest("/api/client-users", requestData.toString());
            
            // Handle response
            if (response.statusCode() == 200) {
                return new AuthResult(true, "User registered successfully");
            } else {
                return handleErrorResponse(response);
            }
        } catch (Exception e) {
            return new AuthResult(false, "Registration error: " + e.getMessage());
        }
    }

    /**
     * Login an existing user with username.
     * 
     * @param username The username to login
     * @return AuthResult with success status and message
     */
    public AuthResult login(String username) {
        try {
            // Generate HWID based on machine info
            String hwid = getHardwareId();
            
            // Prepare request data
            JsonObject requestData = new JsonObject();
            requestData.addProperty("username", username);
            requestData.addProperty("hwid", hwid);
            requestData.addProperty("redefineDeveloperId", redefineId);
            requestData.addProperty("appVersion", appVersion);
            
            // Make HTTP POST request
            HttpResponse<String> response = makeRequest("/api/client-users/login", requestData.toString());
            
            // Handle response
            if (response.statusCode() == 200) {
                return new AuthResult(true, "Login successful");
            } else {
                return handleErrorResponse(response);
            }
        } catch (Exception e) {
            return new AuthResult(false, "Login error: " + e.getMessage());
        }
    }

    /**
     * Generate a simple HWID based on machine name and platform info.
     * 
     * @return Hex encoded HWID string
     */
    private String getHardwareId() throws NoSuchAlgorithmException, UnknownHostException {
        // Get machine info
        String machineInfo = InetAddress.getLocalHost().getHostName() + 
                            System.getProperty("os.name") + 
                            System.getProperty("os.arch");
        
        // Create SHA256 hash and encode as hex
        MessageDigest digest = MessageDigest.getInstance("SHA-256");
        byte[] hash = digest.digest(machineInfo.getBytes(StandardCharsets.UTF_8));
        
        // Convert byte array to hex string
        StringBuilder hexString = new StringBuilder();
        for (byte b : hash) {
            String hex = Integer.toHexString(0xff & b);
            if (hex.length() == 1) {
                hexString.append('0');
            }
            hexString.append(hex);
        }
        
        return hexString.toString();
    }

    /**
     * Make HTTP POST request to the API.
     * 
     * @param endpoint API endpoint
     * @param jsonData Request data as JSON string
     * @return HttpResponse with status code and body
     * @throws IOException
     * @throws InterruptedException
     */
    private HttpResponse<String> makeRequest(String endpoint, String jsonData) 
            throws IOException, InterruptedException {
        HttpRequest request = HttpRequest.newBuilder()
                .uri(URI.create(baseUrl + endpoint))
                .header("Content-Type", "application/json")
                .POST(HttpRequest.BodyPublishers.ofString(jsonData))
                .build();
                
        return httpClient.send(request, HttpResponse.BodyHandlers.ofString());
    }

    /**
     * Handle different HTTP error responses.
     * 
     * @param response HTTP response
     * @return AuthResult with appropriate error message
     */
    private AuthResult handleErrorResponse(HttpResponse<String> response) {
        String responseContent = response.body() != null ? response.body() : "Unknown error";
        
        // Handle different status codes
        switch (response.statusCode()) {
            case 503:
                return new AuthResult(false, "Application Paused: " + responseContent);
            case 400:
                return new AuthResult(false, "Invalid Request: " + responseContent);
            case 404:
                return new AuthResult(false, "Resource Not Found: " + responseContent);
            case 403:
                return new AuthResult(false, "Access Denied: " + responseContent);
            default:
                return new AuthResult(false, "HTTP " + response.statusCode() + ": " + responseContent);
        }
    }

    /**
     * AuthResult class to represent the result of authentication operations.
     */
    public static class AuthResult {
        public final boolean success;
        public final String message;
        
        public AuthResult(boolean success, String message) {
            this.success = success;
            this.message = message;
        }
    }

    /**
     * Example usage
     */
    public static void main(String[] args) {
        // Initialize the client
        RedefineAuth client = new RedefineAuth(
            "YOUR_REDEFINE_ID_HERE",           // Change this to your Redefine ID
            "https://redefine-auth-v2.vercel.app/",          // Change this to your deployed RedefineAuth URL
            "1.0.0.0"                         // Change this to match your application version
        );
        
        // Example registration
        System.out.println("Registering user...");
        AuthResult result = client.register("testuser", "LICENSE-KEY-123");
        System.out.println("Registration result: " + (result.success ? "SUCCESS" : "FAILED") + 
                          " - " + result.message);
        
        // Example login
        System.out.println("\nLogging in user...");
        result = client.login("testuser");
        System.out.println("Login result: " + (result.success ? "SUCCESS" : "FAILED") + 
                          " - " + result.message);
    }
}
