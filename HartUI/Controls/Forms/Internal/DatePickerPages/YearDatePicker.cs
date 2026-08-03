using System;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls.Forms.Internal.DatePickerPages
{
    internal partial class YearDatePicker : UserControl
    {
        DatePicker _datePickerForm;

        protected internal cuiButtonGroup SelectedYearButton => Controls.OfType<cuiButtonGroup>().FirstOrDefault(b => b.Checked);

        public YearDatePicker(DatePicker datePickerForm)
        {
            InitializeComponent();
            _datePickerForm = datePickerForm;
            UpdateYearButtons();

            leftYearButton.Click += (e, s) =>
            {
                datePickerForm.SetYear(datePickerForm.Value.Year - 10, false);
                UpdateYearButtons();
            };

            rightYearButton.Click += (e, s) =>
            {
                datePickerForm.SetYear(datePickerForm.Value.Year + 10, false);
                UpdateYearButtons();
            };
        }

        internal void UpdateYearButtons()
        {
            int i = 0;
            foreach (Control c in Controls)
            {
                if (c is cuiButtonGroup yearButton)
                {
                    int thisButtonsYear = _datePickerForm.Value.Year + i;
                    yearButton.Content = thisButtonsYear.ToString();

                    yearButton.Checked = false;

                    yearButton.Click -= YearButton_Click;
                    yearButton.Click += YearButton_Click;

                    yearButton.KeyDown -= YearButton_KeyDown;
                    yearButton.KeyDown += YearButton_KeyDown;
                    yearButton.PreviewKeyDown -= YearButton_PreviewKeyDown;
                    yearButton.PreviewKeyDown += YearButton_PreviewKeyDown;

                    i++;
                }
            }

            cuiButtonGroup1.Checked = true; // because the current selected year always appears first
        }

        private void YearButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                ActiveControl = leftYearButton;
                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        private void YearButton_KeyDown(object sender, KeyEventArgs e)
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
                    delta = -5;
                    break;
                case Keys.Down:
                    delta = 5;
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
            cuiButtonGroup focused = Controls
                .OfType<cuiButtonGroup>()
                .FirstOrDefault(b => b.Focused);

            if (focused == null)
                return;

            int targetYear = int.Parse(focused.Content) + delta;

            cuiButtonGroup target = Controls
                .OfType<cuiButtonGroup>()
                .FirstOrDefault(b =>
                    b.Visible &&
                    int.Parse(b.Content) == targetYear);

            if (target == null)
            {
                if (delta < 0)
                {
                    _datePickerForm.ActiveControl = _datePickerForm.cuiButton1;
                }
                else
                {
                    ActiveControl = leftYearButton;
                }
            }
            else
            {
                target.Focus();
            }
        }

        private void YearButton_Click(object sender, EventArgs e)
        {
            if (sender is cuiButtonGroup cbg && int.TryParse(cbg.Content, out int selectedYear))
            {
                _datePickerForm.SetYear(selectedYear);
                UpdateYearButtons();
            }
        }

        private void rightYearButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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
                ActiveControl = SelectedYearButton;
                e.IsInputKey = true;
            }
        }

        private void leftYearButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                ActiveControl = SelectedYearButton;
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
