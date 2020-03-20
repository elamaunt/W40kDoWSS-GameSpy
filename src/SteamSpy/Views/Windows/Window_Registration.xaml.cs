using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class Window_Registration
    {
        /**
         * Отклоняем в полях ввода невалидные для геймспая ники
         */
        private void TextBox_OnPreviewTextInputLogin(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            var inPutIsValid = inputIsValid(fullText);
            if (inPutIsValid) CoreContext.AccountService.RegInputFieldLogin = fullText;
            e.Handled = !inPutIsValid;
        }
        
        /**
         * И пароли тоже на всякий случай
         */
        private void TextBox_OnPreviewTextInputPassword(object sender, TextCompositionEventArgs e)
        {
            var passBox = sender as PasswordBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            // var fullText = passBox.Password.Insert(pass.SelectionStart, e.Text);
            var fullPass = passBox.Password + e.Text;

            var inPutIsValid = inputIsValid(fullPass);
            if (inPutIsValid) CoreContext.AccountService.RegInputFieldPassword = fullPass;
            e.Handled = !inPutIsValid;
        }
        
        private void TextBox_OnPreviewTextInputPasswordRepeat(object sender, TextCompositionEventArgs e)
        {
            var passBox = sender as PasswordBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            // var fullText = passBox.Password.Insert(pass.SelectionStart, e.Text);
            var fullPass = passBox.Password + e.Text;

            var inPutIsValid = inputIsValid(fullPass);
            if (inPutIsValid) CoreContext.AccountService.RegInputFieldConfirmPassword = fullPass;
            e.Handled = !inPutIsValid;
        }

        private bool inputIsValid(string text)
        {
            Regex regexObj = new Regex("^[A-Za-z0-9_-]+$(?#case sensitive, matches only lower a-z)", RegexOptions.Multiline);
            Match matchResults = regexObj.Match(text);
            
            if (matchResults.Success && text.Length < 15) return true;

            return false;
        }

    }
}