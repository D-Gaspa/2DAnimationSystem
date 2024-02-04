namespace Transformations;

public enum ResizePosition
{
    TopMiddle,
    BottomMiddle,
    RightMiddle,
    LeftMiddle,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

internal abstract class CanvasOperation
{
    public abstract void Execute(Canvas canvas);
    public abstract void Undo(Canvas canvas);
    public bool IsNewOperation { get; set; }
}

internal class BatchCanvasOperation(List<CanvasOperation> operations) : CanvasOperation
{
    public List<CanvasOperation> Operations { get; } = operations;

    public override void Execute(Canvas canvas)
    {
        foreach (var operation in Operations)
        {
            operation.Execute(canvas);
        }
    }

    public override void Undo(Canvas canvas)
    {
        foreach (var operation in Operations)
        {
            operation.Undo(canvas);
        }
    }
}

internal class AddFigureOperation(Figure figure) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        canvas.Figures.Add(figure);
        
        if (figure is UnfinishedCustomFigure)
        {
            return;
        }
        
        canvas.OnFigureAdded(figure);

        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new DeleteFigureOperation(figure)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class DeleteFigureOperation(Figure figure) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        canvas.Figures.Remove(figure);
        canvas.OnFigureRemoved(figure);
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new AddFigureOperation(figure)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal abstract class TransformFigureOperation(Figure figure) : CanvasOperation
{
    protected readonly Figure Figure = figure;
}

internal class RotateFigureOperation(Figure figure, double angle) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        Figure.Rotate(angle);
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new RotateFigureOperation(Figure, -angle)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class TranslateFigureOperation(Figure figure, double dx, double dy) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        Figure.Translate(dx, dy);
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new TranslateFigureOperation(Figure, -dx, -dy)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class AddUnfinishedCustomFigurePointOperation(UnfinishedCustomFigure figure, PointF point) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        // Check if it's the first point
        if (canvas.CustomFigurePoints.Count == 0)
        {
            figure.RemoveLastPoint();
        }
        
        figure.AddPoint(point);
        
        canvas.CustomFigurePoints.Add(point);
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.CustomFigureRedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        figure.RemoveLastPoint();
        
        canvas.CustomFigurePoints.RemoveAt(canvas.CustomFigurePoints.Count - 1);
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.CustomFigureRedoStack.Clear();
        }
    }
}

internal class ChangeFillColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private readonly Color _oldColor = figure.FillColor;

    public override void Execute(Canvas canvas)
    {
        figure.FillColor = newColor;
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.CustomFigureRedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new ChangeFillColorOperation(figure, _oldColor)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class ChangeBorderColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private readonly Color _oldColor = figure.BorderColor;

    public override void Execute(Canvas canvas)
    {
        figure.BorderColor = newColor;
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.CustomFigureRedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new ChangeBorderColorOperation(figure, _oldColor)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class ResizeFigureOperation(Figure figure, ResizePosition resizePosition, PointF newMousePosition)
    : CanvasOperation
{
    private readonly RectangleF _oldBoundingBox = figure.GetBounds();

    public override void Execute(Canvas canvas)
    {
        var newBoundingBox = CalculateNewBoundingBox();
        
        // Scale the figure
        var sx = newBoundingBox.Width / _oldBoundingBox.Width;
        var sy = newBoundingBox.Height / _oldBoundingBox.Height;
        figure.Scale(sx, sy);
        
        // Translate the figure based on the new mouse position
        var realBoundingBox = figure.GetBounds();
        float dx;
        float dy;
        
        switch (resizePosition)
        {
            case ResizePosition.RightMiddle:
                dx = newMousePosition.X - realBoundingBox.Right;
                figure.Translate(dx, 0);
                break;
            case ResizePosition.LeftMiddle:
                dx = newMousePosition.X - realBoundingBox.X;
                figure.Translate(dx, 0);
                break;
            case ResizePosition.TopMiddle:
                dy = newMousePosition.Y - realBoundingBox.Y;
                figure.Translate(0, dy);
                break;
            case ResizePosition.BottomMiddle:
                dy = newMousePosition.Y - realBoundingBox.Bottom;
                figure.Translate(0, dy);
                break;
            case ResizePosition.TopLeft:
                dx = newMousePosition.X - realBoundingBox.X;
                dy = newMousePosition.Y - realBoundingBox.Y;
                figure.Translate(dx, dy);
                break;
            case ResizePosition.TopRight:
                dx = newMousePosition.X - realBoundingBox.Right;
                dy = newMousePosition.Y - realBoundingBox.Y;
                figure.Translate(dx, dy);
                break;
            case ResizePosition.BottomLeft:
                dx = newMousePosition.X - realBoundingBox.X;
                dy = newMousePosition.Y - realBoundingBox.Bottom;
                figure.Translate(dx, dy);
                break;
            case ResizePosition.BottomRight:
                dx = newMousePosition.X - realBoundingBox.Right;
                dy = newMousePosition.Y - realBoundingBox.Bottom;
                figure.Translate(dx, dy);
                break;
        }

        figure.CalculatePivot(new PointF(0, 0));
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    public override void Undo(Canvas canvas)
    {
        var sx = _oldBoundingBox.Width / figure.GetBounds().Width;
        var sy = _oldBoundingBox.Height / figure.GetBounds().Height;
        figure.Scale(sx, sy);
        var dx = _oldBoundingBox.X - figure.GetBounds().X;
        var dy = _oldBoundingBox.Y - figure.GetBounds().Y;
        figure.Translate(dx, dy);
        
        figure.CalculatePivot(new PointF(0, 0));
        
        // Clear the redo stack only if it's a new operation
        if (IsNewOperation)
        {
            canvas.RedoStack.Clear();
        }
    }

    private RectangleF CalculateNewBoundingBox()
    {
        var oldBox = figure.GetBounds();
        return resizePosition switch
        {
            ResizePosition.TopMiddle => oldBox with { Y = newMousePosition.Y, Height = oldBox.Bottom - newMousePosition.Y },
            ResizePosition.BottomMiddle => oldBox with { Height = newMousePosition.Y - oldBox.Y },
            ResizePosition.RightMiddle => oldBox with { Width = newMousePosition.X - oldBox.X },
            ResizePosition.LeftMiddle => oldBox with { X = newMousePosition.X, Width = oldBox.Right - newMousePosition.X },
            ResizePosition.TopLeft => new RectangleF(newMousePosition.X, newMousePosition.Y, oldBox.Right - newMousePosition.X, oldBox.Bottom - newMousePosition.Y),
            ResizePosition.TopRight => new RectangleF(oldBox.X, newMousePosition.Y, newMousePosition.X - oldBox.X, oldBox.Bottom - newMousePosition.Y),
            ResizePosition.BottomLeft => new RectangleF(newMousePosition.X, oldBox.Y, oldBox.Right - newMousePosition.X, newMousePosition.Y - oldBox.Y),
            ResizePosition.BottomRight => oldBox with { Width = newMousePosition.X - oldBox.X, Height = newMousePosition.Y - oldBox.Y },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
