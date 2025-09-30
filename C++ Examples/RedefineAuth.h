#ifndef REDEFINE_AUTH_H
#define REDEFINE_AUTH_H

#include <string>
#include <memory>
#include <iostream>

// Forward declarations
struct AuthResult;
class RedefineAuthClientImpl;

class AuthResult {
public:
    bool success;
    std::string message;
    
    AuthResult(bool s, const std::string& m) : success(s), message(m) {}
};

class RedefineAuthClient {
public:
    /**
     * Initialize the RedefineAuth client.
     * 
     * @param redefineId Your Redefine ID
     * @param baseUrl Base URL of your RedefineAuth deployment (default: "https://redefine-auth-v2.vercel.app/")
     * @param appVersion Version of your application (default: "1.0.0.0")
     */
    RedefineAuthClient(const std::string& redefineId, 
                      const std::string& baseUrl = "https://redefine-auth-v2.vercel.app/",
                      const std::string& appVersion = "1.0.0.0");
    
    // Destructor
    ~RedefineAuthClient();
    
    /**
     * Register a new user with username and license key.
     * 
     * @param username The username to register
     * @param licenseKey The license key provided by the admin
     * @return AuthResult with success status and message
     */
    AuthResult registerUser(const std::string& username, const std::string& licenseKey);
    
    /**
     * Login an existing user with username.
     * 
     * @param username The username to login
     * @return AuthResult with success status and message
     */
    AuthResult loginUser(const std::string& username);

private:
    std::unique_ptr<RedefineAuthClientImpl> impl;
};

#endif // REDEFINE_AUTH_H
