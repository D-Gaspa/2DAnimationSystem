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

        // Update the button states.
        UpdateButtonStates();

        // Render the canvas.
        _canvas.Render(_g, pictureBox1);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // Set the default values for the figure properties.
        addFigureCheckBox_CheckedChanged(this, EventArgs.Empty);
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
        var isChecked = addFigureCheckBox.Checked;
        figuresComboBox.Visible = isChecked;
        sizeLabel.Visible = isChecked;
        sizeTextBox.Visible = isChecked;
        positionLabel.Visible = isChecked;
        positionXTextBox.Visible = isChecked;
        positionYTextBox.Visible = isChecked;
        customPivotCheckBox.Visible = isChecked;
        // Pivot components should be visible only if customPivotCheckBox is checked
        pivotOffsetXTextBox.Visible = isChecked && customPivotCheckBox.Checked;
        pivotOffsetYTextBox.Visible = isChecked && customPivotCheckBox.Checked;
        pivotOffsetLabel.Visible = isChecked && customPivotCheckBox.Checked;
        addFigureButton.Visible = isChecked;
        borderColorButton.Visible = isChecked;
        fillColorButton.Visible = isChecked;

        borderColorButton.Location = borderColorButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = isChecked ? _originalBorderColorButtonLocation.Y - 65 : _originalBorderColorButtonLocation.Y
        };

        fillColorButton.Location = fillColorButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = isChecked ? _originalFillColorButtonLocation.Y - 65 : _originalFillColorButtonLocation.Y
        };

        addFigureButton.Location = addFigureButton.Location with
        {
            // Change the Y coordinate of the button based on the visibility of the addFigureCheckBox
            Y = isChecked ? _originalAddButtonLocation.Y - 65 : _originalAddButtonLocation.Y
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
        newFigure.FillColor = _fillColor;
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
