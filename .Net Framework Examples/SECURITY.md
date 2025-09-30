# Security Settings Implementation

This document explains how the Security section in the RedefineAuth dashboard Settings page is connected and implemented in the end applications.

## Security Settings Overview

The Security section in the Settings page includes the following security settings:

1. **HWID Lock for Product Keys** - Controls whether license keys are locked to specific hardware devices
2. **Confirm on "Delete All"** - Requires confirmation before deleting all keys or credentials

## Implementation Details

### HWID Lock for Product Keys

This setting is implemented in the license key validation API (`/api/keys/validate`). When enabled:
- License keys can only be used on a single hardware device
- If a user tries to validate a key from a different device, they receive an HWID mismatch error
- The custom HWID Mismatch Message from the Settings page is used in the error response

### Confirm on "Delete All"

This setting controls whether a confirmation dialog is shown before deleting all keys or credentials. It's a UI-only setting that affects the dashboard interface.

## How It Works

1. **Settings Storage**: Security settings are stored in the `UserSettings` model in the database
2. **API Integration**: The license key validation API queries these settings
3. **Automatic Enforcement**: When security settings are enabled, the APIs automatically enforce the restrictions
4. **Custom Messages**: Error messages are customized using the messages configured in the Settings page

## Client Application Behavior

The .NET client applications don't need any special code to handle these security settings:
- The restrictions are automatically enforced by the server-side APIs
- When a security violation occurs, the API returns an appropriate error message
- The client applications display these error messages to the end users
- No changes are needed to the client code when security settings are modified in the dashboard

## Error Handling

When security violations occur, the APIs return specific HTTP status codes:
- **403 Forbidden** - For HWID mismatch errors
- **503 Service Unavailable** - For application paused status

The .NET client applications handle these errors gracefully and display the custom messages configured in the dashboard.
