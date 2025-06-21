using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceNameHere                                 // <<<--- REPLACE WITH YOUR ACTUAL NAMESPACE
{
    public partial class Form1 : Form
    {
        // Configuration
        private const string YOUR_REDEFINE_API_URL = "https://redefine-auth-v2.vercel.app/";          // <<<--- DO NOT CHANGE IT
        private const string YOUR_REDEFINE_DEVELOPER_ID = "68556e4b42e6abb22177f63c";                // <<<--- GET IT FROM REDEFINE PROFILE
        private const string CLIENT_APP_VERSION = "1.0.0";                                           

        private RedefineAuthClient _authClient;
        private bool _isActivated;
        private string _settingsFile;

        public Form1()
        {
            InitializeComponent();
            _authClient = new RedefineAuthClient(YOUR_REDEFINE_API_URL, YOUR_REDEFINE_DEVELOPER_ID);
            _settingsFile = Path.Combine(Application.StartupPath, "activation.dat");
            LoadAndUpdateUI();
        }

        private void LoadAndUpdateUI()
        {
            _isActivated = File.Exists(_settingsFile) &&
                          File.ReadAllText(_settingsFile).Trim().Equals("ACTIVATED", StringComparison.OrdinalIgnoreCase);

            if (_isActivated)
            {
                lblStatus.Text = "Product is activated. Please login.";
                EnableControls(login: true, register: false);
            }
            else
            {
                lblStatus.Text = "Product Not Activated. Please Register.";
                EnableControls(login: false, register: true);
            }
        }

        private void EnableControls(bool login, bool register)
        {
            BtnLogin.Enabled = login;
            BtnRegisterWithLicenseKey.Enabled = register;
            BtnRegisterWithUserPass.Enabled = register;
            txtKey.Enabled = register;
            txtAppUser.Enabled = register;
            txtAppPass.Enabled = register;
        }

        private void SaveActivation(bool activated)
        {
            try
            {
                if (activated)
                    File.WriteAllText(_settingsFile, "ACTIVATED");
                else if (File.Exists(_settingsFile))
                    File.Delete(_settingsFile);

                _isActivated = activated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Could not save activation status. {ex.Message}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ProcessResult(ValidationResponse result)
        {
            bool success = result?.Success == true;
            SaveActivation(success);

            lblStatus.Text = success
                ? $"Registration Successful! Owner: {result.OwnerUsername ?? "Unknown"}"
                : $"Registration Failed: {result?.Message ?? "Unknown error"}";

            MessageBox.Show(success ? "Registration successful! You can now login." : $"Registration failed: {result?.Message}",
                success ? "Success" : "Error", MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            LoadAndUpdateUI();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (_isActivated)
            {
                lblStatus.Text = "Login Successful! Welcome!";
                MessageBox.Show("Login successful! Welcome to the application.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Login failed. Please register the product first.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


// BELOW ARE TWO BUTTONS AND YOU HAVE TO USE ONE OF THEM 👇

        // USE THIS BUTTON IF YOU WANT TO USE LICENSE KEY AUTHENTICATION
        private async void BtnRegisterWithLicenseKey_Click(object sender, EventArgs e)
        {
            string key = txtKey?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Please enter a valid license key.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "Validating license key...";
            try
            {
                var result = await _authClient.ValidateLicenseKeyAsync(key, CLIENT_APP_VERSION);
                ProcessResult(result);
            }
            catch (Exception ex)
            {
                ProcessResult(new ValidationResponse { Success = false, Message = ex.Message });
            }
        }

        // USE THIS BUTTON IF YOU WANT TO USE USER/PASS AUTHENTICATION 
        private async void BtnRegisterWithUserPass_Click(object sender, EventArgs e)
        {
            string username = txtAppUser?.Text?.Trim();
            string password = txtAppPass?.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "Validating credentials...";
            try
            {
                var result = await _authClient.ValidateAppCredentialAsync(username, password, CLIENT_APP_VERSION);
                ProcessResult(result);
            }
            catch (Exception ex)
            {
                ProcessResult(new ValidationResponse { Success = false, Message = ex.Message });
            }
        }
    }
}
