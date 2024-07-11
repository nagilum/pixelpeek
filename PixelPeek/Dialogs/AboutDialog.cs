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
            Tag = Program.GitHubRepo,
            Text = $"Source and documentation available at {Program.GitHubRepo}"
        };

        var iconCreditLabel = new LinkLabel
        {
            AutoSize = true,
            Location = new(14, 300),
            Tag = "https://www.iconfinder.com/icons/6556720/eye_eyeball_lens_picture_view_icon",
            Text = "Icon by Yogi Aprelliyanto"
        };

        documentationLabel.LinkClicked += LinkLabelClicked;
        iconCreditLabel.LinkClicked += LinkLabelClicked;
        
        // User input.
        var okButton = new Button
        {
            Location = new(600, 280),
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
        this.Controls.Add(iconCreditLabel);
        
        this.Controls.Add(okButton);
    }

    /// <summary>
    /// Setup form with events.
    /// </summary>
    private void SetupForm()
    {
        this.ClientSize = new Size(710, 330);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.Icon = Program.ApplicationIcon;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = $"About {Program.Name}";
    }
    
    #endregion
    
    #region Event functions

    /// <summary>
    /// Attempts to open the senders tag, as a URL, in the users default browser.
    /// </summary>
    private void LinkLabelClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        string? url = null;
        
        try
        {
            if (sender is not LinkLabel label)
            {
                throw new Exception("Invalid LinkLabel");
            }

            url = label.Tag?.ToString();

            if (url is null)
            {
                throw new Exception("No URL saved in LinkLabel tag.");
            }
            
            var info = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (Exception ex)
        {
            if (url is null)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                Clipboard.SetText(url);
            
                MessageBox.Show(
                    $"Unable to open URL in default browser. The URL {url} has been copied to your clipboard.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
    
    #endregion
}