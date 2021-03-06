﻿using Prism.Mvvm;
using Prism.Navigation;

namespace GitRemote.ViewModels.MasterMenuPage
{
    public class BookmarksPageViewModel : BindableBase
    {
        private INavigationService _navigationService;

        public BookmarksPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
    }
}
