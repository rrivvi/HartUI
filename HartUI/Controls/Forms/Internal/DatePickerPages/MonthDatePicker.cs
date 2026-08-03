using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls.Forms.Internal.DatePickerPages
{
    internal partial class MonthDatePicker : System.Windows.Forms.UserControl
    {
        DatePicker _datePickerForm;

        protected internal cuiButtonGroup SelectedDayButton => dayPanel.Controls.OfType<cuiButtonGroup>().FirstOrDefault(b => b.Checked);

        public MonthDatePicker(DatePicker datePickerForm)
        {
            InitializeComponent();
            _datePickerForm = datePickerForm;

            int dayIndex = 0;
            foreach (cuiLabel dayNameLabel in panel1.Controls)
            {
                dayNameLabel.Content = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[dayIndex % 7];
                dayIndex++;
            }

            UpdateDayButtons();
        }

        internal void UpdateDayButtons()
        {
            int daysInMonth = DateTime.DaysInMonth(_datePickerForm.Value.Year, _datePickerForm.Value.Month);
            DateTime firstDay = new DateTime(_datePickerForm.Value.Year, _datePickerForm.Value.Month, 1);
            int startColumn = (int)firstDay.DayOfWeek; // Sunday = 0, Monday = 1, ..., Saturday = 6

            int[] columnPositions = { 0, 56, 112, 168, 224, 280, 336 };
            int[] rowPositions = { 3, 33, 63, 93, 123, 153, 183 };

            int currentRow = 0;
            int currentColumn = startColumn;

            foreach (cuiButtonGroup dayButton in dayPanel.Controls)
            {
                int thisButtonsDay = int.Parse(dayButton.Content);
                if (thisButtonsDay > daysInMonth)
                {
                    dayButton.Visible = false; // Hide extra buttons
                    continue;
                }

                dayButton.Checked = thisButtonsDay == _datePickerForm.Value.Day;

                dayButton.Visible = true;
                dayButton.Location = new Point(columnPositions[currentColumn], rowPositions[currentRow]);

                dayButton.Click -= DayButton_Click;
                dayButton.Click += DayButton_Click;

                dayButton.KeyDown -= DayButton_KeyDown;
                dayButton.KeyDown += DayButton_KeyDown;
                dayButton.PreviewKeyDown -= DayButton_PreviewKeyDown;
                dayButton.PreviewKeyDown += DayButton_PreviewKeyDown;

                currentColumn++;
                if (currentColumn > 6)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        private void DayButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                ActiveControl = leftMonthButton;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        private void DayButton_KeyDown(object sender, KeyEventArgs e)
        {
            int delta;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    delta = -1;
                    break;
                case Keys.Right:
                    delta = 1;
                    break;
                case Keys.Up:
                    delta = -7;
                    break;
                case Keys.Down:
                    delta = 7;
                    break;
                default:
                    return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            ChangeFocusedButtonBy(delta);
        }

        private void ChangeFocusedButtonBy(int delta)
        {
            cuiButtonGroup focused = dayPanel.Controls
                .OfType<cuiButtonGroup>()
                .FirstOrDefault(b => b.Focused);

            if (focused == null)
                return;

            int targetDay = int.Parse(focused.Content) + delta;

            cuiButtonGroup target = dayPanel.Controls
                .OfType<cuiButtonGroup>()
                .FirstOrDefault(b =>
                    b.Visible &&
                    int.Parse(b.Content) == targetDay);

            if (target == null)
            {
                if (delta < 0)
                {
                    _datePickerForm.ActiveControl = _datePickerForm.cuiButton1;
                }
                else
                {
                    ActiveControl = leftMonthButton;
                }
            }
            else
            {
                target.Focus();
            }
        }

        private void DayButton_Click(object sender, EventArgs e)
        {
            if (sender is cuiButtonGroup dayButton && int.TryParse(dayButton.Content, out int thisButtonsDay))
            {
                _datePickerForm.SetDayMonth(thisButtonsDay, _datePickerForm.Value.Month);
                UpdateDayButtons();
            }
        }

        private void leftMonthButton_Click(object sender, EventArgs e)
        {
            int wantedMonth = _datePickerForm.Value.Month - 1;
            if (wantedMonth < 1) // go back 1 year
            {
                int daysInMonth = DateTime.DaysInMonth(_datePickerForm.Value.Year - 1, 12);
                int dayToSelect = Math.Min(daysInMonth, _datePickerForm.Value.Day);
                DateTime newDatePickerValue = new DateTime(_datePickerForm.Value.Year - 1, 12, dayToSelect);
                _datePickerForm.Value = newDatePickerValue;
            }
            else
            {
                _datePickerForm.SetDayMonth(_datePickerForm.Value.Day, wantedMonth);
            }
            UpdateDayButtons();
        }

        private void rightMonthButton_Click(object sender, EventArgs e)
        {
            int wantedMonth = _datePickerForm.Value.Month + 1;
            if (wantedMonth > 12) // go back 1 year
            {
                int daysInMonth = DateTime.DaysInMonth(_datePickerForm.Value.Year + 1, 1);
                int dayToSelect = Math.Min(daysInMonth, _datePickerForm.Value.Day);
                DateTime newDatePickerValue = new DateTime(_datePickerForm.Value.Year + 1, 1, dayToSelect);
                _datePickerForm.Value = newDatePickerValue;
            }
            else
            {
                _datePickerForm.SetDayMonth(_datePickerForm.Value.Day, wantedMonth);
            }
            UpdateDayButtons();
        }

        private void rightMonthButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Right)
            {
                _datePickerForm.ActiveControl = _datePickerForm.cuiButton2;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                _datePickerForm.ActiveControl = _datePickerForm.cuiButton3;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                ActiveControl = SelectedDayButton;
                e.IsInputKey = true;
            }
        }

        private void leftMonthButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                ActiveControl = SelectedDayButton;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                _datePickerForm.ActiveControl = _datePickerForm.cuiButton3;
                e.IsInputKey = true;
            }
        }
    }
}
