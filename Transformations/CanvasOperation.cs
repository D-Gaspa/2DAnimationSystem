namespace Transformations;

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
        canvas.AddFigure(figure, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.RemoveFigure(figure, IsNewOperation);
    }
}

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
        canvas.RotateFigure(Figure, angle, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.RotateFigure(Figure, -angle, IsNewOperation);
    }
}

internal class TranslateFigureOperation(Figure figure, double dx, double dy) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        canvas.TranslateFigure(Figure, dx, dy, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.TranslateFigure(Figure, -dx, -dy, IsNewOperation);
    }
}

internal class ScaleFigureOperation(Figure figure, double sx, double sy) : TransformFigureOperation(figure)
{
    public override void Execute(Canvas canvas)
    {
        canvas.ScaleFigure(Figure, sx, sy, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.ScaleFigure(Figure, 1 / sx, 1 / sy, IsNewOperation);
    }
}

internal class AddUnfinishedCustomFigurePointOperation(UnfinishedCustomFigure figure, PointF point) : CanvasOperation
{
    public override void Execute(Canvas canvas)
    {
        canvas.AddUnfinishedCustomFigurePoint(figure, point, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.RemoveUnfinishedCustomFigurePoint(figure, IsNewOperation);
    }
}

internal class ChangeFillColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private readonly Color _oldColor = figure.FillColor;

    public override void Execute(Canvas canvas)
    {
        canvas.ChangeFillColor(figure, newColor, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.ChangeFillColor(figure, _oldColor, IsNewOperation);
    }
}

internal class ChangeBorderColorOperation(Figure figure, Color newColor) : CanvasOperation
{
    private readonly Color _oldColor = figure.BorderColor;

    public override void Execute(Canvas canvas)
    {
        canvas.ChangeBorderColor(figure, newColor, IsNewOperation);
    }

    public override void Undo(Canvas canvas)
    {
        canvas.ChangeBorderColor(figure, _oldColor, IsNewOperation);
    }
}