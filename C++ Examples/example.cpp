#include "RedefineAuth.h"
#include <iostream>

int main() {
    std::cout << "RedefineAuth C++ Integration Example" << std::endl;
    
    // Initialize the client
    RedefineAuthClient client(
        "YOUR_REDEFINE_ID_HERE",           // Your Redefine ID
        "https://redefine-auth-v2.vercel.app/",           // Base URL of RedefineAuth deployment
        "1.0.0.0"                          // Application version
    );
    
    // Example registration
    std::cout << "Registering user..." << std::endl;
    AuthResult result = client.registerUser("testuser", "LICENSE-KEY-123");
    std::cout << "Registration result: " << (result.success ? "SUCCESS" : "FAILED") 
              << " - " << result.message << std::endl;
    
    // Example login
    std::cout << "\nLogging in user..." << std::endl;
    result = client.loginUser("testuser");
    std::cout << "Login result: " << (result.success ? "SUCCESS" : "FAILED") 
              << " - " << result.message << std::endl;
    
    return 0;
}
