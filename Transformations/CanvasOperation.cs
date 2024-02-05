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

internal class ResizeFigureOperation(
    Figure figure,
    ResizePosition resizePosition,
    RectangleF originalBox,
    PointF newMousePosition)
    : CanvasOperation
{
    private readonly RectangleF _oldBoundingBox = figure.GetBounds();
    // private bool _needsXFlip;
    // private bool _needsYFlip;
    
    public override void Execute(Canvas canvas)
    {
        var newBoundingBox = CalculateNewBoundingBox(originalBox);
        
        // Check if the new mouse position has crossed the opposite side of the figure
        if (HasCrossedOppositeSide(originalBox))
        {
            // Calculate the new resize position which is the opposite of the current resize position
            resizePosition = GetOppositeResizePosition(resizePosition);

            // Calculate the new bounding box based on the new resize position
            newBoundingBox = CalculateOppositeNewBoundingBox(originalBox);
            
            // Flip the figure if needed
            // figure.Flip(_needsXFlip, _needsYFlip);
            
            // Don't execute the operation if the new bounding box is invalid
            if (Math.Abs(newBoundingBox.Width) <= 0 || Math.Abs(newBoundingBox.Height) <= 0)
            {
                return;
            }
        }

        // Scale the figure
        var sx = newBoundingBox.Width / _oldBoundingBox.Width;
        var sy = newBoundingBox.Height / _oldBoundingBox.Height;
        figure.Scale(sx, sy);
        
        // Translate the figure based on the new mouse position and the real bounding box
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
        // TODO: REDO IS FLAWED
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

    private RectangleF CalculateNewBoundingBox(RectangleF boundingBox)
    {
        var newBox = resizePosition switch
        {
            ResizePosition.TopMiddle => boundingBox with { Y = newMousePosition.Y, Height = boundingBox.Bottom - newMousePosition.Y },
            ResizePosition.BottomMiddle => boundingBox with { Height = newMousePosition.Y - boundingBox.Y },
            ResizePosition.RightMiddle => boundingBox with { Width = newMousePosition.X - boundingBox.X },
            ResizePosition.LeftMiddle => boundingBox with { X = newMousePosition.X, Width = boundingBox.Right - newMousePosition.X },
            ResizePosition.TopLeft => new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Right - newMousePosition.X, boundingBox.Bottom - newMousePosition.Y),
            ResizePosition.TopRight => new RectangleF(boundingBox.X, newMousePosition.Y, newMousePosition.X - boundingBox.X, boundingBox.Bottom - newMousePosition.Y),
            ResizePosition.BottomLeft => new RectangleF(newMousePosition.X, boundingBox.Y, boundingBox.Right - newMousePosition.X, newMousePosition.Y - boundingBox.Y),
            ResizePosition.BottomRight => boundingBox with { Width = newMousePosition.X - boundingBox.X, Height = newMousePosition.Y - boundingBox.Y },
            _ => throw new ArgumentOutOfRangeException()
        };
        
        // Ensure the width and height are not less than the minimum
        const float minSize = 1f; 
        if (newBox.Width < minSize)
        {
            newBox.Width = minSize;
        }
        if (newBox.Height < minSize)
        {
            newBox.Height = minSize;
        }

        return newBox; 
    }
    
    private bool HasCrossedOppositeSide(RectangleF boundingBox)
    {
        return resizePosition switch
        {
            ResizePosition.TopMiddle => newMousePosition.Y > boundingBox.Bottom,
            ResizePosition.BottomMiddle => newMousePosition.Y < boundingBox.Top,
            ResizePosition.RightMiddle => newMousePosition.X < boundingBox.Left,
            ResizePosition.LeftMiddle => newMousePosition.X > boundingBox.Right,
            ResizePosition.TopLeft => newMousePosition.X > boundingBox.Right || newMousePosition.Y > boundingBox.Bottom,
            ResizePosition.TopRight => newMousePosition.X < boundingBox.Left || newMousePosition.Y > boundingBox.Bottom,
            ResizePosition.BottomLeft => newMousePosition.X > boundingBox.Right || newMousePosition.Y < boundingBox.Top,
            ResizePosition.BottomRight => newMousePosition.X < boundingBox.Left || newMousePosition.Y < boundingBox.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(boundingBox))
        };
    }
    
    private static ResizePosition GetOppositeResizePosition(ResizePosition currentResizePosition)
    {
        return currentResizePosition switch
        {
            ResizePosition.TopMiddle => ResizePosition.BottomMiddle,
            ResizePosition.BottomMiddle => ResizePosition.TopMiddle,
            ResizePosition.RightMiddle => ResizePosition.LeftMiddle,
            ResizePosition.LeftMiddle => ResizePosition.RightMiddle,
            ResizePosition.TopLeft => ResizePosition.BottomRight,
            ResizePosition.TopRight => ResizePosition.BottomLeft,
            ResizePosition.BottomLeft => ResizePosition.TopRight,
            ResizePosition.BottomRight => ResizePosition.TopLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(currentResizePosition))
        };
    }
    
    private RectangleF CalculateOppositeNewBoundingBox(RectangleF boundingBox)
    {
        return resizePosition switch
        {
            ResizePosition.TopMiddle => CalculateTopMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomMiddle => CalculateBottomMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.RightMiddle => CalculateRightMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.LeftMiddle => CalculateLeftMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.TopLeft => CalculateTopLeftOppositeNewBoundingBox(boundingBox),
            ResizePosition.TopRight => CalculateTopRightOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomLeft => CalculateBottomLeftOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomRight => CalculateBottomRightOppositeNewBoundingBox(boundingBox),
            _ => throw new ArgumentOutOfRangeException(nameof(resizePosition))
        };
    }
    
    private RectangleF CalculateTopMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        //_needsYFlip = true;
        return boundingBox with { Y = newMousePosition.Y, Height = boundingBox.Top - newMousePosition.Y };
    }
    
    private RectangleF CalculateBottomMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        //_needsYFlip = true;
        return boundingBox with { Y = newMousePosition.Y, Height = newMousePosition.Y - boundingBox.Bottom };
    }
    
    private RectangleF CalculateRightMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // _needsXFlip = true;
        return boundingBox with { Width = newMousePosition.X - boundingBox.Right };
    }
    
    private RectangleF CalculateLeftMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // _needsXFlip = true;
        return boundingBox with { X = newMousePosition.X, Width = boundingBox.Left - newMousePosition.X };
    }

    private RectangleF CalculateTopLeftOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the left of the figure and above the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y < boundingBox.Y)
        {
            // _needsXFlip = true;
            // _needsYFlip = true;
            return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Left - newMousePosition.X,
                boundingBox.Top - newMousePosition.Y);
        }
        // If the new mouse position is on the left of the figure without going above the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y > boundingBox.Y)
        {
            // _needsXFlip = true;
            resizePosition = ResizePosition.BottomLeft;
            return new RectangleF(newMousePosition.X, boundingBox.Top, boundingBox.Left - newMousePosition.X,
                newMousePosition.Y - boundingBox.Top);
        }
        // If the new mouse position is above the figure without going to the left of the figure
        // _needsYFlip = true;
        resizePosition = ResizePosition.TopRight;
        return boundingBox with { Y = newMousePosition.Y, Width = newMousePosition.X - boundingBox.X, Height = boundingBox.Y - newMousePosition.Y };
    }
    
    private RectangleF CalculateTopRightOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the right of the figure and above the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y < boundingBox.Y)
        {
            // _needsXFlip = true;
            // _needsYFlip = true;
            return new RectangleF(boundingBox.Right, newMousePosition.Y, newMousePosition.X - boundingBox.Right,
                boundingBox.Top - newMousePosition.Y);
        }
        // If the new mouse position is on the right of the figure without going above the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y > boundingBox.Y)
        {
            // _needsXFlip = true;
            resizePosition = ResizePosition.BottomRight;
            return new RectangleF(boundingBox.Right, boundingBox.Top, newMousePosition.X - boundingBox.Right,
                newMousePosition.Y - boundingBox.Top);
        }
        // If the new mouse position is above the figure without going to the right of the figure
        // _needsYFlip = true;
        resizePosition = ResizePosition.TopLeft;
        return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Right - newMousePosition.X,
            boundingBox.Y - newMousePosition.Y);
    }
    
    private RectangleF CalculateBottomLeftOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the left of the figure and below the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y > boundingBox.Bottom)
        {
            // _needsXFlip = true;
            // _needsYFlip = true;
            return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Left - newMousePosition.X,
                newMousePosition.Y - boundingBox.Bottom);
        }
        // If the new mouse position is on the left of the figure without going below the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y < boundingBox.Bottom)
        {
            // _needsXFlip = true;
            resizePosition = ResizePosition.TopLeft;
            return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Left - newMousePosition.X,
                boundingBox.Bottom - newMousePosition.Y);
        }
        // If the new mouse position is below the figure without going to the left of the figure
        // _needsYFlip = true;
        resizePosition = ResizePosition.BottomRight;
        return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Left - newMousePosition.X,
            newMousePosition.Y - boundingBox.Bottom);
    }
    
    private RectangleF CalculateBottomRightOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the right of the figure and below the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y > boundingBox.Bottom)
        {
            // _needsXFlip = true;
            // _needsYFlip = true;
            return new RectangleF(boundingBox.Right, boundingBox.Bottom, newMousePosition.X - boundingBox.Right,
                newMousePosition.Y - boundingBox.Bottom);
        }

        // If the new mouse position is on the right of the figure without going below the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y < boundingBox.Bottom)
        {
            // _needsXFlip = true;
            resizePosition = ResizePosition.TopRight;
            return new RectangleF(boundingBox.Right, newMousePosition.Y, newMousePosition.X - boundingBox.Right,
                boundingBox.Bottom - newMousePosition.Y);
        }
        // If the new mouse position is below the figure without going to the right of the figure
        // _needsYFlip = true;
        resizePosition = ResizePosition.BottomLeft;
        return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Right - newMousePosition.X,
            newMousePosition.Y - boundingBox.Bottom);
    }
}
