namespace Transformations;
public partial class Form1 : Form
{
    private readonly Canvas _canvas;
    private readonly Graphics _g;
    private readonly Point _originalAddButtonLocation;
    private readonly Point _originalBorderColorButtonLocation;
    private readonly Point _originalFillColorButtonLocation;
    private Color _borderColor;
    private Color _fillColor;
    private bool _isAddFigureModeActive;
    private bool _isAddCustomFigureModeActive;
    private readonly List<PointF> _customFigurePoints = [];
    private Bitmap? _tempBitmap;
    
    public Form1()
    {
        // Initialize components and set up the drawing environment.
        InitializeComponent();
        var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        _g = Graphics.FromImage(bmp);
        pictureBox1.Image = bmp;
        _canvas = new Canvas();
        _originalAddButtonLocation = addFigureButton.Location;
        _originalBorderColorButtonLocation = borderColorButton.Location;
        _originalFillColorButtonLocation = fillColorButton.Location;

        // Subscribe to the events.
        _canvas.FigureAdded += OnFigureAdded;
        _canvas.FigureRemoved += OnFigureRemoved;
        figuresCheckedListBox.ItemCheck += FiguresCheckedListBox_ItemCheck;
        pictureBox1.MouseMove += PictureBox1_MouseMove;
        pictureBox1.MouseClick += PictureBox1_MouseClick;
        addCustomFigureCheckBox.CheckedChanged += AddCustomFigureCheckBox_CheckedChanged;
        addCustomFigureButton.Click += AddCustomFigureButton_Click;
        resetCustomFigureButton.Click += ResetCustomFigureButton_Click;
        cancelCustomFigureButton.Click += CancelCustomFigureButton_Click;

        // Update the button states.
        UpdateButtonStates();

        // Render the canvas.
        _canvas.Render(_g, pictureBox1);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // Set the default values for the figure properties.
        addFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
        AddCustomFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
        customPivotCheckBox.Checked = false;
        sizeTextBox.Text = @"1";
        positionXTextBox.Text = @"100";
        positionYTextBox.Text = @"100";

        // Select all figures by default.
        selectAllCheckBox.Checked = true;

        // Add the figure types to the combo box.
        figuresComboBox.Items.Add("Square");
        figuresComboBox.Items.Add("Triangle");

        // Select the first figure type by default.
        figuresComboBox.SelectedIndex = 0;
    }
    
    private void AddCustomFigureCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        // Set the boolean field to the state of the addCustomFigureCheckBox.
        _isAddCustomFigureModeActive = addCustomFigureCheckBox.Checked;
        
        // If add figure mode is active, disable the addFigureCheckBox.
        if (_isAddFigureModeActive)
        {
            addFigureCheckBox.Checked = false;
            addFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
        }
        
        // Update the visibility of the controls based on the state of the addCustomFigureCheckBox.
        addCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        resetCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        cancelCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        fillColorCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        borderColorCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        redoCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        undoCustomFigureButton.Visible = _isAddCustomFigureModeActive;
        selectAllCheckBox.Visible = !_isAddCustomFigureModeActive;
        deleteButton.Visible = !_isAddCustomFigureModeActive;
        resetButton.Visible = !_isAddCustomFigureModeActive;
        undoButton.Visible = !_isAddCustomFigureModeActive;
        redoButton.Visible = !_isAddCustomFigureModeActive;
        figuresCheckedListBox.Visible = !_isAddCustomFigureModeActive;

        // Add a semi-transparent black layer to the form.
        BackColor = _isAddCustomFigureModeActive ? Color.FromArgb( 0, 0, 0) :
            // Remove the semi-transparent black layer from the form.
            Color.FromArgb(64, 64, 64);
    }

    private void FiguresCheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // Delay the call to UpdateButtonStates until after the check state has been updated
        BeginInvoke(UpdateButtonStates);
    }
    
    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        // Update the coordinatesLabel text with the current mouse position
        coordinatesLabel.Text = $@"Coordinates | X: {e.X}, Y: {e.Y} |";
    }
    
    private void PictureBox1_MouseClick(object? sender, MouseEventArgs e)
    {
        if (!_isAddCustomFigureModeActive) return;

        // Add the clicked point to the list of points for the new custom figure.
        _customFigurePoints.Add(e.Location);

        // Create a temporary bitmap if it doesn't exist.
        _tempBitmap ??= new Bitmap(pictureBox1.Width, pictureBox1.Height);

        using (var gTemp = Graphics.FromImage(_tempBitmap))
        {
            // If there is more than one point, draw a line from the last point to the current point.
            if (_customFigurePoints.Count > 1)
            {
                var pen = new Pen(_borderColor == Color.Empty ? Color.White : _borderColor);
                gTemp.DrawLine(pen, _customFigurePoints[^2], e.Location);
            }

            // Draw the points on the canvas.
            gTemp.FillEllipse(Brushes.Red, e.X - 2, e.Y - 2, 5, 5);
        }

        // Clear the canvas and re-render the figures.
        _g.Clear(Color.Transparent);
        _canvas.Render(_g, pictureBox1);

        // Draw the temporary bitmap onto the canvas.
        _g.DrawImage(_tempBitmap, 0, 0);

        // If there are 3 or more points, fill the polygon.
        if (_customFigurePoints.Count >= 3)
        {
            var brush = new SolidBrush(_fillColor == Color.Empty ? Color.FromArgb(128, Color.White) : _fillColor);
            _g.FillPolygon(brush, _customFigurePoints.ToArray());
        }

        pictureBox1.Refresh();
    }
    
    private void AddCustomFigureButton_Click(object? sender, EventArgs e)
    {
        if (_customFigurePoints.Count < 3)
        {
            MessageBox.Show(@"A custom figure must have at least 3 points.");
            return;
        }

        // Create a new CustomFigure instance with the points that the user has added.
        var name = _canvas.GenerateUniqueFigureName("Custom");

        var newFigure = new CustomFigure(_customFigurePoints.ToArray(), name)
        {
            // Set the border color and fill color of the new figure.
            BorderColor = _borderColor == Color.Empty ? Color.White : _borderColor,
            FillColor = _fillColor == Color.Empty ? Color.FromArgb(128, Color.White) : _fillColor
        };

        // Create an AddFigureOperation for the new figure.
        var addOperation = new AddFigureOperation(newFigure)
        {
            IsNewOperation = true
        };

        // Execute the operation and push it to the undo stack.
        addOperation.Execute(_canvas);
        _canvas.UndoStack.Push(addOperation);

        // Uncheck the addCustomFigureCheckBox.
        addCustomFigureCheckBox.Checked = false;

        // Clear the temporary bitmap.
        _tempBitmap?.Dispose();
        _tempBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

        // Render the canvas.
        _canvas.Render(_g, pictureBox1);

        // Clear the list of points for the new custom figure.
        _customFigurePoints.Clear();
    }
    
    private void ResetCustomFigureButton_Click(object? sender, EventArgs e)
    {
        // Clear the list of points for the new custom figure.
        _customFigurePoints.Clear();

        // Clear the temporary bitmap.
        _tempBitmap?.Dispose();
        _tempBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

        // Re-render the canvas.
        _canvas.Render(_g, pictureBox1);
    }
    
    private void CancelCustomFigureButton_Click(object? sender, EventArgs e)
    {
        // Clear the list of points for the new custom figure.
        _customFigurePoints.Clear();

        // Set the boolean field to false.
        _isAddCustomFigureModeActive = false;

        // Uncheck the addCustomFigureCheckBox.
        addCustomFigureCheckBox.Checked = false;

        // Clear the temporary bitmap.
        _tempBitmap?.Dispose();
        _tempBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

        // Re-render the canvas.
        _canvas.Render(_g, pictureBox1);
    }

    private void OnFigureAdded(Figure figure)
    {
        AddFigureToCheckList(figure.Name);
    }

    private void OnFigureRemoved(Figure figure)
    {
        figuresCheckedListBox.Items.Remove(figure.Name);
    }

    private void addFigureCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        // Set the boolean field to the state of the addFigureCheckBox.
        _isAddFigureModeActive = addFigureCheckBox.Checked;
        
        // If custom figure mode is active, disable it and update it
        if (_isAddCustomFigureModeActive)
        {
            addCustomFigureCheckBox.Checked = false;
            AddCustomFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
        }
        
        figuresComboBox.Visible = _isAddFigureModeActive;
        sizeLabel.Visible = _isAddFigureModeActive;
        sizeTextBox.Visible = _isAddFigureModeActive;
        positionLabel.Visible = _isAddFigureModeActive;
        positionXTextBox.Visible = _isAddFigureModeActive;
        positionYTextBox.Visible = _isAddFigureModeActive;
        customPivotCheckBox.Visible = _isAddFigureModeActive;
        // Pivot components should be visible only if customPivotCheckBox is checked
        pivotOffsetXTextBox.Visible = _isAddFigureModeActive && customPivotCheckBox.Checked;
        pivotOffsetYTextBox.Visible = _isAddFigureModeActive && customPivotCheckBox.Checked;
        pivotOffsetLabel.Visible = _isAddFigureModeActive && customPivotCheckBox.Checked;
        addFigureButton.Visible = _isAddFigureModeActive;
        borderColorButton.Visible = _isAddFigureModeActive;
        fillColorButton.Visible = _isAddFigureModeActive;

        borderColorButton.Location = borderColorButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = _isAddFigureModeActive ? _originalBorderColorButtonLocation.Y - 65 : _originalBorderColorButtonLocation.Y
        };

        fillColorButton.Location = fillColorButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = _isAddFigureModeActive ? _originalFillColorButtonLocation.Y - 65 : _originalFillColorButtonLocation.Y
        };

        addFigureButton.Location = addFigureButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = _isAddFigureModeActive ? _originalAddButtonLocation.Y - 65 : _originalAddButtonLocation.Y
        };
    }

    private void customPivotCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        var isChecked = customPivotCheckBox.Checked;
        pivotOffsetLabel.Visible = isChecked;
        pivotOffsetXTextBox.Visible = isChecked;
        pivotOffsetYTextBox.Visible = isChecked;

        borderColorButton.Location = borderColorButton.Location with
        {
            Y = isChecked ? _originalBorderColorButtonLocation.Y : _originalBorderColorButtonLocation.Y - 65
        };

        fillColorButton.Location = fillColorButton.Location with
        {
            Y = isChecked ? _originalFillColorButtonLocation.Y : _originalFillColorButtonLocation.Y - 65
        };

        addFigureButton.Location = addFigureButton.Location with
        {
            Y = isChecked ? _originalAddButtonLocation.Y : _originalAddButtonLocation.Y - 65
        };
    }
    
    private void borderColorCustomFigureButton_Click(object sender, EventArgs e)
    {
        if (borderColorDialog.ShowDialog() == DialogResult.OK)
        {
            _borderColor = borderColorDialog.Color;
        }
    }
    
    private void fillColorCustomFigureButton_Click(object sender, EventArgs e)
    {
        if (fillColorDialog.ShowDialog() == DialogResult.OK)
        {
            _fillColor = fillColorDialog.Color;
        }
    }

    private void borderColorButton_Click(object sender, EventArgs e)
    {
        if (borderColorDialog.ShowDialog() == DialogResult.OK)
        {
            _borderColor = borderColorDialog.Color;
        }
    }

    private void fillColorButton_Click(object sender, EventArgs e)
    {
        if (fillColorDialog.ShowDialog() == DialogResult.OK)
        {
            _fillColor = fillColorDialog.Color;
        }
    }

    private void addFigureButton_Click(object sender, EventArgs e)
    {
        // Validate the inputs.
        if (!ValidateFigureInputs(out var size, out var position, out var pivotOffset))
            return;

        // Get the figure type.
        var figureType = figuresComboBox.SelectedItem?.ToString();
        if (figureType == null)
        {
            MessageBox.Show(@"Please select a figure type.");
            return;
        }

        // Get a unique name for the figure.
        var name = _canvas.GenerateUniqueFigureName(figureType);

        // Create and add the figure.
        var newFigure = CreateFigure(figureType, size, position, name, pivotOffset);
        // Check if the border is not empty, if it is, set the default color
        newFigure.BorderColor = _borderColor == Color.Empty ? Color.White : _borderColor;
        newFigure.FillColor = _fillColor == Color.Empty ? Color.FromArgb(128, Color.White) : _fillColor;

        var addOperation = new AddFigureOperation(newFigure)
        {
            IsNewOperation = true
        };

        // Execute the operation and push it to the undo stack.
        addOperation.Execute(_canvas);
        _canvas.UndoStack.Push(addOperation);

        // Render the canvas.
        _canvas.Render(_g, pictureBox1);
        UpdateButtonStates();
    }

    private bool ValidateFigureInputs(out double size, out PointF position, out PointF pivotOffset)
    {
        // Set the default values.
        size = 0;
        position = PointF.Empty;
        pivotOffset = PointF.Empty;

        // Validate size and position inputs.
        if (!double.TryParse(sizeTextBox.Text, out size) ||
            !double.TryParse(positionXTextBox.Text, out var posX) ||
            !double.TryParse(positionYTextBox.Text, out var posY))
        {
            MessageBox.Show(@"Please enter valid values for size, position, and name.");
            return false;
        }

        position = new PointF((float)posX, (float)posY);

        // Calculate pivot offset
        if (customPivotCheckBox.Checked &&
            double.TryParse(pivotOffsetXTextBox.Text, out var pivotXOffset) &&
            double.TryParse(pivotOffsetYTextBox.Text, out var pivotYOffset))
        {
            pivotOffset = new PointF((float)pivotXOffset, (float)pivotYOffset);
        }
        else
        {
            pivotOffset = PointF.Empty; // Default pivot offset
        }

        return true;
    }

    private static Figure CreateFigure(string figureType, double size, PointF position, string name, PointF pivotOffset)
    {
        return figureType switch
        {
            "Square" => new Square(size, position, name, pivotOffset),
            "Triangle" => new Triangle(size, position, name, pivotOffset),
            _ => throw new InvalidOperationException("Unsupported figure type.")
        };
    }

    private void AddFigureToCheckList(string name)
    {
        figuresCheckedListBox.Items.Add(name, true); // Add to CheckedListBox and set as checked
    }

    private void selectAllCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        for (var i = 0; i < figuresCheckedListBox.Items.Count; i++)
        {
            figuresCheckedListBox.SetItemChecked(i, selectAllCheckBox.Checked);
        }
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
        var operations = new List<CanvasOperation>();

        // Create a delete operation for each checked figure
        var index = 0;
        for (; index < figuresCheckedListBox.CheckedItems.Count; index++)
        {
            var item = figuresCheckedListBox.CheckedItems[index];
            // Get the figure name
            var figureName = item?.ToString();
            if (figureName == null) continue;

            // Find the figure with the given name
            var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
            if (figure == null) continue;

            // Create a delete operation and add it to the list
            var operation = new DeleteFigureOperation(figure)
            {
                IsNewOperation = true
            };
            operations.Add(operation);
        }

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the canvas
        _canvas.Render(_g, pictureBox1);
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        undoButton.Enabled = _canvas.CanUndo();
        // Change background color of the button to green if it is enabled
        undoButton.BackColor = undoButton.Enabled ? Color.Green : DefaultBackColor;

        redoButton.Enabled = _canvas.CanRedo();
        // Change background color of the button to green if it is enabled
        redoButton.BackColor = redoButton.Enabled ? Color.Green : DefaultBackColor;
        
        deleteButton.Enabled = figuresCheckedListBox.CheckedItems.Count > 0;
        
        resetButton.Enabled = _canvas.Figures.Count > 0 || _canvas.UndoStack.Count > 0 || _canvas.RedoStack.Count > 0;
    }

    private void UndoButton_Click(object sender, EventArgs e)
    {
        _canvas.Undo();
        _canvas.Render(_g, pictureBox1);
        UpdateButtonStates();
    }

    private void RedoButton_Click(object sender, EventArgs e)
    {
        _canvas.Redo();
        _canvas.Render(_g, pictureBox1);
        UpdateButtonStates();
    }
    
    private void resetButton_Click(object sender, EventArgs e)
    {
        _canvas.Reset();
        figuresCheckedListBox.Items.Clear();
        _canvas.Render(_g, pictureBox1);
        UpdateButtonStates();
    }

    private bool ApplyRotation(string figureName)
    {
        var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
        if (figure == null) return false;

        var angle = 0; // Get the rotation angle
        var operation = new RotateFigureOperation(figure, angle);
        operation.Execute(_canvas);
        _canvas.UndoStack.Push(operation);

        return true;
    }

    private bool ApplyTranslation(string figureName)
    {
        var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
        if (figure == null) return false;

        var dx = 0; // Get the translation distance
        var dy = 0; // Get the translation distance
        var operation = new TranslateFigureOperation(figure, dx, dy);
        operation.Execute(_canvas);
        _canvas.UndoStack.Push(operation);

        return true;
    }

    private bool ApplyScaling(string figureName)
    {
        var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
        if (figure == null) return false;

        var sx = 0; // Get the scaling factor
        var sy = 0; // Get the scaling factor
        var operation = new ScaleFigureOperation(figure, sx, sy);
        operation.Execute(_canvas);
        _canvas.UndoStack.Push(operation);

        return true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Enter:
                addFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Left:
                ApplyTranslationToSelectedFigures(-10, 0);
                break;
            case Keys.Right:
                ApplyTranslationToSelectedFigures(10, 0);
                break;
            case Keys.Up:
                ApplyTranslationToSelectedFigures(0, -10);
                break;
            case Keys.Down:
                ApplyTranslationToSelectedFigures(0, 10);
                break;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void ApplyTranslationToSelectedFigures(int dx, int dy)
    {
        var operations = new List<CanvasOperation>();

        // Create a translation operation for each checked figure
        var index = 0;
        for (; index < figuresCheckedListBox.CheckedItems.Count; index++)
        {
            // Get the figure name
            var item = figuresCheckedListBox.CheckedItems[index];
            var figureName = item?.ToString();
            if (figureName == null) continue;

            // Find the figure with the given name
            var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
            if (figure == null) continue;

            // Create a translation operation and add it to the list
            var operation = new TranslateFigureOperation(figure, dx, dy);
            operations.Add(operation);
        }

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the canvas
        _canvas.Render(_g, pictureBox1);
    }
}
