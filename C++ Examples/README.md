# RedefineAuth C++ Integration

This is a C++ integration for RedefineAuth that allows end users to register and login using a username and license key.

## Requirements

- C++11 or higher
- libcurl development libraries
- OpenSSL development libraries
- CMake (for building)
- Internet connectivity

## Dependencies Installation

### Ubuntu/Debian:
```bash
sudo apt-get update
sudo apt-get install libcurl4-openssl-dev libssl-dev cmake
```

### CentOS/RHEL:
```bash
sudo yum install libcurl-devel openssl-devel cmake
```

### Windows (using vcpkg):
```bash
vcpkg install curl openssl
```

## Setup

1. Copy the `RedefineAuth.h` and `RedefineAuth.cpp` files to your project
2. Link against libcurl and OpenSSL libraries
3. Include the header in your source files

## Usage

### Basic Implementation

```cpp
#include "RedefineAuth.h"
#include <iostream>

int main() {
    // Initialize the client
    RedefineAuthClient client(
        "YOUR_REDEFINE_ID_HERE",           // Your Redefine ID
        "https://redefine-auth-v2.vercel.app/",           // Base URL of your RedefineAuth deployment
        "1.0.0.0"                          // Application version
    );
    
    // Register a new user
    AuthResult result = client.registerUser("username", "LICENSE-KEY-123");
    if (result.success) {
        std::cout << "Registration successful!" << std::endl;
    } else {
        std::cout << "Registration failed: " << result.message << std::endl;
    }
    
    // Login an existing user
    result = client.loginUser("username");
    if (result.success) {
        std::cout << "Login successful!" << std::endl;
    } else {
        std::cout << "Login failed: " << result.message << std::endl;
    }
    
    return 0;
}
```

### CMakeLists.txt Example

```cmake
cmake_minimum_required(VERSION 3.10)
project(RedefineAuthExample)

set(CMAKE_CXX_STANDARD 11)

# Find required packages
find_package(PkgConfig REQUIRED)
find_package(OpenSSL REQUIRED)
find_package(CURL REQUIRED)

# Add executable
add_executable(example main.cpp RedefineAuth.cpp)

# Link libraries
target_link_libraries(example ${CURL_LIBRARIES} OpenSSL::SSL OpenSSL::Crypto)
target_include_directories(example PRIVATE ${CURL_INCLUDE_DIRS})
```

## API Endpoints

The integration uses the following API endpoints:

- Registration: `POST /api/client-users`
- Login: `POST /api/client-users/login`

## Request Format

### Registration Request
```json
{
  "username": "user123",
  "licenseKey": "LICENSE-KEY-123",
  "hwid": "GENERATED_HARDWARE_ID",
  "redefineDeveloperId": "YOUR_REDEFINE_ID",
  "appVersion": "1.0.0.0"
}
```

### Login Request
```json
{
  "username": "user123",
  "hwid": "GENERATED_HARDWARE_ID",
  "redefineDeveloperId": "YOUR_REDEFINE_ID",
  "appVersion": "1.0.0.0"
}
```

## Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "message": "Error description"
}
```

## HWID (Hardware ID) Generation

The example includes automatic HWID generation based on machine information. This is required by the Redefine Auth API for both registration and login operations.

In a production environment, you might want to implement a more sophisticated HWID generation method that's appropriate for your application's security requirements.

## Application Version Control

The RedefineAuth dashboard includes an Application Version setting that allows you to enforce version control for your client applications:

- When set, client applications must provide a matching version number during login
- If the client version doesn't match the configured version, users receive a customizable "Outdated Version" error message

To use this feature:
1. Set the Application Version in the RedefineAuth dashboard Settings page (e.g., "2.0.0.0")
2. Update the `appVersion` parameter in your C++ application to match (e.g., "2.0.0.0")
3. When a client with a different version tries to login, they will receive an "outdated version" error

## Error Handling

The example includes comprehensive error handling for:
- Network connectivity issues
- Server errors
- Invalid credentials
- Application paused status
- HWID mismatch errors (when security settings are enabled)
- Version mismatch errors (when version control is enabled)
- Custom error messages from the dashboard

## Security Settings

The RedefineAuth dashboard includes security settings that affect how the end applications behave:

- **HWID Lock for Product Keys** - When enabled, license keys can only be used on a single hardware device

These security settings are automatically enforced by the API endpoints without requiring any changes to the client application code.

## Custom Messages

The RedefineAuth dashboard allows you to customize various error messages that will be displayed to end users:

- **Pause Message** - Displayed when the application is paused
- **Outdated Version Message** - Displayed when the client application version is outdated
- **Key Already Used Message** - Displayed when a license key has already been used
- **Subscription Expired Message** - Displayed when a subscription has expired
- **Password Mismatch Message** - Displayed when password authentication fails
- **Username Mismatch Message** - Displayed when username is not found
- **HWID Mismatch Message** - Displayed when hardware ID doesn't match

These messages can be configured in the Settings section of the RedefineAuth dashboard and will be automatically used by the API endpoints.

## Building the Example

```bash
mkdir build
cd build
cmake ..
make
./example
```
