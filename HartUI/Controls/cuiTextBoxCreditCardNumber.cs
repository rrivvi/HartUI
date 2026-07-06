using HartUI.Helpers;
using HartUI.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HartUI.Controls
{
    [ToolboxBitmap(typeof(TextBox))]
    [Description("Card company's logo shows as the user inputs their credit card number")]
    [DefaultEvent("IsCardValidChanged")]
    public partial class cuiTextBoxCreditCardNumber : UserControl
    {
        private Color privateBackgroundColor = Color.White;
        private Color privateFocusBackgroundColor = Color.White;

        private Color privateBorderColor = Color.FromArgb(128, 128, 128, 128);

        private Color privateFocusBorderColor = Helpers.DrawingHelper.PrimaryColor;
        private int privateBorderSize = 1;
        private bool privateUnderlinedStyle = true;

        internal bool internalIsFocused = false;
        private bool privateIsFocused
        {
            get
            {
                return internalIsFocused;
            }
            set
            {
                internalIsFocused = value;
                contentTextField.BackColor = value ? FocusBackgroundColor : BackgroundColor;
                placeholderTextField.BackColor = contentTextField.BackColor;
                Invalidate();
            }
        }

        private Padding privateBorderRadius = new System.Windows.Forms.Padding(8, 8, 8, 8);
        private string privatePlaceholderText = "";
        private bool privateIsPlaceholder = false;

        public event EventHandler ContentChanged;

        public event EventHandler IsCardValidChanged;

        public cuiTextBoxCreditCardNumber()
        {
            InitializeComponent();
            base.BackColor = Color.Empty;
            ForeColor = Color.Gray;
            Load += OnLoad;
            GotFocus += OnLoad;
            ContentChanged += CuiTextBoxCreditCardNumber_ContentChanged;
            PlaceholderText = "Credit card number here..";
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.privateIsFocused = false;
        }

        [Category("HartUI")]
        public Color BackgroundColor
        {
            get
            {
                return privateBackgroundColor;
            }
            set
            {
                privateBackgroundColor = value;
                if (DesignMode)
                {
                    contentTextField.BackColor = value;
                    placeholderTextField.BackColor = value;
                }
                else
                {
                    contentTextField.BackColor = privateIsFocused ? FocusBackgroundColor : value;
                    placeholderTextField.BackColor = contentTextField.BackColor;
                }
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color FocusBackgroundColor
        {
            get
            {
                return privateFocusBackgroundColor;
            }
            set
            {
                privateFocusBackgroundColor = value;
                if (DesignMode)
                {
                    contentTextField.BackColor = BackgroundColor;
                }
                else
                {
                    contentTextField.BackColor = privateIsFocused ? FocusBackgroundColor : value;
                }
                placeholderTextField.BackColor = contentTextField.BackColor;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color OutlineColor
        {
            get
            {
                return privateBorderColor;
            }
            set
            {
                privateBorderColor = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public Color FocusOutlineColor
        {
            get
            {
                return privateFocusBorderColor;
            }
            set
            {
                privateFocusBorderColor = value;
            }
        }

        [Category("HartUI")]
        private int OutlineThickness
        {
            get
            {
                return privateBorderSize;
            }
            set
            {
                if (value >= 1)
                {
                    privateBorderSize = value;
                    Invalidate();
                }
            }
        }

        [Category("HartUI")]
        public bool UnderlinedStyle
        {
            get
            {
                return privateUnderlinedStyle;
            }
            set
            {
                privateUnderlinedStyle = value;
                Invalidate();
            }
        }

        [Category("HartUI")]
        public bool PasswordChar
        {
            get
            {
                return contentTextField.UseSystemPasswordChar;
            }
            set
            {
                contentTextField.UseSystemPasswordChar = value;
            }
        }

        [Category("HartUI")]
        private new Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                value = Color.FromArgb(255, value); // prevent transparency crashes
                base.BackColor = value;
            }
        }

        [Category("HartUI")]
        public override Color ForeColor
        {
            get
            {
                return contentTextField.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                contentTextField.ForeColor = value;
                contentTextField.Invalidate();
            }
        }

        [Category("HartUI")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                contentTextField.Font = value;
                placeholderTextField.Font = value;
            }
        }

        protected string actualText = "";

        [Category("HartUI")]
        public string Content
        {
            get
            {
                return actualText;
            }
            set
            {
                actualText = value;
                contentTextField.Text = value;

                UpdatePlaceholder();
            }
        }

        public new string Text
        {
            get
            {
                return Content;
            }
            set
            {
                Content = value;
            }
        }

        public override void ResetText()
        {
            Content = string.Empty;
            base.ResetText();
        }

        [Category("HartUI")]
        public Padding Rounding
        {
            get
            {
                return privateBorderRadius;
            }
            set
            {
                if (value == new Padding(0, 0, 0, 0))
                {
                    value = new Padding(2, 2, 2, 2);
                }
                if (value.All >= 0 || value.All == -1)
                {
                    privateBorderRadius = value;
                    Invalidate();//Redraw control
                }
            }
        }

        [Category("HartUI")]
        public Color PlaceholderColor
        {
            get
            {
                return placeholderTextField.ForeColor;
            }
            set
            {
                placeholderTextField.ForeColor = value;
            }
        }

        [Category("HartUI")]
        public string PlaceholderText
        {
            get
            {
                return privatePlaceholderText;
            }
            set
            {
                privatePlaceholderText = value;
                UpdatePlaceholder();
            }
        }

        private Image privateImage = Properties.Resources.credit_card;

        protected override void OnPaint(PaintEventArgs e)
        {
            placeholderTextField.Visible = privateIsPlaceholder;
            Graphics g = e.Graphics;

            using (SolidBrush bgBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(bgBrush, ClientRectangle);
            }

            Padding newPadding;

            int newTextboxY = (Height / 2) - (Font.Height / 2);
            if (newTextboxY < 0)
            {
                newTextboxY = -newTextboxY;
            }
            newPadding = new Padding(Font.Height, newTextboxY, Font.Height, 0);

            newPadding.Left += 24;
            newPadding.Right += 24;

            Padding = newPadding;

            if (privateBorderRadius.All > 1 || privateBorderRadius.All == -1) //Rounded
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var rectBorderSmooth = ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -OutlineThickness, -OutlineThickness);
                //rectBorder.Offset(-BorderSize, -BorderSize);

                int smoothSize = privateBorderSize > 0 ? privateBorderSize : 1;

                using (SolidBrush bgBrush = new SolidBrush(privateIsFocused ? FocusBackgroundColor : BackgroundColor))
                using (GraphicsPath pathBorderSmooth = GeneralHelper.RoundRect(rectBorderSmooth, Rounding))
                using (GraphicsPath pathBorder = GeneralHelper.RoundRect(rectBorder, Rounding - new Padding(OutlineThickness, OutlineThickness, OutlineThickness, OutlineThickness) - new Padding(1, 1, 1, 1)))
                using (Pen penBorderSmooth = new Pen(BackColor, smoothSize))
                using (Pen penBorder = new Pen(privateIsFocused ? FocusOutlineColor : OutlineColor, OutlineThickness) { Alignment = PenAlignment.Center })
                {
                    e.Graphics.FillPath(bgBrush, pathBorder);

                    if (UnderlinedStyle)
                    {
                        // Draw border smoothing
                        g.DrawPath(penBorderSmooth, pathBorderSmooth);

                        RectangleF bounds = pathBorder.GetBounds();
                        RectangleF bottomHalfBounds = new RectangleF(bounds.X + 1, bounds.Y + bounds.Height / 2, bounds.Width - 1, bounds.Height / 2 + 1);

                        using (Region bottomHalfRegion = new Region(bottomHalfBounds))
                        {
                            // Set the clipping region for the path
                            g.SetClip(bottomHalfRegion, CombineMode.Intersect);

                            // Draw the bottom half
                            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                            g.DrawPath(penBorder, pathBorder);

                            // Reset the clipping region
                            g.ResetClip();
                        }
                    }
                    else // Normal
                    {
                        // Draw border smoothing
                        g.DrawPath(penBorderSmooth, pathBorderSmooth);

                        g.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else // Square/Normal TextBox
            {
                // Draw border
                using (Pen penBorder = new Pen(OutlineColor, OutlineThickness))
                {
                    Region = new Region(ClientRectangle);
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    if (privateIsFocused)
                        penBorder.Color = FocusOutlineColor;

                    if (UnderlinedStyle)
                        g.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                    else // Normal
                        g.DrawRectangle(penBorder, 0, 0, Width - 0.5F, Height - 0.5F);
                }
            }

            Rectangle imageRectangle = new Rectangle(contentTextField.Height, contentTextField.Location.Y, contentTextField.Height, contentTextField.Height);

            e.Graphics.DrawImage(
                privateImage,
                imageRectangle,
                0, 0, privateImage.Width, privateImage.Height,
                GraphicsUnit.Pixel);

            base.OnPaint(e);
        }

        protected void UpdatePlaceholder()
        {
            placeholderTextField.Text = PlaceholderText;

            if (actualText == "" && !internalIsFocused)
            {
                placeholderTextField.Visible = true;
                privateIsPlaceholder = true;
            }
            else
            {
                placeholderTextField.Visible = false;
                privateIsPlaceholder = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            actualText = contentTextField.Text;
            UpdatePlaceholder();
            ContentChanged?.Invoke(this, e);
        }
        private void textBox1_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }
        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            privateIsFocused = true;
            UpdatePlaceholder();
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            privateIsFocused = false;
            Invalidate();
            UpdatePlaceholder();
        }

        private void cuiTextBox2_Click(object sender, EventArgs e)
        {
            contentTextField.Focus();
            Invalidate();
        }

        private void textBox2_MouseDown(object sender, MouseEventArgs e)
        {
            cuiTextBox2_Click(sender, e);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (placeholderTextField.Text != PlaceholderText)
            {
                placeholderTextField.Text = PlaceholderText;
            }
        }

        private void CuiTextBoxCreditCardNumber_ContentChanged(object sender, EventArgs e)
        {
            CardType detectedCompany = DetectCardType(Content);
            Company = detectedCompany;

            switch (detectedCompany)
            {
                case CardType.Unknown:
                    privateImage = Resources.credit_card;
                    break;

                case CardType.Visa:
                    privateImage = Resources.visa;
                    break;

                case CardType.MasterCard:
                    privateImage = Resources.mastercard;
                    break;

                case CardType.AmericanExpress:
                    privateImage = Resources.americanexpress;
                    break;

                case CardType.Discover:
                    privateImage = Resources.discover;
                    break;

                case CardType.JCB:
                    privateImage = Resources.jcb;
                    break;

                case CardType.Maestro:
                    privateImage = Resources.maestro;
                    break;

                case CardType.DinersClub:
                    privateImage = Resources.dinersclub;
                    break;

                case CardType.UnionPay:
                    privateImage = Resources.unionpay;
                    break;

                case CardType.RuPay:
                    privateImage = Resources.rupay;
                    break;
            }

            Invalidate();
        }

        [Category("HartUI")]
        public bool IsCardValid => privateIsCardValid && DetectCardType(actualText) != CardType.Unknown;

        private bool privateIsCardValid = false;

        private bool IsCardValidGetter
        {
            get
            {
                string cleaned = actualText.Replace(" ", "");
                bool newCardValidValue = cleaned.Length > 11 && cleaned.Length < 20;
                if (privateIsCardValid != newCardValidValue)
                {
                    privateIsCardValid = newCardValidValue;
                    IsCardValidChanged?.Invoke(this, null);
                    return privateIsCardValid;
                }

                privateIsCardValid = newCardValidValue;
                return privateIsCardValid;
            }
        }

        public CardType DetectCardType(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(cardNumber) || !IsCardValidGetter)
                return CardType.Unknown;

            int prefix2 = int.Parse(cardNumber.Substring(0, 2));
            int prefix3 = int.Parse(cardNumber.Substring(0, 3));
            int prefix4 = int.Parse(cardNumber.Substring(0, 4));
            int prefix6 = int.Parse(cardNumber.Substring(0, 6));

            // Visa / Visa Electron
            if (cardNumber.StartsWith("4"))
            {
                return CardType.Visa;
            }

            // MasterCard (incl. 2221–2720)
            if ((prefix2 >= 51 && prefix2 <= 55)
             || (prefix4 >= 2221 && prefix4 <= 2720))
                return CardType.MasterCard;

            // AmEx
            if (cardNumber.StartsWith("34")
             || cardNumber.StartsWith("37"))
                return CardType.AmericanExpress;

            // Discover
            if (cardNumber.StartsWith("6011")
             || cardNumber.StartsWith("65")
             || (prefix6 >= 622126 && prefix6 <= 622925)
             || (prefix3 >= 644 && prefix3 <= 649))
                return CardType.Discover;

            // JCB
            if (prefix4 >= 3528 && prefix4 <= 3589)
                return CardType.JCB;

            // Maestro
            if (prefix4 == 5018
             || prefix4 == 5020
             || prefix4 == 5038
             || prefix4 == 5893
             || prefix4 == 6304
             || prefix4 == 6759
             || prefix4 == 6761
             || prefix4 == 6762
             || prefix4 == 6763)
                return CardType.Maestro;

            // Diners Club
            if (prefix2 == 36 || prefix2 == 38 || prefix2 == 39)
                return CardType.DinersClub;

            // UnionPay
            if (cardNumber.StartsWith("62"))
                return CardType.UnionPay;

            // RuPay
            if ((prefix6 >= 508500 && prefix6 <= 508999)
             || (prefix6 >= 606985 && prefix6 <= 607984)
             || (prefix6 >= 608001 && prefix6 <= 608500)
             || (prefix6 >= 652150 && prefix6 <= 653149))
                return CardType.RuPay;

            // UATP
            if (cardNumber.StartsWith("1"))
                return CardType.UATP;

            return CardType.Unknown;
        }

        public enum CardType
        {
            Unknown,
            Visa,
            MasterCard,
            AmericanExpress,
            Discover,
            JCB,
            Maestro,
            DinersClub,
            UnionPay,
            RuPay,
            UATP
        }

        private CardType privateCompany = CardType.Unknown;
        [Category("HartUI")]
        public CardType Company
        {
            get
            {
                return privateCompany;
            }
            private set
            {
                privateCompany = value;
            }
        }
    }
}
