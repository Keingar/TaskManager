using ToDoList.Services;
using ToDoList.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using ToDoList.DataModel;
using testAvalonijaAbilitiesApp.DataModel;
using System.Reactive;

namespace testAvalonijaAbilitiesApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _contentViewModel;

        //  private ToDoListService toDoListService = new ToDoListService();
        public ReactiveCommand<Unit, Unit> TryLoginCommand { get; }

        public MainWindowViewModel()
        {

            if (!ToDoListService.DatabaseExists())
            {
                ToDoListService.RestoreDatabase("TaskManagerDB");
            }

            TryLoginCommand = ReactiveCommand.Create(TryLogin);
            _contentViewModel = new SignInViewModel();

        }

        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        public void ChangeWindowToRegister()
        {
            SignUpViewModel RegisterViewModel = new();

            ContentViewModel = RegisterViewModel;
        }

        public void ChangeWindowToLogin()
        {
            SignInViewModel LoginViewModel = new();

            ContentViewModel = LoginViewModel;
        }

        public void TryLogin()
        {
            SignInViewModel signInViewModel = (SignInViewModel)ContentViewModel;

            string username = signInViewModel.TryLogin();

            if(username == string.Empty)
            {
                return; // not successful login
            }

            ChnageWindowToDoList(ToDoListService.GetUser(username));

        }

        public void TrySignUp()
        {
            SignUpViewModel signUpViewModel = (SignUpViewModel)ContentViewModel;

            string username = signUpViewModel.TrySignUp();

            if(username == string.Empty)
            {
                return; // not successsful sing up 
            }

            ChnageWindowToDoList(ToDoListService.GetUser(username));

        }

        public void ChnageWindowToDoList(user currentUser)
        {
            ContentViewModel = new ToDoListViewModel(currentUser);
        }

    }
}

