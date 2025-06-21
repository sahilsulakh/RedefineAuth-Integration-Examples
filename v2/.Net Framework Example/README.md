# Redefine Authentication Integration for C# Windows Forms

A complete C# WinForms application demonstrating integration with the Redefine authentication system for software licensing and user management.

## Overview

This project provides a ready-to-use authentication system that supports two validation scenarios:
- **Scenario A**: License Key Authentication
- **Scenario B**: Username/Password Authentication

The application includes hardware fingerprinting (HWID) for enhanced security and persistent activation status management.

## Features

- ✅ License key validation with hardware binding
- ✅ Username/password authentication
- ✅ Hardware ID (HWID) generation using CPU and motherboard identifiers
- ✅ Persistent activation status storage
- ✅ Clean, minimal UI with proper error handling
- ✅ Async/await pattern for non-blocking operations
- ✅ Comprehensive error handling and user feedback

## Prerequisites

- .NET Framework 4.7.2 or higher / .NET Core 3.1+ / .NET 5+
- Visual Studio 2019 or later
- NuGet Package Manager
- Redefine backend service running

## Required NuGet Packages

Install the following packages via NuGet Package Manager:

```bash
Install-Package Newtonsoft.Json
Install-Package System.Management
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.Management" Version="7.0.2" />
```

## Project Structure

```
AppRedefine/
├── Form1.cs                 # Main application form (minimal version)
├── Form1.Designer.cs        # Form designer file
├── RedefineAuthClient.cs    # Authentication client and models
├── Program.cs               # Application entry point
├── activation.dat           # Activation status file (created at runtime)
└── README.md               # This file
```

## Configuration

Before running the application, update the configuration constants in `Form1.cs`:

```csharp
// --- Configuration: Developer MUST set these ---
private const string YOUR_REDEFINE_API_URL = "http://localhost:9002"; // Your Redefine backend URL
private const string YOUR_REDEFINE_DEVELOPER_ID = "68556e4b42e6abb22177f63c"; // Get from Redefine Dashboard
private const string CLIENT_APP_VERSION = "1.0.0"; // Your app version
```

### Getting Your Developer ID

1. Log into your Redefine Dashboard
2. Navigate to your profile page
3. Copy your Developer ID
4. Paste it into the `YOUR_REDEFINE_DEVELOPER_ID` constant

## Form Controls Required

Your Windows Form must include the following controls with these exact names:

### Labels
- `lblStatus` - Status display label

### Buttons
- `BtnLogin` - Login button
- `BtnRegisterWithLicenseKey` - License key registration button
- `BtnRegisterWithUserPass` - Username/password registration button

### Text Boxes
- `txtKey` - License key input
- `txtAppUser` - Username input
- `txtAppPass` - Password input

## API Endpoints

The application expects the following endpoints on your Redefine backend:

### License Key Validation
```
POST /api/keys/validate
Content-Type: application/json

{
  "productKey": "string",
  "hwid": "string",
  "redefineDeveloperId": "string",
  "appVersion": "string"
}
```

### Username/Password Validation
```
POST /api/app-credentials/validate
Content-Type: application/json

{
  "username": "string",
  "password": "string",
  "hwid": "string",
  "redefineDeveloperId": "string",
  "appVersion": "string"
}
```

### Expected Response Format
```json
{
  "success": true,
  "message": "Validation successful",
  "status": "valid",
  "username": "user123",
  "expiresAt": "2024-12-31T23:59:59Z"
}
```

## How It Works

### 1. Hardware ID Generation
The application generates a unique hardware fingerprint using:
- CPU Processor ID
- Motherboard Serial Number
- SHA256 hashing for consistency

### 2. Activation Flow
1. **First Run**: Product is not activated, registration controls are enabled
2. **Registration**: User provides license key OR username/password
3. **Validation**: Credentials are sent to Redefine backend with HWID
4. **Activation**: On successful validation, activation status is saved locally
5. **Subsequent Runs**: Login button is enabled, registration controls are disabled

### 3. Persistent Storage
Activation status is stored in `activation.dat` file in the application directory. The file contains:
- `ACTIVATED` - Product is activated
- File absence or other content - Product is not activated

## Usage Examples

### Scenario A: License Key Authentication
```csharp
// User enters license key in txtKey textbox
// Clicks BtnRegisterWithLicenseKey
// Application validates with Redefine backend
// On success, product is activated
```

### Scenario B: Username/Password Authentication
```csharp
// User enters credentials in txtAppUser and txtAppPass
// Clicks BtnRegisterWithUserPass
// Application validates with Redefine backend
// On success, product is activated
```

## Error Handling

The application handles various error scenarios:

- **Network Errors**: Connection issues with Redefine backend
- **Timeout Errors**: Request timeouts (30-second limit)
- **Validation Errors**: Invalid credentials or license keys
- **Hardware Errors**: HWID generation failures
- **File System Errors**: Activation status save/load issues

## Security Features

- **Hardware Binding**: Each activation is tied to specific hardware
- **Secure Hashing**: SHA256 used for hardware fingerprinting
- **API Security**: All communications with Redefine backend are over HTTPS (in production)
- **Input Validation**: All user inputs are validated before processing

## Customization

### Changing Activation Storage
To use application settings instead of file-based storage, uncomment these lines in the original code:

```csharp
// Properties.Settings.Default.IsActivated = activated;
// Properties.Settings.Default.Save();
```

### Adding Main Application Logic
After successful login, add your main application logic:

```csharp
private void BtnLogin_Click(object sender, EventArgs e)
{
    if (_isActivated)
    {
        // Hide login form and show main application
        this.Hide();
        var mainForm = new MainApplicationForm();
        mainForm.Show();
    }
}
```

## Troubleshooting

### Common Issues

1. **"Network error or Redefine server unreachable"**
   - Check if Redefine backend is running
   - Verify the API URL is correct
   - Check network connectivity

2. **"HWID_GENERATION_ERROR"**
   - Application may not have WMI permissions
   - Run as administrator or check Windows Management Instrumentation service

3. **"Registration failed: Invalid developer ID"**
   - Verify your Developer ID from Redefine Dashboard
   - Ensure the ID is correctly copied (no extra spaces)

4. **Controls not found errors**
   - Ensure all required form controls exist with exact names
   - Check that controls are properly initialized in Form Designer

### Debug Mode
Add this to enable console output for debugging:
```csharp
#if DEBUG
Console.WriteLine($"HWID Generated: {HwidGenerator.GetHardwareId()}");
Console.WriteLine($"API URL: {YOUR_REDEFINE_API_URL}");
#endif
```

## Production Deployment

Before deploying to production:

1. **Update API URL**: Change to your production Redefine backend URL
2. **HTTPS Only**: Ensure all API communications use HTTPS
3. **Code Signing**: Sign your executable for Windows SmartScreen
4. **Obfuscation**: Consider code obfuscation for additional security
5. **Error Logging**: Implement proper logging for production debugging

## Support

For issues related to:
- **Redefine Backend**: Contact Redefine support
- **This Integration**: Check the troubleshooting section above
- **Windows Forms**: Refer to Microsoft documentation

## License

This integration code is provided as-is for demonstration purposes. Modify according to your needs and licensing requirements.

## Changelog

### v1.0.0
- Initial release with dual authentication scenarios
- Hardware ID generation and binding
- Persistent activation status
- Comprehensive error handling
- Minimal, clean UI implementation
