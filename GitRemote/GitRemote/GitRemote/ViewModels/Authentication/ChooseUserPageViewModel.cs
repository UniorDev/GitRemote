﻿using GitRemote.DI;
using GitRemote.GitHub;
using GitRemote.GitHub.Managers;
using GitRemote.Services;
using GitRemote.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Label = Xamarin.Forms.Label;
using LoginingPage = GitRemote.Views.Authentication.LoginingPage;

namespace GitRemote.ViewModels.Authentication
{
    public class ChooseUserPageViewModel : BindableBase
    {
        public ObservableCollection<string> Users => _userManager?.GetAllUsers();

        private ViewCell _previousCell;
        private ViewCell _currentCell;
        private StackLayout _currentStackLayout;
        private StackLayout _previousStackLayout;

        public DelegateCommand<ViewCell> ListItemTappedCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand OpenCommand { get; }

        private readonly INavigationService _navigationService;
        private readonly ISecuredDataProvider _securedDataProvider;
        private readonly IPageDialogService _pageDialogService;

        private readonly UserManager _userManager;

        public ChooseUserPageViewModel(INavigationService navigationService,
                                       ISecuredDataProvider securedDataProvider,
                                       IPageDialogService pageDialogService)
        {
            _navigationService = navigationService;
            _securedDataProvider = securedDataProvider;
            _pageDialogService = pageDialogService;
            _userManager = new UserManager(_securedDataProvider);
            ListItemTappedCommand = new DelegateCommand<ViewCell>(OnListItemTapped);
            AddCommand = new DelegateCommand(OnAdd);
            DeleteCommand = new DelegateCommand(OnDelete);
            OpenCommand = new DelegateCommand(OnOpen);
        }

        /// <summary>
        /// Method that fires when item of list tapped.
        /// Sets property background and Hides/Shows delete and open buttons(images)
        /// </summary>
        /// <param name="cell">Cell that is tapped</param>
        public void OnListItemTapped(ViewCell cell)
        {
            _currentCell = cell;

            if ( _previousCell == _currentCell ) return; // If it is the same cell

            ( ( ListView )_currentCell.Parent ).SelectedItem = 0;

            if ( _previousCell != null )
                _previousCell.View.BackgroundColor = Color.Default; // Clears color from previous tapped list item

            _currentCell.View.BackgroundColor = Color.FromHex("#EDEDED");

            _currentStackLayout = _currentCell.View as StackLayout;

            if ( _currentStackLayout != null ) //Shows buttons of current item
            {
                _currentStackLayout.Children[1].IsVisible = true;
                _currentStackLayout.Children[1].IsEnabled = true;
                _currentStackLayout.Children[2].IsVisible = true;
                _currentStackLayout.Children[2].IsEnabled = true;
            }

            if ( _previousStackLayout != null ) //Hides buttons of previous item
            {
                _previousStackLayout.Children[1].IsVisible = false;
                _previousStackLayout.Children[1].IsEnabled = false;
                _previousStackLayout.Children[2].IsVisible = false;
                _previousStackLayout.Children[2].IsEnabled = false;
            }

            _previousCell = _currentCell;
            _previousStackLayout = _currentStackLayout;
        }

        public void OnAdd()
        {
            _currentCell = null;
            _navigationService.NavigateAsync($"{nameof(LoginingPage)}");
        }

        public async void OnDelete()
        {
            var answer = await _pageDialogService.DisplayAlertAsync("Delete Operation",
                                                              "Are you sure that you want to delete this user?",
                                                              "Yes", "No");
            if ( answer )
                if ( _currentCell != null )
                {
                    var currentCellName = ( ( Label )( ( StackLayout )_currentCell.View ).Children[0] ).Text;
                    _securedDataProvider.Clear(currentCellName);
                    Users.Remove(currentCellName);
                }

            _currentCell = null;
        }

        public void OnOpen()
        {
            if ( _currentCell == null ) return;

            var currentCellName = ( ( Label )( ( StackLayout )_currentCell.View ).Children[0] ).Text;
            var token = _securedDataProvider.Retreive(ConstantsService.ProviderName, currentCellName);

            UserManager.SetLastUser(currentCellName);
            var parameters = new NavigationParameters { { "Session", new Session(currentCellName, token.Properties.First().Value) } };
            var navigationStack = new Uri("https://Necessary/" + $"{nameof(PrivateProfilePage)}/{nameof(NavigationBarPage)}/{nameof(DetailPage)}",
                UriKind.Absolute);
            _currentCell = null;
            _navigationService.NavigateAsync(navigationStack, parameters, animated: false);
        }
    }
}
