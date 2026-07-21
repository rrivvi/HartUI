using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Show progress step-by-step to the user")]
    public partial class cuiProgressTrackerVertical : Control
    {
        public cuiProgressTrackerVertical()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(58, 480);
        }

        private string[] tasks = new string[] { "Task1", "Task2", "Task3", "Task4" };

        [Category("HartUI")]
        [Description("Tasks in text separated by new lines.")]
        public string[] Tasks
        {
            get => tasks;
            set
            {
                tasks = value;

                // https://stackoverflow.com/a/7975983
                longestString = tasks.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);

                this.Invalidate();
            }
        }

        private string longestString = "Task1";

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

        int privateRounding = 10;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Tasks.Length < 2)
            {
                return;
            }

            int longestStringWidthCompensation = (int)(e.Graphics.MeasureString(longestString, Font).Width + 0.5f);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int WantedItemWidth = Width - 1 - longestStringWidthCompensation;

            int PenThicknessCompensation = WantedItemWidth / 8;
            int HalfPenThickness = PenThicknessCompensation / 2;

            int ActualItemWidth = WantedItemWidth - PenThicknessCompensation;

            int itemCount = Tasks.Length;
            int spacing = (Height - (ActualItemWidth * 2)) / (itemCount - 1); // Adjusted spacing

            Point currentItemPosition = new Point(HalfPenThickness, ActualItemWidth);
            Point currentTextRectangle = new Point(ActualItemWidth + PenThicknessCompensation + 2, 0);

            using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
            using (Brush trackBrush = new SolidBrush(CompletedColor))
            using (Brush todoBrush = new SolidBrush(TrackColor))
            {
                GraphicsPath RoundedItemPath = null;

                int tempRounding;
                if (AutoRounding)
                {
                    tempRounding = ActualItemWidth / 2;
                }
                else
                {
                    tempRounding = Math.Min(ActualItemWidth / 2, privateRounding);
                }

                // draw tasks
                for (int i = 0; i < itemCount; i++)
                {
                    currentItemPosition.Y = ActualItemWidth + (i * spacing) - (i * WantedItemWidth / Tasks.Length) - PenThicknessCompensation;
                    currentTextRectangle.Y = currentItemPosition.Y + ((ActualItemWidth + 1) / 2);

                    // current step
                    if (i == TasksProgress - 1)
                    {
                        RoundedItemPath = GeneralHelper.RoundRect(new Rectangle(
                                PenThicknessCompensation + 1,
                                currentItemPosition.Y + (HalfPenThickness / 2) + 1,
                                ActualItemWidth - HalfPenThickness - 2,
                                ActualItemWidth - HalfPenThickness - 2), tempRounding - PenThicknessCompensation / 2 - 1);

                        using (Pen p = new Pen(CompletedColor, (LineThickness / 2) - 1))
                        {
                            e.Graphics.DrawPath(p, RoundedItemPath);
                        }

                        using (SolidBrush textBrush = new SolidBrush(CurrentTaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                        }
                    }
                    // completed steps
                    else if (i < TasksProgress)
                    {
                        // save rect for later in case drawing symbols
                        Rectangle tempRect = new Rectangle(PenThicknessCompensation, currentItemPosition.Y, ActualItemWidth, ActualItemWidth);

                        RoundedItemPath = GeneralHelper.RoundRect(tempRect, tempRounding);
                        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        e.Graphics.FillPath(trackBrush, RoundedItemPath);

                        // checkmark
                        if (ShowSymbols)
                        {
                            tempRect.Inflate(0, -1);
                            tempRect.Inflate(-(ActualItemWidth / 10), -(ActualItemWidth / 10));
                            using (GraphicsPath checkmarkGP = GeneralHelper.Checkmark(tempRect))
                            using (Pen p = new Pen(BackColor, ActualItemWidth / 8) { EndCap = LineCap.Round, StartCap = LineCap.Round })
                            {
                                e.Graphics.DrawPath(p, checkmarkGP);
                            }
                        }

                        using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                        }
                    }
                    // steps yet to be completed
                    else
                    {
                        RoundedItemPath = GeneralHelper.RoundRect(new Rectangle(
                            PenThicknessCompensation,
                            currentItemPosition.Y,
                            ActualItemWidth,
                            ActualItemWidth), tempRounding);

                        e.Graphics.FillPath(todoBrush, RoundedItemPath);

                        using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                        {
                            e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                        }
                    }

                    // lines inbetween
                    if (i != itemCount - 1)
                    {
                        using (Pen p = new Pen(i < TasksProgress - 1 ? CompletedColor : TrackColor, (LineThickness / 2)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            Point ConnectPoint = currentItemPosition;
                            ConnectPoint.X += ((ActualItemWidth + PenThicknessCompensation + 1) / 2);
                            ConnectPoint.Y += PenThicknessCompensation + ActualItemWidth;

                            Point ConnectPoint2 = ConnectPoint;
                            ConnectPoint2.Y = ConnectPoint.Y + spacing - (WantedItemWidth / Tasks.Length) - WantedItemWidth - (PenThicknessCompensation * 2) + 1;

                            ConnectPoint.Y += PenThicknessCompensation;
                            ConnectPoint2.Y -= PenThicknessCompensation;

                            e.Graphics.DrawLine(p, ConnectPoint, ConnectPoint2);
                        }
                    }
                }

                RoundedItemPath?.Dispose();
            }

            base.OnPaint(e);
        }
    }
}