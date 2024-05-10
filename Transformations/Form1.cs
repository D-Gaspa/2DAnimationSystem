namespace Transformations;

public partial class Form1 : Form
{
    private readonly Canvas _canvas;
    private readonly Graphics _g;
    private readonly Point _originalAddButtonLocation;
    private readonly Point _originalBorderColorButtonLocation;
    private readonly Point _originalFillColorButtonLocation;
    private readonly Graphics _timeLineGraphics;
    private Color _borderColor;
    private Point _currentMouseLocation;
    private ResizePosition _currentResizePosition;
    private Color _fillColor;
    private Point _initialMouseLocation;
    private bool _isAddCustomFigureModeActive;
    private bool _isAddFigureModeActive;
    private bool _isDragging;
    private bool _isDraggingPivot;
    private bool _isResizing;
    private bool _isRotating;
    private bool _isTranslating;
    private RectangleF _originalBox;
    private List<Figure>? _tempSelectedFigures;
    private TimeLine? _timeLine;
    private UnfinishedCustomFigure _unfinishedCustomFigure = null!;

    public Form1()
    {
        // Initialize components and set up the drawing environment.
        InitializeComponent();
        var bmp = new Bitmap(canvasPictureBox.Width, canvasPictureBox.Height);
        _g = Graphics.FromImage(bmp);
        canvasPictureBox.Image = bmp;

        var timeLineBmp = new Bitmap(timeLinePictureBox.Width, timeLinePictureBox.Height);
        _timeLineGraphics = Graphics.FromImage(timeLineBmp);
        timeLinePictureBox.Image = timeLineBmp;

        _canvas = new Canvas();
        _originalAddButtonLocation = addFigureButton.Location;
        _originalBorderColorButtonLocation = borderColorButton.Location;
        _originalFillColorButtonLocation = fillColorButton.Location;

        // Subscribe to the events.
        SubscribeToEvents();

        // Update the button states.
        UpdateAllButtonStates();

        // Render the canvas.
        Canvas.Render(_g, canvasPictureBox);
    }

    private void SubscribeToEvents()
    {
        _canvas.FigureAdded += OnFigureAdded;
        _canvas.FigureRemoved += OnFigureRemoved;
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

    public void RenderFigures()
    {
        Canvas.Render(_g, canvasPictureBox);
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

        if (_timeLine != null) playTimeLineButton.Visible = !_isAddCustomFigureModeActive;

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
        canvasPictureBox.Cursor = _isAddCustomFigureModeActive ? Cursors.Cross : Cursors.Default;

        // Change the background color based on the state of the addCustomFigureCheckBox.
        BackColor = _isAddCustomFigureModeActive ? Color.Black : Color.FromArgb(10, 10, 20);

        DisableFigureSelection();
        UpdateAllButtonStates();
    }

    private static void SetControlVisibility(List<Control> controls, bool visibility)
    {
        foreach (var control in controls) control.Visible = visibility;
    }

    private void FiguresCheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // Get the figure name
        var figureName = figuresCheckedListBox.Items[e.Index].ToString();
        if (figureName == null) return;

        // Find the figure with the given name
        var figure = _canvas.Figures.FirstOrDefault(f => f.Name == figureName);
        if (figure == null) return;

        // Select or deselect the figure based on the new check state
        figure.IsSelected = e.NewValue == CheckState.Checked;

        // Update the check state
        UpdateSelectAllCheckBox();

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

    private void CanvasPictureBoxMouseMove(object? sender, MouseEventArgs e)
    {
        UpdateCoordinatesLabel(e);

        // Check if a figure is selected
        var selectedFigures = _canvas.Figures.Where(f => f.IsSelected).ToList();
        if (selectedFigures.Count == 0)
        {
            canvasPictureBox.Cursor = Cursors.Default;
            return;
        }

        if (_isDraggingPivot)
        {
            HandlePivotDragging(e);
            return;
        }

        if (_isDragging)
        {
            HandleDragging(e);
            return;
        }

        HandleCursorChange(selectedFigures, e);
    }

    private void UpdateCoordinatesLabel(MouseEventArgs e)
    {
        coordinatesLabel.Text = $@"X: {e.X}, Y: {e.Y}";
    }

    private void HandlePivotDragging(MouseEventArgs e)
    {
        // Create a move pivot operation for each selected figure
        if (_tempSelectedFigures == null) return;

        var operations = _tempSelectedFigures.Where(f => f.IsSelected)
            .Select(figure => new MovePivotOperation(figure, e.Location) { IsNewOperation = false })
            .Cast<CanvasOperation>().ToList();

        // Execute the operation
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);

        // Render the canvas
        Canvas.Render(_g, canvasPictureBox);

        // Render all figures that are not selected (in the background)
        foreach (var figure in _canvas.Figures.Where(f => !f.IsSelected)) figure.Draw(_g);

        // Render the temporary figures
        foreach (var figure in _tempSelectedFigures) figure.Draw(_g);
    }

    private void HandleDragging(MouseEventArgs e)
    {
        if (_isTranslating)
        {
            HandleTranslationDragging(e);
            return;
        }

        if (_isResizing)
        {
            HandleResizeDragging(e);
            return;
        }

        if (_isRotating) HandleRotationDragging(e);
    }

    private void HandleTranslationDragging(MouseEventArgs e)
    {
        // Calculate the translation vector
        var dx = e.X - _currentMouseLocation.X;
        var dy = e.Y - _currentMouseLocation.Y;

        // Translate each temporary figure
        if (_tempSelectedFigures == null) return;
        foreach (var figure in _tempSelectedFigures) figure.Translate(dx, dy);

        // Update the current mouse location
        _currentMouseLocation = e.Location;

        // Render the canvas
        Canvas.Render(_g, canvasPictureBox);

        // Render all figures that are not selected (in the background)
        foreach (var figure in _canvas.Figures.Where(f => !f.IsSelected)) figure.Draw(_g);

        // Render the temporary figures
        foreach (var figure in _tempSelectedFigures) figure.Draw(_g);
    }

    private void HandleResizeDragging(MouseEventArgs e)
    {
        // Create a resize operation for each selected figure
        if (_tempSelectedFigures == null) return;

        var operations = _tempSelectedFigures.Where(f => f.IsSelected)
            .Select(figure => new ResizeFigureOperation(figure, _currentResizePosition, _originalBox, e.Location)
                { IsNewOperation = false })
            .Cast<CanvasOperation>().ToList();

        // Execute the operation
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);

        // Render the canvas
        Canvas.Render(_g, canvasPictureBox);

        // Render all figures that are not selected (in the background)
        foreach (var figure in _canvas.Figures.Where(f => !f.IsSelected)) figure.Draw(_g);

        // Render the temporary figures
        foreach (var figure in _tempSelectedFigures) figure.Draw(_g);
    }

    private void HandleRotationDragging(MouseEventArgs e)
    {
        // Get the center of the figure to be rotated
        var center = _canvas.Figures.First(f => f.IsRotating).GetCenter();

        // Calculate the vector from the center of the figure to the cursor's position
        var dx = e.X - center.X;
        var dy = e.Y - center.Y;

        var angle = GetRotationAngle(dy, dx);

        // Create a rotate operation for each selected figure (clone it first)
        _tempSelectedFigures?.Clear();

        _tempSelectedFigures = _canvas.Figures.Where(f => f.IsSelected).Select(f => f.Clone()).ToList();

        var operations = _tempSelectedFigures
            .Select(figure => new RotateFigureOperation(figure, angle) { IsNewOperation = false })
            .Cast<CanvasOperation>().ToList();

        // Execute the operation
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);

        // Render the canvas
        Canvas.Render(_g, canvasPictureBox);

        // Render all figures that are not selected (in the background)
        foreach (var figure in _canvas.Figures.Where(f => !f.IsSelected)) figure.Draw(_g);

        // Render the temporary figures
        foreach (var figure in _tempSelectedFigures) figure.Draw(_g);
    }

    private static double GetRotationAngle(float dy, float dx)
    {
        // Calculate the angle of this vector relative to the positive x-axis
        var angle = Math.Atan2(dy, dx);

        // Convert this angle to degrees and adjust it to the range [0, 360]
        angle = angle * 180 / Math.PI;
        if (angle < 0)
        {
            angle += 360;
            angle = 360 - angle;
        }

        angle = dx switch
        {
            // Adjust the angle based on the quadrant in which the cursor is located
            > 0 when dy < 0 => 90 - angle,
            > 0 when dy == 0 => 90,
            > 0 when dy > 0 => 90 + angle,
            0 when dy > 0 => 180,
            < 0 when dy > 0 => 90 + angle,
            < 0 when dy == 0 => 270,
            < 0 when dy < 0 => 90 - angle,
            0 when dy < 0 => 0,
            _ => angle
        };
        return angle;
    }

    private void HandleCursorChange(IEnumerable<Figure> selectedFigures, MouseEventArgs e)
    {
        foreach (var figure in selectedFigures)
        {
            // Check if the cursor is near the pivot point
            if (IsCursorAtPivot(figure, e.Location))
            {
                canvasPictureBox.Cursor = Cursors.Hand;
                return;
            }

            // Check if the cursor is inside the rotation icon
            if (IsInsideRotationIcon(figure, e.Location))
            {
                canvasPictureBox.Cursor = Cursors.Hand;
                _isRotating = true;
                figure.IsRotating = true;
                return;
            }

            // Check if the cursor is inside the figure
            if (figure.IsInsideFigure(e.Location))
            {
                canvasPictureBox.Cursor = Cursors.SizeAll;
                return;
            }

            if (IsCursorAtCorner(figure.GetBounds(), e.Location))
            {
                // Change the cursor based on its position relative to the figure's bounding rectangle
                canvasPictureBox.Cursor = GetCursorForCorner(figure.GetBounds(), e.Location);
                return;
            }

            if (IsCursorAtSide(figure.GetBounds(), e.Location))
            {
                // Change the cursor based on its position relative to the figure's bounding rectangle
                canvasPictureBox.Cursor = GetCursorForSide(figure.GetBounds(), e.Location);
                return;
            }

            figure.IsRotating = false;
        }

        _isRotating = false;

        canvasPictureBox.Cursor = Cursors.Default;
    }

    private static bool IsCursorAtPivot(Figure figure, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to the pivot
        const float tolerance = 5;

        // Check if the cursor is near the pivot point
        return Math.Abs(figure.Pivot.X - point.X) <= tolerance && Math.Abs(figure.Pivot.Y - point.Y) <= tolerance;
    }

    private static bool IsInsideRotationIcon(Figure figure, PointF point)
    {
        // Calculate the position of the rotation icon
        var iconPosition =
            new PointF(figure.GetBounds().Left + figure.GetBounds().Width / 2 - (float)figure.RotationIcon.Width / 2,
                figure.GetBounds().Top - figure.RotationIcon.Height - 7);

        // Create a rectangle that represents the bounds of the rotation icon
        var iconBounds = new RectangleF(iconPosition.X, iconPosition.Y, figure.RotationIcon.Width,
            figure.RotationIcon.Height);

        // Check if the point is inside the rectangle
        return iconBounds.Contains(point);
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

        return corners.Any(corner =>
            Math.Abs(corner.X - point.X) <= tolerance && Math.Abs(corner.Y - point.Y) <= tolerance);
    }

    private Cursor GetCursorForCorner(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a corner
        const float tolerance = 10;

        // Check each corner
        if (Math.Abs(bounds.Left - point.X) <= tolerance && Math.Abs(bounds.Top - point.Y) <= tolerance)
        {
            _currentResizePosition = ResizePosition.TopLeft;
            return Cursors.SizeNWSE;
        }

        if (Math.Abs(bounds.Right - point.X) <= tolerance && Math.Abs(bounds.Bottom - point.Y) <= tolerance)
        {
            _currentResizePosition = ResizePosition.BottomRight;
            return Cursors.SizeNWSE;
        }

        if (Math.Abs(bounds.Right - point.X) <= tolerance && Math.Abs(bounds.Top - point.Y) <= tolerance)
        {
            _currentResizePosition = ResizePosition.TopRight;
            return Cursors.SizeNESW;
        }

        if (!(Math.Abs(bounds.Left - point.X) <= tolerance) || !(Math.Abs(bounds.Bottom - point.Y) <= tolerance))
            return Cursors.Default;
        _currentResizePosition = ResizePosition.BottomLeft;
        return Cursors.SizeNESW;
    }

    private static bool IsCursorAtSide(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a side
        const float tolerance = 10;

        // Check each side
        return (Math.Abs(bounds.Left - point.X) <= tolerance && point.Y >= bounds.Top && point.Y <= bounds.Bottom) ||
               (Math.Abs(bounds.Right - point.X) <= tolerance && point.Y >= bounds.Top && point.Y <= bounds.Bottom) ||
               (Math.Abs(bounds.Top - point.Y) <= tolerance && point.X >= bounds.Left && point.X <= bounds.Right) ||
               (Math.Abs(bounds.Bottom - point.Y) <= tolerance && point.X >= bounds.Left && point.X <= bounds.Right);
    }

    private Cursor GetCursorForSide(RectangleF bounds, PointF point)
    {
        // Define a tolerance for how close the cursor needs to be to a side
        const float tolerance = 10;

        // Check each side
        if (Math.Abs(bounds.Left - point.X) <= tolerance)
        {
            _currentResizePosition = ResizePosition.LeftMiddle;
            return Cursors.SizeWE;
        }

        if (Math.Abs(bounds.Right - point.X) <= tolerance)
        {
            _currentResizePosition = ResizePosition.RightMiddle;
            return Cursors.SizeWE;
        }

        if (Math.Abs(bounds.Top - point.Y) <= tolerance)
        {
            _currentResizePosition = ResizePosition.TopMiddle;
            return Cursors.SizeNS;
        }

        if (!(Math.Abs(bounds.Bottom - point.Y) <= tolerance)) return Cursors.Default;
        _currentResizePosition = ResizePosition.BottomMiddle;
        return Cursors.SizeNS;
    }

    private void CanvasPictureBoxClick(object? sender, MouseEventArgs e)
    {
        if (_isAddCustomFigureModeActive)
        {
            HandleCustomFigureCreationOnClick(e);
            return;
        }

        if (_isDraggingPivot)
        {
            HandlePivotDragOnClick(e);
            return;
        }

        if (_isDragging)
        {
            HandleFigureDragOnClick(e);
            return;
        }

        HandleFigureSelection(e);

        // Update the button states
        UpdateButtonStates();
    }

    private void HandleCustomFigureCreationOnClick(MouseEventArgs e)
    {
        if (_canvas.CustomFigurePoints.Count == 0)
        {
            // Create a new UnfinishedCustomFigure and add it to the canvas with the first point
            _unfinishedCustomFigure = new UnfinishedCustomFigure([e.Location], "Custom");
            var addFigureOperation = new AddFigureOperation(_unfinishedCustomFigure);
            addFigureOperation.Execute(_canvas);
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
        canvasPictureBox.Refresh();
    }

    private void HandlePivotDragOnClick(MouseEventArgs e)
    {
        // Create a move pivot operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new MovePivotOperation(figure, e.Location) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the figures
        RenderFigures();

        // Update the button states
        UpdateAllButtonStates();
    }

    private void HandleFigureDragOnClick(MouseEventArgs e)
    {
        if (canvasPictureBox.Cursor == Cursors.SizeAll)
        {
            TranslateSelectedFigures(e);
            return;
        }

        if (canvasPictureBox.Cursor == Cursors.Hand && _isRotating)
        {
            RotateSelectedFigures(e);
            return;
        }

        if (canvasPictureBox.Cursor != Cursors.Default) ResizeSelectedFigures(e);
    }

    private void RotateSelectedFigures(MouseEventArgs e)
    {
        // Get the center of the figure to be rotated
        var center = _canvas.Figures.First(f => f.IsRotating).GetCenter();

        // Calculate the vector from the center of the figure to the cursor's position
        var dx = e.X - center.X;
        var dy = e.Y - center.Y;

        // Calculate the angle of this vector relative to the positive x-axis
        var angle = GetRotationAngle(dy, dx);

        // Create a rotate operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new RotateFigureOperation(figure, angle) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Render the figures
        RenderFigures();

        // Update the button states
        UpdateAllButtonStates();
    }

    private void TranslateSelectedFigures(MouseEventArgs e)
    {
        // Calculate the translation vector
        var dx = e.X - _initialMouseLocation.X;
        var dy = e.Y - _initialMouseLocation.Y;

        // Create a translation operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new TranslateFigureOperation(figure, dx, dy) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Move translated figures to the end of the list
        var translatedFigures = _canvas.Figures.Where(f => f.IsSelected).ToList();
        foreach (var figure in translatedFigures)
        {
            _canvas.Figures.Remove(figure);
            _canvas.Figures.Add(figure);
        }

        // Render the figures
        RenderFigures();

        // Update the button states
        UpdateAllButtonStates();
    }

    private void ResizeSelectedFigures(MouseEventArgs e)
    {
        // Create a resize operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new ResizeFigureOperation(figure, _currentResizePosition, _originalBox, e.Location)
                { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        // Update the initial mouse location
        _initialMouseLocation = e.Location;

        // Render the figures
        RenderFigures();

        // Update the button states
        UpdateAllButtonStates();
    }

    private void HandleFigureSelection(MouseEventArgs e)
    {
        // Check if a figure is clicked
        var clickedFigure = _canvas.Figures.LastOrDefault(f => f.IsInsideFigure(e.Location));
        if (clickedFigure == null)
        {
            // If no figure is clicked, deselect all figures and uncheck all checkboxes
            foreach (var figure in _canvas.Figures) SetFigureSelection(figure, false);
        }
        else
        {
            // If a figure is clicked, check if the Shift or Control key is being held down
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) != 0)
            {
                // If either key is being held down, toggle its selected state
                SetFigureSelection(clickedFigure, !clickedFigure.IsSelected);
            }
            else
            {
                // If neither key is being held down, deselect all other figures before selecting the clicked figure
                foreach (var figure in _canvas.Figures) SetFigureSelection(figure, false);
                SetFigureSelection(clickedFigure, true);
            }
        }

        // Disable the addFigureCheckBox
        addFigureCheckBox.Checked = false;

        // Render the figures
        RenderFigures();
    }

    private void SetFigureSelection(Figure figure, bool isSelected)
    {
        figure.IsSelected = isSelected;

        // Find the corresponding item in FiguresCheckBoxList and update its checked state
        FindCorrectItem(figure.Name, isSelected);

        // Update button visibility
        UpdateButtonVisibilityBasedOnSelection();
    }

    private void FindCorrectItem(string name, bool isSelected)
    {
        var index = figuresCheckedListBox.Items.IndexOf(name);
        if (index != -1) figuresCheckedListBox.SetItemChecked(index, isSelected);
    }

    private void CanvasPictureBoxMouseDown(object? sender, MouseEventArgs e)
    {
        // Check if the cursor is not default or if no figure is selected
        if (canvasPictureBox.Cursor == Cursors.Default || !_canvas.Figures.Any(f => f.IsSelected))
            return;

        // Check if we are dragging the pivot
        if (canvasPictureBox.Cursor == Cursors.Hand && !_isRotating)
        {
            // If a pivot dragging operation is about to be performed, deselect all other figures and select the one that is being operated on
            var figureToOperateOn = _canvas.Figures.FirstOrDefault(f => IsCursorAtPivot(f, e.Location));
            if (figureToOperateOn != null)
            {
                foreach (var figure in _canvas.Figures) figure.IsSelected = false;

                figureToOperateOn.IsSelected = true;
                figureToOperateOn.PreviousResizePosition = _currentResizePosition;
            }

            // Create a temporary list with the selected figure to be operated on
            _tempSelectedFigures = _canvas.Figures.Where(f => f.IsSelected).Select(f => f.Clone()).ToList();

            _isDraggingPivot = true;
            return;
        }

        // Else, set the dragging state to true and update the initial mouse location
        _isDragging = true;
        _initialMouseLocation = e.Location;
        _currentMouseLocation = e.Location;

        // Check if we have a translation or rotation operation
        if (canvasPictureBox.Cursor == Cursors.SizeAll || _isRotating)
        {
            if (!_isRotating) _isTranslating = true;
            // Create a temporary list of selected figures
            _tempSelectedFigures = _canvas.Figures.Where(f => f.IsSelected).Select(f => f.Clone()).ToList();

            return;
        }

        var isResizeCursor = canvasPictureBox.Cursor == Cursors.SizeWE || canvasPictureBox.Cursor == Cursors.SizeNS ||
                             canvasPictureBox.Cursor == Cursors.SizeNWSE || canvasPictureBox.Cursor == Cursors.SizeNESW;

        // Check if we have a resize operation
        if (!isResizeCursor) return;
        {
            // If a resize operation is about to be performed, deselect all other figures and select the figure to be resized where the mouse is clicked.
            var figureToResize = _canvas.Figures.FirstOrDefault(f =>
                IsCursorAtSide(f.GetBounds(), e.Location) || IsCursorAtCorner(f.GetBounds(), e.Location));
            if (figureToResize != null)
            {
                foreach (var figure in _canvas.Figures) figure.IsSelected = false;
                figureToResize.IsSelected = true;
                figureToResize.PreviousResizePosition = _currentResizePosition;
            }

            // Create a temporary list with the selected figure to be resized
            _tempSelectedFigures = _canvas.Figures.Where(f => f.IsSelected).Select(f => f.Clone()).ToList();

            // Get the original bounding box of the figure
            if (figureToResize != null) _originalBox = figureToResize.GetBounds();

            _isResizing = true;
        }
    }

    private void CanvasPictureBoxMouseUp(object? sender, MouseEventArgs e)
    {
        _isDraggingPivot = false;
        _isDragging = false;
        _isTranslating = false;
        _isResizing = false;
        _isRotating = false;

        // If the temporary list of selected figures is null, return
        if (_tempSelectedFigures == null) return;
        _tempSelectedFigures.Clear();
        _tempSelectedFigures = null;

        // Update the button states
        UpdateAllButtonStates();
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
        var name = _canvas.GenerateUniqueFigureName("CustomFigure");

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

        if (_timeLine != null) playTimeLineButton.Visible = true;

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

        // Remove the unfinished custom figure from the canvas
        _canvas.Figures.Remove(_unfinishedCustomFigure);
        _unfinishedCustomFigure = null!;

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

        // Remove the unfinished custom figure from the canvas
        _canvas.Figures.Remove(_unfinishedCustomFigure);
        _unfinishedCustomFigure = null!;

        // Render the figures
        RenderFigures();
    }

    private void OnFigureAdded(Figure figure)
    {
        UpdateCheckedListBox(() =>
        {
            // Deselect all other items in the figuresCheckedListBox
            for (var i = 0; i < figuresCheckedListBox.Items.Count; i++) figuresCheckedListBox.SetItemChecked(i, false);

            // Add the figure to the CheckedListBox and set it as checked
            figuresCheckedListBox.Items.Add(figure.Name, true);
        });

        // Select the figure
        figure.IsSelected = true;

        // Update the button visibility
        UpdateButtonVisibilityBasedOnSelection();
    }

    private void OnFigureRemoved(Figure figure)
    {
        figuresCheckedListBox.Items.Remove(figure.Name);
    }

    private void addFigureCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        // Uncheck the pivot checkbox
        customPivotCheckBox.Checked = false;

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
        SetButtonLocation(borderColorButton,
            borderColorButton.Location with
            {
                Y = _isAddFigureModeActive
                    ? _originalBorderColorButtonLocation.Y - 65
                    : _originalBorderColorButtonLocation.Y
            });
        SetButtonLocation(fillColorButton,
            fillColorButton.Location with
            {
                Y = _isAddFigureModeActive
                    ? _originalFillColorButtonLocation.Y - 65
                    : _originalFillColorButtonLocation.Y
            });
        SetButtonLocation(addFigureButton,
            addFigureButton.Location with
            {
                Y = _isAddFigureModeActive ? _originalAddButtonLocation.Y - 65 : _originalAddButtonLocation.Y
            });

        // Only call DisableFigureSelection when the checkbox is checked
        if (_isAddFigureModeActive) DisableFigureSelection();
    }

    private void DisableFigureSelection()
    {
        // Deselect all figures
        foreach (var figure in _canvas.Figures) figure.IsSelected = false;

        // Uncheck all checkboxes
        for (var i = 0; i < figuresCheckedListBox.Items.Count; i++) figuresCheckedListBox.SetItemChecked(i, false);

        // Update button visibility
        UpdateButtonVisibilityBasedOnSelection();
    }

    private void customPivotCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        var isChecked = customPivotCheckBox.Checked;
        pivotOffsetLabel.Visible = isChecked;
        pivotOffsetXTextBox.Visible = isChecked;
        pivotOffsetYTextBox.Visible = isChecked;

        SetButtonLocation(borderColorButton,
            borderColorButton.Location with
            {
                Y = isChecked ? _originalBorderColorButtonLocation.Y : _originalBorderColorButtonLocation.Y - 65
            });
        SetButtonLocation(fillColorButton,
            fillColorButton.Location with
            {
                Y = isChecked ? _originalFillColorButtonLocation.Y : _originalFillColorButtonLocation.Y - 65
            });
        SetButtonLocation(addFigureButton,
            addFigureButton.Location with
            {
                Y = isChecked ? _originalAddButtonLocation.Y : _originalAddButtonLocation.Y - 65
            });
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
        if (borderColorDialog.ShowDialog() == DialogResult.OK) _borderColor = borderColorDialog.Color;
    }

    private void fillColorButton_Click(object sender, EventArgs e)
    {
        if (fillColorDialog.ShowDialog() == DialogResult.OK) _fillColor = fillColorDialog.Color;
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
            pivotOffset = new PointF((float)pivotXOffset, (float)pivotYOffset);
        else
            pivotOffset = PointF.Empty; // Default pivot offset

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

        // Deselect all figures but the new one
        foreach (var figure in _canvas.Figures) figure.IsSelected = false;
        newFigure.IsSelected = true;
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

    private void UpdateCheckedListBox(Action action)
    {
        // Temporarily unsubscribe the ItemCheck event
        figuresCheckedListBox.ItemCheck -= FiguresCheckedListBox_ItemCheck;

        // Perform the action
        action();

        // Resubscribe the ItemCheck event
        figuresCheckedListBox.ItemCheck += FiguresCheckedListBox_ItemCheck;
    }

    private void UpdateSelectAllCheckBox(Action action)
    {
        // Temporarily unsubscribe the CheckedChanged event
        selectAllCheckBox.CheckedChanged -= SelectAllCheckBox_CheckedChanged;

        // Perform the action
        action();

        // Resubscribe the CheckedChanged event
        selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;
    }

    private void SelectAllCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        // Get the new check state
        var newCheckState = selectAllCheckBox.Checked ? CheckState.Checked : CheckState.Unchecked;

        UpdateCheckedListBox(() =>
        {
            // Update the check state of all items in the figuresCheckedListBox
            for (var i = 0; i < figuresCheckedListBox.Items.Count; i++)
                figuresCheckedListBox.SetItemCheckState(i, newCheckState);
        });

        // Select or deselect all figures based on the new check state
        foreach (var figure in _canvas.Figures) figure.IsSelected = newCheckState == CheckState.Checked;

        // Render the figures
        RenderFigures();

        // Update the button states
        UpdateButtonStates();

        // Update the button visibility
        UpdateButtonVisibilityBasedOnSelection();
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
        UpdateButtonVisibilityBasedOnSelection();

        // Check if all items are deleted and uncheck the selectAllCheckBox
        if (_canvas.Figures.Count == 0) selectAllCheckBox.Checked = false;
    }

    private void UndoButton_Click(object sender, EventArgs e)
    {
        _canvas.Undo();
        UpdateAllButtonStates();
        foreach (var figure in _canvas.Figures) SetFigureSelection(figure, false);
        RenderFigures();

        _timeLine?.Draw();

        selectAllCheckBox.Checked = false;
        UpdateAllButtonStates();
    }

    private void RedoButton_Click(object sender, EventArgs e)
    {
        _canvas.Redo();
        foreach (var figure in _canvas.Figures) SetFigureSelection(figure, false);
        RenderFigures();

        _timeLine?.Draw();


        selectAllCheckBox.Checked = false;
        UpdateAllButtonStates();
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

        _timeLine?.Reset();

        figuresCheckedListBox.Items.Clear();

        // Restore the default border and fill colors
        RestoreDefaultColors();

        // Render the figures
        RenderFigures();
        UpdateButtonStates();
        UpdateButtonVisibilityBasedOnSelection();
    }

    private void fillColorSelectedButton_Click(object sender, EventArgs e)
    {
        if (fillColorDialog.ShowDialog() != DialogResult.OK) return;
        _fillColor = fillColorDialog.Color;
        FillSelectedFigures();
        RenderFigures();
        UpdateButtonStates();
    }

    private void FillSelectedFigures()
    {
        // Create a change fill color operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new ChangeFillColorOperation(figure, _fillColor) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);
    }

    private void borderColorSelectedButton_Click(object sender, EventArgs e)
    {
        if (borderColorDialog.ShowDialog() != DialogResult.OK) return;
        _borderColor = borderColorDialog.Color;
        BorderSelectedFigures();
        RenderFigures();
        UpdateButtonStates();
    }

    private void BorderSelectedFigures()
    {
        // Create a change border color operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure => new ChangeBorderColorOperation(figure, _borderColor) { IsNewOperation = true })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);
    }

    private void DuplicateButton_Click(object sender, EventArgs e)
    {
        // Create a duplicate operation for each selected figure
        var operations = _canvas.Figures.Where(f => f.IsSelected)
            .Select(figure =>
            {
                // Generate a unique name for the duplicated figure
                var duplicatedFigureName = _canvas.GenerateUniqueFigureName(figure.GetType().Name);
                return new DuplicateFigureOperation(figure, duplicatedFigureName) { IsNewOperation = true };
            })
            .Cast<CanvasOperation>().ToList();

        // Execute the batch operation and push it to the undo stack
        var batchOperation = new BatchCanvasOperation(operations);
        batchOperation.Execute(_canvas);
        _canvas.UndoStack.Push(batchOperation);

        UpdateCheckedListBox(() =>
        {
            // For all figures, update the item checked state
            foreach (var figure in _canvas.Figures) FindCorrectItem(figure.Name, figure.IsSelected);
        });

        UpdateSelectAllCheckBox(() =>
        {
            // Update the selectAllCheckBox
            selectAllCheckBox.Checked = false;
        });

        RenderFigures();
        UpdateButtonStates();
        UpdateButtonVisibilityBasedOnSelection();
    }

    private void UpdateButtonVisibilityBasedOnSelection()
    {
        var isAnyFigureSelected = _canvas.Figures.Any(f => f.IsSelected);
        duplicateButton.Visible = isAnyFigureSelected;
        borderColorSelectedButton.Visible = isAnyFigureSelected;
        fillColorSelectedButton.Visible = isAnyFigureSelected;
    }

    public void UpdateAllButtonStates()
    {
        UpdateButtonStates();
        UpdateCustomFigureButtonStates();
        UpdateButtonVisibilityBasedOnSelection();
        UpdateTimeLineButtonStates();
    }

    private void UpdateCustomFigureButtonStates()
    {
        UpdateButtonState(redoCustomFigureButton, _canvas.CanCustomFigureRedo(), Color.Green, DefaultBackColor);
        UpdateButtonState(undoCustomFigureButton, _canvas.CanCustomFigureUndo(), Color.Green, DefaultBackColor);
        UpdateButtonState(addCustomFigureButton, _canvas.CustomFigurePoints.Count >= 3, Color.MidnightBlue,
            DefaultBackColor);
        UpdateButtonState(resetCustomFigureButton, _canvas.CustomFigurePoints.Count > 0, Color.DarkGreen,
            DefaultBackColor);
        UpdateButtonState(borderColorCustomFigureButton, _canvas.CustomFigurePoints.Count > 1, Color.SteelBlue,
            DefaultBackColor);
        UpdateButtonState(fillColorCustomFigureButton, _canvas.CustomFigurePoints.Count > 2, Color.SteelBlue,
            DefaultBackColor);
    }

    public void UpdateTimeLineButtonStates()
    {
        UpdateButtonState(resetTimeLineButton, _timeLine != null && _timeLine.CanReset(), Color.DarkGreen,
            DefaultBackColor);
        UpdateButtonState(playTimeLineButton, _timeLine != null && _timeLine.CanPlay(), Color.MidnightBlue,
            DefaultBackColor);
    }

    public void UpdateButtonStates()
    {
        UpdateButtonState(undoButton, _canvas.CanUndo(), Color.Green, DefaultBackColor);
        UpdateButtonState(redoButton, _canvas.CanRedo(), Color.Green, DefaultBackColor);
        UpdateButtonState(deleteButton, figuresCheckedListBox.CheckedItems.Count > 0, Color.Red, DefaultBackColor);
        UpdateButtonState(resetButton,
            _canvas.Figures.Count > 0 || _canvas.UndoStack.Count > 0 || _canvas.RedoStack.Count > 0, Color.DarkGreen,
            DefaultBackColor);
    }

    private static void UpdateButtonState(Control button, bool condition, Color enabledColor, Color disabledColor)
    {
        button.Enabled = condition;
        button.BackColor = condition ? enabledColor : disabledColor;
    }

    public void DisableButtons()
    {
        DisableButton(undoButton);
        DisableButton(redoButton);
        DisableButton(deleteButton);
        DisableButton(resetButton);
        DisableButton(addCustomFigureButton);
        DisableButton(resetCustomFigureButton);
        DisableButton(undoCustomFigureButton);
        DisableButton(redoCustomFigureButton);
        DisableButton(borderColorCustomFigureButton);
        DisableButton(fillColorCustomFigureButton);
        DisableButton(playTimeLineButton);
        DisableButton(resetTimeLineButton);
    }

    private static void DisableButton(Control button)
    {
        button.Enabled = false;
        button.BackColor = DefaultBackColor;
    }

    private void RestoreDefaultColors()
    {
        _borderColor = Color.White;
        _fillColor = Color.FromArgb(128, Color.White);
    }

    private void customTimeLineDurationCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        var isChecked = customTimeLineDurationCheckBox.Checked;
        customTimeLineDurationTextBox.Visible = isChecked;
        customTimeLineDurationLabel.Visible = isChecked;
    }

    private void addTimeLineButton_Click(object sender, EventArgs e)
    {
        var duration = TimeLine.DefaultDuration;
        if (customTimeLineDurationCheckBox.Checked)
            if (!int.TryParse(customTimeLineDurationTextBox.Text, out duration) || duration < 3)
            {
                MessageBox.Show(
                    @"Invalid custom timeline duration. It should be an integer greater than or equal to 3.");
                return;
            }

        // Set the control visibility
        timeLinePictureBox.Visible = true;
        playTimeLineButton.Visible = true;
        resetTimeLineButton.Visible = true;
        customTimeLineDurationCheckBox.Visible = false;
        customTimeLineDurationLabel.Visible = false;
        customTimeLineDurationTextBox.Visible = false;
        addTimeLineButton.Visible = false;

        // Create a new TimeLine instance
        _timeLine = new TimeLine(timeLinePictureBox, _timeLineGraphics, _canvas, this, duration);

        _canvas.TimeLine = _timeLine;

        // Draw the timeline
        _timeLine.Draw();

        // Update the button states
        UpdateTimeLineButtonStates();
    }

    private void playTimeLineButton_Click(object sender, EventArgs e)
    {
        addFigureCheckBox.Checked = false;
        _isAddFigureModeActive = false;
        _isAddCustomFigureModeActive = false;

        foreach (var figure in _canvas.Figures) figure.IsSelected = false;

        // Uncheck all selected items in the figuresCheckedListBox
        for (var i = 0; i < figuresCheckedListBox.Items.Count; i++) figuresCheckedListBox.SetItemChecked(i, false);

        UpdateButtonVisibilityBasedOnSelection();

        playTimeLineButton.Visible = false;

        _timeLine?.Play(_g, canvasPictureBox);

        playTimeLineButton.Visible = true;
    }

    private void resetTimeLineButton_Click(object sender, EventArgs e)
    {
        _timeLine?.Reset();
        UpdateTimeLineButtonStates();
    }

    private void timeLinePictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        _timeLine?.HandleMouseMove(e);
    }

    private void timeLinePictureBox_Click(object sender, EventArgs e)
    {
        _timeLine?.HandleClick();
    }

    private void timeLinePictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        _timeLine?.HandleMouseDown();
    }

    private void timeLinePictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        _timeLine?.HandleMouseUp();
        UpdateTimeLineButtonStates();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Enter:
                if (_isAddFigureModeActive)
                    addFigureButton_Click(this, EventArgs.Empty);
                else if (_isAddCustomFigureModeActive) AddCustomFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Left:
                if (timeLinePictureBox.Visible) _timeLine?.MoveCursorLeft();
                break;
            case Keys.Right:
                if (timeLinePictureBox.Visible) _timeLine?.MoveCursorRight();
                break;
            case Keys.Up:
                break;
            case Keys.Down:
                break;
            case Keys.Back:
                if (deleteButton.Enabled) DeleteButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Control | Keys.Z:
                if (undoButton.Enabled && !_isAddCustomFigureModeActive) UndoButton_Click(this, EventArgs.Empty);
                if (undoCustomFigureButton.Enabled && _isAddCustomFigureModeActive)
                    UndoCustomFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Control | Keys.Y:
                if (redoButton.Enabled && !_isAddCustomFigureModeActive) RedoButton_Click(this, EventArgs.Empty);
                if (redoCustomFigureButton.Enabled && _isAddCustomFigureModeActive)
                    RedoCustomFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Control | Keys.Shift | Keys.Z:
                if (redoButton.Enabled && !_isAddCustomFigureModeActive) RedoButton_Click(this, EventArgs.Empty);
                if (redoCustomFigureButton.Enabled && _isAddCustomFigureModeActive)
                    RedoCustomFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Control | Keys.A:
                if (!_isAddCustomFigureModeActive)
                {
                    selectAllCheckBox.Checked = true;

                    // Update the button visibility
                    UpdateButtonVisibilityBasedOnSelection();

                    // Update the button states
                    UpdateButtonStates();
                }

                break;
            case Keys.Control | Keys.Shift | Keys.A:
                if (!_isAddCustomFigureModeActive)
                {
                    selectAllCheckBox.Checked = false;

                    // Update the button visibility
                    UpdateButtonVisibilityBasedOnSelection();

                    // Update the button states
                    UpdateButtonStates();
                }

                break;
            case Keys.Control | Keys.R:
                if (resetButton.Enabled && !_isAddCustomFigureModeActive) resetButton_Click(this, EventArgs.Empty);
                if (resetCustomFigureButton.Enabled && _isAddCustomFigureModeActive)
                    ResetCustomFigureButton_Click(this, EventArgs.Empty);
                break;
            case Keys.Control | Keys.D:
                if (duplicateButton.Enabled) DuplicateButton_Click(this, EventArgs.Empty);
                break;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}