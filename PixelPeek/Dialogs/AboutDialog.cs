using System.Diagnostics;

namespace PixelPeek.Dialogs;

public class AboutDialog : Form
{
    #region Constructor functions
    
    /// <summary>
    /// Create a new about dialog.
    /// </summary>
    public AboutDialog()
    {
        this.SetupControls();
        this.SetupForm();
    }

    /// <summary>
    /// Setup and add controls with events.
    /// </summary>
    private void SetupControls()
    {
        // Product name and version.
        var productNameLabel = new Label
        {
            AutoSize = true,
            Font = new(this.Font.FontFamily, this.Font.Size + 5),
            Location = new(10, 10),
            Text = Program.Name
        };

        var productVersionLabel = new Label
        {
            AutoSize = true,
            Location = new(13, 35),
            Text = $"Version {Program.Version}"
        };
        
        // Usage.
        var usageLabel = new Label
        {
            AutoSize = true,
            Location = new(13, 90),
            Text = $"Usage: {Program.Name} <path> [<options>]"
        };

        var optionsTriggersLabel = new Label
        {
            AutoSize = true,
            Location = new(13, 120),
            Text =
                $"Options:{Environment.NewLine}" +
                $"    -f | --fullscreen{Environment.NewLine}" +
                $"    -i | --interval <milliseconds>{Environment.NewLine}" +
                $"    -r | --random{Environment.NewLine}" +
                $"    -s | --slideshow"
        };

        var optionsDescriptionsLabel = new Label
        {
            AutoSize = true,
            Location = new(200, 120),
            Text =
                $"{Environment.NewLine}" +
                $"Start the app in full screen mode.{Environment.NewLine}" +
                $"Interval between each slideshow image, in milliseconds. Defaults to 5000 (5 seconds).{Environment.NewLine}" +
                $"Randomize order of files.{Environment.NewLine}" +
                $"Start slideshow when app starts."
        };
        
        // Copyright and documentation.
        var copyrightLabel = new Label
        {
            AutoSize = true,
            Location = new(13, 255),
            Text = "Copyright \u00a9 2024 Stian Hanger"
        };
        
        var documentationLabel = new LinkLabel
        {
            AutoSize = true,
            LinkArea = new(38, 36),
            Location = new(14, 270),
            Text = $"Source and documentation available at {Program.GitHubRepo}"
        };

        documentationLabel.LinkClicked += DocumentationLabelLinkClicked;
        
        // User input.
        var okButton = new Button
        {
            Location = new(600, 250),
            Size = new(100, 40),
            Text = "Ok"
        };

        okButton.Click += (_, _) =>
        {
            this.Close();
        };

        // Add controls.
        this.Controls.Add(productNameLabel);
        this.Controls.Add(productVersionLabel);
        
        this.Controls.Add(usageLabel);
        this.Controls.Add(optionsTriggersLabel);
        this.Controls.Add(optionsDescriptionsLabel);

        this.Controls.Add(copyrightLabel);
        this.Controls.Add(documentationLabel);
        
        this.Controls.Add(okButton);
    }

    /// <summary>
    /// Setup form with events.
    /// </summary>
    private void SetupForm()
    {
        this.ClientSize = new Size(710, 300);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = $"About {Program.Name}";
    }
    
    #endregion
    
    #region Event functions

    /// <summary>
    /// Opens the GitHub repo URL in the users default browser.
    /// </summary>
    private void DocumentationLabelLinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = Program.GitHubRepo,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch
        {
            Clipboard.SetText(Program.GitHubRepo);
            
            MessageBox.Show(
                $"Unable to open URL in default browser. The URL {Program.GitHubRepo} has been copied to your clipboard.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    #endregion
}