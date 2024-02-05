using System.Drawing.Drawing2D;

namespace Transformations;

public abstract class Figure
{
    public string Name { get; }
    protected PointF[] Points;
    private PointF _pivot;
    public Color BorderColor { get; set; } = Color.White;
    public Color FillColor { get; set; } = Color.FromArgb(128, Color.White);
    public bool IsSelected { get; set; }
    public bool HasFlipped { get; set; }
    public ResizePosition PreviousResizePosition { get; set; }
    
    protected Figure(PointF[] points, PointF position, PointF pivotOffset, string name)
    {
        Name = name;
        Points = points;
        CalculatePivot(pivotOffset); // Calculate the pivot point
        AdjustPositionToPivot(position); // Adjust the position to match the pivot point
    }

    public void CalculatePivot(PointF pivotOffset)
    {
        _pivot = new PointF(Points.Average(p => p.X) + pivotOffset.X, Points.Average(p => p.Y) + pivotOffset.Y);
    }

    private void AdjustPositionToPivot(PointF position)
    {
        Translate(position.X - _pivot.X, position.Y - _pivot.Y);
        _pivot = position; // Adjust pivot to match the specified position
    }

    public void Rotate(double angle)
    {
        var radiansAngle = angle * Math.PI / 180;
        Points = Points.Select(p =>
        {
            var x = p.X - _pivot.X;
            var y = p.Y - _pivot.Y;

            var newX = x * Math.Cos(radiansAngle) - y * Math.Sin(radiansAngle) + _pivot.X;
            var newY = x * Math.Sin(radiansAngle) + y * Math.Cos(radiansAngle) + _pivot.Y;

            return new PointF((float)newX, (float)newY);
        }).ToArray();
    }

    public void Translate(double dx, double dy)
    {
        Points = Points.Select(p => new PointF((float)(p.X + dx), (float)(p.Y + dy))).ToArray();
        _pivot = new PointF((float)(_pivot.X + dx), (float)(_pivot.Y + dy));
    }

    public void Scale(double sx, double sy)
    {
        Points = Points.Select(p =>
        {
            var x = (p.X - _pivot.X) * sx + _pivot.X;
            var y = (p.Y - _pivot.Y) * sy + _pivot.Y;

            return new PointF((float)x, (float)y);
        }).ToArray();
    }
    
    public bool IsInsideFigure(PointF point)
    {
        using var path = new GraphicsPath();
        path.AddPolygon(Points);
        return path.IsVisible(point);
    }
    
    public RectangleF GetBounds()
    {
        var minX = Points.Min(p => p.X);
        var minY = Points.Min(p => p.Y);
        var maxX = Points.Max(p => p.X);
        var maxY = Points.Max(p => p.Y);

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    private IEnumerable<PointF> GetSelectionPoints()
    {
        var bounds = GetBounds();

        return
        [
            new PointF(bounds.Left, bounds.Top), // Top-left corner
            new PointF(bounds.Right, bounds.Top), // Top-right corner
            new PointF(bounds.Right, bounds.Bottom), // Bottom-right corner
            new PointF(bounds.Left, bounds.Bottom), // Bottom-left corner
            new PointF(bounds.Left + bounds.Width / 2, bounds.Top), // Top side
            new PointF(bounds.Right, bounds.Top + bounds.Height / 2), // Right side
            new PointF(bounds.Left + bounds.Width / 2, bounds.Bottom), // Bottom side
            new PointF(bounds.Left, bounds.Top + bounds.Height / 2) // Left side
        ];
    }
    
    public Figure Clone()
    {
        return (Figure)MemberwiseClone();
    }
    
    public void Flip(bool needsXFlip, bool needsYFlip)
    {
        var bounds = GetBounds();
        var pivot = new PointF(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        var flipMatrix = new Matrix();
        flipMatrix.Translate(pivot.X, pivot.Y);
        flipMatrix.Scale(needsXFlip ? -1 : 1, needsYFlip ? -1 : 1);
        flipMatrix.Translate(-pivot.X, -pivot.Y);
        flipMatrix.TransformPoints(Points);
        
        var pivotPoints = new[] { _pivot };
        flipMatrix.TransformPoints(pivotPoints);
        _pivot = pivotPoints[0];
    }
    
    public virtual void Draw(Graphics g)
    {
        // Draw the figure
        using var fillBrush = new SolidBrush(FillColor);
        g.FillPolygon(fillBrush, Points);

        using var borderPen = new Pen(BorderColor);
        g.DrawPolygon(borderPen, Points);

        // Draw the pivot point as a small circle
        const float pivotSize = 5; // Size of the pivot circle
        if (_pivot.X - pivotSize / 2 >= 0 && _pivot.Y - pivotSize / 2 >= 0 && _pivot.X + pivotSize / 2 <= g.VisibleClipBounds.Width && _pivot.Y + pivotSize / 2 <= g.VisibleClipBounds.Height)
        {
            using var pivotPen = new Pen(Color.Red);
            g.DrawEllipse(pivotPen, _pivot.X - pivotSize / 2, _pivot.Y - pivotSize / 2, pivotSize, pivotSize);
        }

        // Draw the selection points and the selection rectangle
        if (!IsSelected) return;
        var bounds = GetBounds();
        var pen = new Pen(Color.White) { DashStyle = DashStyle.Dash };
        g.DrawRectangle(pen, bounds);

        var points = GetSelectionPoints();
        foreach (var point in points)
        {
            // Draw the selection points as small rectangles
            g.FillRectangle(Brushes.White, point.X - 4, point.Y - 4, 8, 8);
        }
    }
}

public class Square(double size, PointF position, string name, PointF pivotOffset = default)
    : Figure([
        new PointF((float)(position.X - size * 50), (float)(position.Y - size * 50)),
        new PointF((float)(position.X + size * 50), (float)(position.Y - size * 50)),
        new PointF((float)(position.X + size * 50), (float)(position.Y + size * 50)),
        new PointF((float)(position.X - size * 50), (float)(position.Y + size * 50))
    ], position, pivotOffset, name);

public class Triangle(double size, PointF position, string name, PointF pivotOffset = default)
    : Figure([
        position with { Y = (float)(position.Y - size * 50) },
        new PointF((float)(position.X - size * 50), (float)(position.Y + size * 50)),
        new PointF((float)(position.X + size * 50), (float)(position.Y + size * 50))
    ], position, pivotOffset, name);

public class CustomFigure(PointF[] points, string name)
    : Figure(points, CalculateCenter(points), new PointF(0, 0), name)
{
    private static PointF CalculateCenter(PointF[] points)
    {
        var x = points.Average(p => p.X);
        var y = points.Average(p => p.Y);
        return new PointF(x, y);
    }
}

public class UnfinishedCustomFigure(PointF[] points, string name)
    : Figure(points, CalculateCenter(points), new PointF(0, 0), name)
{
    private static PointF CalculateCenter(IReadOnlyCollection<PointF> points)
    {
        var x = points.Average(p => p.X);
        var y = points.Average(p => p.Y);
        return new PointF(x, y);
    }
    
    public void AddPoint(PointF point)
    {
        var newPoints = new PointF[Points.Length + 1];
        Points.CopyTo(newPoints, 0);
        newPoints[^1] = point;
        Points = newPoints;
    }
    
    public void RemoveLastPoint()
    {
        if (Points.Length <= 0) return;
        var newPoints = new PointF[Points.Length - 1];
        Array.Copy(Points, newPoints, Points.Length - 1);
        Points = newPoints;
    }
    
    // Override the Draw method to draw the figure
    public override void Draw(Graphics g)
    {
        // If there are 3 or more points, draw the figure
        if (Points.Length >= 3)
        {
            using var fillBrush = new SolidBrush(FillColor);
            g.FillPolygon(fillBrush, Points);
        }
        
        var pen = new Pen(BorderColor);
        // Draw the lines between the points
        for (var i = 0; i < Points.Length - 1; i++)
        {
            g.DrawLine(pen, Points[i], Points[i + 1]);
        }
        // Draw the points as small circles
        foreach (var point in Points)
        {
            g.FillEllipse(Brushes.Red, point.X - 2, point.Y - 2, 5, 5);
        }
    }
}