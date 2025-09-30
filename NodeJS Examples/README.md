# RedefineAuth Node.js Integration

This is a Node.js integration for RedefineAuth that allows end users to register and login using a username and license key.

## Requirements

- Node.js 12.0 or higher
- Internet connectivity

## Installation

No additional dependencies are required as this integration uses only built-in Node.js modules.

Simply copy the `RedefineAuth.js` file to your project.

## Usage

### Basic Implementation

```javascript
const { RedefineAuthClient } = require('./RedefineAuth');

// Initialize the client
const client = new RedefineAuthClient(
    "YOUR_REDEFINE_ID_HERE",           // Your Redefine ID
    "https://redefine-auth-v2.vercel.app/",          // Base URL of your RedefineAuth deployment
    "1.0.0.0"                         // Application version
);

// Register a new user
client.register("username", "LICENSE-KEY-123")
    .then(result => {
        if (result.success) {
            console.log("Registration successful!");
        } else {
            console.log(`Registration failed: ${result.message}`);
        }
    })
    .catch(error => {
        console.error(`Registration error: ${error.message}`);
    });

// Login an existing user
client.login("username")
    .then(result => {
        if (result.success) {
            console.log("Login successful!");
        } else {
            console.log(`Login failed: ${result.message}`);
        }
    })
    .catch(error => {
        console.error(`Login error: ${error.message}`);
    });
```

### Async/Await Implementation

```javascript
const { RedefineAuthClient } = require('./RedefineAuth');

async function example() {
    // Initialize the client
    const client = new RedefineAuthClient(
        "YOUR_REDEFINE_ID_HERE",           // Your Redefine ID
        "https://redefine-auth-v2.vercel.app/",          // Base URL of your RedefineAuth deployment
        "1.0.0.0"                         // Application version
    );
    
    try {
        // Register a new user
        const registerResult = await client.register("username", "LICENSE-KEY-123");
        if (registerResult.success) {
            console.log("Registration successful!");
        } else {
            console.log(`Registration failed: ${registerResult.message}`);
        }
        
        // Login an existing user
        const loginResult = await client.login("username");
        if (loginResult.success) {
            console.log("Login successful!");
        } else {
            console.log(`Login failed: ${loginResult.message}`);
        }
    } catch (error) {
        console.error(`Error: ${error.message}`);
    }
}

example();
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

The example includes automatic HWID generation based on the machine hostname, platform, architecture, and CPU information. This is required by the Redefine Auth API for both registration and login operations.

In a production environment, you might want to implement a more sophisticated HWID generation method that's appropriate for your application's security requirements.

## Application Version Control

The RedefineAuth dashboard includes an Application Version setting that allows you to enforce version control for your client applications:

- When set, client applications must provide a matching version number during login
- If the client version doesn't match the configured version, users receive a customizable "Outdated Version" error message

To use this feature:
1. Set the Application Version in the RedefineAuth dashboard Settings page (e.g., "2.0.0.0")
2. Update the `appVersion` parameter in your Node.js application to match (e.g., "2.0.0.0")
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

## Running the Example

```bash
node RedefineAuth.js
```
