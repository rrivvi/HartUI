using HartUI.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls.Charts
{
    [Description("Simple line chart control")]
    public partial class cuiChartLine : UserControl
    {
        public cuiChartLine()
        {
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            DoubleBuffered = true;
            Font = new Font("Microsoft Yahei UI", 8.25f);
        }

        private float[] privateDataPoints = { 100, 90, 80, 75, 70, 65, 60 };
        private bool usePercent = true;
        private float privateMaxValue = 100;
        private int chartPadding = 40;
        private bool showPopup = false;
        private PointF popupLocation;
        private string popupText;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private bool privateGradientBackground = true;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Whether the background under the lines should be a gradient.")]
        public bool GradientBackground
        {
            get
            {
                return privateGradientBackground;
            }
            set
            {
                if (privateGradientBackground != value)
                {
                    privateGradientBackground = value;
                    Invalidate();
                }
            }
        }

        private bool privateShowLines = true;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Whether the dashed lines should render.")]
        public bool ShowLines
        {
            get
            {
                return privateShowLines;
            }
            set
            {
                if (privateShowLines != value)
                {
                    privateShowLines = value;
                    Invalidate();
                }
            }
        }

        private Color privatePointColor = Helpers.DrawingHelper.PrimaryColor;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Color of the circular points.")]
        public Color PointColor
        {
            get
            {
                return privatePointColor;
            }
            set
            {
                if (privatePointColor != value)
                {
                    privatePointColor = value;
                    Invalidate();
                }

            }
        }

        private Color privateAxisColor = Color.Gray;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Color of the axis (x and y lines).")]
        public Color AxisColor
        {
            get
            {
                return privateAxisColor;
            }
            set
            {
                privateAxisColor = value;
                Invalidate();
            }
        }

        string[] xLabelsLong = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        string[] xLabelsShort = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        [Browsable(true)]
        [Category("HartUI Chart Data")]
        [Description("The data points that will be plotted on the line chart.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public float[] DataPoints
        {
            get
            {
                return privateDataPoints;
            }
            set
            {
                privateDataPoints = value;

                if (privateAutoMaxValue)
                {
                    privateMaxValue = privateDataPoints.Max();
                }

                Invalidate();
            }
        }

        private string[] privateCustomXAxis = { };
        [Browsable(true)]
        [Category("HartUI Chart Data")]
        [Description("The X axis values, if you don't want to use the weekdays.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string[] CustomXAxis
        {
            get
            {
                return privateCustomXAxis;
            }
            set
            {
                if (value.Length == 1)
                {
                    if (DesignMode)
                    {
                        MessageBox.Show("Please input at least 2 values for the X axis.\nNo changes have been made to the array.");
                    }
                    else
                    {
                        throw new Exception("CustomXAxis.Length must not be equal to 1.");
                    }
                }
                else
                {
                    privateCustomXAxis = value;
                    Invalidate();
                }
            }
        }

        [Browsable(true)]
        [Category("HartUI Chart Layout")]
        [Description("The padding around the chart area.")]
        public int ChartPadding
        {
            get
            {
                return chartPadding;
            }
            set
            {
                chartPadding = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("HartUI Chart Layout")]
        [Description("If true, the Y-axis will show percentages. If false, it will show absolute values.")]
        public bool UsePercent
        {
            get
            {
                return usePercent;
            }
            set
            {
                usePercent = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("HartUI Chart Layout")]
        [Description("The maximum value for the Y-axis.")]
        public float MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                AutoMaxValue = false;
                privateMaxValue = value;
                Invalidate();
            }
        }

        private Color privateChartLineColor = Helpers.DrawingHelper.PrimaryColor;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Color of the line that connects points.")]
        public Color ChartLineColor
        {
            get
            {
                return privateChartLineColor;
            }
            set
            {
                privateChartLineColor = value;
                Invalidate();
            }
        }

        private Color privateDayColor = Color.DarkGray;

        [Browsable(true)]
        [Category("HartUI Chart Colors")]
        [Description("Color of the day label.")]
        public Color DayColor
        {
            get
            {
                return privateDayColor;
            }
            set
            {
                privateDayColor = value;
                Invalidate();
            }
        }

        private bool privateShortDates = true;

        [Browsable(true)]
        [Category("HartUI Chart Data")]
        [Description("Whether the dates should be minified.")]
        public bool ShortDates
        {
            get
            {
                return privateShortDates;
            }
            set
            {
                privateShortDates = value;
                Invalidate();
            }
        }

        private bool privateAutoMaxValue = true;
        [Browsable(true)]
        [Category("HartUI Chart Layout")]
        [Description("Whether the MaxValue should update automatically to fit chart data.")]
        public bool AutoMaxValue
        {
            get
            {
                return privateAutoMaxValue;
            }
            set
            {
                privateAutoMaxValue = value;
                if (value)
                {
                    privateMaxValue = privateDataPoints.Max();
                }
                Invalidate();
            }
        }

        private bool privateUseBezier = false;

        [Browsable(true)]
        [Category("HartUI Chart Layout")]
        [Description("Smoothens the lines by using Bezier curves instead.")]
        public bool UseBezier
        {
            get
            {
                return privateUseBezier;
            }
            set
            {
                privateUseBezier = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int chartWidth = Width - chartPadding * 2;
            int chartHeight = Height - chartPadding * 2;
            int pointCount = privateDataPoints?.Length ?? 0;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using (Pen axisPen = new Pen(AxisColor, 1))
            using (Pen dashedPen = new Pen(Color.FromArgb(64, AxisColor), 1) { DashStyle = DashStyle.Dash })
            using (Pen linePen = new Pen(ChartLineColor, 2))
            {
                g.DrawLine(axisPen, chartPadding, chartPadding, chartPadding, chartHeight + chartPadding);
                g.DrawLine(axisPen, chartPadding, chartHeight + chartPadding, chartWidth + chartPadding, chartHeight + chartPadding);

                if (privateShowLines)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        float y = chartPadding + chartHeight - (i * chartHeight / 5f);
                        g.DrawLine(dashedPen, chartPadding, y, chartWidth + chartPadding, y);
                    }
                }

                if (pointCount > 1 && privateMaxValue > 0)
                {
                    float yScaleFactor = 100f / privateMaxValue;
                    float xStep = chartWidth / (float)(pointCount - 1);

                    using (GraphicsPath linePath = new GraphicsPath())
                    {
                        float startX = chartPadding;
                        float startY = chartPadding + chartHeight - (privateDataPoints[0] * yScaleFactor / 100f * chartHeight);

                        linePath.StartFigure();
                        linePath.AddLine(startX, chartHeight + chartPadding, startX, startY);

                        for (int i = 0; i < pointCount - 1; i++)
                        {
                            float x1 = chartPadding + (i * xStep);
                            float y1 = chartPadding + chartHeight - (privateDataPoints[i] * yScaleFactor / 100f * chartHeight);
                            float x2 = chartPadding + ((i + 1) * xStep);
                            float y2 = chartPadding + chartHeight - (privateDataPoints[i + 1] * yScaleFactor / 100f * chartHeight);

                            if (privateUseBezier)
                            {
                                float cp1X = x1 + (x2 - x1) / 3f;
                                float cp1Y = y1;
                                float cp2X = x2 - (x2 - x1) / 3f;
                                float cp2Y = y2;

                                g.DrawBezier(linePen, x1, y1, cp1X, cp1Y, cp2X, cp2Y, x2, y2);
                                linePath.AddBezier(x1, y1, cp1X, cp1Y, cp2X, cp2Y, x2, y2);
                            }
                            else
                            {
                                g.DrawLine(linePen, x1, y1, x2, y2);
                                linePath.AddLine(x1, y1, x2, y2);
                            }
                        }

                        float endX = chartPadding + chartWidth;
                        float endY = chartPadding + chartHeight - (privateDataPoints[pointCount - 1] * yScaleFactor / 100f * chartHeight);
                        linePath.AddLine(endX, endY, endX, chartHeight + chartPadding);
                        linePath.CloseFigure();

                        if (GradientBackground)
                        {
                            Color startColor = ChartLineColor.A == 255
                                ? Color.FromArgb(64, ChartLineColor)
                                : Color.FromArgb((64 + ChartLineColor.A) / 2, ChartLineColor);

                            using (LinearGradientBrush gradientBrush =
                                   new LinearGradientBrush(new Point(0, 0), new Point(0, Height), startColor, Color.Transparent))
                            {
                                g.FillPath(gradientBrush, linePath);
                            }
                        }
                        else
                        {
                            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(32, ChartLineColor)))
                            {
                                g.FillPath(solidBrush, linePath);
                            }
                        }

                        using (SolidBrush pointBrush = new SolidBrush(PointColor))
                        {
                            for (int i = 0; i < pointCount; i++)
                            {
                                float x = chartPadding + (i * xStep);
                                float y = chartPadding + chartHeight - (privateDataPoints[i] * yScaleFactor / 100f * chartHeight);
                                g.FillEllipse(pointBrush, x - 4, y - 4, 8, 8);
                            }
                        }

                        if (showPopup)
                        {
                            SizeF textSize = g.MeasureString(popupText, Font);
                            RectangleF popupRect = new RectangleF(
                                popupLocation.X - textSize.Width / 2f,
                                popupLocation.Y - textSize.Height - 10f,
                                textSize.Width,
                                textSize.Height);

                            using (GraphicsPath popupPath = GeneralHelper.RoundRect(popupRect, (int)(popupRect.Height / 4f)))
                            using (SolidBrush popupBrush = new SolidBrush(PopupBackground))
                            using (SolidBrush textBrush = new SolidBrush(PopupText))
                            {
                                g.FillPath(popupBrush, popupPath);
                                g.DrawString(popupText, Font, textBrush, popupRect.X + 1.5f, popupRect.Y + 1f);
                            }
                        }
                    }
                }
            }

            DrawLabels(g, chartPadding, chartWidth, chartHeight);
            base.OnPaint(e);
        }

        private Color PopupBackground
        {
            get
            {
                if (BackColor.R + BackColor.G + BackColor.B > 128)
                {
                    return Color.FromArgb(64, Color.Black);
                }
                else
                {
                    return Color.FromArgb(64, Color.White);
                }
            }
        }

        private Color PopupText
        {
            get
            {
                if (BackColor.R + BackColor.G + BackColor.B > 128)
                {
                    return Color.White;
                }
                else
                {
                    return Color.Black;
                }
            }
        }

        private void DrawLabels(Graphics g, int padding, int width, int height)
        {
            using (Brush labelBrush = new SolidBrush(DayColor))
            {

                if (usePercent)
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        float y = padding + height - (i * height / 5);
                        g.DrawString($"{Math.Round(i * 20 * privateMaxValue / 100, 2)}%", Font, labelBrush, padding - 35, y - 7);
                    }
                }
                else
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        float y = padding + height - (i * height / 5);
                        float value = i * privateMaxValue / 5;
                        g.DrawString($"{Math.Round(value, 2)}", Font, labelBrush, padding - 35, y - 7);
                    }
                }

                if (privateCustomXAxis.Length > 0)
                {
                    for (int i = 0; i < privateCustomXAxis.Length; i++)
                    {
                        float x = padding + (i * width / (privateCustomXAxis.Length - 1));
                        g.DrawString(privateCustomXAxis[i], Font, labelBrush, x - 20, padding + height + 5);
                    }
                }
                else
                {
                    for (int i = 0; i < xLabelsLong.Length; i++)
                    {
                        float x = padding + (i * width / (xLabelsLong.Length - 1));
                        if (ShortDates)
                        {
                            g.DrawString(xLabelsShort[i], Font, labelBrush, x - 20, padding + height + 5);
                        }
                        else
                        {
                            g.DrawString(xLabelsLong[i], Font, labelBrush, x - 20, padding + height + 5);
                        }
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var data = privateDataPoints;
            int count = data?.Length ?? 0;

            if (count == 0 || privateMaxValue <= 0)
            {
                if (showPopup)
                {
                    showPopup = false;
                    InvalidatePopupRegion();
                }
                return;
            }

            int chartWidth = ClientSize.Width - chartPadding * 2;
            int chartHeight = ClientSize.Height - chartPadding * 2;
            int lastIndex = count - 1;
            float yScale = chartHeight / privateMaxValue;

            for (int i = 0; i < count; i++)
            {
                float x = chartPadding + (lastIndex == 0 ? 0f : (i * (float)chartWidth / lastIndex));
                float y = chartPadding + chartHeight - (data[i] * yScale);

                if (Math.Abs(e.X - x) < 10f && Math.Abs(e.Y - y) < 10f)
                {
                    var newLocation = new PointF(x, y);
                    string newText = usePercent ? $"{data[i]}%" : data[i].ToString();

                    if (!showPopup || !popupLocation.Equals(newLocation) || popupText != newText)
                    {
                        popupLocation = newLocation;
                        popupText = newText;
                        showPopup = true;
                        InvalidatePopupRegion();
                    }
                    else
                    {
                        showPopup = true;
                    }

                    return;
                }
            }

            if (showPopup)
            {
                showPopup = false;
                InvalidatePopupRegion();
            }
        }

        private void InvalidatePopupRegion()
        {
            SizeF textSize = TextRenderer.MeasureText(popupText, Font);

            RectangleF popupRect = new RectangleF(popupLocation.X - textSize.Width / 2, popupLocation.Y - textSize.Height - 10, textSize.Width, textSize.Height + 1);
            using (GraphicsPath popupPath = GeneralHelper.RoundRect(popupRect, (int)(popupRect.Height / 4)))
            {
                using (Region roundedRegion = new Region(popupPath))
                {
                    Invalidate(roundedRegion);
                }
            }
        }
    }
}
