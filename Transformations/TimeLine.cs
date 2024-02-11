namespace Transformations;

public class TimeLine(Control pictureBox, Graphics timeLineGraphics, int duration = TimeLine.DefaultDuration, int fps = TimeLine.DefaultFps)
{
    private const int DefaultFps = 30;
    public const int DefaultDuration = 3; // in seconds
    private const int FrameHeightOdd = 10; // height of line for odd frames
    private const int FrameHeightEven = 16; // height of line for even frames
    private const int FrameHeightSecond = 24; // height of line for each second
    private const int Padding = 10;
    private readonly int _totalFrames = fps * duration + 1;
    private int _currentFrame;
    private bool _isAtPlayHead;
    private bool _isAtAddKeyFrameIcon;
    private bool _isDraggingPlayHead;
    private readonly List<KeyFrame> _keyFrames = [];

    public bool CanReset() => _keyFrames.Count > 0;
    public bool CanAddKeyFrame() => _keyFrames.All(kf => kf.Frame != _currentFrame);

    public void AddKeyFrame(KeyFrame keyFrame)
    {
        _keyFrames.Add(keyFrame);
    }
    
    public void MoveCursor(int frame)
    {
        _currentFrame = frame;
    }
    
    public void MoveCursorLeft()
    {
        // If there are available frames to move to
        if (_currentFrame <= 0) return;
        _currentFrame--;
        var clientPoint = pictureBox.PointToClient(Cursor.Position);
        HandleCursorChange(new MouseEventArgs(MouseButtons.None, 0, clientPoint.X, clientPoint.Y, 0));
        Draw();
    }
    
    public void MoveCursorRight()
    {
        // If there are available frames to move to
        if (_currentFrame >= fps * duration) return;
        _currentFrame++;
        var clientPoint = pictureBox.PointToClient(Cursor.Position);
        HandleCursorChange(new MouseEventArgs(MouseButtons.None, 0, clientPoint.X, clientPoint.Y, 0));
        Draw();
    }
    private void MoveCursorToStart()
    {
        _currentFrame = 0;
    }
    
    public void Play()
    {
        // TODO: Implement animation logic
    }
    
    public void Reset()
    {
        _keyFrames.Clear();
        
        // Move the cursor to the start
        MoveCursorToStart();
        
        // Draw the time line
        Draw();
    }

    public void HandleMouseMove(MouseEventArgs e)
    {
        if (_isDraggingPlayHead)
        {
            HandlePlayHeadDragging(e);
            return;
        }
        
        HandleCursorChange(e);
    }
    
    private void HandlePlayHeadDragging(MouseEventArgs e)
    {
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / (_totalFrames - 1);
        var cursorX = e.Location.X;
        var frame = (int)((cursorX - Padding) / frameWidth);
        MoveCursor(frame);
        Draw();
    }
    
    private void HandleCursorChange(MouseEventArgs e)
    {
        if (IsCursorAtPlayHead(e.Location))
        {
            pictureBox.Cursor = Cursors.Hand;
            _isAtPlayHead = true;
            _isAtAddKeyFrameIcon = false;
            return;
        }
        if (IsCursorAtAddKeyFrameIcon(e.Location))
        {
            pictureBox.Cursor = Cursors.Hand;
            _isAtPlayHead = false;
            _isAtAddKeyFrameIcon = true;
            return;
        }
        pictureBox.Cursor = Cursors.Default;
        _isAtPlayHead = false;
        _isAtAddKeyFrameIcon = false;
    }
    
    private bool IsCursorAtPlayHead(Point cursorLocation)
    {
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / (_totalFrames - 1);
        var cursorX = _currentFrame * frameWidth + Padding;
        var playHeadRectangle = new Rectangle((int)cursorX - 5, 0, 10, 15);

        return playHeadRectangle.Contains(cursorLocation);
    }
    
    private bool IsCursorAtAddKeyFrameIcon(Point cursorLocation)
    {
        var iconX = pictureBox.Width - 12 - Padding;
        var iconY = pictureBox.Height - 12 - Padding;
        var addKeyFrameIconRectangle = new Rectangle(iconX, iconY, 12, 12);

        return addKeyFrameIconRectangle.Contains(cursorLocation);
    }
    
    public void HandleClick(EventArgs e)
    {
        if (_isDraggingPlayHead)
        {
            HandlePlayHeadDragOnClick(e);
            return;
        }
    }

    private void HandlePlayHeadDragOnClick(EventArgs e)
    {
        var mouseEvent = (MouseEventArgs)e;
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / (_totalFrames - 1);
        var cursorX = mouseEvent.Location.X;
        var frame = (int)((cursorX - Padding) / frameWidth);
        MoveCursor(frame);
        _isDraggingPlayHead = false;
    }

    public void HandleMouseDown(MouseEventArgs e)
    {
        if (_isAtPlayHead)
        {
            _isDraggingPlayHead = true;
        }
    }
    
    public void HandleMouseUp(MouseEventArgs e)
    {
        _isDraggingPlayHead = false;
        
        Draw();
    }

    public void Draw()
    {
        timeLineGraphics.Clear(pictureBox.BackColor);
        
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / (_totalFrames - 1);
        
        for (var frame = 0; frame < _totalFrames; frame++)
        {
            var x = frame * frameWidth + Padding;
            
            DrawFrameLine(frame, x);

            if (_keyFrames.All(kf => kf.Frame != frame)) continue; // if the frame is a keyframe
            
            // draw a diamond at the middle of the PictureBox at the cursor
            var middleY = pictureBox.Height / 2;
            var frameKeyFrames = new[]
            {
                new PointF(x, middleY - frameWidth / 2),
                new PointF(x + frameWidth / 2, middleY),
                new PointF(x + frameWidth, middleY - frameWidth / 2),
                new PointF(x + frameWidth / 2, middleY - frameWidth),
                new PointF(x, middleY - frameWidth / 2)
            };
            timeLineGraphics.FillPolygon(Brushes.White, frameKeyFrames);
        }

        // Draw the play head at the current frame
        DrawPlayHead(frameWidth, Padding);
        
        // Draw the add key frame icon
        DrawAddKeyFrameIcon(Padding);
        
        // Refresh the PictureBox
        pictureBox.Invalidate();
    }
    
    private void DrawFrameLine(int frame, float x)
    {
        int frameHeight;
        Pen pen;
        if (frame % fps == 0) // each second
        {
            frameHeight = FrameHeightSecond;
            pen = Pens.White;
        }
        else if (frame % 2 == 0) // even frames
        {
            frameHeight = FrameHeightEven;
            pen = new Pen(Color.Gray);
        }
        else // odd frames
        {
            frameHeight = FrameHeightOdd;
            pen = new Pen(Color.Gray);
        }
        timeLineGraphics.DrawLine(pen, x, 0, x, frameHeight);
    }
    
    private void DrawPlayHead(float frameWidth, int padding)
    {
        var cursorX = _currentFrame * frameWidth + padding;
        var cursorHeight = pictureBox.Height;
        timeLineGraphics.DrawLine(Pens.Red, cursorX, 0, cursorX, cursorHeight);

        // Draw the marker at the top of the play head
        // Draw a rectangle
        const int rectangleHeight = 5; 
        timeLineGraphics.FillRectangle(Brushes.Red, cursorX - 5, 0, 10, rectangleHeight);

        // Draw a downward-facing triangle
        var trianglePoints = new[]
        {
            new PointF(cursorX - 5, 0 + rectangleHeight), // left point
            new PointF(cursorX + 5, 0 + rectangleHeight), // right point
            new PointF(cursorX, 0 + rectangleHeight + 10)  // bottom point
        };
        timeLineGraphics.FillPolygon(Brushes.Red, trianglePoints);
    }
    
    private void DrawAddKeyFrameIcon(int padding)
    {
        // Draw the icon at the bottom right corner of the PictureBox
        var iconX = pictureBox.Width - 12 - padding;
        var iconY = pictureBox.Height - 12 - padding;

        var keyFramePoints = new[]
        {
            new PointF(iconX, iconY + 6),
            new PointF(iconX + 6, iconY),
            new PointF(iconX + 12, iconY + 6),
            new PointF(iconX + 6, iconY + 12),
            new PointF(iconX, iconY + 6)
        };
        timeLineGraphics.FillPolygon(Brushes.White, keyFramePoints);
        
        const string text = "Add KeyFrame";
        var font = new Font("Segoe UI", 9);
        var brush = Brushes.White;
        var textX = iconX - timeLineGraphics.MeasureString(text, font).Width - 2;
        var textY = iconY + 6 - timeLineGraphics.MeasureString(text, font).Height / 2; // vertically center the text
        timeLineGraphics.DrawString(text, font, brush, textX, textY);
    }
}