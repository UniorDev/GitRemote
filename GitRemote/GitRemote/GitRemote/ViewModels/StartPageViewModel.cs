﻿using GitRemote.Services;
using GitRemote.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace GitRemote.ViewModels
{
    public class StartPageViewModel : BindableBase
    {
        public string StartPageImagePath => "gitremote_logo.png";
        public DelegateCommand LogInWithExistUserButtonCommand { get; }
        public DelegateCommand CreateNewUserButtonCommand { get; }
        private readonly INavigationService _navigationService;

        public StartPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LogInWithExistUserButtonCommand = new DelegateCommand(OnLogInWithExistUserButtonTapped);
            CreateNewUserButtonCommand = new DelegateCommand(OnCreateNewUserButtonTapped);
        }

        private void OnLogInWithExistUserButtonTapped()
        {
            _navigationService.NavigateAsync($"{nameof(NavigationBarPage)}/{nameof(ChooseUserPage)}");
        }

        private void OnCreateNewUserButtonTapped()
        {
            _navigationService.NavigateAsync($"{nameof(NavigationBarPage)}/{nameof(LoginingPage)}");
        }
    }
}
