using System;
using System.Windows.Forms;
using RedefineAuth;

namespace YourWorkspaceNameHere
{
    public partial class Form1 : Form
    {   
        // RedefineAuth client
        private RedefineAuthClient _authClient;
        
        // Developer sets their Redefine ID and Base URL here
        private const string REDEFINE_ID = "YOUR_REDEFINE_ID_HERE"; // <<<--- GET IT FROM THE PROFILE PAGE OF REDEFINE LITE
        private const string BASE_URL = "http://localhost:9002"; // DO NOT CHANGE IT!
        private const string APP_VERSION = "1.0.0.0"; // Change this to match your application version

        public Form1()
        {
            InitializeComponent();
            _authClient = new RedefineAuthClient(REDEFINE_ID, BASE_URL, APP_VERSION);
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            // Get values from the UI components (assuming they exist)
            string username = txtUsername.Text.Trim();
            string licenseKey = txtLicenseKey.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(licenseKey))
            {
                MessageBox.Show("Please enter both username and license key.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblStatus.Text = "Please enter both username and license key";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            lblStatus.Text = "Registering...";
            lblStatus.ForeColor = System.Drawing.Color.Orange;
            btnRegister.Enabled = false;
            btnLogin.Enabled = false;

            try
            {
                var result = await _authClient.RegisterAsync(username, licenseKey);
                
                lblStatus.Text = result.Message;
                lblStatus.ForeColor = result.Success ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                
                if (result.Success)
                {
                    MessageBox.Show($"Registration successful!\n\n{result.Message}", "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Check if the application is paused
                    if (result.Message.StartsWith("Application Paused"))
                    {
                        MessageBox.Show($"Registration failed:\n\n{result.Message}\n\nThe application has been paused by the developer.", "Application Paused", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Registration failed:\n\n{result.Message}", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Registration error: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"An error occurred during registration:\n\n{ex.Message}", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnLogin.Enabled = true;
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Get values from the UI components (assuming they exist)
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter username.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblStatus.Text = "Please enter username";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            lblStatus.Text = "Logging in...";
            lblStatus.ForeColor = System.Drawing.Color.Orange;
            btnRegister.Enabled = false;
            btnLogin.Enabled = false;

            try
            {
                var result = await _authClient.LoginAsync(username);
                
                lblStatus.Text = result.Message;
                lblStatus.ForeColor = result.Success ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                
                if (result.Success)
                {
                    MessageBox.Show($"Login successful!\n\n{result.Message}", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Check if the application is paused
                    if (result.Message.StartsWith("Application Paused"))
                    {
                        MessageBox.Show($"Login failed:\n\n{result.Message}\n\nThe application has been paused by the developer.", "Application Paused", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    // Check if it's an outdated version error
                    else if (result.Message.StartsWith("Access Denied") && result.Message.Contains("version"))
                    {
                        MessageBox.Show($"Login failed:\n\n{result.Message}\n\nYour application version is outdated. Please update to the latest version.", "Outdated Version", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Login failed:\n\n{result.Message}", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Login error: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"An error occurred during login:\n\n{ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnLogin.Enabled = true;
            }
        }
    }
}
