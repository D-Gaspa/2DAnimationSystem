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

public abstract class CanvasOperation
{
    public abstract void Execute(Canvas canvas);
    public abstract void Undo(Canvas canvas);
    public bool IsNewOperation { get; set; }
    
    protected void ClearRedoStackIfNewOperation(Stack<CanvasOperation> redoStack)
    {
        if (IsNewOperation)
        {
            redoStack.Clear();
        }
    }
}

internal class BatchCanvasOperation(List<CanvasOperation> operations) : CanvasOperation
{
    private List<CanvasOperation> Operations { get; } = operations;

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
            // Mark the operation as not new
            operation.IsNewOperation = false;
            operation.Undo(canvas);
        }
    }
}

internal class AddFigureOperation(Figure figure) : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;

    public override void Execute(Canvas canvas)
    {
        canvas.Figures.Add(figure);
        
        if (figure is UnfinishedCustomFigure)
        {
            return;
        }
        
        canvas.OnFigureAdded(figure);

        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }

        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new DeleteFigureOperation(figure)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);

        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal class DeleteFigureOperation(Figure figure) : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    public Figure Figure => figure;

    public override void Execute(Canvas canvas)
    {
        canvas.Figures.Remove(figure);
        canvas.OnFigureRemoved(figure);
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new AddFigureOperation(figure)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal abstract class TransformFigureOperation(Figure figure) : CanvasOperation
{
    protected readonly Figure Figure = figure;
}

internal class RotateFigureOperation(Figure figure, double angle) : TransformFigureOperation(figure)
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;

    public override void Execute(Canvas canvas)
    {
        Figure.Rotate(angle);
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new RotateFigureOperation(Figure, -angle)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal class TranslateFigureOperation(Figure figure, double dx, double dy) : TransformFigureOperation(figure)
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    
    public override void Execute(Canvas canvas)
    {
        Figure.Translate(dx, dy);
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new TranslateFigureOperation(Figure, -dx, -dy)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
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
        
        ClearRedoStackIfNewOperation(canvas.CustomFigureRedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        figure.RemoveLastPoint();
        
        canvas.CustomFigurePoints.RemoveAt(canvas.CustomFigurePoints.Count - 1);
        
        ClearRedoStackIfNewOperation(canvas.CustomFigureRedoStack);
    }
}

internal class ChangeFillColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    private readonly Color _oldColor = figure.FillColor;

    public override void Execute(Canvas canvas)
    {
        figure.FillColor = newColor;
        
        if (figure is UnfinishedCustomFigure)
        {
            ClearRedoStackIfNewOperation(canvas.CustomFigureRedoStack);
            return;
        }
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new ChangeFillColorOperation(figure, _oldColor)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal class ChangeBorderColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    private readonly Color _oldColor = figure.BorderColor;

    public override void Execute(Canvas canvas)
    {
        figure.BorderColor = newColor;
        
        if (figure is UnfinishedCustomFigure)
        {
            ClearRedoStackIfNewOperation(canvas.CustomFigureRedoStack);
            return;
        }
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new ChangeBorderColorOperation(figure, _oldColor)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal class ResizeFigureOperation(
    Figure figure,
    ResizePosition currentResizePosition,
    RectangleF originalBox,
    PointF newMousePosition)
    : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    private readonly RectangleF _oldBoundingBox = figure.GetBounds();
    private bool _needsXFlip;
    private bool _needsYFlip;
    private readonly bool _hasFlipped = figure.HasFlipped;
    private readonly ResizePosition _previousResizePosition = figure.PreviousResizePosition;
    private bool _wasFlippedX;
    private bool _wasFlippedY;
    
    public override void Execute(Canvas canvas)
    {
        var newBoundingBox = CalculateNewBoundingBox(originalBox);
        
        // Check if the new mouse position has crossed the opposite side of the figure
        if (HasCrossedOppositeSide(originalBox))
        {
            // Calculate the new resize position which is the opposite of the current resize position
            currentResizePosition = GetOppositeResizePosition(currentResizePosition);

            // Calculate the new bounding box based on the new resize position
            newBoundingBox = CalculateOppositeNewBoundingBox(originalBox);
            
            // Don't execute the operation if the new bounding box is invalid
            if (Math.Abs(newBoundingBox.Width) <= 0 || Math.Abs(newBoundingBox.Height) <= 0)
            {
                return;
            }
        }
        
        ScaleAndTranslateFigure(newBoundingBox);

        HandleFlip();
        
        // If figure was already flipped, set the flags to false
        if (IsNewOperation)
        {
            if (figure.HasFlippedX)
            {
                if (_wasFlippedX)
                {
                    figure.HasFlippedX = false;
                }
            }
            else
            {
                if (_wasFlippedX)
                {
                    figure.HasFlippedX = true;
                }
            }
        
            if (figure.HasFlippedY)
            {
                if (_wasFlippedY)
                {
                    figure.HasFlippedY = false;
                }
            }
            else
            {
                if (_wasFlippedY)
                {
                    figure.HasFlippedY = true;
                }
            }
            
            if (figure is { HasFlippedX: false, HasFlippedY: false })
            {
                figure.HasFlipped = false;
            }
        }

        figure.PreviousResizePosition = currentResizePosition;
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        // If the figure was flipped, flip it back
        if (_wasFlippedX || _wasFlippedY)
        {
            figure.Flip(_wasFlippedX, _wasFlippedY);
        }

        // Scale and translate the figure back to its original state
        var sx = _oldBoundingBox.Width / figure.GetBounds().Width;
        var sy = _oldBoundingBox.Height / figure.GetBounds().Height;
        figure.Scale(sx, sy);
        var dx = _oldBoundingBox.X - figure.GetBounds().X;
        var dy = _oldBoundingBox.Y - figure.GetBounds().Y;
        figure.Translate(dx, dy);
        
        // Reset the flip flags
        figure.HasFlipped = false;
        _wasFlippedX = false;
        _wasFlippedY = false;
        
        // Reset the previous resize position
        currentResizePosition = _previousResizePosition;
        
        _handleKeyFrameOperation?.Undo(canvas);
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    private RectangleF CalculateNewBoundingBox(RectangleF boundingBox)
    {
        var newBox = currentResizePosition switch
        {
            ResizePosition.TopMiddle => boundingBox with { Y = newMousePosition.Y, Height = boundingBox.Bottom - newMousePosition.Y },
            ResizePosition.BottomMiddle => boundingBox with { Height = newMousePosition.Y - boundingBox.Y },
            ResizePosition.RightMiddle => boundingBox with { Width = newMousePosition.X - boundingBox.X },
            ResizePosition.LeftMiddle => boundingBox with { X = newMousePosition.X, Width = boundingBox.Right - newMousePosition.X },
            ResizePosition.TopLeft => new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Right - newMousePosition.X, boundingBox.Bottom - newMousePosition.Y),
            ResizePosition.TopRight => new RectangleF(boundingBox.X, newMousePosition.Y, newMousePosition.X - boundingBox.X, boundingBox.Bottom - newMousePosition.Y),
            ResizePosition.BottomLeft => new RectangleF(newMousePosition.X, boundingBox.Y, boundingBox.Right - newMousePosition.X, newMousePosition.Y - boundingBox.Y),
            ResizePosition.BottomRight => boundingBox with { Width = newMousePosition.X - boundingBox.X, Height = newMousePosition.Y - boundingBox.Y },
            _ => throw new ArgumentOutOfRangeException(nameof(boundingBox))
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
        return currentResizePosition switch
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
        return currentResizePosition switch
        {
            ResizePosition.TopMiddle => CalculateTopMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomMiddle => CalculateBottomMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.RightMiddle => CalculateRightMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.LeftMiddle => CalculateLeftMiddleOppositeNewBoundingBox(boundingBox),
            ResizePosition.TopLeft => CalculateTopLeftOppositeNewBoundingBox(boundingBox),
            ResizePosition.TopRight => CalculateTopRightOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomLeft => CalculateBottomLeftOppositeNewBoundingBox(boundingBox),
            ResizePosition.BottomRight => CalculateBottomRightOppositeNewBoundingBox(boundingBox),
            _ => throw new ArgumentOutOfRangeException(nameof(boundingBox))
        };
    }
    
    private RectangleF CalculateTopMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        _needsYFlip = true;
        return boundingBox with { Y = newMousePosition.Y, Height = boundingBox.Top - newMousePosition.Y };
    }
    
    private RectangleF CalculateBottomMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        _needsYFlip = true;
        return boundingBox with { Y = newMousePosition.Y, Height = newMousePosition.Y - boundingBox.Bottom };
    }
    
    private RectangleF CalculateRightMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        _needsXFlip = true;
        return boundingBox with { Width = newMousePosition.X - boundingBox.Right };
    }
    
    private RectangleF CalculateLeftMiddleOppositeNewBoundingBox(RectangleF boundingBox)
    {
        _needsXFlip = true;
        return boundingBox with { X = newMousePosition.X, Width = boundingBox.Left - newMousePosition.X };
    }

    private RectangleF CalculateTopLeftOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the left of the figure and above the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y < boundingBox.Y)
        {
            _needsXFlip = true;
            _needsYFlip = true;
            return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Left - newMousePosition.X,
                boundingBox.Top - newMousePosition.Y);
        }
        // If the new mouse position is on the left of the figure without going above the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y > boundingBox.Y)
        {
            _needsXFlip = true;
            currentResizePosition = ResizePosition.BottomLeft;
            return new RectangleF(newMousePosition.X, boundingBox.Top, boundingBox.Left - newMousePosition.X,
                newMousePosition.Y - boundingBox.Top);
        }
        // If the new mouse position is above the figure without going to the left of the figure
        _needsYFlip = true;
        currentResizePosition = ResizePosition.TopRight;
        return boundingBox with { Y = newMousePosition.Y, Width = newMousePosition.X - boundingBox.X, Height = boundingBox.Y - newMousePosition.Y };
    }
    
    private RectangleF CalculateTopRightOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the right of the figure and above the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y < boundingBox.Y)
        {
            _needsXFlip = true;
            _needsYFlip = true;
            return new RectangleF(boundingBox.Right, newMousePosition.Y, newMousePosition.X - boundingBox.Right,
                boundingBox.Top - newMousePosition.Y);
        }
        // If the new mouse position is on the right of the figure without going above the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y > boundingBox.Y)
        {
            _needsXFlip = true;
            currentResizePosition = ResizePosition.BottomRight;
            return new RectangleF(boundingBox.Right, boundingBox.Top, newMousePosition.X - boundingBox.Right,
                newMousePosition.Y - boundingBox.Top);
        }
        // If the new mouse position is above the figure without going to the right of the figure
        _needsYFlip = true;
        currentResizePosition = ResizePosition.TopLeft;
        return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Right - newMousePosition.X,
            boundingBox.Y - newMousePosition.Y);
    }
    
    private RectangleF CalculateBottomLeftOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the left of the figure and below the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y > boundingBox.Bottom)
        {
            _needsXFlip = true;
            _needsYFlip = true;
            return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Left - newMousePosition.X,
                newMousePosition.Y - boundingBox.Bottom);
        }
        // If the new mouse position is on the left of the figure without going below the figure
        if (newMousePosition.X <= boundingBox.X && newMousePosition.Y < boundingBox.Bottom)
        {
            _needsXFlip = true;
            currentResizePosition = ResizePosition.TopLeft;
            return new RectangleF(newMousePosition.X, newMousePosition.Y, boundingBox.Left - newMousePosition.X,
                boundingBox.Bottom - newMousePosition.Y);
        }
        // If the new mouse position is below the figure without going to the left of the figure
        _needsYFlip = true;
        currentResizePosition = ResizePosition.BottomRight;
        return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Left - newMousePosition.X,
            newMousePosition.Y - boundingBox.Bottom);
    }
    
    private RectangleF CalculateBottomRightOppositeNewBoundingBox(RectangleF boundingBox)
    {
        // If the new mouse position is on the right of the figure and below the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y > boundingBox.Bottom)
        {
            _needsXFlip = true;
            _needsYFlip = true;
            return new RectangleF(boundingBox.Right, boundingBox.Bottom, newMousePosition.X - boundingBox.Right,
                newMousePosition.Y - boundingBox.Bottom);
        }

        // If the new mouse position is on the right of the figure without going below the figure
        if (newMousePosition.X >= boundingBox.Right && newMousePosition.Y < boundingBox.Bottom)
        {
            _needsXFlip = true;
            currentResizePosition = ResizePosition.TopRight;
            return new RectangleF(boundingBox.Right, newMousePosition.Y, newMousePosition.X - boundingBox.Right,
                boundingBox.Bottom - newMousePosition.Y);
        }
        // If the new mouse position is below the figure without going to the right of the figure
        _needsYFlip = true;
        currentResizePosition = ResizePosition.BottomLeft;
        return new RectangleF(newMousePosition.X, boundingBox.Bottom, boundingBox.Right - newMousePosition.X,
            newMousePosition.Y - boundingBox.Bottom);
    }
    
    private void ScaleAndTranslateFigure(RectangleF newBoundingBox)
    {
        var sx = newBoundingBox.Width / _oldBoundingBox.Width;
        var sy = newBoundingBox.Height / _oldBoundingBox.Height;
        figure.Scale(sx, sy);

        TranslateFigureBasedOnNewMousePositionAndRealBoundingBox();
    }

    private void TranslateFigureBasedOnNewMousePositionAndRealBoundingBox()
    {
        var realBoundingBox = figure.GetBounds();
        float dx;
        float dy;

        switch (currentResizePosition)
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
            default:
                throw new ArgumentOutOfRangeException(nameof(currentResizePosition), currentResizePosition, null);
        }
    }

    private void HandleFlip()
    {
        var needsFlip = _needsYFlip || _needsXFlip;

        if (_hasFlipped)
        {
            if (_previousResizePosition == currentResizePosition)
            {
                needsFlip = false;
            }
            else
            {
                _needsXFlip = false;
                _needsYFlip = false;

                var flipActions = CreateFlipActions();

                if (flipActions.TryGetValue((_previousResizePosition, currentResizePosition), out var action))
                {
                    action();
                }

                needsFlip = true;
            }
        }

        if (!needsFlip) return;
        figure.Flip(_needsXFlip, _needsYFlip);
        _wasFlippedX = _needsXFlip;
        _wasFlippedY = _needsYFlip;
        figure.HasFlipped = true;
    }
    
    private Dictionary<(ResizePosition, ResizePosition), Action> CreateFlipActions()
    {
        return new Dictionary<(ResizePosition, ResizePosition), Action>
        {
            { (ResizePosition.LeftMiddle, ResizePosition.RightMiddle), () => _needsXFlip = true },
            { (ResizePosition.RightMiddle, ResizePosition.LeftMiddle), () => _needsXFlip = true },
            { (ResizePosition.TopMiddle, ResizePosition.BottomMiddle), () => _needsYFlip = true },
            { (ResizePosition.BottomMiddle, ResizePosition.TopMiddle), () => _needsYFlip = true },
            { (ResizePosition.BottomLeft, ResizePosition.TopLeft), () => _needsYFlip = true },
            { (ResizePosition.BottomLeft, ResizePosition.BottomRight), () => _needsXFlip = true },
            { (ResizePosition.BottomLeft, ResizePosition.TopRight), () => { _needsXFlip = true; _needsYFlip = true; } },
            { (ResizePosition.BottomRight, ResizePosition.TopRight), () => _needsYFlip = true },
            { (ResizePosition.BottomRight, ResizePosition.BottomLeft), () => _needsXFlip = true },
            { (ResizePosition.BottomRight, ResizePosition.TopLeft), () => { _needsXFlip = true; _needsYFlip = true; } },
            { (ResizePosition.TopLeft, ResizePosition.BottomLeft), () => _needsYFlip = true },
            { (ResizePosition.TopLeft, ResizePosition.TopRight), () => _needsXFlip = true },
            { (ResizePosition.TopLeft, ResizePosition.BottomRight), () => { _needsXFlip = true; _needsYFlip = true; } },
            { (ResizePosition.TopRight, ResizePosition.BottomRight), () => _needsYFlip = true },
            { (ResizePosition.TopRight, ResizePosition.TopLeft), () => _needsXFlip = true },
            { (ResizePosition.TopRight, ResizePosition.BottomLeft), () => { _needsXFlip = true; _needsYFlip = true; } }
        };
    }
}

internal class MovePivotOperation(Figure figure, PointF newPivot) : CanvasOperation
{
    private readonly PointF _oldPivot = figure.Pivot;

    public override void Execute(Canvas canvas)
    {
        figure.Pivot = newPivot;
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new MovePivotOperation(figure, _oldPivot)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class DuplicateFigureOperation(Figure figure, string duplicateFigureName) : CanvasOperation
{
    private HandleKeyFrameOperation? _handleKeyFrameOperation;
    private Figure? _duplicate;

    public override void Execute(Canvas canvas)
    {
        figure.IsSelected = false;
        if (_duplicate == null)
        {
            _duplicate = figure.Clone();
            _duplicate.Translate(20, 20);
            _duplicate.Name = duplicateFigureName;
            _duplicate.IsSelected = true;
        }

        canvas.Figures.Add(_duplicate);
        canvas.OnFigureAdded(_duplicate);
        
        if (canvas.TimeLine != null)
        {
            _handleKeyFrameOperation = new HandleKeyFrameOperation(canvas.TimeLine, canvas.TimeLine.CurrentFrame, canvas.Figures, this);
            _handleKeyFrameOperation.Execute(canvas);
        }
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new DeleteFigureOperation(_duplicate!)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
        
        _handleKeyFrameOperation?.Undo(canvas);
    }
}

internal class AddKeyFrameOperation(KeyFrame? keyFrame, TimeLine timeLine) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        timeLine.KeyFrames.Add(keyFrame);
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new DeleteKeyFrameOperation(keyFrame, timeLine)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class DeleteKeyFrameOperation(KeyFrame? keyFrame, TimeLine timeLine) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        timeLine.KeyFrames.Remove(keyFrame);
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new AddKeyFrameOperation(keyFrame, timeLine)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class ChangeKeyFrameOperation(KeyFrame keyFrame, TimeLine timeLine, int oldFrame, int newFrame) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        keyFrame.Frame = newFrame;
        
        timeLine.Draw();
        
        ClearRedoStackIfNewOperation(canvas.RedoStack);
    }

    public override void Undo(Canvas canvas)
    {
        var operation = new ChangeKeyFrameOperation(keyFrame, timeLine, newFrame, oldFrame)
        {
            IsNewOperation = IsNewOperation
        };
        operation.Execute(canvas);
    }
}

internal class HandleKeyFrameOperation(TimeLine timeLine, int currentFrame, IEnumerable<Figure> figures, CanvasOperation operation) : CanvasOperation
{
    private KeyFrame? _keyFrame;

    public override void Execute(Canvas canvas)
    {
        var keyFrames = timeLine.KeyFrames;

        if (keyFrames.Count <= 0) return;
        
        // If the operation is a delete figure operation, remove the figure from all keyframes
        if (operation is DeleteFigureOperation deleteFigureOperation)
        {
            foreach (var kf in keyFrames)
            {
                kf?.Figures?.RemoveAll(f => f.Name == deleteFigureOperation.Figure.Name);
            }
            
            // Remove any keyframes that have no figures
            keyFrames.RemoveAll(kf => kf?.Figures?.Count == 0);
            
            timeLine.Draw();
            return;
        }
        
        var keyFrame = keyFrames.FirstOrDefault(kf => kf != null && kf.Frame == currentFrame);

        if (keyFrame != null)
        {
            // If a keyframe for the current frame exists, update its Figures property
            keyFrame.Figures = figures.Select(f => f.Clone()).ToList();
            keyFrame.Figures.ForEach(f => f.IsSelected = false);
            _keyFrame = keyFrame;
        }
        else
        {
            // If a keyframe for the current frame does not exist, create a new one and add it to the list
            _keyFrame = new KeyFrame(currentFrame) { Figures = figures.Select(f => f.Clone()).ToList() };
            _keyFrame.Figures.ForEach(f => f.IsSelected = false);
            keyFrames.Add(_keyFrame);
        }
        
        timeLine.Draw();
    }

    public override void Undo(Canvas canvas)
    {
        var keyFrames = timeLine.KeyFrames;

        if (_keyFrame != null)
        {
            keyFrames.Remove(_keyFrame);
        }
        
        timeLine.Draw();
    }
}