using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Show progress step-by-step to the user")]
    public partial class cuiProgressTrackerHorizontal : Control
    {
        public cuiProgressTrackerHorizontal()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            Size = DefaultControlSize;
            TabStop = false;
        }

        protected virtual Size DefaultControlSize => new Size(480, 36);

        private string[] tasks = new string[] { "Task1", "Task2", "Task3", "Task4" };

        [Category("HartUI")]
        [Description("Tasks in text separated by new lines.")]
        public virtual string[] Tasks
        {
            get => tasks;
            set
            {
                tasks = value;
                OnTasksChanged();
                this.Invalidate();
            }
        }

        // the vertical version overrides this so that its painted correctly
        protected virtual void OnTasksChanged()
        {
        }

        public int privateTasksProgress = 2;

        [Category("HartUI")]
        [Description("How many tasks are completed.")]
        public int TasksProgress
        {
            get
            {
                return privateTasksProgress;
            }
            set
            {
                if (privateTasksProgress != value)
                {
                    privateTasksProgress = value;
                    Invalidate();
                }
            }
        }

        public int privateLineThickness = 4;

        [Category("HartUI")]
        public int LineThickness
        {
            get
            {
                return privateLineThickness;
            }
            set
            {
                privateLineThickness = value;
                Invalidate();
            }
        }

        private bool privateShowSymbols = true;

        [Category("HartUI")]
        [Description("Whether to show the checkmark symbol on the completed tasks.")]
        public bool ShowSymbols
        {
            get
            {
                return privateShowSymbols;
            }
            set
            {
                privateShowSymbols = value;
                Invalidate();
            }
        }

        private Color privateCompletedColor = Helpers.DrawingHelper.PrimaryColor;

        [Category("HartUI")]
        [Description("The primary color of the control, the color of completed tasks and current task.")]
        public Color CompletedColor
        {
            get
            {
                return privateCompletedColor;
            }
            set
            {
                privateCompletedColor = value;
                Invalidate();
            }
        }

        private Color privateCurrentTaskForeColor = Color.FromArgb(128, 128, 128);

        [Category("HartUI")]
        [Description("The color of the text of the current task.")]
        public Color CurrentTaskForeColor
        {
            get
            {
                return privateCurrentTaskForeColor;
            }
            set
            {
                privateCurrentTaskForeColor = value;
                Invalidate();
            }
        }

        private Color privateTaskForeColor = Color.FromArgb(128, 128, 128);

        [Category("HartUI")]
        [Description("The color of the text for every task other than the current task.")]
        public Color TaskForeColor
        {
            get
            {
                return privateTaskForeColor;
            }
            set
            {
                privateTaskForeColor = value;
                Invalidate();
            }
        }

        private Color privateTrackColor = Color.FromArgb(64, 128, 128, 128);

        [Category("HartUI")]
        [Description("The color of the track of the uncompleted tasks.")]
        public Color TrackColor
        {
            get
            {
                return privateTrackColor;
            }
            set
            {
                privateTrackColor = value;
                Invalidate();
            }
        }

        protected int privateRounding = 10;

        [Category("HartUI")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        bool privateAutoRounding = true;

        [Category("HartUI")]
        public bool AutoRounding
        {
            get
            {
                return privateAutoRounding;
            }
            set
            {
                privateAutoRounding = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        [Description("Read-only. Returns the name of the current task (as string).")]
        public string CurrentTask
        {
            get
            {
                try
                {
                    return Tasks[TasksProgress - 1];
                }
                catch
                {
                    return "";
                }
            }
        }

        // vertical version overrides these
        protected virtual int GetCrossExtent() => Height;
        protected virtual int GetPrimaryExtent() => Width;
        protected virtual int GetTextCompensation(Graphics g) => Font.Height;
        protected virtual int TextGap => 1;
        // horizontal uses Alignment, vertical uses LineAlignment
        protected virtual StringFormat CreateStringFormat() => new StringFormat { Alignment = StringAlignment.Center };
        protected virtual Point MakePoint(int primary, int secondary) => new Point(primary, secondary);
        protected virtual Rectangle MakeRect(int primary, int secondary, int size) => new Rectangle(primary, secondary, size, size);

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Tasks.Length < 2)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int textCompensation = GetTextCompensation(e.Graphics);
            int wantedItemSize = GetCrossExtent() - 1 - textCompensation;

            int penThicknessCompensation = wantedItemSize / 8;
            int halfPenThickness = penThicknessCompensation / 2;

            int actualItemSize = wantedItemSize - penThicknessCompensation;

            int itemCount = Tasks.Length;
            int spacing = (GetPrimaryExtent() - (actualItemSize * 2)) / (itemCount - 1); // Adjusted spacing

            int itemPrimary = actualItemSize;
            int itemSecondary = halfPenThickness;
            int textPrimary = 0;
            int textSecondary = actualItemSize + penThicknessCompensation + TextGap;

            using (StringFormat sf = CreateStringFormat())
            using (Brush trackBrush = new SolidBrush(CompletedColor))
            using (Brush todoBrush = new SolidBrush(TrackColor))
            {
                GraphicsPath RoundedItemPath = null;

                int tempRounding;
                if (AutoRounding)
                {
                    tempRounding = actualItemSize / 2;
                }
                else
                {
                    tempRounding = Math.Min(actualItemSize / 2, privateRounding);
                }

                // draw tasks
                for (int i = 0; i < itemCount; i++)
                {
                    itemPrimary = actualItemSize + (i * spacing) - (i * wantedItemSize / Tasks.Length) - penThicknessCompensation;
                    textPrimary = itemPrimary + ((actualItemSize + 1) / 2);

                    // current step
                    if (i == TasksProgress - 1)
                    {
                        RoundedItemPath = GeneralHelper.RoundRect(
                            MakeRect(itemPrimary + (halfPenThickness / 2) + 1, penThicknessCompensation + 1, actualItemSize - halfPenThickness - 2),
                            tempRounding - penThicknessCompensation / 2 - 1);

                        using (Pen p = new Pen(CompletedColor, (LineThickness / 2) - 1))
                        {
                            e.Graphics.DrawPath(p, RoundedItemPath);
                        }

                        using (SolidBrush textBrush = new SolidBrush(CurrentTaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, MakePoint(textPrimary, textSecondary), sf);
                        }
                    }
                    // completed steps
                    else if (i < TasksProgress)
                    {
                        // save rect for later in case drawing symbols
                        Rectangle tempRect = MakeRect(itemPrimary, penThicknessCompensation, actualItemSize);

                        RoundedItemPath = GeneralHelper.RoundRect(tempRect, tempRounding);
                        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        e.Graphics.FillPath(trackBrush, RoundedItemPath);

                        // checkmark
                        if (ShowSymbols)
                        {
                            tempRect.Inflate(0, -1);
                            tempRect.Inflate(-(actualItemSize / 10), -(actualItemSize / 10));
                            using (GraphicsPath checkmarkGP = GeneralHelper.Checkmark(tempRect))
                            using (Pen p = new Pen(BackColor, actualItemSize / 8) { EndCap = LineCap.Round, StartCap = LineCap.Round })
                            {
                                e.Graphics.DrawPath(p, checkmarkGP);
                            }
                        }

                        using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, MakePoint(textPrimary, textSecondary), sf);
                        }
                    }
                    // steps yet to be completed
                    else
                    {
                        RoundedItemPath = GeneralHelper.RoundRect(
                            MakeRect(itemPrimary, penThicknessCompensation, actualItemSize),
                            tempRounding);

                        e.Graphics.FillPath(todoBrush, RoundedItemPath);

                        using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, MakePoint(textPrimary, textSecondary), sf);
                        }
                    }

                    // lines inbetween
                    if (i != itemCount - 1)
                    {
                        using (Pen p = new Pen(i < TasksProgress - 1 ? CompletedColor : TrackColor, (LineThickness / 2)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            int connectPrimary = itemPrimary;
                            int connectSecondary = itemSecondary + ((actualItemSize + penThicknessCompensation + 1) / 2);
                            connectPrimary += penThicknessCompensation + actualItemSize;

                            int connect2Primary = connectPrimary + spacing - (wantedItemSize / Tasks.Length) - wantedItemSize - (penThicknessCompensation * 2) + 1;
                            int connect2Secondary = connectSecondary;

                            connectPrimary += penThicknessCompensation;
                            connect2Primary -= penThicknessCompensation;

                            e.Graphics.DrawLine(p, MakePoint(connectPrimary, connectSecondary), MakePoint(connect2Primary, connect2Secondary));
                        }
                    }
                }

                RoundedItemPath?.Dispose();
            }

            base.OnPaint(e);
        }
    }
}
