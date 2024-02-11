namespace Transformations;
internal sealed class Canvas
{
    // TODO: If enough time, add selection box when dragging the mouse and when the mouse is released, select all figures inside the box.
    // TODO: Rotation needs to be added.
    
    public readonly List<Figure> Figures = [];
    public readonly List<PointF> CustomFigurePoints = [];
    public readonly Stack<CanvasOperation> UndoStack = new();
    public readonly Stack<CanvasOperation> RedoStack = new();
    public readonly Stack<CanvasOperation> CustomFigureUndoStack = new();
    public readonly Stack<CanvasOperation> CustomFigureRedoStack = new();
    
    public bool CanUndo() => UndoStack.Count > 0;
    public bool CanRedo() => RedoStack.Count > 0;
    public bool CanCustomFigureUndo() => CustomFigureUndoStack.Count > 0;
    public bool CanCustomFigureRedo() => CustomFigureRedoStack.Count > 0;
    
    private int _squareCounter;
    private int _triangleCounter;
    private int _customFigureCounter;
    
    public event Action<Figure>? FigureAdded;
    public event Action<Figure>? FigureRemoved;
    
    public string GenerateUniqueFigureName(string figureType)
    {
        return figureType switch
        {
            "Square" => $"Square {_squareCounter++}",
            "Triangle" => $"Triangle {_triangleCounter++}",
            "CustomFigure" => $"Custom {_customFigureCounter++}",
            _ => throw new InvalidOperationException("Unsupported figure type.")
        };
    }
    
    public void OnFigureAdded(Figure obj)
    {
        FigureAdded?.Invoke(obj);
    }
    
    public void OnFigureRemoved(Figure obj)
    {
        FigureRemoved?.Invoke(obj);
    }
    
    public void Undo(bool isCustomFigureOperation = false)
    {
        var stack = isCustomFigureOperation ? CustomFigureUndoStack : UndoStack;
        var redoStack = isCustomFigureOperation ? CustomFigureRedoStack : RedoStack;

        if (!stack.TryPop(out var operation)) return;

        operation.IsNewOperation = false; // Mark the operation as not new
        
        operation.Undo(this); // Perform the undo operation

        redoStack.Push(operation); // Push the operation to the redo stack
    }

    public void Redo(bool isCustomFigureOperation = false)
    {
        var stack = isCustomFigureOperation ? CustomFigureRedoStack : RedoStack;
        var undoStack = isCustomFigureOperation ? CustomFigureUndoStack : UndoStack;

        if (!stack.TryPop(out var operation)) return;

        operation.IsNewOperation = false; // Mark the operation as not new

        operation.Execute(this); // Perform the redo operation

        undoStack.Push(operation); // Push the operation to the undo stack
    }
    
    public void Reset()
    {
        Figures.Clear();
        _squareCounter = 0;
        _triangleCounter = 0;
        UndoStack.Clear();
        RedoStack.Clear();
    }
    
    public static void Render(Graphics g, PictureBox pictureBox)
    {
        // Clear the canvas
        g.Clear(Color.Black);
        
        // Invalidate the picture box to redraw it
        pictureBox.Invalidate();
    }
    
    public void RenderFigures(Graphics g)
    {
        foreach (var figure in Figures)
        {
            figure.Draw(g);
        }
    }
}
