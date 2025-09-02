using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceName
{
    public partial class Form1 : Form
    {
        private RedefineAuth _auth;
        public Form1()
        {
            InitializeComponent();
            // Initialize RedefineAuth with your server URL and developer ID
            _auth = new RedefineAuth(
                baseUrl: "https://redefine-auth-v2.vercel.app/",
                developerId: "YOUR_REDEFINE_ID"     // <<<--- GET IT FROM REDEFINE PROFILE PAGE
            );
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            await HandleAuthAction(isRegistration: true);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await HandleAuthAction(isRegistration: false);
        }

        private async Task HandleAuthAction(bool isRegistration)
        {
            try
            {
                // Disable buttons during operation
                btnRegister.Enabled = false;
                btnLogin.Enabled = false;
                lblStatus.Text = isRegistration ? "Registering..." : "Logging in...";
                lblStatus.ForeColor = System.Drawing.Color.Orange;

                string username = txtUsername.Text.Trim();
                string licenseKey = txtLicenseKey.Text.Trim();

                AuthResult result;
                if (isRegistration)
                {
                    result = await _auth.RegisterAsync(username, licenseKey);
                }
                else
                {
                    result = await _auth.LoginAsync(username);
                }

                // Update UI based on result
                if (result.Success)
                {
                    lblStatus.Text = $"Success: {result.Message}";
                    lblStatus.ForeColor = System.Drawing.Color.Green;

                    // Clear sensitive data
                    txtLicenseKey.Clear();

                    // Here you can proceed to your main application
                    MessageBox.Show($"Welcome, {result.Username}!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = $"Error: {result.Message}";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Unexpected error: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // Re-enable buttons
                btnRegister.Enabled = true;
                btnLogin.Enabled = true;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _auth?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
