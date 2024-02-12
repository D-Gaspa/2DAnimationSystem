using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Transformations;

public class TimeLine(
    Control pictureBox,
    Graphics timeLineGraphics,
    Canvas canvas,
    Form1 form,
    int duration = TimeLine.DefaultDuration,
    int fps = TimeLine.DefaultFps)
{
    private const int DefaultFps = 30;
    public const int DefaultDuration = 3; // in seconds
    private const int FrameHeightOdd = 10; // height of line for odd frames
    private const int FrameHeightEven = 16; // height of line for even frames
    private const int FrameHeightSecond = 24; // height of line for each second
    private const int Padding = 10;
    private readonly int _totalFrames = fps * duration;
    public int CurrentFrame;
    private bool _isAtPlayHead;
    private bool _isAtKeyFrame;
    private bool _isAtAddKeyFrameIcon;
    private bool _isDraggingPlayHead;
    private bool _isDraggingKeyFrame;
    private KeyFrame? _draggedKeyFrame;
    private int _oldDraggedKeyFrame;
    public readonly List<KeyFrame?> KeyFrames = [];
    
    public bool CanReset() => KeyFrames.Count > 0;
    public bool CanPlay() => KeyFrames.Count > 1;

    private void AddKeyFrame(int frame)
    {
        // Create a new KeyFrame with the current frame number and the current state of the figures and unselect them
        var keyFrame = new KeyFrame(frame) { Figures = canvas.Figures.Select(f => f.Clone()).ToList() };
        keyFrame.Figures.ForEach(f => f.IsSelected = false);

        var addKeyFrameOperation = new AddKeyFrameOperation(keyFrame, this)
        {
            IsNewOperation = true
        };
        addKeyFrameOperation.Execute(canvas);
        
        canvas.UndoStack.Push(addKeyFrameOperation);
        
        form.UpdateButtonStates();
        
        Draw();
    }

    private bool CanAddKeyFrame()
    {
        return KeyFrames.All(kf => kf != null && kf.Frame != CurrentFrame) && canvas.Figures is { Count: > 0 };
    }

    private void MoveCursor(int frame)
    {
        CurrentFrame = frame;
        Draw();
    }
    
    public void MoveCursorLeft()
    {
        // If there are available frames to move to
        if (CurrentFrame <= 0) return;
        CurrentFrame--;
        var clientPoint = pictureBox.PointToClient(Cursor.Position);
        HandleCursorChange(new MouseEventArgs(MouseButtons.None, 0, clientPoint.X, clientPoint.Y, 0));
        
        Draw();
    }
    
    public void MoveCursorRight()
    {
        // If there are available frames to move to
        if (CurrentFrame >= fps * duration) return;
        CurrentFrame++;
        var clientPoint = pictureBox.PointToClient(Cursor.Position);
        HandleCursorChange(new MouseEventArgs(MouseButtons.None, 0, clientPoint.X, clientPoint.Y, 0));
        Draw();
    }
    
    private void MoveCursorToStart()
    {
        CurrentFrame = 0;
    }

    private void MoveKeyFrame(KeyFrame keyFrame, int newFrame)
    {
        // Check if the new frame is already occupied by another keyframe
        if (KeyFrames.Any(kf => kf != null && kf.Frame == newFrame)) return;
        
        // Move the keyframe to the new frame
        keyFrame.Frame = newFrame;
        
        Draw();
    }

    public async Task Play(Graphics canvasGraphics, PictureBox canvasPictureBox)
    {
        var isStartKeyFrameAdded = false;
        var isEndKeyFrameAdded = false;
        
        // Sort the keyframes by their frame number
        KeyFrames.Sort((a, b) =>
        {
            Debug.Assert(a != null, nameof(a) + " != null");
            Debug.Assert(b != null, nameof(b) + " != null");
            return a.Frame.CompareTo(b.Frame);
        });
        
        // Sort the keyframes by their frame number
        KeyFrames.Sort((a, b) =>
        {
            Debug.Assert(a != null, nameof(a) + " != null");
            Debug.Assert(b != null, nameof(b) + " != null");
            return a.Frame.CompareTo(b.Frame);
        });

        // Check if there is a keyframe at the beginning
        if (KeyFrames.All(kf => kf != null && kf.Frame != 0))
        {
            // If not, create a keyframe at the beginning with the same figures as the next keyframe
            var nextKeyFrame = KeyFrames[0];
            var startKeyFrame = new KeyFrame(0) { Figures = nextKeyFrame!.Figures!.Select(f => f.Clone()).ToList(), IsVisible = false };
            KeyFrames.Insert(0, startKeyFrame);
            isStartKeyFrameAdded = true;
        }

        // Check if there is a keyframe at the end
        if (KeyFrames.All(kf => kf != null && kf.Frame != _totalFrames))
        {
            // If not, create a keyframe at the end with the same figures as the last keyframe
            var lastKeyFrame = KeyFrames[^1];
            var endKeyFrame = new KeyFrame(_totalFrames) { Figures = lastKeyFrame!.Figures!.Select(f => f.Clone()).ToList(), IsVisible = false };
            KeyFrames.Add(endKeyFrame);
            isEndKeyFrameAdded = true;
        }
        
        // Disable the buttons on the form
        form.DisableButtons();
        
        // Disable mouse events on the canvas
        canvasPictureBox.Enabled = false;
        
        // Iterate over each pair of consecutive keyframes
        for (var i = 0; i < KeyFrames.Count - 1; i++)
        {
            var startKeyFrame = KeyFrames[i];
            var endKeyFrame = KeyFrames[i + 1];

            // Create a dictionary for each keyframe
            var startFiguresDict = startKeyFrame?.Figures!.ToDictionary(f => f.Name, f => f);
            var endFiguresDict = endKeyFrame?.Figures!.ToDictionary(f => f.Name, f => f);

            // Calculate the number of frames in this animation segment
            var segmentFrames = endKeyFrame!.Frame - startKeyFrame!.Frame;

            // Iterate over each frame in the animation segment

            // Iterate over each frame in the animation segment
            for (var frame = 0; frame <= segmentFrames; frame++)
            {
                // Measure the start time
                var startTime = DateTime.Now;

                // Update the current frame
                CurrentFrame = startKeyFrame.Frame + frame;

                // Calculate the interpolation factor
                var t = (float)frame / segmentFrames;
                
                // Apply the easing function to the interpolation factor
                t = t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
                
                // Clear the canvas before drawing the interpolated figures
                canvasGraphics.Clear(Color.Black);

                // Interpolate between the states of the figures in the two keyframes
                foreach (var startFigureName in startFiguresDict?.Keys!)
                {
                    var startFigure = startFiguresDict[startFigureName].Clone(); // Create a copy of the startFigure
                    
                    // If the end keyframe does not contain a figure with the same name, continue to the next figure
                    if (!endFiguresDict!.ContainsKey(startFigureName)) continue;

                    var endFigure = endFiguresDict[startFigureName].Clone();  // Create a copy of the endFigure
                    
                    // Interpolate the size (scaling) of the figure
                    var startBounds = startFigure.GetBounds();
                    RectangleF endBounds;
                    
                    // If the figures angle is not the same, correct the end figure to get the right bounds
                    if (Math.Abs(startFigure.RotationAngle - endFigure.RotationAngle) > 0.00005)
                    {
                        // Calculate the difference in the angles
                        var angleDifference = endFigure.RotationAngle - startFigure.RotationAngle;
                        var endFigureCorrected = endFigure.Clone();
                        endFigureCorrected.Rotate(-angleDifference);
                        
                        endFigureCorrected.Rotate(-endFigure.RotationAngle);
                        endBounds = endFigureCorrected.GetBounds();
                    }
                    else
                    {
                        endBounds = endFigure.GetBounds();
                    }
                    
                    var interpolatedBounds = Interpolate(startBounds, endBounds, t);

                    // Apply interpolated bounds to the figure
                    startFigure.SetBounds(interpolatedBounds);
                    
                    // Apply flips based on flags and threshold
                    if (startFigure.HasFlippedX != endFigure.HasFlippedX && t > 0.5) 
                    {
                        startFigure.Flip(true, false); 
                    }
                    if (startFigure.HasFlippedY != endFigure.HasFlippedY && t > 0.5) 
                    {
                        startFigure.Flip(false, true); 
                    }

                    // Interpolate the position (translation) of the figure
                    var dx = (endFigure.Pivot.X - startFigure.Pivot.X) * t;
                    var dy = (endFigure.Pivot.Y - startFigure.Pivot.Y) * t;
                    startFigure.Translate(dx, dy);
                    
                    // Interpolate the rotation of the figure
                    var startAngle = startFigure.RotationAngle;
                    var endAngle = endFigure.RotationAngle;
                    var angle = startAngle + (endAngle - startAngle) * t;
                    startFigure.Rotate(angle - startAngle);

                    // Interpolate the colors of the figure
                    var fillColor = Interpolate(startFigure.FillColor, endFigure.FillColor, t);
                    var borderColor = Interpolate(startFigure.BorderColor, endFigure.BorderColor, t);

                    // Update the figure with the interpolated values
                    startFigure.FillColor = fillColor;
                    startFigure.BorderColor = borderColor;

                    // Draw the interpolated figure
                    startFigure.Draw(canvasGraphics);
                }

                Draw();

                canvasPictureBox.Invalidate();

                // Measure the end time
                var endTime = DateTime.Now;

                // Calculate the time it took to perform the calculations and rendering
                var renderTime = (endTime - startTime).Milliseconds;

                // Calculate the delay, subtracting the render time
                var delay = Math.Max(0, 1000 / fps - renderTime);

                // Wait for the next frame
                await Task.Delay(delay);
            }
        }
        
        // Remove the start and end keyframes if they were added
        if (isStartKeyFrameAdded)
        {
            KeyFrames.RemoveAt(0);
        }
        if (isEndKeyFrameAdded)
        {
            KeyFrames.RemoveAt(KeyFrames.Count - 1);
        }
        
        // Regain control of the form and the canvas
        canvasGraphics.Clear(Color.Black);
        canvasPictureBox.Enabled = true;
        form.RenderFigures();
        form.UpdateAllButtonStates();
        canvasPictureBox.Invalidate();
    }

    private static Color Interpolate(Color start, Color end, float t)
    {
        var a = (int)(start.A + t * (end.A - start.A));
        var r = (int)(start.R + t * (end.R - start.R));
        var g = (int)(start.G + t * (end.G - start.G));
        var b = (int)(start.B + t * (end.B - start.B));
        return Color.FromArgb(a, r, g, b);
    }
    
    private static RectangleF Interpolate(RectangleF startBounds, RectangleF endBounds, float t)
    {
        var interpolatedX = startBounds.X + (endBounds.X - startBounds.X) * t;
        var interpolatedY = startBounds.Y + (endBounds.Y - startBounds.Y) * t;
        var interpolatedWidth = startBounds.Width + (endBounds.Width - startBounds.Width) * t;
        var interpolatedHeight = startBounds.Height + (endBounds.Height - startBounds.Height) * t;

        return new RectangleF(interpolatedX, interpolatedY, interpolatedWidth, interpolatedHeight);
    }

    public void Reset()
    {
        KeyFrames.Clear();
        
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
        
        if (_isDraggingKeyFrame)
        {
            HandleKeyFrameDragging(e);
            return;
        }
        
        HandleCursorChange(e);
    }
    
    private void HandlePlayHeadDragging(MouseEventArgs e)
    {
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / _totalFrames;
        var cursorX = e.Location.X;
        var frame = (int)((cursorX - Padding) / frameWidth);
        
        // Ensure the frame number is within the valid range
        frame = Math.Max(0, Math.Min(frame, _totalFrames));
        
        MoveCursor(frame);
    }
    
    private void HandleKeyFrameDragging(MouseEventArgs e)
    {
        if (_draggedKeyFrame == null) return;

        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / _totalFrames;
        var cursorX = e.Location.X;
        var newFrame = (int)((cursorX - Padding) / frameWidth);
        
        // Ensure the frame number is within the valid range
        newFrame = Math.Max(0, Math.Min(newFrame, _totalFrames));
        
        // Find the keyframe at the current cursor position
        MoveKeyFrame(_draggedKeyFrame, newFrame);
    }
    
    private void HandleCursorChange(MouseEventArgs e)
    {
        if (IsCursorAtPlayHead(e.Location))
        {
            pictureBox.Cursor = Cursors.Hand;
            _isAtPlayHead = true;
            _isAtAddKeyFrameIcon = false;
            _isAtKeyFrame = false;
            return;
        }
        
        if (IsCursorAtAddKeyFrameIcon(e.Location))
        {
            pictureBox.Cursor = Cursors.Hand;
            _isAtPlayHead = false;
            _isAtAddKeyFrameIcon = true;
            _isAtKeyFrame = false;
            return;
        }

        if (IsCursorAtKeyFrame(e.Location))
        {
            pictureBox.Cursor = Cursors.Hand;
            _isAtPlayHead = false;
            _isAtAddKeyFrameIcon = false;
            _isAtKeyFrame = true;
            return;
        }
        
        pictureBox.Cursor = Cursors.Default;
        _isAtPlayHead = false;
        _isAtAddKeyFrameIcon = false;
        _isAtKeyFrame = false;
    }
    
    private bool IsCursorAtPlayHead(Point cursorLocation)
    {
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / _totalFrames;
        var cursorX = CurrentFrame * frameWidth + Padding;
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
    
    private bool IsCursorAtKeyFrame(Point cursorLocation)
    {
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / _totalFrames;
        var middleY = pictureBox.Height / 2;
        var halfDiamondWidth = frameWidth / 2;

        foreach (var keyFrame in KeyFrames)
        {
            if (keyFrame != null)
            {
                var x = keyFrame.Frame * frameWidth + Padding;
                var diamondPoints = new[] { new PointF(x - halfDiamondWidth, middleY), new PointF(x, middleY + halfDiamondWidth), new PointF(x + halfDiamondWidth, middleY), new PointF(x, middleY - halfDiamondWidth), new PointF(x - halfDiamondWidth, middleY) };
                using var path = new GraphicsPath();
                path.AddPolygon(diamondPoints);

                if (!path.IsVisible(cursorLocation)) continue;
            }

            _draggedKeyFrame = keyFrame;
            if (keyFrame != null) _oldDraggedKeyFrame = keyFrame.Frame;
            return true;
        }

        return false;
    }
    
    public void HandleClick()
    {
        if (_isDraggingKeyFrame)
        {
            HandleKeyFrameDragOnClick();
            return;
        }
        
        if (_isAtAddKeyFrameIcon)
        {
            HandleAddKeyFrameOnClick();
        }
    }
    
    private void HandleKeyFrameDragOnClick()
    {
        if (_draggedKeyFrame == null) return;
        var keyFrame = _draggedKeyFrame;
        
        var changeKeyFrameOperation = new ChangeKeyFrameOperation(keyFrame, this, _oldDraggedKeyFrame, keyFrame.Frame)
        {
            IsNewOperation = true
        };
        changeKeyFrameOperation.Execute(canvas);
        
        canvas.UndoStack.Push(changeKeyFrameOperation);
        
        form.UpdateButtonStates();
        
        Draw();
    }
    
    private void HandleAddKeyFrameOnClick()
    {
        if (!CanAddKeyFrame())
        {
            MessageBox.Show(@"Cannot add a key frame at the current frame.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        AddKeyFrame(CurrentFrame);
        Draw();
    }

    public void HandleMouseDown()
    {
        if (_isAtPlayHead)
        {
            _isDraggingPlayHead = true;
            return;
        }
        if (_isAtKeyFrame)
        {
            _isDraggingKeyFrame = true;
        }
    }
    
    public void HandleMouseUp()
    {
        _isDraggingPlayHead = false;
        _isDraggingKeyFrame = false;
        _draggedKeyFrame = null;
        _oldDraggedKeyFrame = 0;
        
        Draw();
    }

    public void Draw()
    {
        timeLineGraphics.Clear(pictureBox.BackColor);
        
        var frameWidth = (float)(pictureBox.Width - 2 * Padding) / _totalFrames;
        
        for (var frame = 0; frame < _totalFrames + 1; frame++)
        {
            var x = frame * frameWidth + Padding;
            
            DrawFrameLine(frame, x);

            if (KeyFrames.All(kf => kf != null && kf.Frame != frame || !kf!.IsVisible)) continue; // if the frame is a keyframe
            
            // draw a diamond at the middle of the PictureBox at the cursor
            var middleY = pictureBox.Height / 2;
            var halfDiamondWidth = frameWidth / 2;
            var frameKeyFrames = new[]
            {
                new PointF(x - halfDiamondWidth, middleY),
                new PointF(x, middleY + halfDiamondWidth),
                new PointF(x + halfDiamondWidth, middleY),
                new PointF(x, middleY - halfDiamondWidth),
                new PointF(x - halfDiamondWidth, middleY)
            };
            timeLineGraphics.FillPolygon(Brushes.White, frameKeyFrames);
        }

        // Draw the play head at the current frame
        DrawPlayHead(frameWidth, Padding);
        
        // Draw the add key frame icon
        DrawAddKeyFrameIcon(Padding);
        
        // Refresh the PictureBox
        pictureBox.Invalidate();
        
        form.UpdateTimeLineButtonStates();
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
        var cursorX = CurrentFrame * frameWidth + padding;
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