using testAvalonijaAbilitiesApp.ViewModels;
using ReactiveUI;
using ToDoList.Services;

namespace ToDoList.ViewModels
{
    public class SignInViewModel : ViewModelBase
    {

        public SignInViewModel()
        {

        }

        private bool _showErrorMessage = false;
        public bool ShowErrorMessage
        {
            get => _showErrorMessage;
            set => this.RaiseAndSetIfChanged(ref _showErrorMessage, value);
        }

        private string _errorMessageText = string.Empty;
        public string ErrorMessageText
        {
            get => _errorMessageText;
            set => this.RaiseAndSetIfChanged(ref _errorMessageText, value);
        }

        private string _usernameLogin = string.Empty;
        public string UsernameLogin
        {
            get => _usernameLogin;
            set => this.RaiseAndSetIfChanged(ref _usernameLogin, value);
        }

        private string _usernamePassword = string.Empty;
        public string UsernamePassword
        {
            get => _usernamePassword;
            set => this.RaiseAndSetIfChanged(ref _usernamePassword, value);
        }

        public string TryLogin()
        {
            int status = ToDoListService.SignInUser(UsernameLogin, UsernamePassword);

            if(status != 0)
            {
                ErrorMessageText = "Invalid username or password";
                ShowErrorMessage = true;

                return string.Empty;
            }

            return UsernameLogin;

        }

    }
}
