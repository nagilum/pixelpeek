using PixelPeek.Models;
using PixelPeek.Models.Interfaces;
using WebPLib;
using Timer = System.Windows.Forms.Timer;

namespace PixelPeek.Forms;

public class ViewerForm : Form
{
    #region Fields and properties

    /// <summary>
    /// Selected file index.
    /// </summary>
    private int? _fileIndex;

    /// <summary>
    /// Index of file to clear bitmap data for.
    /// </summary>
    private int? _fileIndexToClear;
    
    /// <summary>
    /// Image files.
    /// </summary>
    private List<IFileEntry> _files = [];

    /// <summary>
    /// Program options.
    /// </summary>
    private readonly IOptions _options;

    /// <summary>
    /// The last recorded window state.
    /// </summary>
    private FormWindowState _lastWindowState;

    /// <summary>
    /// Whether the image box is currently being moved.
    /// </summary>
    private bool _imageIsMoving;

    /// <summary>
    /// Original cursor position.
    /// </summary>
    private Point? _imageCursorPosition;

    /// <summary>
    /// Original image position.
    /// </summary>
    private Point? _imageOriginalPosition;

    /// <summary>
    /// WebP reader.
    /// </summary>
    private readonly WebP _webP = new();

    /// <summary>
    /// Zoom factor.
    /// </summary>
    private double? _zoomFactor;

    /// <summary>
    /// Zoom factor step.
    /// </summary>
    private double? _zoomFactorStep;
    
    #endregion
    
    #region Form controls

    /// <summary>
    /// Error label.
    /// </summary>
    private Label _errorLabel = null!;

    /// <summary>
    /// Image viewer box.
    /// </summary>
    private PictureBox _imageBox = null!;

    /// <summary>
    /// Wrapper, for centering.
    /// </summary>
    private PictureBox _wrapperBox = null!;

    /// <summary>
    /// Timer, for slideshow.
    /// </summary>
    private Timer _timer = null!;
    
    #endregion
    
    #region Constructor functions
    
    /// <summary>
    /// Create a new image viewer window.
    /// </summary>
    /// <param name="options">Program options.</param>
    public ViewerForm(IOptions options)
    {
        _options = options;
        
        this.GetFiles();
        this.SetupControls();
        this.SetupForm();

        if (options.SetFullscreen)
        {
            this.ToggleFullscreen();
        }

        if (options.StartSlideshow)
        {
            this.ToggleSlideshow();
        }
    }

    /// <summary>
    /// Scan the folder for files.
    /// </summary>
    private void GetFiles()
    {
        try
        {
            var files = Directory.GetFiles(
                _options.Path!,
                "*",
                SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                _files.Add(new FileEntry(file));
            }
        }
        catch
        {
            MessageBox.Show(
                $"Unable to get files from {_options.Path!}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        _files = _options.SortOrder switch
        {
            FilesSortOrder.Alphabetical => _files.OrderBy(n => n.Filename).ToList(),
            FilesSortOrder.Random => _files.OrderBy(_ => Random.Shared.Next()).ToList(),
            _ => throw new Exception($"Invalid sort order {_options.SortOrder}")
        };

        if (_options.File is null)
        {
            if (_files.Count > 0)
            {
                _fileIndex = 0;
            }
            
            return;
        }

        for (var i = 0; i < _files.Count; i++)
        {
            if (!_files[i].FullPath.Equals(_options.File, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _fileIndex = i;
            break;
        }

        if (_fileIndex is null)
        {
            MessageBox.Show(
                $"Unable to find {_options.File} among the found files.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            _options.File = null;
        }

        if (_fileIndex is null &&
            _files.Count > 0)
        {
            _fileIndex = 0;
        }
    }

    /// <summary>
    /// Setup and add controls with events.
    /// </summary>
    private void SetupControls()
    {
        _errorLabel = new Label
        {
            BackColor = Color.Black,
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false
        };
        
        _imageBox = new PictureBox
        {
            Visible = false
        };

        _imageBox.MouseDoubleClick += this.ImageBoxMouseDoubleClickEvent;
        _imageBox.MouseDown += this.ImageBoxMouseDownEvent;
        _imageBox.MouseMove += this.ImageBoxMouseMoveEvent;
        _imageBox.MouseUp += this.ImageBoxMouseUpEvent;

        _wrapperBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Visible = true
        };

        _wrapperBox.Controls.Add(_errorLabel);
        _wrapperBox.Controls.Add(_imageBox);

        _timer = new Timer
        {
            Enabled = false,
            Interval = _options.SlideshowInterval
        };

        _timer.Tick += (_, _) =>
        {
            this.GoToNextImage();
        };

        this.Controls.Add(_wrapperBox);
    }

    /// <summary>
    /// Setup form with events.
    /// </summary>
    private void SetupForm()
    {
        var path = _options.File ?? _options.Path;
        var separator = path is not null ? " - " : string.Empty;

        this.BackColor = Color.Black;
        this.Icon = Program.ApplicationIcon;
        this.Location = new(100, 100);
        this.Size = new(Screen.PrimaryScreen!.Bounds.Width - 200, Screen.PrimaryScreen.Bounds.Height - 200);
        this.Text = $"{path}{separator}{Program.Name}";
        this.WindowState = FormWindowState.Maximized;

        _lastWindowState = this.WindowState;

        this.KeyDown += this.FormKeyDownEvent;
        this.Resize += this.FormResizeEvent;
        this.ResizeEnd += this.FormResizeEndEvent;
        this.Shown += this.FormShownEvent;
    }
    
    #endregion
    
    #region From event functions

    /// <summary>
    /// Handles the KeyDown event for all controls on the window.
    /// </summary>
    private void FormKeyDownEvent(object? _, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            // Close window.
            case Keys.Escape:
                if (_timer.Enabled)
                {
                    this.ToggleSlideshow();
                }
                else if (this.FormBorderStyle is FormBorderStyle.None)
                {
                    this.ToggleFullscreen();
                }
                else
                {
                    this.Close();    
                }
                
                break;
            
            // Go to the next image in the folder.
            case Keys.Down or Keys.Right:
                this.GoToNextImage();
                break;
            
            // Go to the previous image in the folder.
            case Keys.Up or Keys.Left:
                this.GoToPreviousImage();
                break;
            
            // Decrease zoom factor on image.
            case Keys.Subtract:
                this.ZoomOut(e.Control);
                break;
            
            // Increase zoom factor on image.
            case Keys.Add:
                this.ZoomIn(e.Control);
                break;
            
            // Go to the first image in the folder.
            case Keys.Home:
                this.GoToFirstImage();
                break;
            
            // Go to the last image in the folder.
            case Keys.End:
                this.GoToLastImage();
                break;
            
            // Toggle fullscreen.
            case Keys.F11:
                this.ToggleFullscreen();
                break;
            
            // Toggle slideshow.
            case Keys.F5:
                this.ToggleSlideshow();
                break;
        }
    }

    /// <summary>
    /// Reposition all controls.
    /// </summary>
    private void FormResizeEvent(object? _, EventArgs e)
    {
        if (this.WindowState == _lastWindowState)
        {
            return;
        }

        _lastWindowState = this.WindowState;

        if (this.WindowState is FormWindowState.Minimized)
        {
            return;
        }

        this.FormResizeEndEvent(null, null!);
    }

    /// <summary>
    /// Reposition all controls.
    /// </summary>
    private void FormResizeEndEvent(object? _, EventArgs e)
    {
        if (_fileIndex is null ||
            _zoomFactor is null ||
            _files[_fileIndex.Value].Bitmap is null)
        {
            return;
        }
        
        var entry = _files[_fileIndex.Value];
        
        var height = (int)(entry.Bitmap!.Height / _zoomFactor.Value);
        var width = (int)(entry.Bitmap!.Width / _zoomFactor.Value);

        if (height < 0 || height > entry.Bitmap!.Height ||
            width < 0 || width > entry.Bitmap!.Width)
        {
            height = entry.Bitmap!.Height;
            width = entry.Bitmap!.Width;
        }

        _imageBox.Visible = false;
        _imageBox.Location = new((_wrapperBox.ClientSize.Width - width) / 2, (_wrapperBox.ClientSize.Height - height) / 2);
        _imageBox.Size = new(width, height);
        _imageBox.Visible = true;
    }
    
    /// <summary>
    /// Show the selected file.
    /// </summary>
    private void FormShownEvent(object? _, EventArgs e)
    {
        this.ShowFile();
    }
    
    #endregion
    
    #region Control event functions

    /// <summary>
    /// Zoom to max.
    /// </summary>
    private void ImageBoxMouseDoubleClickEvent(object? _, MouseEventArgs e)
    {
        if (_zoomFactor is 1)
        {
            this.ZoomOut(true);
        }
        else
        {
            this.ZoomIn(true);
        }
    }

    /// <summary>
    /// Readies the image box for movement.
    /// </summary>
    private void ImageBoxMouseDownEvent(object? _, MouseEventArgs e)
    {
        _imageCursorPosition = Cursor.Position;
        _imageOriginalPosition = _imageBox.Location;
        _imageIsMoving = true;
    }

    /// <summary>
    /// Move the image box.
    /// </summary>
    private void ImageBoxMouseMoveEvent(object? _, MouseEventArgs e)
    {
        if (!_imageIsMoving ||
            _imageCursorPosition is null ||
            _imageOriginalPosition is null)
        {
            return;
        }

        var diffX = Cursor.Position.X - _imageCursorPosition.Value.X;
        var diffY = Cursor.Position.Y - _imageCursorPosition.Value.Y;

        var newX = _imageOriginalPosition.Value.X + diffX;
        var newY = _imageOriginalPosition.Value.Y + diffY;

        _imageBox.Location = new(newX, newY);
    }

    /// <summary>
    /// Finish moving the image box.
    /// </summary>
    private void ImageBoxMouseUpEvent(object? _, MouseEventArgs e)
    {
        _imageCursorPosition = null;
        _imageOriginalPosition = null;
        _imageIsMoving = false;
    }
    
    #endregion
    
    #region Event helper functions

    /// <summary>
    /// Go to the first image in the folder.
    /// </summary>
    private void GoToFirstImage()
    {
        if (_files.Count is 0)
        {
            return;
        }

        _fileIndexToClear = _fileIndex;
        _fileIndex = 0;
        
        this.ShowFile();
    }

    /// <summary>
    /// Go to the last image in the folder.
    /// </summary>
    private void GoToLastImage()
    {
        if (_files.Count is 0)
        {
            return;
        }

        _fileIndexToClear = _fileIndex;
        _fileIndex = _files.Count - 1;
        
        this.ShowFile();
    }
    
    /// <summary>
    /// Go to the next image in the folder.
    /// </summary>
    private void GoToNextImage()
    {
        if (_files.Count is 0)
        {
            return;
        }
        
        _fileIndexToClear = _fileIndex;

        _fileIndex ??= 0;
        _fileIndex++;

        if (_fileIndex == _files.Count)
        {
            _fileIndex = 0;
        }
        
        this.ShowFile();
    }

    /// <summary>
    /// Go to the previous image in the folder.
    /// </summary>
    private void GoToPreviousImage()
    {
        if (_files.Count is 0)
        {
            return;
        }
        
        _fileIndexToClear = _fileIndex;

        _fileIndex ??= 0;
        _fileIndex--;

        if (_fileIndex == -1)
        {
            _fileIndex = _files.Count - 1;
        }

        this.ShowFile();
    }

    /// <summary>
    /// Toggle fullscreen.
    /// </summary>
    private void ToggleFullscreen()
    {
        if (this.FormBorderStyle is FormBorderStyle.None)
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
        }
        else
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen!.Bounds;
            this.WindowState = FormWindowState.Maximized;
        }
        
        if (_fileIndex is null ||
            _files[_fileIndex.Value].Bitmap is null)
        {
            return;
        }
        
        var entry = _files[_fileIndex.Value];
        
        // Calculate display height/width.
        var height = entry.Bitmap!.Height;
        var width = entry.Bitmap!.Width;

        if (height > _wrapperBox.ClientSize.Height ||
            width > _wrapperBox.ClientSize.Width)
        {
            var zoomFactorHeight = (double)height / _wrapperBox.ClientSize.Height;
            var zoomFactorWidth = (double)width / _wrapperBox.ClientSize.Width;

            _zoomFactor = zoomFactorHeight > zoomFactorWidth
                ? zoomFactorHeight
                : zoomFactorWidth;

            _zoomFactorStep = _zoomFactor / 10;

            height = (int)(height / _zoomFactor.Value);
            width = (int)(width / _zoomFactor.Value);
        }
        else
        {
            _zoomFactor = 1;
        }
        
        var percentage = (int)(100D / _zoomFactor);

        // Set image and position correctly.
        _imageBox.Visible = false;
        _imageBox.Location = new((_wrapperBox.ClientSize.Width - width) / 2, (_wrapperBox.ClientSize.Height - height) / 2);
        _imageBox.Size = new(width, height);
        _imageBox.Visible = true;
        
        // Update window title.
        var parts = new List<string>
        {
            entry.Filename,
            $"{percentage}%"
        };

        if (height != entry.Bitmap!.Height ||
            width != entry.Bitmap!.Width)
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height} ({width}x{height})");
        }
        else
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height}");
        }

        parts.Add(Program.Name);

        this.Text = string.Join(" - ", parts);
    }

    /// <summary>
    /// Toggle slideshow.
    /// </summary>
    private void ToggleSlideshow()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _timer.Enabled = false;
        }
        else
        {
            _timer.Enabled = true;
            _timer.Start();
        }
    }
    
    /// <summary>
    /// Increase zoom factor on image.
    /// </summary>
    /// <param name="max">Zoom to max size.</param>
    private void ZoomIn(bool? max)
    {
        if (_fileIndex is null ||
            _files[_fileIndex.Value].Bitmap is null)
        {
            return;
        }
        
        var entry = _files[_fileIndex.Value];
        
        if (max is true)
        {
            _zoomFactor = 1;
        }
        else if (_zoomFactor is not null &&
                 _zoomFactorStep is not null)
        {
            _zoomFactor -= _zoomFactorStep;

            if (_zoomFactor < 1)
            {
                _zoomFactor = 1;
            }
        }

        if (_zoomFactor is null)
        {
            return;
        }
        
        var height = (int)(entry.Bitmap!.Height / _zoomFactor.Value);
        var width = (int)(entry.Bitmap!.Width / _zoomFactor.Value);
        var percentage = (int)(100D / _zoomFactor);

        if (height < 0 || height > entry.Bitmap!.Height ||
            width < 0 || width > entry.Bitmap!.Width)
        {
            height = entry.Bitmap!.Height;
            width = entry.Bitmap!.Width;
            percentage = 100;
        }

        _imageBox.Visible = false;
        _imageBox.Location = new((_wrapperBox.ClientSize.Width - width) / 2, (_wrapperBox.ClientSize.Height - height) / 2);
        _imageBox.Size = new(width, height);
        _imageBox.Visible = true;
        
        var parts = new List<string>
        {
            entry.Filename,
            $"{percentage}%"
        };

        if (height != entry.Bitmap!.Height ||
            width != entry.Bitmap!.Width)
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height} ({width}x{height})");
        }
        else
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height}");
        }

        parts.Add(Program.Name);

        this.Text = string.Join(" - ", parts);
    }

    /// <summary>
    /// Decrease zoom factor on image.
    /// </summary>
    /// <param name="fitToScreen">Whether to fix image to screen or decrease the zoom factor.</param>
    private void ZoomOut(bool? fitToScreen)
    {
        if (_fileIndex is null ||
            _files[_fileIndex.Value].Bitmap is null)
        {
            return;
        }
        
        var entry = _files[_fileIndex.Value];
        
        var zoomFactorHeight = (double)entry.Bitmap!.Height / _wrapperBox.ClientSize.Height;
        var zoomFactorWidth = (double)entry.Bitmap!.Width / _wrapperBox.ClientSize.Width;

        var defaultZoomFactor = zoomFactorHeight > zoomFactorWidth
            ? zoomFactorHeight
            : zoomFactorWidth;

        if (fitToScreen is true)
        {
            _zoomFactor = defaultZoomFactor;
        }
        else if (_zoomFactor is not null &&
                 _zoomFactorStep is not null)
        {
            _zoomFactor += _zoomFactorStep;

            if (_zoomFactor > defaultZoomFactor)
            {
                _zoomFactor = defaultZoomFactor;
            }
        }
        
        if (_zoomFactor is null)
        {
            return;
        }
        
        var height = (int)(entry.Bitmap!.Height / _zoomFactor.Value);
        var width = (int)(entry.Bitmap!.Width / _zoomFactor.Value);
        var percentage = 100D / _zoomFactor;

        if (height < 0 || height > entry.Bitmap!.Height ||
            width < 0 || width > entry.Bitmap!.Width)
        {
            height = entry.Bitmap!.Height;
            width = entry.Bitmap!.Width;
            percentage = 100;
        }

        _imageBox.Visible = false;
        _imageBox.Location = new((_wrapperBox.ClientSize.Width - width) / 2, (_wrapperBox.ClientSize.Height - height) / 2);
        _imageBox.Size = new(width, height);
        _imageBox.Visible = true;
        
        var parts = new List<string>
        {
            entry.Filename,
            $"{percentage:N2}%"
        };

        if (height != entry.Bitmap!.Height ||
            width != entry.Bitmap!.Width)
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height} ({width}x{height})");
        }
        else
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height}");
        }

        parts.Add(Program.Name);

        this.Text = string.Join(" - ", parts);
    }
    
    #endregion
    
    #region Image handling functions

    /// <summary>
    /// Show the selected file.
    /// </summary>
    private void ShowFile()
    {
        if (_fileIndex is null)
        {
            return;
        }

        _zoomFactor = null;
        _zoomFactorStep = null;

        var entry = _files[_fileIndex.Value];

        if (entry.Bitmap is null)
        {
            try
            {
                entry.Bitmap = entry.FullPath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
                    ? _webP.Load(entry.FullPath)
                    : new Bitmap(entry.FullPath);
            }
            catch (Exception ex)
            {
                entry.Error = ex.Message;
            }
        }

        if (entry.Bitmap is null)
        {
            _imageBox.Visible = false;
            
            _errorLabel.Text = $"Error loading {entry.Filename}";
            _errorLabel.Visible = true;

            this.BackColor = Color.Black;
            this.Text = $"{entry.Filename} - {Program.Name}";

            if (_fileIndexToClear is null ||
                _files[_fileIndexToClear.Value].Bitmap is null)
            {
                return;
            }

            _files[_fileIndexToClear.Value].Bitmap!.Dispose();
            _files[_fileIndexToClear.Value].Bitmap = null;
            _fileIndexToClear = null;

            return;
        }

        _errorLabel.Visible = false;

        // Calculate new background color.
        var thumb = (Bitmap)entry.Bitmap!.GetThumbnailImage(1, 1, null, IntPtr.Zero);
        var pixel = thumb.GetPixel(0, 0);

        this.BackColor = Color.FromArgb(pixel.R / 2, pixel.G / 2, pixel.B / 2);

        // Calculate display height/width.
        var height = entry.Bitmap!.Height;
        var width = entry.Bitmap!.Width;

        if (height > _wrapperBox.ClientSize.Height ||
            width > _wrapperBox.ClientSize.Width)
        {
            var zoomFactorHeight = (double)height / _wrapperBox.ClientSize.Height;
            var zoomFactorWidth = (double)width / _wrapperBox.ClientSize.Width;

            _zoomFactor = zoomFactorHeight > zoomFactorWidth
                ? zoomFactorHeight
                : zoomFactorWidth;

            _zoomFactorStep = _zoomFactor / 10;

            height = (int)(height / _zoomFactor.Value);
            width = (int)(width / _zoomFactor.Value);
        }
        else
        {
            _zoomFactor = 1;
        }

        var percentage = (int)(100D / _zoomFactor);

        // Set image and position correctly.
        _imageBox.Visible = false;
        _imageBox.Image = entry.Bitmap!;
        _imageBox.Location = new(
            (_wrapperBox.ClientSize.Width - width) / 2,
            (_wrapperBox.ClientSize.Height - height) / 2);
        _imageBox.Size = new(width, height);
        _imageBox.SizeMode = PictureBoxSizeMode.Zoom;
        _imageBox.Visible = true;

        // Update window title.
        var parts = new List<string>
        {
            entry.Filename,
            $"{percentage}%"
        };

        if (height != entry.Bitmap!.Height ||
            width != entry.Bitmap!.Width)
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height} ({width}x{height})");
        }
        else
        {
            parts.Add($"{entry.Bitmap!.Width}x{entry.Bitmap!.Height}");
        }

        parts.Add(Program.Name);

        this.Text = string.Join(" - ", parts);
        
        // Clear previous image.
        if (_fileIndexToClear is null ||
            _files[_fileIndexToClear.Value].Bitmap is null)
        {
            return;
        }

        _files[_fileIndexToClear.Value].Bitmap!.Dispose();
        _files[_fileIndexToClear.Value].Bitmap = null;
        _fileIndexToClear = null;
    }

    #endregion
}