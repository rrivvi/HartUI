using System;
using System.Linq;
using System.Windows.Forms;

namespace HartUI.Controls.Internal
{
    internal partial class DigitsOnlyTextBox : TextBox
    {
        public DigitsOnlyTextBox()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_CHAR = 0x0102;
            const int WM_PASTE = 0x0302;

            if (m.Msg == WM_CHAR)
            {
                char c = (char)m.WParam;

                // Allow Backspace
                if (c == (char)Keys.Back)
                {
                    base.WndProc(ref m);
                    int caretPositionBeforeFormat2 = SelectionStart;
                    string newText2 = Text.Insert(caretPositionBeforeFormat2, c.ToString());
                    string cleaned2 = new string(newText2.Where(char.IsDigit).Take(19).ToArray());

                    string formatted2 = string.Join(" ", Enumerable.Range(0, (cleaned2.Length + 3) / 4)
                        .Select(i => cleaned2.Substring(i * 4, Math.Min(4, cleaned2.Length - i * 4))));

                    int digitsBeforeCaret2 = newText2.Take(caretPositionBeforeFormat2 + 1).Count(char.IsDigit);
                    int newCaretPosition2 = 0;
                    int digitsCounted2 = 0;
                    foreach (var part in formatted2.Split(' '))
                    {
                        if (digitsCounted2 + part.Length >= digitsBeforeCaret2)
                        {
                            newCaretPosition2 += digitsBeforeCaret2 - digitsCounted2;
                            break;
                        }
                        newCaretPosition2 += part.Length + 1;
                        digitsCounted2 += part.Length;
                    }

                    Text = formatted2;
                    SelectionStart = Math.Min(newCaretPosition2, Text.Length);
                    return;
                }

                // Allow only digits
                if (!char.IsDigit(c))
                    return;

                int caretPositionBeforeFormat = SelectionStart;
                string newText = Text.Insert(caretPositionBeforeFormat, c.ToString());
                string cleaned = new string(newText.Where(char.IsDigit).Take(19).ToArray());

                string formatted = string.Join(" ", Enumerable.Range(0, (cleaned.Length + 3) / 4)
                    .Select(i => cleaned.Substring(i * 4, Math.Min(4, cleaned.Length - i * 4))));

                int digitsBeforeCaret = newText.Take(caretPositionBeforeFormat + 1).Count(char.IsDigit);
                int newCaretPosition = 0;
                int digitsCounted = 0;
                foreach (var part in formatted.Split(' '))
                {
                    if (digitsCounted + part.Length >= digitsBeforeCaret)
                    {
                        newCaretPosition += digitsBeforeCaret - digitsCounted;
                        break;
                    }
                    newCaretPosition += part.Length + 1;
                    digitsCounted += part.Length;
                }

                Text = formatted;
                SelectionStart = Math.Min(newCaretPosition, Text.Length);
                return;
            }
            else if (m.Msg == WM_PASTE)
            {
                try
                {
                    string c = Clipboard.GetText();
                    int caretPositionBeforeFormat = SelectionStart;
                    string newText = Text.Insert(caretPositionBeforeFormat, c.ToString());
                    string cleaned = new string(newText.Where(char.IsDigit).Take(19).ToArray());

                    string formatted = string.Join(" ", Enumerable.Range(0, (cleaned.Length + 3) / 4)
                                    .Select(i => cleaned.Substring(i * 4, Math.Min(4, cleaned.Length - i * 4))));

                    int digitsBeforeCaret = newText.Take(caretPositionBeforeFormat + 1).Count(char.IsDigit);
                    int newCaretPosition = 0;
                    int digitsCounted = 0;
                    foreach (var part in formatted.Split(' '))
                    {
                        if (digitsCounted + part.Length >= digitsBeforeCaret)
                        {
                            newCaretPosition += digitsBeforeCaret - digitsCounted;
                            break;
                        }
                        newCaretPosition += part.Length + 1;
                        digitsCounted += part.Length;
                    }

                    Text = formatted;
                }
                finally
                {

                }
                return;
            }

            base.WndProc(ref m);
        }
    }
}
