using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedefineApp                                 // <<<--- REPLACE WITH YOUR ACTUAL NAMESPACE
{
    public partial class Form1 : Form
    {
        // Configuration - Only these should be in Form1
        private const string YOUR_REDEFINE_API_URL = "https://redefine-auth-v2.vercel.app/";          // <<<--- DO NOT CHANGE IT
        private const string YOUR_REDEFINE_DEVELOPER_ID = "688776780aa3c2a2342bd2a7";                // <<<--- GET IT FROM REDEFINE PROFILE
        private const string CLIENT_APP_VERSION = "1.0.0";

        private RedefineAuthClient _authClient;

        public Form1()
        {
            InitializeComponent();
            _authClient = new RedefineAuthClient(YOUR_REDEFINE_API_URL, YOUR_REDEFINE_DEVELOPER_ID);
            InitializeUI();
            LoadStoredCredentials();
        }

        private void InitializeUI()
        {
            // Check if user has stored credentials
            if (_authClient.IsRegistered)
            {
                lblStatus.Text = "Welcome back! Your credentials are loaded. Click Login to continue.";
                BtnLogin.Enabled = true;
                BtnRegisterWithLicenseKey.Enabled = false;
                BtnRegisterWithUserPass.Enabled = false;
            }
            else
            {
                lblStatus.Text = "Please enter your credentials and register first to enable the Login button.";
                BtnLogin.Enabled = false;
                BtnRegisterWithLicenseKey.Enabled = true;
                BtnRegisterWithUserPass.Enabled = true;
            }

            // Always enable input controls
            txtKey.Enabled = true;
            txtAppUser.Enabled = true;
            txtAppPass.Enabled = true;
        }

        private void LoadStoredCredentials()
        {
            var storedCredentials = _authClient.GetStoredCredentials();
            if (storedCredentials != null)
            {
                // Prefill the form with stored credentials
                if (storedCredentials.AuthType == "License Key")
                {
                    txtKey.Text = storedCredentials.LicenseKey;
                    txtAppUser.Clear();
                    txtAppPass.Clear();
                }
                else // Username/Password
                {
                    txtAppUser.Text = storedCredentials.Username;
                    txtAppPass.Text = storedCredentials.Password;
                    txtKey.Clear();
                }
            }
        }

        private void ProcessAuthResult(AuthenticationResult result, string operation)
        {
            if (result.Success)
            {
                var validationResult = result.ValidationResponse;
                lblStatus.Text = $"{operation} Successful! Owner: {validationResult?.OwnerUsername ?? "Unknown"}";

                string successMessage = $"{operation} successful using {result.AuthType}!";
                if (validationResult?.ExpiresAt.HasValue == true)
                {
                    successMessage += $"\n\nExpires: {validationResult.ExpiresAt.Value:yyyy-MM-dd HH:mm:ss}";
                }

                if (operation == "Registration")
                {
                    successMessage += "\n\nYou can now use the Login button to access the application.";
                    // Disable registration buttons after successful registration
                    BtnRegisterWithLicenseKey.Enabled = false;
                    BtnRegisterWithUserPass.Enabled = false;
                    // Enable login button after successful registration
                    BtnLogin.Enabled = true;
                }
                else if (operation == "Login")
                {
                    successMessage += "\n\nWelcome to the application!";

                    // Execute post-login actions
                    ExecutePostLoginActions(result.ValidationResponse);
                }

                MessageBox.Show(successMessage, $"{operation} Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = $"{operation} Failed: {result.Message}";
                string errorMessage = $"{operation} failed using {result.AuthType}:\n\n{result.Message}";

                if (operation == "Login")
                {
                    errorMessage += "\n\nIf you're a new user, please use the Register buttons instead.";
                }
                else
                {
                    errorMessage += "\n\nPlease check your credentials and try again.";
                }

                MessageBox.Show(errorMessage, $"{operation} Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // REGISTRATION METHODS
        private async void BtnRegisterWithLicenseKey_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Registering with license key...";
            BtnRegisterWithLicenseKey.Enabled = false;

            try
            {
                var result = await _authClient.RegisterWithLicenseKeyAsync(txtKey?.Text?.Trim(), CLIENT_APP_VERSION);
                ProcessAuthResult(result, "Registration");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Registration error occurred.";
                MessageBox.Show($"Registration failed: {ex.Message}", "Registration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BtnRegisterWithLicenseKey.Enabled = true;
            }
        }

        private async void BtnRegisterWithUserPass_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Registering with credentials...";
            BtnRegisterWithUserPass.Enabled = false;

            try
            {
                var result = await _authClient.RegisterWithCredentialsAsync(txtAppUser?.Text?.Trim(), txtAppPass?.Text, CLIENT_APP_VERSION);
                ProcessAuthResult(result, "Registration");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Registration error occurred.";
                MessageBox.Show($"Registration failed: {ex.Message}", "Registration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BtnRegisterWithUserPass.Enabled = true;
            }
        }

        // LOGIN METHOD
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Double-check that user is registered (this should not happen if UI is working correctly)
            if (!_authClient.IsRegistered)
            {
                MessageBox.Show("Please register first before attempting to login.", "Registration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BtnLogin.Enabled = false; // Ensure login button is disabled
                return;
            }

            lblStatus.Text = "Logging in...";
            BtnLogin.Enabled = false;

            try
            {
                AuthenticationResult result;

                // Check if we have stored credentials and use them
                var storedCredentials = _authClient.GetStoredCredentials();
                if (storedCredentials != null)
                {
                    result = await _authClient.LoginWithStoredCredentialsAsync(CLIENT_APP_VERSION);
                }
                else
                {
                    // Fallback to manual credentials
                    result = await _authClient.LoginAsync(txtKey?.Text?.Trim(), txtAppUser?.Text?.Trim(), txtAppPass?.Text, CLIENT_APP_VERSION);
                }

                ProcessAuthResult(result, "Login");

                if (result.Success)
                {
                    // Disable registration buttons since user is now logged in
                    BtnRegisterWithLicenseKey.Enabled = false;
                    BtnRegisterWithUserPass.Enabled = false;
                    // Keep login button enabled for future logins
                    BtnLogin.Enabled = true;
                }
                else
                {
                    // If login fails, user might need to register again
                    BtnLogin.Enabled = _authClient.IsRegistered;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Login error occurred.";
                MessageBox.Show($"Login failed: {ex.Message}", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Re-enable login button only if user is still registered
                BtnLogin.Enabled = _authClient.IsRegistered;
            }
        }


        // Logout and clear stored credentials
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to clear your saved credentials? If you choose 'No', you can login again without re-registering.",
                "Logout Options", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                return;

            if (result == DialogResult.Yes)
            {
                // Clear stored credentials completely
                _authClient.ClearStoredCredentials();
                txtKey.Clear();
                txtAppUser.Clear();
                txtAppPass.Clear();
                lblStatus.Text = "Logged out and credentials cleared. Please register again.";
                MessageBox.Show("You have been logged out and your credentials have been cleared. Please register again to use the application.",
                    "Logged Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Just reset session but keep stored credentials
                _authClient.ResetSession();
                lblStatus.Text = "Logged out. Your credentials are saved - click Login to continue.";
                MessageBox.Show("You have been logged out. Your credentials are still saved, so you can login again without re-registering.",
                    "Logged Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            InitializeUI(); // Refresh UI state
        }


        // ========================================================================================================
        // DEVELOPER CUSTOMIZATION SECTION - ADD YOUR POST-LOGIN LOGIC HERE
        // ========================================================================================================

        /// This method is called after successful login. Add your main application logic here.
        private void ExecutePostLoginActions(ValidationResponse validationResponse)
        {
            // Add your custom logic here, e.g., open main application window, etc.

        }

        // ========================================================================================================
        // FORM DISPOSAL - CLEANUP RESOURCES
        // ========================================================================================================
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_authClient != null)
                {
                    _authClient.Dispose();
                    _authClient = null;
                }
                
                // Dispose components if they exist
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        // Handle form closing to cleanup shared resources
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Cleanup shared HttpClient when application closes
            RedefineAuthClient.DisposeSharedResources();
            base.OnFormClosed(e);
        }
    }
}
