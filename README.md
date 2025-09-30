# RedefineAuth Integrations

This directory contains integration examples for various programming languages to work with the RedefineAuth service.

## Available Integrations

1. [.NET](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/dotnet) - Original implementation (C#)
2. [Python](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/python) - Python implementation
3. [C++](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/cpp) - C++ implementation
4. [Node.js](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/nodejs) - JavaScript/Node.js implementation
5. [Java](file:///c:/Users/MY-COMPUTER/Downloads/Redefine/redefine-auth-main/src/integrations/java) - Java implementation

## Common Features

All integrations provide the same core functionality:

- User registration with username and license key
- User login with username
- Automatic HWID (Hardware ID) generation
- Application version control
- Comprehensive error handling
- Support for custom error messages from the dashboard

## API Endpoints

All integrations use the same API endpoints:

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

## Security Features

All integrations support the security features of RedefineAuth:

- **HWID Lock for Product Keys** - When enabled, license keys can only be used on a single hardware device
- **Application Version Control** - Ensures users are running the correct version of your application
- **Application Pause** - Allows you to temporarily disable access to your application
- **Custom Error Messages** - Provides user-friendly error messages that can be configured in the dashboard

## Getting Started

1. Choose the integration for your preferred programming language
2. Follow the setup instructions in the language-specific README
3. Configure your Redefine ID, base URL, and application version
4. Integrate the authentication functionality into your application

## Support

For issues with any of the integrations, please check the language-specific README files for troubleshooting information or contact support.
