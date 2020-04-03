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

            var inPutIsValid = InputFieldValidation.IsValidInputName(fullText);
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
            var fullPass = passBox.Password + e.Text;

            var inPutIsValid = InputFieldValidation.IsValidInputPassword(fullPass);
            if (inPutIsValid) CoreContext.AccountService.RegInputFieldPassword = fullPass;
            e.Handled = !inPutIsValid;
        }
        
        private void TextBox_OnPreviewTextInputPasswordRepeat(object sender, TextCompositionEventArgs e)
        {
            var passBox = sender as PasswordBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var fullPass = passBox.Password + e.Text;

            var inPutIsValid = InputFieldValidation.IsValidInputPassword(fullPass);
            if (inPutIsValid) CoreContext.AccountService.RegInputFieldConfirmPassword = fullPass;
            e.Handled = !inPutIsValid;
        }

    }
}