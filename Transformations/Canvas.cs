﻿namespace Transformations;

public sealed class Canvas
{
    public readonly List<PointF> CustomFigurePoints = [];
    public readonly Stack<CanvasOperation> CustomFigureRedoStack = new();
    public readonly Stack<CanvasOperation> CustomFigureUndoStack = new();
    public readonly List<Figure> Figures = [];
    public readonly Stack<CanvasOperation> RedoStack = new();
    public readonly Stack<CanvasOperation> UndoStack = new();
    private int _customFigureCounter;

    private int _squareCounter;
    private int _triangleCounter;
    public TimeLine? TimeLine;

    public bool CanUndo()
    {
        return UndoStack.Count > 0;
    }

    public bool CanRedo()
    {
        return RedoStack.Count > 0;
    }

    public bool CanCustomFigureUndo()
    {
        return CustomFigureUndoStack.Count > 0;
    }

    public bool CanCustomFigureRedo()
    {
        return CustomFigureRedoStack.Count > 0;
    }

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
        foreach (var figure in Figures) figure.Draw(g);
    }
}