namespace Transformations;
internal sealed class Canvas
{
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
            "Custom" => $"Custom {_customFigureCounter++}",
            _ => throw new InvalidOperationException("Unsupported figure type.")
        };
    }
    
    public void AddFigure(Figure figure, bool isNewOperation = true)
    {
        Figures.Add(figure);
        
        if (figure is UnfinishedCustomFigure)
        {
            return;
        }
        
        OnFigureAdded(figure);

        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }
    
    public void RemoveFigure(Figure figure, bool isNewOperation = true)
    {
        Figures.Remove(figure);
        OnFigureRemoved(figure);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }
    
    public void RotateFigure(Figure figure, double angle, bool isNewOperation = true)
    {
        figure.Rotate(angle);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }
    
    public void TranslateFigure(Figure figure, double dx, double dy, bool isNewOperation = true)
    {
        figure.Translate(dx, dy);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }
    
    public void ScaleFigure(Figure figure, double sx, double sy, bool isNewOperation = true)
    {
        figure.Scale(sx, sy);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }
    
    public void AddUnfinishedCustomFigurePoint(UnfinishedCustomFigure figure, PointF point, bool isNewOperation = true)
    {
        // Check if it's the first point
        if (CustomFigurePoints.Count == 0)
        {
            figure.RemoveLastPoint();
        }
        
        figure.AddPoint(point);
        
        CustomFigurePoints.Add(point);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            CustomFigureRedoStack.Clear();
        }
    }
    
    public void RemoveUnfinishedCustomFigurePoint(UnfinishedCustomFigure figure, bool isNewOperation = true)
    {
        figure.RemoveLastPoint();
        
        CustomFigurePoints.RemoveAt(CustomFigurePoints.Count - 1);
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            CustomFigureRedoStack.Clear();
        }
    }
    
    public void ChangeFillColor(Figure figure, Color color, bool isNewOperation = true)
    {
        figure.FillColor = color;
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            CustomFigureRedoStack.Clear();
        }
    }
    
    public void ChangeBorderColor(Figure figure, Color color, bool isNewOperation = true)
    {
        figure.BorderColor = color;
        
        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            CustomFigureRedoStack.Clear();
        }
    }
    
    private void OnFigureAdded(Figure obj)
    {
        FigureAdded?.Invoke(obj);
    }

    private void OnFigureRemoved(Figure obj)
    {
        FigureRemoved?.Invoke(obj);
    }
    
    public void Undo(bool isCustomFigureOperation = false)
    {
        var stack = isCustomFigureOperation ? CustomFigureUndoStack : UndoStack;
        var redoStack = isCustomFigureOperation ? CustomFigureRedoStack : RedoStack;

        if (!stack.TryPop(out var operation)) return;

        operation.IsNewOperation = false; // Mark the operation as not new
        
        // If batch operation, mark all operations as not new
        if (operation is BatchCanvasOperation batchOperation)
        {
            foreach (var op in batchOperation.Operations)
            {
                op.IsNewOperation = false;
            }
        }

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
