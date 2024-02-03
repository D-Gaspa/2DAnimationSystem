namespace Transformations;
public partial class Form1 : Form
{
    private readonly Canvas _canvas;
    private readonly Graphics _g;
    private readonly Point _originalAddButtonLocation;
    private readonly Point _originalBorderColorButtonLocation;
    private readonly Point _originalFillColorButtonLocation;
    private UnfinishedCustomFigure _unfinishedCustomFigure = null!;
    private Color _borderColor;
    private Color _fillColor;
    private bool _isAddFigureModeActive;
    private bool _isAddCustomFigureModeActive;
    private bool _isDragging;
    private Point _initialMousePosition;
    private Point _currentMousePosition;
    private List<Figure>? _tempSelectedFigures;
    
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
        SubscribeToEvents();

        // Update the button states.
        UpdateAllButtonStates();

        // Render the canvas.
        Canvas.Render(_g, pictureBox1);
    }

    private void SubscribeToEvents()
    {
        _canvas.FigureAdded += OnFigureAdded;
        _canvas.FigureRemoved += OnFigureRemoved;
        figuresCheckedListBox.ItemCheck += FiguresCheckedListBox_ItemCheck;
        pictureBox1.MouseMove += PictureBox1_MouseMove;
        pictureBox1.MouseClick += PictureBox1_MouseClick;
        pictureBox1.MouseUp += PictureBox1_MouseUp;
        pictureBox1.MouseDown += PictureBox1_MouseDown;
        addCustomFigureCheckBox.CheckedChanged += AddCustomFigureCheckBox_CheckedChanged;
        addCustomFigureButton.Click += AddCustomFigureButton_Click;
        resetCustomFigureButton.Click += ResetCustomFigureButton_Click;
        cancelCustomFigureButton.Click += CancelCustomFigureButton_Click;
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
        selectAllCheckBox.Checked = false;

        // Add the figure types to the combo box.
        figuresComboBox.Items.Add("Square");
        figuresComboBox.Items.Add("Triangle");

        // Select the first figure type by default.
        figuresComboBox.SelectedIndex = 0;
    }
    
    private void RenderFigures()
    {
        Canvas.Render(_g, pictureBox1);
        _canvas.RenderFigures(_g);
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
        var invisibleControls = new List<Control>
        {
            addCustomFigureButton, resetCustomFigureButton, cancelCustomFigureButton, fillColorCustomFigureButton,
            borderColorCustomFigureButton, redoCustomFigureButton, undoCustomFigureButton
        };
        var visibleControls = new List<Control>
        {
            selectAllCheckBox, deleteButton, resetButton, undoButton, redoButton, figuresCheckedListBox
        };
        SetControlVisibility(invisibleControls, _isAddCustomFigureModeActive);
        SetControlVisibility(visibleControls, !_isAddCustomFigureModeActive);
        
        // Change the cursor based on the state of the addCustomFigureCheckBox.
        pictureBox1.Cursor = _isAddCustomFigureModeActive ? Cursors.Cross : Cursors.Default;

        // Change the background color based on the state of the addCustomFigureCheckBox.
        BackColor = _isAddCustomFigureModeActive ? Color.Black : Color.FromArgb(64, 64, 64);
        
        DisableFigureSelection();
        UpdateAllButtonStates();
    }
    
    private static void SetControlVisibility(List<Control> controls, bool visibility)
    {
        foreach (var control in controls)
        {
            control.Visible = visibility;
        }
    }

    private void FiguresCheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // Delay the call to UpdateButtonStates until after the check state has been updated
        BeginInvoke(UpdateButtonStates);

        // Delay the call to UpdateSelectAllCheckBox until after the check state has been updated
        BeginInvoke(UpdateSelectAllCheckBox);
        
        // Disable the addFigureCheckBox
        addFigureCheckBox.Checked = false;

        // Get the figure name
        var figureName = figuresCheckedListBox.Items[e.Index].ToString();
        if (figureName == null) return;

        // Find the figure with the given name
        var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
        if (figure == null) return;

        // Select or deselect the figure based on the new check state
        figure.IsSelected = e.NewValue == CheckState.Checked;

        // Render the canvas
        RenderFigures();
    }
    
    private void UpdateSelectAllCheckBox()
    {
        // Check if all figures are selected
        var allChecked = _canvas.Figures.All(f => f.IsSelected);

        // Update the Checked property of the selectAllCheckBox
        selectAllCheckBox.Checked = allChecked;
    }
    
    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        // Update the coordinatesLabel text with the current mouse position
        coordinatesLabel.Text = $@"Coordinates | X: {e.X}, Y: {e.Y} |";

        // Check if a figure is selected
        var selectedFigures = _canvas.Figures.Where(f => f.IsSelected).ToList();
        if (selectedFigures.Count == 0)
        {
            pictureBox1.Cursor = Cursors.Default;
            return;
        }
        
        if (_isDragging)
        {
            // Calculate the translation vector
            var dx = e.X - _currentMousePosition.X;
            var dy = e.Y - _currentMousePosition.Y;

            // Translate each temporary figure
            if (_tempSelectedFigures == null) return;
            foreach (var figure in _tempSelectedFigures)
            {
                figure.Translate(dx, dy);
            }

            // Update the current mouse position
            _currentMousePosition = e.Location;

            // Render the canvas
            Canvas.Render(_g, pictureBox1);
            
            // Render the temporary figures
            foreach (var figure in _tempSelectedFigures)
            {
                figure.Draw(_g);
            }
            
            // Render all figures that are not selected (in the background)
            foreach (var figure in _canvas.Figures.Where(f => !f.IsSelected))
            {
                figure.Draw(_g);
            }

            return;
        }

        foreach (var bounds in selectedFigures.Select(selectedFigure => selectedFigure.GetBounds()))
        {
            // Check if the cursor is inside the selection rectangle
            if (bounds.Contains(e.Location))
            {
                pictureBox1.Cursor = Cursors.SizeAll;
                return;
            }

            if (IsCursorAtCorner(bounds, e.Location))
            {
                // Change the cursor based on its position relative to the figure's bounding rectangle
                pictureBox1.Cursor = GetCursorForCorner(bounds, e.Location);
                return;
            }

            if (!IsCursorAtSide(bounds, e.Location)) continue;
            // Change the cursor based on its position relative to the figure's bounding rectangle
            pictureBox1.Cursor = GetCursorForSide(bounds, e.Location);
            return;
        }

        pictureBox1.Cursor = Cursors.Default;
    }
    
    private static bool IsCursorAtCorner(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a corner
        const float tolerance = 10;

        // Check each corner
        var corners = new[]
        {
            new PointF(bounds.Left, bounds.Top),
            new PointF(bounds.Right, bounds.Top),
            new PointF(bounds.Left, bounds.Bottom),
            new PointF(bounds.Right, bounds.Bottom)
        };

        return corners.Any(corner => Math.Abs(corner.X - point.X) <= tolerance && Math.Abs(corner.Y - point.Y) <= tolerance);
    }

    private static Cursor GetCursorForCorner(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a corner
        const float tolerance = 10;

        // Check each corner
        if (Math.Abs(bounds.Left - point.X) <= tolerance && Math.Abs(bounds.Top - point.Y) <= tolerance ||
            Math.Abs(bounds.Right - point.X) <= tolerance && Math.Abs(bounds.Bottom - point.Y) <= tolerance)
        {
            return Cursors.SizeNWSE;
        }

        if (Math.Abs(bounds.Right - point.X) <= tolerance && Math.Abs(bounds.Top - point.Y) <= tolerance ||
            Math.Abs(bounds.Left - point.X) <= tolerance && Math.Abs(bounds.Bottom - point.Y) <= tolerance)
        {
            return Cursors.SizeNESW;
        }

        return Cursors.Default;
    }

    private static bool IsCursorAtSide(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a side
        const float tolerance = 10;

        // Check each side
        return Math.Abs(bounds.Left - point.X) <= tolerance || Math.Abs(bounds.Right - point.X) <= tolerance ||
               Math.Abs(bounds.Top - point.Y) <= tolerance || Math.Abs(bounds.Bottom - point.Y) <= tolerance;
    }

    private static Cursor GetCursorForSide(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a side
        const float tolerance = 10;

        // Check each side
        if (Math.Abs(bounds.Left - point.X) <= tolerance || Math.Abs(bounds.Right - point.X) <= tolerance)
        {
            return Cursors.SizeWE;
        }

        if (Math.Abs(bounds.Top - point.Y) <= tolerance || Math.Abs(bounds.Bottom - point.Y) <= tolerance)
        {
            return Cursors.SizeNS;
        }

        return Cursors.Default;
    }
    
    private void PictureBox1_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_isAddCustomFigureModeActive)
        {
            HandleCustomFigureCreationOnClick(e);
            return;
        }
        if (_isDragging)
        {
            HandleFigureDraggingOnClick(e);
            return;
        }
        HandleFigureSelection(e);
    }
    
    private void HandleCustomFigureCreationOnClick(MouseEventArgs e)
    {
        if (_canvas.CustomFigurePoints.Count == 0)
        {
            // Create a new UnfinishedCustomFigure and add it to the canvas with the first point
            _unfinishedCustomFigure = new UnfinishedCustomFigure([e.Location], "Custom");
            _canvas.AddFigure(_unfinishedCustomFigure);
        }

        // Add the point to the unfinishedCustomFigure
        var addPointOperation = new AddUnfinishedCustomFigurePointOperation(_unfinishedCustomFigure, e.Location)
        {
            IsNewOperation = true
        };
        addPointOperation.Execute(_canvas);
        _canvas.CustomFigureUndoStack.Push(addPointOperation);

        // Redraw the custom figure.
        RedrawCustomFigure();
    }
    
    private void RedrawCustomFigure()
    {
        // If the addCustomFigureCheckBox is not checked return.
        if (!_isAddCustomFigureModeActive) return;

        // Clear the canvas and re-render the figures.
        _g.Clear(Color.Transparent);
        
        // Render the figures
        RenderFigures();

        // Update the button states and refresh the picture box.
        UpdateCustomFigureButtonStates();
        pictureBox1.Refresh();
    }
    
    private void HandleFigureDraggingOnClick(MouseEventArgs e)
    {
        if (pictureBox1.Cursor == Cursors.SizeAll)
        {
            TranslateSelectedFigures(e);
        }
        
    }
    
    private void TranslateSelectedFigures(MouseEventArgs e)
    {
        // Calculate the translation vector
        var dx = e.X - _initialMousePosition.X;
        var dy = e.Y - _initialMousePosition.Y;

        // Create a translation operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new TranslateFigureOperation(figure, dx, dy) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Update the initial mouse position
        _initialMousePosition = e.Location;
        
        // Render the figures
        RenderFigures();
    }
    
    private void HandleFigureSelection(MouseEventArgs e)
    {
        // Check if a figure is clicked
        var clickedFigure = _canvas.Figures.FirstOrDefault(f => f.IsInsideFigure(e.Location));
        if (clickedFigure == null)
        {
            // If no figure is clicked, deselect all figures and uncheck all checkboxes
            foreach (var figure in _canvas.Figures)
            {
                SetFigureSelection(figure, false);
            }
        }
        else
        {
            // If a figure is clicked, toggle its selected state
            SetFigureSelection(clickedFigure, !clickedFigure.IsSelected);
        }

        // Disable the addFigureCheckBox
        addFigureCheckBox.Checked = false;

        // Render the figures
        RenderFigures();
    }
    
    private void SetFigureSelection(Figure figure, bool isSelected)
    {
        figure.IsSelected = isSelected;

        // Find the corresponding checkbox and update its checked state
        var index = figuresCheckedListBox.Items.IndexOf(figure.Name);
        if (index != -1)
        {
            figuresCheckedListBox.SetItemChecked(index, isSelected);
        }
    }
    
    private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (pictureBox1.Cursor != Cursors.SizeAll || !_canvas.Figures.Any(f => f.IsSelected)) 
            return;
        _isDragging = true;
        _initialMousePosition = e.Location;
        _currentMousePosition = e.Location;
        
        // Create a temporary list of selected figures
        _tempSelectedFigures = _canvas.Figures.Where(f => f.IsSelected).Select(f => f.Clone()).ToList();
    }
    
    private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
        
        // If the temporary list of selected figures is null, return
        if (_tempSelectedFigures == null) return;
        _tempSelectedFigures.Clear();
        _tempSelectedFigures = null;
    }
    
    private void AddCustomFigureButton_Click(object? sender, EventArgs e)
    {
        if (_canvas.CustomFigurePoints.Count < 3)
        {
            MessageBox.Show(@"A custom figure must have at least 3 points.");
            return;
        }
        
        // Remove the unfinished custom figure from the canvas
        _canvas.Figures.Remove(_unfinishedCustomFigure);

        // Create a new CustomFigure instance with the points that the user has added
        var name = _canvas.GenerateUniqueFigureName("Custom");

        var newFigure = new CustomFigure(_canvas.CustomFigurePoints.ToArray(), name)
        {
            // Set the border color and fill color of the new figure
            BorderColor = _borderColor == Color.Empty ? Color.White : _borderColor,
            FillColor = _fillColor == Color.Empty ? Color.FromArgb(128, Color.White) : _fillColor
        };

        // Create an AddFigureOperation for the new figure
        var addOperation = new AddFigureOperation(newFigure)
        {
            IsNewOperation = true
        };

        // Execute the operation and push it to the undo stack
        addOperation.Execute(_canvas);
        _canvas.UndoStack.Push(addOperation);

        // Uncheck the addCustomFigureCheckBox.
        addCustomFigureCheckBox.Checked = false;

        // Clear the custom figure stacks
        _canvas.CustomFigureUndoStack.Clear();
        _canvas.CustomFigureRedoStack.Clear();
        
        _unfinishedCustomFigure = null!;
        
        // Render the figures
        RenderFigures();

        // Clear the list of points for the new custom figure
        _canvas.CustomFigurePoints.Clear();
    }
    
    private void ResetCustomFigureButton_Click(object? sender, EventArgs e)
    {
        // Clear the list of points for the new custom figure
        _canvas.CustomFigurePoints.Clear();

        // Clear custom figure stacks
        _canvas.CustomFigureUndoStack.Clear();
        _canvas.CustomFigureRedoStack.Clear();
        
        _unfinishedCustomFigure = null!;
        
        _canvas.Reset();
        
        // Update the button states
        UpdateCustomFigureButtonStates();
        
        // Render the figures
        RenderFigures();
    }
    
    private void CancelCustomFigureButton_Click(object? sender, EventArgs e)
    {
        // Clear the list of points for the new custom figure
        _canvas.CustomFigurePoints.Clear();

        // Set the boolean field to false
        _isAddCustomFigureModeActive = false;

        // Uncheck the addCustomFigureCheckBox
        addCustomFigureCheckBox.Checked = false;

        // Clear the custom figure stacks.
        _canvas.CustomFigureUndoStack.Clear();
        _canvas.CustomFigureRedoStack.Clear();
        
        _unfinishedCustomFigure = null!;
        _canvas.Reset();

        // Render the figures
        RenderFigures();
    }

    private void OnFigureAdded(Figure figure)
    {
        AddFigureToCheckedListBox(figure.Name);
    }

    private void OnFigureRemoved(Figure figure)
    {
        figuresCheckedListBox.Items.Remove(figure.Name);
    }

    private void addFigureCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        // Set the boolean field to the state of the addFigureCheckBox
        _isAddFigureModeActive = addFigureCheckBox.Checked;

        // If custom figure mode is active, disable it and update it
        if (_isAddCustomFigureModeActive)
        {
            addCustomFigureCheckBox.Checked = false;
            AddCustomFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
        }
        
        // Update the visibility of the controls based on the state of the addFigureCheckBox
        var controls = new List<Control>
        {
            figuresComboBox, sizeLabel, sizeTextBox, positionLabel, positionXTextBox, positionYTextBox, 
            customPivotCheckBox, addFigureButton, borderColorButton, fillColorButton
        };
        SetControlVisibility(controls, _isAddFigureModeActive);
        
        var isPivotAvailable = _isAddFigureModeActive && customPivotCheckBox.Checked;
        var pivotControls = new List<Control>
        {
            pivotOffsetLabel, pivotOffsetXTextBox, pivotOffsetYTextBox
        };
        SetControlVisibility(pivotControls, isPivotAvailable);

        // Update the location of the controls based on the state of the addFigureCheckBox
        SetButtonLocation(borderColorButton, borderColorButton.Location with { Y = _isAddFigureModeActive ? _originalBorderColorButtonLocation.Y - 65 : _originalBorderColorButtonLocation.Y });
        SetButtonLocation(fillColorButton, fillColorButton.Location with { Y = _isAddFigureModeActive ? _originalFillColorButtonLocation.Y - 65 : _originalFillColorButtonLocation.Y });
        SetButtonLocation(addFigureButton, addFigureButton.Location with { Y = _isAddFigureModeActive ? _originalAddButtonLocation.Y - 65 : _originalAddButtonLocation.Y });

        // Only call DisableFigureSelection when the checkbox is checked
        if (_isAddFigureModeActive)
        {
            DisableFigureSelection();
        }
    }

    private void DisableFigureSelection()
    {
        // Deselect all figures
        foreach (var figure in _canvas.Figures)
        {
            figure.IsSelected = false;
        }

        // Uncheck all checkboxes
        for (var i = 0; i < figuresCheckedListBox.Items.Count; i++)
        {
            figuresCheckedListBox.SetItemChecked(i, false);
        }

        // Render the canvas
        RenderFigures();
    }

    private void customPivotCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        var isChecked = customPivotCheckBox.Checked;
        pivotOffsetLabel.Visible = isChecked;
        pivotOffsetXTextBox.Visible = isChecked;
        pivotOffsetYTextBox.Visible = isChecked;

        SetButtonLocation(borderColorButton, borderColorButton.Location with { Y = isChecked ? _originalBorderColorButtonLocation.Y : _originalBorderColorButtonLocation.Y - 65 });
        SetButtonLocation(fillColorButton, fillColorButton.Location with { Y = isChecked ? _originalFillColorButtonLocation.Y : _originalFillColorButtonLocation.Y - 65 });
        SetButtonLocation(addFigureButton, addFigureButton.Location with { Y = isChecked ? _originalAddButtonLocation.Y : _originalAddButtonLocation.Y - 65 });
    }
    
    private static void SetButtonLocation(Control button, Point location)
    {
        button.Location = location;
    }
    
    private void borderColorCustomFigureButton_Click(object sender, EventArgs e)
    {
        if (borderColorDialog.ShowDialog() != DialogResult.OK) return;
        _borderColor = borderColorDialog.Color;
        
        // Create a ChangeBorderColorOperation
        var operation = new ChangeBorderColorOperation(_unfinishedCustomFigure, _borderColor)
        {
            IsNewOperation = true
        };
        operation.Execute(_canvas);
        _canvas.CustomFigureUndoStack.Push(operation);
        
        RedrawCustomFigure();
    }

    private void fillColorCustomFigureButton_Click(object sender, EventArgs e)
    {
        if (fillColorDialog.ShowDialog() != DialogResult.OK) return;
        _fillColor = fillColorDialog.Color;
        
        // Create a ChangeFillColorOperation
        var operation = new ChangeFillColorOperation(_unfinishedCustomFigure, _fillColor)
        {
            IsNewOperation = true
        };
        operation.Execute(_canvas);
        _canvas.CustomFigureUndoStack.Push(operation);
        
        RedrawCustomFigure();
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
            // Create a ChangeFillColorOperation
        }
    }

    private void addFigureButton_Click(object sender, EventArgs e)
    {
        // Validate the inputs
        if (!ValidateFigureInputs(out var size, out var position, out var pivotOffset))
            return;

        // Get the figure type
        var figureType = figuresComboBox.SelectedItem?.ToString();
        if (figureType == null)
        {
            MessageBox.Show(@"Please select a figure type.");
            return;
        }

        // Get a unique name for the figure
        var name = _canvas.GenerateUniqueFigureName(figureType);

        // Create and add the figure
        CreateAndAddFigure(figureType, size, position, name, pivotOffset);

        // Update the button states and render the canvas
        UpdateButtonStates();
        RenderFigures();
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
    
    private void CreateAndAddFigure(string figureType, double size, PointF position, string name, PointF pivotOffset)
    {
        // Create the figure
        var newFigure = CreateFigure(figureType, size, position, name, pivotOffset);

        // Set the border and fill colors
        newFigure.BorderColor = _borderColor == Color.Empty ? Color.White : _borderColor;
        newFigure.FillColor = _fillColor == Color.Empty ? Color.FromArgb(128, Color.White) : _fillColor;

        // Create an AddFigureOperation for the new figure
        var addOperation = new AddFigureOperation(newFigure)
        {
            IsNewOperation = true
        };

        // Execute the operation and push it to the undo stack
        addOperation.Execute(_canvas);
        _canvas.UndoStack.Push(addOperation);
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

    private void AddFigureToCheckedListBox(string name)
    {
        figuresCheckedListBox.Items.Add(name, false); // Add to CheckedListBox and set as unchecked
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
        // Create a delete operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new DeleteFigureOperation(figure) { IsNewOperation = true }).Cast<CanvasOperation>()
            .ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the canvas
        RenderFigures();
        UpdateButtonStates();

        // Check if all items are deleted and uncheck the selectAllCheckBox
        if (_canvas.Figures.Count == 0)
        {
            selectAllCheckBox.Checked = false;
        }
    }
    
    private void UpdateAllButtonStates()
    {
        UpdateButtonStates();
        UpdateCustomFigureButtonStates();
    }
    
    private void UpdateCustomFigureButtonStates()
    {
        UpdateButtonState(redoCustomFigureButton, _canvas.CanCustomFigureRedo(), Color.Green, DefaultBackColor);
        UpdateButtonState(undoCustomFigureButton, _canvas.CanCustomFigureUndo(), Color.Green, DefaultBackColor);
        UpdateButtonState(addCustomFigureButton, _canvas.CustomFigurePoints.Count >= 3, Color.MidnightBlue, DefaultBackColor);
        UpdateButtonState(resetCustomFigureButton, _canvas.CustomFigurePoints.Count > 0, Color.DarkGreen, DefaultBackColor);
    }

    private void UpdateButtonStates()
    {
        UpdateButtonState(undoButton, _canvas.CanUndo(), Color.Green, DefaultBackColor);
        UpdateButtonState(redoButton, _canvas.CanRedo(), Color.Green, DefaultBackColor);
        UpdateButtonState(deleteButton, figuresCheckedListBox.CheckedItems.Count > 0, Color.Red, DefaultBackColor);
        UpdateButtonState(resetButton, _canvas.Figures.Count > 0 || _canvas.UndoStack.Count > 0 || _canvas.RedoStack.Count > 0, Color.DarkGreen, DefaultBackColor);
    }
    
    private static void UpdateButtonState(Control button, bool condition, Color enabledColor, Color disabledColor)
    {
        button.Enabled = condition;
        button.BackColor = condition ? enabledColor : disabledColor;
    }

    private void UndoButton_Click(object sender, EventArgs e)
    {
        _canvas.Undo();
        UpdateAllButtonStates();
        foreach (var figure in _canvas.Figures)
        {
            figure.IsSelected = false;
        }
        RenderFigures();
    }

    private void RedoButton_Click(object sender, EventArgs e)
    {
        _canvas.Redo();
        UpdateAllButtonStates();
        foreach (var figure in _canvas.Figures)
        {
            figure.IsSelected = false;
        }
        RenderFigures();
    }
    
    private void UndoCustomFigureButton_Click(object sender, EventArgs e)
    {
        _canvas.Undo(true);
        RedrawCustomFigure();
        RenderFigures();
    }

    private void RedoCustomFigureButton_Click(object sender, EventArgs e)
    {
        _canvas.Redo(true);
        RedrawCustomFigure();
    }
    
    private void resetButton_Click(object sender, EventArgs e)
    {
        _canvas.Reset();
        figuresCheckedListBox.Items.Clear();
        // Render the figures
        RenderFigures();
        UpdateButtonStates();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Enter:
                if (_isAddFigureModeActive)
                {
                    addFigureButton_Click(this, EventArgs.Empty);
                }
                else if (_isAddCustomFigureModeActive)
                {
                    AddCustomFigureButton_Click(this, EventArgs.Empty);
                }
                break;
            case Keys.Left:
                TranslateSelectedFigures(-10, 0);
                break;
            case Keys.Right:
                TranslateSelectedFigures(10, 0);
                break;
            case Keys.Up:
                TranslateSelectedFigures(0, -10);
                break;
            case Keys.Down:
                TranslateSelectedFigures(0, 10);
                break;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void TranslateSelectedFigures(int dx, int dy)
    {
        // Create a translation operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new TranslateFigureOperation(figure, dx, dy) {IsNewOperation = true}).Cast<CanvasOperation>().ToList();
        
        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the figures
        RenderFigures();
    }
}
