# RedefineAuth .NET Integration

This is a simple, production-ready .NET integration for RedefineAuth that allows end users to register and login using a username and license key.

We provide two implementations:

1. **MinimalExample.cs** - A complete self-contained example with UI components
2. **RedefineCore.cs + Form1.cs** - Separated core logic and UI event handlers

## Requirements

- .NET Framework 4.7.2 or higher
- Windows Forms application
- Internet connectivity

## Setup (Separated Core Implementation)

1. Add both `RedefineCore.cs` and `Form1.cs` to your project
2. Set your Redefine ID in `Form1.cs`:
   ```csharp
   private const string REDEFINE_ID = "YOUR_REDEFINE_ID_HERE";
   private const string BASE_URL = "http://localhost:9002"; // DO NOT CHANGE IT
   ```
3. Make sure your form has the following controls:
   - `txtUsername` (TextBox)
   - `txtLicenseKey` (TextBox)
   - `btnRegister` (Button)
   - `btnLogin` (Button)
   - `lblStatus` (Label)

4. Connect the button click events to the handlers in Form1.cs:
   ```csharp
   btnRegister.Click += new EventHandler(btnRegister_Click);
   btnLogin.Click += new EventHandler(btnLogin_Click);
   ```

## Usage

### Core Implementation

The core functionality is encapsulated in the `RedefineAuthClient` class which provides:

- `RegisterAsync()` - For user registration
- `LoginAsync()` - For user login

Both methods automatically generate a hardware ID (HWID) as required by the API.

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
  "redefineDeveloperId": "YOUR_REDEFINE_ID"
}
```

### Login Request
```json
{
  "username": "user123",
  "hwid": "GENERATED_HARDWARE_ID",
  "redefineDeveloperId": "YOUR_REDEFINE_ID"
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

The example now includes automatic HWID generation based on the machine name and user name. This is required by the Redefine Auth API for both registration and login operations.

## Application Paused Handling

When the application is paused from the RedefineAuth dashboard settings, both registration and login operations will return a 503 Service Unavailable status with a custom pause message. The .NET client will receive this error and display it to the user.

## Security Settings

The RedefineAuth dashboard includes security settings that affect how the end applications behave:

- **HWID Lock for Product Keys** - When enabled, license keys can only be used on a single hardware device

These security settings are automatically enforced by the API endpoints without requiring any changes to the client application code.

For detailed information about how security settings are implemented, see [SECURITY.md](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/dotnet/SECURITY.md).

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

## Error Handling

The example includes comprehensive error handling for:
- Network connectivity issues
- Server errors
- Invalid credentials
- Timeout errors
- Application paused status
- HWID mismatch errors (when security settings are enabled)
- Custom error messages from the dashboard

All errors are displayed to the user through the status label and message boxes.

## Common Issues and Troubleshooting

1. **Registration Failed**: If registration is failing, check:
   - The license key is valid and created for your developer ID
   - The license key hasn't expired
   - The license key hasn't reached its maximum usage limit
   - The Redefine Auth server is running and accessible
   - Your Redefine ID is correctly set
   - The HWID field is properly included in requests

2. **Network Errors**: Ensure your firewall allows outbound connections to the API endpoint.

3. **Timeout Errors**: Check your internet connection and try again.

4. **Application Paused**: If you receive a "Service Unavailable" error, the application has been paused in the dashboard settings.

5. **HWID Mismatch**: If you receive an HWID mismatch error, the security settings are preventing login from a different device.

## Disposal

The example properly disposes of HTTP clients to prevent resource leaks.
