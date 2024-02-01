namespace Transformations;

internal abstract class CanvasOperation
{
    public abstract void Execute(Canvas canvas);
    public abstract void Undo(Canvas canvas);
    public bool IsNewOperation { get; set; }
}

// Operation for adding a figure
internal class AddFigureOperation(Figure figure) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        canvas.AddFigure(figure, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.RemoveFigure(figure, IsNewOperation);
    }
}

// Operation for deleting a figure
internal class DeleteFigureOperation(Figure figure) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        canvas.RemoveFigure(figure, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.AddFigure(figure, IsNewOperation);
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
    }

    public override void Undo(Canvas canvas)
    {
        Figure.Rotate(-angle);
    }
}

internal class TranslateFigureOperation(Figure figure, double dx, double dy) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        Figure.Translate(dx, dy);
    }

    public override void Undo(Canvas canvas)
    {
        Figure.Translate(-dx, -dy);
    }
}

internal class ScaleFigureOperation(Figure figure, double sx, double sy) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        Figure.Scale(sx, sy);
    }

    public override void Undo(Canvas canvas)
    {
        Figure.Scale(1/sx, 1/sy);
    }
}

// Operation for multiple operations
internal class BatchCanvasOperation(List<CanvasOperation> operations) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        foreach (var operation in operations)
        {
            operation.Execute(canvas);
        }
    }

    public override void Undo(Canvas canvas)
    {
        foreach (var operation in operations)
        {
            operation.Undo(canvas);
        }
    }
}
