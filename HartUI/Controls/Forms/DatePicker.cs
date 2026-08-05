using HartUI.Controls.Forms.Internal.DatePickerPages;
using System;
using System.Windows.Forms;

namespace HartUI.Controls.Forms
{
    internal partial class DatePicker : Form
    {
        private DateTime privateValue;
        public DateTime Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                privateValue = value;
                cuiLabel3.Content = value.ToString("D");
                yearPickerControl?.UpdateYearButtons();
                monthDayPickerControl?.UpdateDayButtons();
            }
        }

        YearDatePicker yearPickerControl;
        MonthDatePicker monthDayPickerControl;

        bool isMonthDayPicker => pagePanel.Controls[0] == monthDayPickerControl;

        private bool _closingFromDeactivate;

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (_closingFromDeactivate)
                return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || Disposing)
                    return;

                // Another control inside this form still owns focus
                if (ContainsFocus)
                    return;

                _closingFromDeactivate = true;
                DialogResult = DialogResult.Cancel;
                Close();
            }));
        }

        public DatePicker(DateTime startWithDateTime)
        {
            InitializeComponent();

            yearPickerControl = new YearDatePicker(this);
            monthDayPickerControl = new MonthDatePicker(this);

            Value = startWithDateTime;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            pagePanel.Controls.Add(monthDayPickerControl);
            ActiveControl = cuiButton5;
        }

        void SetPage(UserControl pageControl)
        {
            pagePanel.Controls.Clear();
            pagePanel.Controls.Add(pageControl);
        }

        private void cuiButton3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cuiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        internal void SetYear(int year, bool returnToChoosingDay = true)
        {
            int fixedDay = Math.Min(Value.Day, DateTime.DaysInMonth(year, Value.Month));
            Value = new DateTime(year, Value.Month, fixedDay);

            if (returnToChoosingDay)
            {
                SetPage(monthDayPickerControl);
            }
        }

        internal void SetDayMonth(int day, int month)
        {
            int fixedDay = Math.Min(day, DateTime.DaysInMonth(Value.Year, month));
            Value = new DateTime(Value.Year, month, fixedDay);
        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cuiButton5_Click(object sender, EventArgs e)
        {
            if (pagePanel.Controls[0] == monthDayPickerControl)
            {
                SetPage(yearPickerControl);
            }
            else
            {
                SetPage(monthDayPickerControl);
            }
        }

        private void cuiButton2_ForeColorChanged(object sender, EventArgs e)
        {
            cuiButton2.HoverForeColor = cuiButton2.ForeColor;
            cuiButton2.PressedForeColor = cuiButton2.ForeColor;

            cuiButton3.ForeColor = cuiButton2.ForeColor;

            cuiButton3.HoverForeColor = cuiButton2.ForeColor;
            cuiButton3.PressedForeColor = cuiButton2.ForeColor;

            cuiButton2.NormalImageTint = cuiButton2.ForeColor;
            cuiButton3.NormalImageTint = cuiButton2.ForeColor;
            cuiButton2.HoverImageTint = cuiButton2.ForeColor;
            cuiButton3.HoverImageTint = cuiButton2.ForeColor;
            cuiButton2.PressedImageTint = cuiButton2.ForeColor;
            cuiButton3.PressedImageTint = cuiButton2.ForeColor;
        }

        private void cuiButton3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Down || e.KeyCode == Keys.Right)
            {
                ActiveControl = cuiButton5;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                ActiveControl = isMonthDayPicker
                    ? monthDayPickerControl.leftMonthButton
                    : yearPickerControl.leftYearButton;

                e.IsInputKey = true;
            }
        }

        private void cuiButton1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                ActiveControl = isMonthDayPicker
                    ? monthDayPickerControl.SelectedDayButton
                    : yearPickerControl.SelectedYearButton;

                e.IsInputKey = true;
            }
        }

        private void cuiButton2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                ActiveControl = isMonthDayPicker
                    ? monthDayPickerControl.SelectedDayButton
                    : yearPickerControl.SelectedYearButton;

                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                ActiveControl = isMonthDayPicker
                    ? monthDayPickerControl.rightMonthButton
                    : yearPickerControl.rightYearButton;

                e.IsInputKey = true;
            }
        }

        private void cuiButton5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Left)
            {
                ActiveControl = cuiButton3;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                ActiveControl = isMonthDayPicker
                    ? monthDayPickerControl.SelectedDayButton
                    : yearPickerControl.SelectedYearButton;

                e.IsInputKey = true;
            }
        }
    }
}
