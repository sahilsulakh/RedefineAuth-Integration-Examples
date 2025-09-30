import hashlib
import platform
import requests
import json
from typing import Dict, Any, Optional

class AuthResult:
    """Represents the result of an authentication operation."""
    def __init__(self, success: bool, message: str):
        self.success = success
        self.message = message

class RedefineAuthClient:
    """Client for RedefineAuth authentication service."""
    
    def __init__(self, redefine_id: str, base_url: str = "https://redefine-auth-v2.vercel.app/", app_version: str = "1.0.0.0"):
        """
        Initialize the RedefineAuth client.
        
        Args:
            redefine_id: Your Redefine ID
            base_url: Base URL of your RedefineAuth deployment
            app_version: Version of your application
        """
        self.redefine_id = redefine_id
        self.base_url = base_url.rstrip('/')  # Remove trailing slash if present
        self.app_version = app_version

    def register(self, username: str, license_key: str) -> AuthResult:
        """
        Register a new user with username and license key.
        
        Args:
            username: The username to register
            license_key: The license key provided by the admin
            
        Returns:
            AuthResult with success status and message
        """
        try:
            # Generate HWID based on machine info
            hwid = self._get_hardware_id()
            
            # Prepare request data
            request_data = {
                "username": username,
                "licenseKey": license_key,
                "hwid": hwid,
                "redefineDeveloperId": self.redefine_id,
                "appVersion": self.app_version
            }
            
            # Make HTTP POST request
            response = requests.post(
                f"{self.base_url}/api/client-users",
                json=request_data,
                headers={"Content-Type": "application/json"}
            )
            
            # Handle response
            if response.status_code == 200:
                return AuthResult(True, "User registered successfully")
            else:
                return self._handle_error_response(response)
                
        except requests.exceptions.RequestException as e:
            return AuthResult(False, f"Network error: {str(e)}")
        except Exception as e:
            return AuthResult(False, f"Registration error: {str(e)}")

    def login(self, username: str) -> AuthResult:
        """
        Login an existing user with username.
        
        Args:
            username: The username to login
            
        Returns:
            AuthResult with success status and message
        """
        try:
            # Generate HWID based on machine info
            hwid = self._get_hardware_id()
            
            # Prepare request data
            request_data = {
                "username": username,
                "hwid": hwid,
                "redefineDeveloperId": self.redefine_id,
                "appVersion": self.app_version
            }
            
            # Make HTTP POST request
            response = requests.post(
                f"{self.base_url}/api/client-users/login",
                json=request_data,
                headers={"Content-Type": "application/json"}
            )
            
            # Handle response
            if response.status_code == 200:
                return AuthResult(True, "Login successful")
            else:
                return self._handle_error_response(response)
                
        except requests.exceptions.RequestException as e:
            return AuthResult(False, f"Network error: {str(e)}")
        except Exception as e:
            return AuthResult(False, f"Login error: {str(e)}")

    def _get_hardware_id(self) -> str:
        """
        Generate a simple HWID based on machine name and platform info.
        
        Returns:
            Base64 encoded HWID string
        """
        # Get machine info
        machine_info = f"{platform.node()}{platform.platform()}"
        
        # Create SHA256 hash and encode as base64
        hashed = hashlib.sha256(machine_info.encode('utf-8')).digest()
        return hashed.hex()

    def _handle_error_response(self, response: requests.Response) -> AuthResult:
        """
        Handle different HTTP error responses.
        
        Args:
            response: HTTP response object
            
        Returns:
            AuthResult with appropriate error message
        """
        try:
            response_content = response.text
        except:
            response_content = "Unknown error"
            
        # Handle different status codes
        if response.status_code == 503:
            return AuthResult(False, f"Application Paused: {response_content}")
        elif response.status_code == 400:
            return AuthResult(False, f"Invalid Request: {response_content}")
        elif response.status_code == 404:
            return AuthResult(False, f"Resource Not Found: {response_content}")
        elif response.status_code == 403:
            return AuthResult(False, f"Access Denied: {response_content}")
        else:
            return AuthResult(False, f"HTTP {response.status_code}: {response_content}")

# Example usage
if __name__ == "__main__":
    # Initialize the client
    client = RedefineAuthClient(
        redefine_id="YOUR_REDEFINE_ID_HERE",
        base_url="https://redefine-auth-v2.vercel.app/",  # Change this to your deployed RedefineAuth URL
        app_version="1.0.0.0"  # Change this to match your application version
    )
    
    # Example registration
    print("Registering user...")
    result = client.register("testuser", "LICENSE-KEY-123")
    print(f"Registration result: {result.success} - {result.message}")
    
    # Example login
    print("\nLogging in user...")
    result = client.login("testuser")
    print(f"Login result: {result.success} - {result.message}")
