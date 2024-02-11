namespace Transformations
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            figuresCheckedListBox = new CheckedListBox();
            redoButton = new Button();
            undoButton = new Button();
            deleteButton = new Button();
            figuresComboBox = new ComboBox();
            sizeTextBox = new TextBox();
            sizeLabel = new Label();
            positionYTextBox = new TextBox();
            positionXTextBox = new TextBox();
            positionLabel = new Label();
            pivotOffsetYTextBox = new TextBox();
            pivotOffsetXTextBox = new TextBox();
            pivotOffsetLabel = new Label();
            addFigureButton = new Button();
            customPivotCheckBox = new CheckBox();
            addFigureCheckBox = new CheckBox();
            selectAllCheckBox = new CheckBox();
            borderColorDialog = new ColorDialog();
            fillColorDialog = new ColorDialog();
            borderColorButton = new Button();
            fillColorButton = new Button();
            resetButton = new Button();
            coordinatesLabel = new Label();
            addCustomFigureCheckBox = new CheckBox();
            addCustomFigureButton = new Button();
            resetCustomFigureButton = new Button();
            cancelCustomFigureButton = new Button();
            borderColorCustomFigureButton = new Button();
            fillColorCustomFigureButton = new Button();
            redoCustomFigureButton = new Button();
            undoCustomFigureButton = new Button();
            fillColorSelectedButton = new Button();
            borderColorSelectedButton = new Button();
            duplicateButton = new Button();
            timeLinePictureBox = new PictureBox();
            addTimeLineButton = new Button();
            resetTimeLineButton = new Button();
            playTimeLineButton = new Button();
            customTimeLineDurationCheckBox = new CheckBox();
            customTimeLineDurationLabel = new Label();
            customTimeLineDurationTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)timeLinePictureBox).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ActiveCaptionText;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(12, 37);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1178, 711);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.MouseClick += PictureBox1_Click;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            // 
            // figuresCheckedListBox
            // 
            figuresCheckedListBox.BackColor = SystemColors.WindowFrame;
            figuresCheckedListBox.ForeColor = SystemColors.Control;
            figuresCheckedListBox.FormattingEnabled = true;
            figuresCheckedListBox.Location = new Point(1202, 492);
            figuresCheckedListBox.Name = "figuresCheckedListBox";
            figuresCheckedListBox.Size = new Size(150, 136);
            figuresCheckedListBox.TabIndex = 10;
            figuresCheckedListBox.ItemCheck += FiguresCheckedListBox_ItemCheck;
            // 
            // redoButton
            // 
            redoButton.BackColor = SystemColors.GrayText;
            redoButton.Cursor = Cursors.Hand;
            redoButton.Font = new Font("Segoe UI Black", 9F);
            redoButton.ForeColor = SystemColors.Control;
            redoButton.Location = new Point(1283, 680);
            redoButton.Name = "redoButton";
            redoButton.Size = new Size(69, 30);
            redoButton.TabIndex = 13;
            redoButton.Text = "REDO";
            redoButton.UseVisualStyleBackColor = false;
            redoButton.Click += RedoButton_Click;
            // 
            // undoButton
            // 
            undoButton.BackColor = SystemColors.GrayText;
            undoButton.Cursor = Cursors.Hand;
            undoButton.Font = new Font("Segoe UI Black", 9F);
            undoButton.ForeColor = SystemColors.Control;
            undoButton.Location = new Point(1201, 680);
            undoButton.Name = "undoButton";
            undoButton.Size = new Size(68, 30);
            undoButton.TabIndex = 12;
            undoButton.Text = "UNDO";
            undoButton.UseVisualStyleBackColor = false;
            undoButton.Click += UndoButton_Click;
            // 
            // deleteButton
            // 
            deleteButton.BackColor = Color.Red;
            deleteButton.Cursor = Cursors.Hand;
            deleteButton.Font = new Font("Segoe UI Black", 9F);
            deleteButton.ForeColor = SystemColors.Control;
            deleteButton.Location = new Point(1201, 641);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(151, 30);
            deleteButton.TabIndex = 14;
            deleteButton.Text = "DELETE";
            deleteButton.UseVisualStyleBackColor = false;
            deleteButton.Click += DeleteButton_Click;
            // 
            // figuresComboBox
            // 
            figuresComboBox.BackColor = SystemColors.WindowFrame;
            figuresComboBox.Cursor = Cursors.Hand;
            figuresComboBox.Font = new Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            figuresComboBox.ForeColor = SystemColors.Control;
            figuresComboBox.FormattingEnabled = true;
            figuresComboBox.Location = new Point(1201, 93);
            figuresComboBox.Name = "figuresComboBox";
            figuresComboBox.Size = new Size(151, 28);
            figuresComboBox.TabIndex = 15;
            // 
            // sizeTextBox
            // 
            sizeTextBox.BackColor = Color.DimGray;
            sizeTextBox.Cursor = Cursors.IBeam;
            sizeTextBox.Font = new Font("Segoe UI Black", 10F);
            sizeTextBox.ForeColor = SystemColors.Control;
            sizeTextBox.Location = new Point(1272, 139);
            sizeTextBox.Name = "sizeTextBox";
            sizeTextBox.Size = new Size(80, 30);
            sizeTextBox.TabIndex = 17;
            // 
            // sizeLabel
            // 
            sizeLabel.AutoSize = true;
            sizeLabel.Font = new Font("Segoe UI Black", 10F);
            sizeLabel.ImageAlign = ContentAlignment.MiddleRight;
            sizeLabel.Location = new Point(1217, 139);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new Size(49, 23);
            sizeLabel.TabIndex = 16;
            sizeLabel.Text = "Size:";
            sizeLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // positionYTextBox
            // 
            positionYTextBox.BackColor = Color.DimGray;
            positionYTextBox.Cursor = Cursors.IBeam;
            positionYTextBox.Font = new Font("Segoe UI Black", 10F);
            positionYTextBox.ForeColor = SystemColors.Control;
            positionYTextBox.Location = new Point(1289, 209);
            positionYTextBox.Name = "positionYTextBox";
            positionYTextBox.Size = new Size(63, 30);
            positionYTextBox.TabIndex = 20;
            // 
            // positionXTextBox
            // 
            positionXTextBox.BackColor = Color.DimGray;
            positionXTextBox.Cursor = Cursors.IBeam;
            positionXTextBox.Font = new Font("Segoe UI Black", 10F);
            positionXTextBox.ForeColor = SystemColors.Control;
            positionXTextBox.Location = new Point(1201, 209);
            positionXTextBox.Name = "positionXTextBox";
            positionXTextBox.Size = new Size(63, 30);
            positionXTextBox.TabIndex = 19;
            // 
            // positionLabel
            // 
            positionLabel.AutoSize = true;
            positionLabel.Font = new Font("Segoe UI Black", 10F);
            positionLabel.Location = new Point(1209, 183);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new Size(134, 23);
            positionLabel.TabIndex = 18;
            positionLabel.Text = "Position (x, y):";
            positionLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pivotOffsetYTextBox
            // 
            pivotOffsetYTextBox.BackColor = Color.DimGray;
            pivotOffsetYTextBox.Cursor = Cursors.IBeam;
            pivotOffsetYTextBox.Font = new Font("Segoe UI Black", 10F);
            pivotOffsetYTextBox.ForeColor = SystemColors.Control;
            pivotOffsetYTextBox.Location = new Point(1289, 308);
            pivotOffsetYTextBox.Name = "pivotOffsetYTextBox";
            pivotOffsetYTextBox.Size = new Size(63, 30);
            pivotOffsetYTextBox.TabIndex = 23;
            // 
            // pivotOffsetXTextBox
            // 
            pivotOffsetXTextBox.BackColor = Color.DimGray;
            pivotOffsetXTextBox.Cursor = Cursors.IBeam;
            pivotOffsetXTextBox.Font = new Font("Segoe UI Black", 10F);
            pivotOffsetXTextBox.ForeColor = SystemColors.Control;
            pivotOffsetXTextBox.Location = new Point(1202, 308);
            pivotOffsetXTextBox.Name = "pivotOffsetXTextBox";
            pivotOffsetXTextBox.Size = new Size(63, 30);
            pivotOffsetXTextBox.TabIndex = 22;
            // 
            // pivotOffsetLabel
            // 
            pivotOffsetLabel.AutoSize = true;
            pivotOffsetLabel.Font = new Font("Segoe UI Black", 10F);
            pivotOffsetLabel.Location = new Point(1195, 282);
            pivotOffsetLabel.Name = "pivotOffsetLabel";
            pivotOffsetLabel.Size = new Size(166, 23);
            pivotOffsetLabel.TabIndex = 21;
            pivotOffsetLabel.Text = "Pivot Offset (x, y):";
            pivotOffsetLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // addFigureButton
            // 
            addFigureButton.BackColor = Color.MidnightBlue;
            addFigureButton.Cursor = Cursors.Hand;
            addFigureButton.Font = new Font("Segoe UI Black", 9F);
            addFigureButton.ForeColor = SystemColors.Control;
            addFigureButton.Location = new Point(1201, 425);
            addFigureButton.Name = "addFigureButton";
            addFigureButton.Size = new Size(151, 30);
            addFigureButton.TabIndex = 27;
            addFigureButton.Text = "ADD FIGURE";
            addFigureButton.UseVisualStyleBackColor = false;
            addFigureButton.Click += addFigureButton_Click;
            // 
            // customPivotCheckBox
            // 
            customPivotCheckBox.AutoSize = true;
            customPivotCheckBox.Cursor = Cursors.Hand;
            customPivotCheckBox.Location = new Point(1201, 253);
            customPivotCheckBox.Name = "customPivotCheckBox";
            customPivotCheckBox.Size = new Size(155, 24);
            customPivotCheckBox.TabIndex = 28;
            customPivotCheckBox.Text = "Custom Pivot Point";
            customPivotCheckBox.UseVisualStyleBackColor = true;
            customPivotCheckBox.CheckedChanged += customPivotCheckBox_CheckedChanged;
            // 
            // addFigureCheckBox
            // 
            addFigureCheckBox.AutoSize = true;
            addFigureCheckBox.Cursor = Cursors.Hand;
            addFigureCheckBox.Location = new Point(1202, 62);
            addFigureCheckBox.Name = "addFigureCheckBox";
            addFigureCheckBox.Size = new Size(104, 24);
            addFigureCheckBox.TabIndex = 29;
            addFigureCheckBox.Text = "Add Figure";
            addFigureCheckBox.UseVisualStyleBackColor = true;
            addFigureCheckBox.CheckedChanged += addFigureCheckBox_CheckedChanged;
            // 
            // selectAllCheckBox
            // 
            selectAllCheckBox.AutoSize = true;
            selectAllCheckBox.Cursor = Cursors.Hand;
            selectAllCheckBox.Location = new Point(1205, 468);
            selectAllCheckBox.Name = "selectAllCheckBox";
            selectAllCheckBox.Size = new Size(93, 24);
            selectAllCheckBox.TabIndex = 30;
            selectAllCheckBox.Text = "Select All";
            selectAllCheckBox.UseVisualStyleBackColor = true;
            selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;
            // 
            // borderColorButton
            // 
            borderColorButton.BackColor = Color.SteelBlue;
            borderColorButton.Cursor = Cursors.Hand;
            borderColorButton.Font = new Font("Segoe UI Black", 7F);
            borderColorButton.ForeColor = SystemColors.Control;
            borderColorButton.Location = new Point(1217, 350);
            borderColorButton.Name = "borderColorButton";
            borderColorButton.Size = new Size(123, 30);
            borderColorButton.TabIndex = 31;
            borderColorButton.Text = "BORDER COLOR";
            borderColorButton.UseVisualStyleBackColor = false;
            borderColorButton.Click += borderColorButton_Click;
            // 
            // fillColorButton
            // 
            fillColorButton.BackColor = Color.SteelBlue;
            fillColorButton.Cursor = Cursors.Hand;
            fillColorButton.Font = new Font("Segoe UI Black", 7F);
            fillColorButton.ForeColor = SystemColors.Control;
            fillColorButton.Location = new Point(1217, 386);
            fillColorButton.Name = "fillColorButton";
            fillColorButton.Size = new Size(123, 30);
            fillColorButton.TabIndex = 32;
            fillColorButton.Text = "FILL COLOR";
            fillColorButton.UseVisualStyleBackColor = false;
            fillColorButton.Click += fillColorButton_Click;
            // 
            // resetButton
            // 
            resetButton.BackColor = Color.Green;
            resetButton.Cursor = Cursors.Hand;
            resetButton.Font = new Font("Segoe UI Black", 9F);
            resetButton.ForeColor = SystemColors.Control;
            resetButton.Location = new Point(1202, 719);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(150, 30);
            resetButton.TabIndex = 33;
            resetButton.Text = "RESET";
            resetButton.UseVisualStyleBackColor = false;
            resetButton.Click += resetButton_Click;
            // 
            // coordinatesLabel
            // 
            coordinatesLabel.AutoSize = true;
            coordinatesLabel.Location = new Point(12, 6);
            coordinatesLabel.Name = "coordinatesLabel";
            coordinatesLabel.Size = new Size(89, 20);
            coordinatesLabel.TabIndex = 34;
            coordinatesLabel.Text = "Coordinates";
            // 
            // addCustomFigureCheckBox
            // 
            addCustomFigureCheckBox.AutoSize = true;
            addCustomFigureCheckBox.Cursor = Cursors.Hand;
            addCustomFigureCheckBox.Location = new Point(1202, 32);
            addCustomFigureCheckBox.Name = "addCustomFigureCheckBox";
            addCustomFigureCheckBox.Size = new Size(158, 24);
            addCustomFigureCheckBox.TabIndex = 35;
            addCustomFigureCheckBox.Text = "Add Custom Figure";
            addCustomFigureCheckBox.UseVisualStyleBackColor = true;
            addCustomFigureCheckBox.CheckedChanged += AddCustomFigureCheckBox_CheckedChanged;
            // 
            // addCustomFigureButton
            // 
            addCustomFigureButton.BackColor = Color.MidnightBlue;
            addCustomFigureButton.Cursor = Cursors.Hand;
            addCustomFigureButton.Font = new Font("Segoe UI Black", 9F);
            addCustomFigureButton.ForeColor = SystemColors.Control;
            addCustomFigureButton.Location = new Point(974, 6);
            addCustomFigureButton.Name = "addCustomFigureButton";
            addCustomFigureButton.Size = new Size(216, 30);
            addCustomFigureButton.TabIndex = 36;
            addCustomFigureButton.Text = "ADD CUSTOM FIGURE";
            addCustomFigureButton.UseVisualStyleBackColor = false;
            addCustomFigureButton.Click += AddCustomFigureButton_Click;
            // 
            // resetCustomFigureButton
            // 
            resetCustomFigureButton.BackColor = Color.Green;
            resetCustomFigureButton.Cursor = Cursors.Hand;
            resetCustomFigureButton.Font = new Font("Segoe UI Black", 9F);
            resetCustomFigureButton.ForeColor = SystemColors.Control;
            resetCustomFigureButton.Location = new Point(662, 6);
            resetCustomFigureButton.Name = "resetCustomFigureButton";
            resetCustomFigureButton.Size = new Size(150, 30);
            resetCustomFigureButton.TabIndex = 37;
            resetCustomFigureButton.Text = "RESET";
            resetCustomFigureButton.UseVisualStyleBackColor = false;
            resetCustomFigureButton.Click += ResetCustomFigureButton_Click;
            // 
            // cancelCustomFigureButton
            // 
            cancelCustomFigureButton.BackColor = Color.Red;
            cancelCustomFigureButton.Cursor = Cursors.Hand;
            cancelCustomFigureButton.Font = new Font("Segoe UI Black", 9F);
            cancelCustomFigureButton.ForeColor = SystemColors.Control;
            cancelCustomFigureButton.Location = new Point(817, 6);
            cancelCustomFigureButton.Name = "cancelCustomFigureButton";
            cancelCustomFigureButton.Size = new Size(151, 30);
            cancelCustomFigureButton.TabIndex = 38;
            cancelCustomFigureButton.Text = "CANCEL";
            cancelCustomFigureButton.UseVisualStyleBackColor = false;
            cancelCustomFigureButton.Click += CancelCustomFigureButton_Click;
            // 
            // borderColorCustomFigureButton
            // 
            borderColorCustomFigureButton.BackColor = Color.SteelBlue;
            borderColorCustomFigureButton.Cursor = Cursors.Hand;
            borderColorCustomFigureButton.Font = new Font("Segoe UI Black", 7F);
            borderColorCustomFigureButton.ForeColor = SystemColors.Control;
            borderColorCustomFigureButton.Location = new Point(221, 6);
            borderColorCustomFigureButton.Name = "borderColorCustomFigureButton";
            borderColorCustomFigureButton.Size = new Size(123, 30);
            borderColorCustomFigureButton.TabIndex = 39;
            borderColorCustomFigureButton.Text = "BORDER COLOR";
            borderColorCustomFigureButton.UseVisualStyleBackColor = false;
            borderColorCustomFigureButton.Click += borderColorCustomFigureButton_Click;
            // 
            // fillColorCustomFigureButton
            // 
            fillColorCustomFigureButton.BackColor = Color.SteelBlue;
            fillColorCustomFigureButton.Cursor = Cursors.Hand;
            fillColorCustomFigureButton.Font = new Font("Segoe UI Black", 7F);
            fillColorCustomFigureButton.ForeColor = SystemColors.Control;
            fillColorCustomFigureButton.Location = new Point(350, 6);
            fillColorCustomFigureButton.Name = "fillColorCustomFigureButton";
            fillColorCustomFigureButton.Size = new Size(123, 30);
            fillColorCustomFigureButton.TabIndex = 40;
            fillColorCustomFigureButton.Text = "FILL COLOR";
            fillColorCustomFigureButton.UseVisualStyleBackColor = false;
            fillColorCustomFigureButton.Click += fillColorCustomFigureButton_Click;
            // 
            // redoCustomFigureButton
            // 
            redoCustomFigureButton.BackColor = SystemColors.GrayText;
            redoCustomFigureButton.Cursor = Cursors.Hand;
            redoCustomFigureButton.Font = new Font("Segoe UI Black", 9F);
            redoCustomFigureButton.ForeColor = SystemColors.Control;
            redoCustomFigureButton.Location = new Point(570, 6);
            redoCustomFigureButton.Name = "redoCustomFigureButton";
            redoCustomFigureButton.Size = new Size(69, 30);
            redoCustomFigureButton.TabIndex = 42;
            redoCustomFigureButton.Text = "REDO";
            redoCustomFigureButton.UseVisualStyleBackColor = false;
            redoCustomFigureButton.Click += RedoCustomFigureButton_Click;
            // 
            // undoCustomFigureButton
            // 
            undoCustomFigureButton.BackColor = SystemColors.GrayText;
            undoCustomFigureButton.Cursor = Cursors.Hand;
            undoCustomFigureButton.Font = new Font("Segoe UI Black", 9F);
            undoCustomFigureButton.ForeColor = SystemColors.Control;
            undoCustomFigureButton.Location = new Point(496, 6);
            undoCustomFigureButton.Name = "undoCustomFigureButton";
            undoCustomFigureButton.Size = new Size(68, 30);
            undoCustomFigureButton.TabIndex = 41;
            undoCustomFigureButton.Text = "UNDO";
            undoCustomFigureButton.UseVisualStyleBackColor = false;
            undoCustomFigureButton.Click += UndoCustomFigureButton_Click;
            // 
            // fillColorSelectedButton
            // 
            fillColorSelectedButton.BackColor = Color.SteelBlue;
            fillColorSelectedButton.Cursor = Cursors.Hand;
            fillColorSelectedButton.Font = new Font("Segoe UI Black", 7F);
            fillColorSelectedButton.ForeColor = SystemColors.Control;
            fillColorSelectedButton.Location = new Point(910, 6);
            fillColorSelectedButton.Name = "fillColorSelectedButton";
            fillColorSelectedButton.Size = new Size(123, 30);
            fillColorSelectedButton.TabIndex = 45;
            fillColorSelectedButton.Text = "FILL COLOR";
            fillColorSelectedButton.UseVisualStyleBackColor = false;
            fillColorSelectedButton.Visible = false;
            fillColorSelectedButton.Click += fillColorSelectedButton_Click;
            // 
            // borderColorSelectedButton
            // 
            borderColorSelectedButton.BackColor = Color.SteelBlue;
            borderColorSelectedButton.Cursor = Cursors.Hand;
            borderColorSelectedButton.Font = new Font("Segoe UI Black", 7F);
            borderColorSelectedButton.ForeColor = SystemColors.Control;
            borderColorSelectedButton.Location = new Point(781, 6);
            borderColorSelectedButton.Name = "borderColorSelectedButton";
            borderColorSelectedButton.Size = new Size(123, 30);
            borderColorSelectedButton.TabIndex = 44;
            borderColorSelectedButton.Text = "BORDER COLOR";
            borderColorSelectedButton.UseVisualStyleBackColor = false;
            borderColorSelectedButton.Visible = false;
            borderColorSelectedButton.Click += borderColorSelectedButton_Click;
            // 
            // duplicateButton
            // 
            duplicateButton.BackColor = Color.MidnightBlue;
            duplicateButton.Cursor = Cursors.Hand;
            duplicateButton.Font = new Font("Segoe UI Black", 9F);
            duplicateButton.ForeColor = SystemColors.Control;
            duplicateButton.Location = new Point(1039, 6);
            duplicateButton.Name = "duplicateButton";
            duplicateButton.Size = new Size(151, 30);
            duplicateButton.TabIndex = 43;
            duplicateButton.Text = "DUPLICATE";
            duplicateButton.UseVisualStyleBackColor = false;
            duplicateButton.Visible = false;
            duplicateButton.Click += DuplicateButton_Click;
            // 
            // timeLinePictureBox
            // 
            timeLinePictureBox.BackColor = Color.FromArgb(40, 40, 40);
            timeLinePictureBox.BorderStyle = BorderStyle.FixedSingle;
            timeLinePictureBox.Location = new Point(172, 759);
            timeLinePictureBox.Name = "timeLinePictureBox";
            timeLinePictureBox.Size = new Size(1180, 75);
            timeLinePictureBox.TabIndex = 46;
            timeLinePictureBox.TabStop = false;
            timeLinePictureBox.Visible = false;
            timeLinePictureBox.Click += timeLinePictureBox_Click;
            timeLinePictureBox.MouseDown += timeLinePictureBox_MouseDown;
            timeLinePictureBox.MouseMove += timeLinePictureBox_MouseMove;
            timeLinePictureBox.MouseUp += timeLinePictureBox_MouseUp;
            // 
            // addTimeLineButton
            // 
            addTimeLineButton.BackColor = Color.MidnightBlue;
            addTimeLineButton.Cursor = Cursors.Hand;
            addTimeLineButton.Font = new Font("Segoe UI Black", 9F);
            addTimeLineButton.ForeColor = SystemColors.Control;
            addTimeLineButton.Location = new Point(12, 754);
            addTimeLineButton.Name = "addTimeLineButton";
            addTimeLineButton.Size = new Size(151, 30);
            addTimeLineButton.TabIndex = 49;
            addTimeLineButton.Text = "ADD TIMELINE";
            addTimeLineButton.UseVisualStyleBackColor = false;
            addTimeLineButton.Click += addTimeLineButton_Click;
            // 
            // resetTimeLineButton
            // 
            resetTimeLineButton.BackColor = Color.DarkGreen;
            resetTimeLineButton.Cursor = Cursors.Hand;
            resetTimeLineButton.Font = new Font("Segoe UI Black", 9F);
            resetTimeLineButton.ForeColor = SystemColors.Control;
            resetTimeLineButton.Location = new Point(12, 789);
            resetTimeLineButton.Name = "resetTimeLineButton";
            resetTimeLineButton.Size = new Size(150, 30);
            resetTimeLineButton.TabIndex = 52;
            resetTimeLineButton.Text = "RESET TIMELINE";
            resetTimeLineButton.UseVisualStyleBackColor = false;
            resetTimeLineButton.Visible = false;
            resetTimeLineButton.Click += resetTimeLineButton_Click;
            // 
            // playTimeLineButton
            // 
            playTimeLineButton.BackColor = Color.MidnightBlue;
            playTimeLineButton.Cursor = Cursors.Hand;
            playTimeLineButton.Font = new Font("Segoe UI Black", 9F);
            playTimeLineButton.ForeColor = SystemColors.Control;
            playTimeLineButton.Location = new Point(12, 754);
            playTimeLineButton.Name = "playTimeLineButton";
            playTimeLineButton.Size = new Size(151, 30);
            playTimeLineButton.TabIndex = 53;
            playTimeLineButton.Text = "PLAY TIMELINE";
            playTimeLineButton.UseVisualStyleBackColor = false;
            playTimeLineButton.Visible = false;
            playTimeLineButton.Click += playTimeLineButton_Click;
            // 
            // customTimeLineDurationCheckBox
            // 
            customTimeLineDurationCheckBox.AutoSize = true;
            customTimeLineDurationCheckBox.Cursor = Cursors.Hand;
            customTimeLineDurationCheckBox.Location = new Point(175, 757);
            customTimeLineDurationCheckBox.Name = "customTimeLineDurationCheckBox";
            customTimeLineDurationCheckBox.Size = new Size(288, 24);
            customTimeLineDurationCheckBox.TabIndex = 54;
            customTimeLineDurationCheckBox.Text = "Custom TimeLine Duration (Default 3s)";
            customTimeLineDurationCheckBox.UseVisualStyleBackColor = true;
            customTimeLineDurationCheckBox.CheckedChanged += customTimeLineDurationCheckBox_CheckedChanged;
            // 
            // customTimeLineDurationLabel
            // 
            customTimeLineDurationLabel.AutoSize = true;
            customTimeLineDurationLabel.Location = new Point(475, 758);
            customTimeLineDurationLabel.Name = "customTimeLineDurationLabel";
            customTimeLineDurationLabel.Size = new Size(154, 20);
            customTimeLineDurationLabel.TabIndex = 47;
            customTimeLineDurationLabel.Text = "TimeLine Duration (s):";
            customTimeLineDurationLabel.Visible = false;
            // 
            // customTimeLineDurationTextBox
            // 
            customTimeLineDurationTextBox.BackColor = Color.DimGray;
            customTimeLineDurationTextBox.BorderStyle = BorderStyle.FixedSingle;
            customTimeLineDurationTextBox.ForeColor = SystemColors.Control;
            customTimeLineDurationTextBox.Location = new Point(635, 756);
            customTimeLineDurationTextBox.Name = "customTimeLineDurationTextBox";
            customTimeLineDurationTextBox.Size = new Size(77, 27);
            customTimeLineDurationTextBox.TabIndex = 48;
            customTimeLineDurationTextBox.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1362, 869);
            Controls.Add(customTimeLineDurationCheckBox);
            Controls.Add(playTimeLineButton);
            Controls.Add(resetTimeLineButton);
            Controls.Add(addTimeLineButton);
            Controls.Add(customTimeLineDurationTextBox);
            Controls.Add(customTimeLineDurationLabel);
            Controls.Add(timeLinePictureBox);
            Controls.Add(fillColorSelectedButton);
            Controls.Add(borderColorSelectedButton);
            Controls.Add(duplicateButton);
            Controls.Add(redoCustomFigureButton);
            Controls.Add(undoCustomFigureButton);
            Controls.Add(fillColorCustomFigureButton);
            Controls.Add(borderColorCustomFigureButton);
            Controls.Add(cancelCustomFigureButton);
            Controls.Add(resetCustomFigureButton);
            Controls.Add(addCustomFigureButton);
            Controls.Add(addCustomFigureCheckBox);
            Controls.Add(coordinatesLabel);
            Controls.Add(resetButton);
            Controls.Add(fillColorButton);
            Controls.Add(borderColorButton);
            Controls.Add(selectAllCheckBox);
            Controls.Add(addFigureCheckBox);
            Controls.Add(customPivotCheckBox);
            Controls.Add(addFigureButton);
            Controls.Add(pivotOffsetYTextBox);
            Controls.Add(pivotOffsetXTextBox);
            Controls.Add(pivotOffsetLabel);
            Controls.Add(positionYTextBox);
            Controls.Add(positionXTextBox);
            Controls.Add(positionLabel);
            Controls.Add(sizeTextBox);
            Controls.Add(sizeLabel);
            Controls.Add(figuresComboBox);
            Controls.Add(deleteButton);
            Controls.Add(undoButton);
            Controls.Add(redoButton);
            Controls.Add(figuresCheckedListBox);
            Controls.Add(pictureBox1);
            DoubleBuffered = true;
            ForeColor = SystemColors.Control;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)timeLinePictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private CheckedListBox figuresCheckedListBox;
        private Button redoButton;
        private Button undoButton;
        private Button deleteButton;
        private ComboBox figuresComboBox;
        private TextBox sizeTextBox;
        private Label sizeLabel;
        private TextBox positionYTextBox;
        private TextBox positionXTextBox;
        private Label positionLabel;
        private TextBox pivotOffsetYTextBox;
        private TextBox pivotOffsetXTextBox;
        private Label pivotOffsetLabel;
        private Button addFigureButton;
        private CheckBox customPivotCheckBox;
        private CheckBox addFigureCheckBox;
        private CheckBox selectAllCheckBox;
        private ColorDialog borderColorDialog;
        private ColorDialog fillColorDialog;
        private Button borderColorButton;
        private Button fillColorButton;
        private Button resetButton;
        private Label coordinatesLabel;
        private CheckBox addCustomFigureCheckBox;
        private Button addCustomFigureButton;
        private Button resetCustomFigureButton;
        private Button cancelCustomFigureButton;
        private Button borderColorCustomFigureButton;
        private Button fillColorCustomFigureButton;
        private Button redoCustomFigureButton;
        private Button undoCustomFigureButton;
        private Button fillColorSelectedButton;
        private Button borderColorSelectedButton;
        private Button duplicateButton;
        private PictureBox timeLinePictureBox;
        private Button addTimeLineButton;
        private Button resetTimeLineButton;
        private Button playTimeLineButton;
        private CheckBox customTimeLineDurationCheckBox;
        private Label customTimeLineDurationLabel;
        private TextBox customTimeLineDurationTextBox;
    }
}
