# RedefineAuth Python Integration

This is a Python integration for RedefineAuth that allows end users to register and login using a username and license key.

## Requirements

- Python 3.6 or higher
- requests library
- Internet connectivity

## Installation

1. Install the required dependencies:
   ```bash
   pip install requests
   ```

2. Copy the `RedefineAuth.py` file to your project

## Usage

### Basic Implementation

```python
from RedefineAuth import RedefineAuthClient

# Initialize the client
client = RedefineAuthClient(
    redefine_id="YOUR_REDEFINE_ID_HERE",
    base_url="http://localhost:9002",  # Change this to your deployed RedefineAuth URL
    app_version="1.0.0.0"  # Change this to match your application version
)

# Register a new user
result = client.register("username", "LICENSE-KEY-123")
if result.success:
    print("Registration successful!")
else:
    print(f"Registration failed: {result.message}")

# Login an existing user
result = client.login("username")
if result.success:
    print("Login successful!")
else:
    print(f"Login failed: {result.message}")
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

The example includes automatic HWID generation based on the machine name and platform information. This is required by the Redefine Auth API for both registration and login operations.

In a production environment, you might want to implement a more sophisticated HWID generation method that's appropriate for your application's security requirements.

## Application Version Control

The RedefineAuth dashboard includes an Application Version setting that allows you to enforce version control for your client applications:

- When set, client applications must provide a matching version number during login
- If the client version doesn't match the configured version, users receive a customizable "Outdated Version" error message

To use this feature:
1. Set the Application Version in the RedefineAuth dashboard Settings page (e.g., "2.0.0.0")
2. Update the `app_version` parameter in your Python application to match (e.g., "2.0.0.0")
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
