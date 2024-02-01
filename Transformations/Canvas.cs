namespace Transformations;
internal sealed class Canvas
{
    //TODO: Implement CustomFigure class to allow the user to draw a custom figure on the canvas.
    //TODO: Implement a method to save the canvas to a file, and a button to call it.
    //TODO: Implement a method to load a canvas from a file, and a button to call it.
    //TODO: Implement figure selection by clicking on it, displaying its properties and allowing to modify them.
    //TODO: Implement simple animations (e.g. rotation, translation, scaling).
    //TODO: Implement responsive canvas (e.g. resize the canvas and the figures should resize accordingly).
    
    public readonly List<Figure> Figures = [];
    public readonly Stack<CanvasOperation> UndoStack = new();
    public readonly Stack<CanvasOperation> RedoStack = new();
    
    public bool CanUndo() => UndoStack.Count > 0;
    public bool CanRedo() => RedoStack.Count > 0;
    
    private int _squareCounter;
    private int _triangleCounter;
    
    public event Action<Figure>? FigureAdded;
    public event Action<Figure>? FigureRemoved;

    public string GenerateUniqueFigureName(string figureType)
    {
        return figureType switch
        {
            "Square" => $"Square {_squareCounter++}",
            "Triangle" => $"Triangle {_triangleCounter++}",
            "Custom" => $"Custom {_squareCounter++}",
            _ => throw new InvalidOperationException("Unsupported figure type.")
        };
    }
    
    public void AddFigure(Figure figure, bool isNewOperation = true)
    {
        Figures.Add(figure);
        OnFigureAdded(figure);

        // Clear the redo stack only if it's a new operation
        if (isNewOperation)
        {
            RedoStack.Clear();
        }
    }

    private void OnFigureAdded(Figure obj)
    {
        FigureAdded?.Invoke(obj);
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

    private void OnFigureRemoved(Figure obj)
    {
        FigureRemoved?.Invoke(obj);
    }
    
    public void ResetFigure(string figureName)
    {
        var figure = Figures.FirstOrDefault(f => f.Name == figureName);
        figure?.Reset();
    }
    
    // Undo operation
    public void Undo()
    {
        if (!UndoStack.TryPop(out var operation)) return;
        
        operation.IsNewOperation = false; // Mark the operation as not new

        operation.Undo(this); // Perform the undo operation
        
        RedoStack.Push(operation); // Push the operation to the redo stack
    }

    // Redo operation
    public void Redo()
    {
        if (!RedoStack.TryPop(out var operation)) return;
        
        operation.IsNewOperation = false; // Mark the operation as not new

        operation.Execute(this); // Perform the redo operation
        
        UndoStack.Push(operation); // Push the operation to the undo stack
    }
    
    public void Reset()
    {
        Figures.Clear();
        _squareCounter = 0;
        _triangleCounter = 0;
        UndoStack.Clear();
        RedoStack.Clear();
    }
    
    public void Render(Graphics g, PictureBox pictureBox)
    {
        // Clear the canvas
        g.Clear(Color.Black);
        
        // Draw all figures
        Figures.ForEach(f => f.Draw(g));

        // Invalidate the picture box to redraw it
        pictureBox.Invalidate();
    }
}
