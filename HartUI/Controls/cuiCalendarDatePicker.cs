using HartUI.Controls.Forms;
using HartUI.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [Description("Allows the user to select a date with the custom HartUI date picker form")]
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("DateChanged")]
    public partial class cuiCalendarDatePicker : cuiButton
    {
        public cuiCalendarDatePicker()
        {
            InitializeComponent();

            ForeColor = Color.Gray;
            Size = new Size(153, 45);
            OutlineThickness = 1.5f;

            Image = Resources.calendar;
            NormalImageTint = Color.Gray;
            HoverImageTint = Color.Gray;
            PressedImageTint = Color.Gray;

            Content = DateTime.Now.Date;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CheckButton
        {
            get { return false; }
            set { }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DialogResult DialogResult
        {
            get { return DialogResult.None; }
            set { }
        }

        private bool privateShowIcon = true;
        private Image privateHiddenImage;

        [Category("HartUI")]
        [Description("Whether to show the calendar icon.")]
        public bool ShowIcon
        {
            get
            {
                return privateShowIcon;
            }
            set
            {
                if (privateShowIcon == value) return;
                privateShowIcon = value;

                if (value)
                {
                    Image = privateHiddenImage;
                    privateHiddenImage = null;
                }
                else
                {
                    privateHiddenImage = Image;
                    Image = null;
                }
            }
        }

        private DateTime privateContentValue = DateTime.MinValue;

        [Category("HartUI")]
        [Description("The currently selected date. Rendered as text through the button's own Content.")]
        public new DateTime Content
        {
            get
            {
                return privateContentValue;
            }
            set
            {
                DateTime normalized = value.Date;
                if (privateContentValue == normalized) return;

                privateContentValue = normalized;
                base.Content = privateContentValue.ToShortDateString();

                DateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("HartUI")]
        public event EventHandler DateChanged;

        DatePicker _PickerForm;
        public bool isDialogVisible = false;

        [Category("HartUI")]
        [Description("Where the picker should show up, relative to where the control's bounds.")]
        public enum Position
        {
            Top = 1,
            Left = 2,
            Bottom = 3,
            Right = 4,

            TopLeft = 5,
            TopRight = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [Category("HartUI")]
        public Position PickerPosition { get; set; } = Position.Bottom;

        internal void ShowDialog()
        {
            if (isDialogVisible)
                return;

            isDialogVisible = true;

            var pickerForm = new DatePicker(Content);
            _PickerForm = pickerForm;

            pickerForm.StartPosition = FormStartPosition.Manual;
            pickerForm.ShowInTaskbar = false;

            pickerForm.FormClosed += (_, __) =>
            {
                if (pickerForm.DialogResult == DialogResult.OK)
                    Content = pickerForm.Value;

                if (ReferenceEquals(_PickerForm, pickerForm))
                    _PickerForm = null;

                isDialogVisible = false;
            };

            Point basePoint = FindForm().Location + ((Size)Location);
            int rounding = pickerForm.cuiFormRounder1.Rounding;
            Size pickerSize = pickerForm.Size;

            Point location;

            switch (PickerPosition)
            {
                case Position.Top:
                    location = basePoint + new Size(Width / 2 + rounding, rounding * 4) - new Size(pickerSize.Width / 2, pickerSize.Height);
                    break;

                case Position.Left:
                    location = basePoint + new Size(rounding, Height / 2 + rounding * 4) - new Size(pickerSize.Width, pickerSize.Height / 2);
                    break;

                case Position.Right:
                    location = basePoint + new Size(Width + rounding, Height / 2 + rounding * 4) - new Size(0, pickerSize.Height / 2);
                    break;

                case Position.TopLeft:
                    location = basePoint + new Size(rounding, rounding * 4) - pickerSize;
                    break;

                case Position.TopRight:
                    location = basePoint + new Size(Width + rounding, rounding * 4) - new Size(0, pickerSize.Height);
                    break;

                case Position.BottomLeft:
                    location = basePoint + new Size(rounding, Height + rounding * 4) - new Size(pickerSize.Width, 0);
                    break;

                case Position.BottomRight:
                    location = basePoint + new Size(Width + rounding, Height + rounding * 4);
                    break;

                default: // Position.Bottom
                    location = basePoint + new Size(Width / 2 + rounding, Height + rounding * 4) - new Size(pickerSize.Width / 2, 0);
                    break;
            }

            pickerForm.Location = location;

            var owner = FindForm();
            if (owner != null)
                pickerForm.Show(owner);
            else
                pickerForm.Show();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (!isDialogVisible)
            {
                ShowDialog();
            }
        }
    }
}
