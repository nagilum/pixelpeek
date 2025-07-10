using System.Diagnostics;
using System.Reflection;

namespace PixelPeek.Forms;

public class StartupForm : Form
{
    #region Fields and properties

    /// <summary>
    /// Copy run-command button.
    /// </summary>
    private Button? _copyCommandButton;

    /// <summary>
    /// Source and documentation link button.
    /// </summary>
    private LinkLabel? _documentationLabel;

    /// <summary>
    /// Checkbox for 'run in fullscreen'.
    /// </summary>
    private CheckBox? _fullscreenCheckbox;

    /// <summary>
    /// Numeric textbox for slideshow interval in milliseconds.
    /// </summary>
    private NumericUpDown? _intervalNumeric;

    /// <summary>
    /// Path input textbox.
    /// </summary>
    private TextBox? _pathTextbox;
    
    /// <summary>
    /// Checkbox for 'sort files randomized'.
    /// </summary>
    private CheckBox? _randomOrderCheckbox;

    /// <summary>
    /// Run button.
    /// </summary>
    private Button? _runButton;
    
    /// <summary>
    /// Checkbox for 'start slideshow automatically'.
    /// </summary>
    private CheckBox? _startSlideshowCheckbox;
    
    #endregion
    
    #region Constructor functions

    /// <summary>
    /// Create a new startup form.
    /// </summary>
    public StartupForm()
    {
        this.SetupControls();
        this.SetupForm();
    }

    /// <summary>
    /// Setup and add controls with events.
    /// </summary>
    private void SetupControls()
    {
        // Path browse
        
        var pathLabel = new Label
        {
            AutoSize = true,
            Location = new Point(10, 10),
            Text = "Select path to file or directory to view:"
        };
        
        _pathTextbox = new TextBox
        {
            AutoSize = true,
            Location = new Point(10, 27),
            Width = 690
        };

        var browseDirectory = new Button
        {
            Location = new Point(10, 55),
            Size = new Size(120, _pathTextbox.Size.Height + 6),
            Text = "Browse &Directory"
        };

        var browseFile = new Button
        {
            Location = new Point(130, 55),
            Size = new Size(120, _pathTextbox.Size.Height + 6),
            Text = "Browse &File"
        };

        _pathTextbox.TextChanged += (_, _) => { _runButton!.Enabled = !string.IsNullOrWhiteSpace(_pathTextbox!.Text); };
        browseDirectory.Click += this.BrowseDirectoryButtonClick;
        browseFile.Click += this.BrowseFileButtonClick;

        this.Controls.Add(pathLabel);
        this.Controls.Add(_pathTextbox);
        this.Controls.Add(browseDirectory);
        this.Controls.Add(browseFile);
        
        // Settings.

        _fullscreenCheckbox = new CheckBox
        {
            AutoSize = true,
            Location = new Point(10, 100),
            Text = "Run in fullscreen"
        };

        _randomOrderCheckbox = new CheckBox
        {
            AutoSize = true,
            Location = new Point(10, 120),
            Text = "Sort files randomized"
        };

        _startSlideshowCheckbox = new CheckBox
        {
            AutoSize = true,
            Location = new Point(10, 140),
            Text = "Start slideshow automatically"
        };

        var intervalLabel = new Label
        {
            AutoSize = true,
            Location = new Point(10, 180),
            Text = "Slideshow interval:"
        };

        _intervalNumeric = new NumericUpDown
        {
            AutoSize = false,
            Increment = 10,
            Location = new Point(120, 177),
            Maximum = decimal.MaxValue,
            Minimum = 1,
            Value = 5000,
            Width = 80
        };

        var msLabel = new Label
        {
            AutoSize = true,
            Location = new Point(200, 180),
            Text = "milliseconds"
        };

        this.Controls.Add(_fullscreenCheckbox);
        this.Controls.Add(_randomOrderCheckbox);
        this.Controls.Add(_startSlideshowCheckbox);
        
        this.Controls.Add(intervalLabel);
        this.Controls.Add(_intervalNumeric);
        this.Controls.Add(msLabel);
        
        // CTA bar.

        var hr = new Label
        {
            AutoSize = false,
            BorderStyle = BorderStyle.Fixed3D,
            Location = new Point(0, 300),
            Size = new Size(710, 2),
            Text = string.Empty
        };

        _documentationLabel = new LinkLabel
        {
            AutoSize = true,
            LinkArea = new LinkArea(38, 36),
            Location = new Point(10, 314),
            Text = $"Source and documentation available at{Environment.NewLine}{Program.GitHubRepo}"
        };

        _copyCommandButton = new Button
        {
            Enabled = false,
            Location = new Point(340, 310),
            Size = new Size(120, 40),
            Text = "C&opy Command"
        };

        _runButton = new Button
        {
            Enabled = false,
            Location = new Point(460, 310),
            Size = new Size(120, 40),
            Text = "&Run"
        };

        var closeButton = new Button
        {
            Location = new Point(580, 310),
            Size = new Size(120, 40),
            Text = "&Close"
        };

        _documentationLabel.LinkClicked += this.OpenDocumentationLinkButtonClick;
        _copyCommandButton.Click += this.CopyRunCommandButtonClick;
        _runButton.Click += this.RunCurrentSetupButtonClick;
        closeButton.Click += (_, _) => this.Close();

        this.Controls.Add(hr);
        this.Controls.Add(_documentationLabel);
        this.Controls.Add(_copyCommandButton);
        this.Controls.Add(_runButton);
        this.Controls.Add(closeButton);
    }

    /// <summary>
    /// Setup form and form events.
    /// </summary>
    private void SetupForm()
    {
        this.ClientSize = new Size(710, 360);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.Icon = Program.ApplicationIcon;
        this.MaximizeBox = false;
        this.MinimizeBox = true;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = $"{Program.Name} v{Program.Version}";
    }
    
    #endregion
    
    #region Event handlers

    /// <summary>
    /// Open dialog to browse for a directory.
    /// </summary>
    private void BrowseDirectoryButtonClick(object? sender, EventArgs e)
    {
        var dialog = new FolderBrowserDialog
        {
            ShowNewFolderButton = true,
            InitialDirectory = !string.IsNullOrWhiteSpace(_pathTextbox!.Text)
                ? _pathTextbox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        };

        if (dialog.ShowDialog(this) is not DialogResult.OK)
        {
            return;
        }
        
        _pathTextbox!.Text = dialog.SelectedPath;

        _copyCommandButton!.Enabled = true;
        _runButton!.Enabled = true;
    }
    
    /// <summary>
    /// Open dialog to browse for a file.
    /// </summary>
    private void BrowseFileButtonClick(object? sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            FileName = _pathTextbox!.Text,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) is not DialogResult.OK)
        {
            return;
        }
        
        _pathTextbox!.Text = dialog.FileName;
        
        _copyCommandButton!.Enabled = true;
        _runButton!.Enabled = true;
    }

    /// <summary>
    /// Assemble run command for current setup and copy to clipboard.
    /// </summary>
    private void CopyRunCommandButtonClick(object? sender, EventArgs e)
    {
        try
        {
            var args = this.GetExecutingArguments();

            args[0] = "\"" + args[0] + "\"";
            
            var command = string.Join(" ", args);
            
            Clipboard.SetText(command, TextDataFormat.Text);

            MessageBox.Show(
                "Command copied to clipboard.",
                "Copied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to assemble run command arguments for current setup." + Environment.NewLine + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// Attempt to open the GitHub repo in the users default browser.
    /// </summary>
    private void OpenDocumentationLinkButtonClick(object? sender, LinkLabelLinkClickedEventArgs e)
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
        catch (Exception ex)
        {
            Clipboard.SetText(Program.GitHubRepo, TextDataFormat.Text);
            
            MessageBox.Show(
                $"Unable to open URL in default browser. The URL {Program.GitHubRepo} has been copied to your clipboard." +
                Environment.NewLine +
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Run the program with the current setup configured.
    /// </summary>
    private void RunCurrentSetupButtonClick(object? sender, EventArgs e)
    {
        try
        {
            var args = this.GetExecutingArguments();
            var info = new ProcessStartInfo
            {
                Arguments = string.Join(" ", args.Skip(1)),
                FileName = args[0]
            };
            
            Process.Start(info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to run the current setup." + Environment.NewLine + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    #endregion
    
    #region Helper functions

    /// <summary>
    /// Gather all arguments to start PixelPeek with the current setup.
    /// </summary>
    /// <returns>List of arguments.</returns>
    private List<string> GetExecutingArguments()
    {
        var args = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var path = Path.Combine(
            Path.GetDirectoryName(assembly.Location) ?? string.Empty,
            Path.GetFileNameWithoutExtension(assembly.Location) + ".exe");

        args.Add(path);
        
        if (!string.IsNullOrWhiteSpace(_pathTextbox!.Text.Trim()))
        {
            args.Add("\"" + _pathTextbox.Text.Trim() + "\"");
        }

        if (_fullscreenCheckbox!.Checked)
        {
            args.Add("-f");
        }

        if (_randomOrderCheckbox!.Checked)
        {
            args.Add("-r");
        }

        if (_startSlideshowCheckbox!.Checked)
        {
            args.Add("-s"); 
            args.Add($"-i {(int)_intervalNumeric!.Value}");
        }
        
        return args;
    }
    
    #endregion
}