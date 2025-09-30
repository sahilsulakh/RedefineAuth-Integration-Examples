# RedefineAuth Java Integration

This is a Java integration for RedefineAuth that allows end users to register and login using a username and license key.

## Requirements

- Java 11 or higher
- Gson library for JSON processing
- Internet connectivity

## Dependencies

Add the following dependency to your `pom.xml` if using Maven:

```xml
<dependency>
    <groupId>com.google.code.gson</groupId>
    <artifactId>gson</artifactId>
    <version>2.8.9</version>
</dependency>
```

Or if using Gradle, add this to your `build.gradle`:

```gradle
dependencies {
    implementation 'com.google.code.gson:gson:2.8.9'
}
```

## Setup

1. Copy the `RedefineAuth.java` file to your project
2. Add Gson dependency to your project
3. Compile and run

## Usage

### Basic Implementation

```java
public class Example {
    public static void main(String[] args) {
        // Initialize the client
        RedefineAuth client = new RedefineAuth(
            "YOUR_REDEFINE_ID_HERE",           // Your Redefine ID
            "https://redefine-auth-v2.vercel.app/",          // Base URL of your RedefineAuth deployment
            "1.0.0.0"                         // Application version
        );
        
        // Register a new user
        RedefineAuth.AuthResult result = client.register("username", "LICENSE-KEY-123");
        if (result.success) {
            System.out.println("Registration successful!");
        } else {
            System.out.println("Registration failed: " + result.message);
        }
        
        // Login an existing user
        result = client.login("username");
        if (result.success) {
            System.out.println("Login successful!");
        } else {
            System.out.println("Login failed: " + result.message);
        }
    }
}
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

The example includes automatic HWID generation based on the machine hostname, OS name, and architecture. This is required by the Redefine Auth API for both registration and login operations.

In a production environment, you might want to implement a more sophisticated HWID generation method that's appropriate for your application's security requirements.

## Application Version Control

The RedefineAuth dashboard includes an Application Version setting that allows you to enforce version control for your client applications:

- When set, client applications must provide a matching version number during login
- If the client version doesn't match the configured version, users receive a customizable "Outdated Version" error message

To use this feature:
1. Set the Application Version in the RedefineAuth dashboard Settings page (e.g., "2.0.0.0")
2. Update the `appVersion` parameter in your Java application to match (e.g., "2.0.0.0")
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

## Compiling and Running

```bash
# Compile
javac -cp ".:gson-2.8.9.jar" RedefineAuth.java

# Run
java -cp ".:gson-2.8.9.jar" RedefineAuth
```

Note: On Windows, use semicolons instead of colons in the classpath:
```bash
# Compile (Windows)
javac -cp ".;gson-2.8.9.jar" RedefineAuth.java

# Run (Windows)
java -cp ".;gson-2.8.9.jar" RedefineAuth
```
