﻿using GitRemote.CustomClasses;
using GitRemote.DI;
using GitRemote.GitHub.Managers;
using GitRemote.Models;
using GitRemote.Services;
using GitRemote.Views;
using Nito.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using static System.String;

namespace GitRemote.ViewModels
{
    public class FileExplorerPageViewModel : BindableBase, INavigationAware
    {
        #region Props
        public ObservableCollection<FileExplorerModel> FileTree { get; set; }
        public DelegateCommand BotPanelTapped { get; }
        public DelegateCommand ListItemTappedCommand { get; }
        public FileExplorerModel LastTappedItem { get; set; }
        public string PathPartsIcon => FontIconsService.Octicons.SubModule;
        public string BranchIcon => _currentSourceType == "Branch"
            ? FontIconsService.Octicons.Branch
            : FontIconsService.Octicons.Tag;

        public string CurrentBranch
        {
            get { return _currentBranch; }
            set { SetProperty(ref _currentBranch, value); }
        }

        public GridLength PathPartsRowWidth => new GridLength(App.ScreenWidth - 40 - 10 - 10); // fontIcon, spaceBefore, spaceAfter
        public bool PathPartsGridIsVisible
        {
            get { return _pathPartsGridIsVisible; }
            set { SetProperty(ref _pathPartsGridIsVisible, value); }
        }
        #endregion

        #region Fields
        private readonly INavigationService _navigationService;
        public NotifyTask SetCurrentBranchTask;
        public NotifyTask SetTreeTask;
        private readonly FileExplorerManager _manager;
        private Grid _pathPartsGrid;
        private bool _pathPartsGridIsVisible;
        private readonly List<bool> _layoutIsFull = new List<bool>();
        //[0] max index of layout in grid, [1] max index of parts in layout, [2] count of parts in grid
        private readonly List<int> _partsIndexes = new List<int>();
        private int _partsCount;
        private readonly Color _hyperLinkColor = Color.FromHex("3366BB");
        private string _currentSourceType = "Branch";
        private string _currentBranch = Empty;
        #endregion

        public FileExplorerPageViewModel(INavigationService navigationService)
        {
            //_manager = new FileExplorerManager("UniorDev", "GitRemote");
            _manager = new FileExplorerManager("UniorDev", "ForkHub");
            SetCurrentBranchTask = NotifyTask.Create(_manager.SetCurrentBranchAsync());
            SetCurrentBranchTask.TaskCompleted.ContinueWith(branchTask =>
            {
                CurrentBranch = _manager.CurrentBranch;
                SetTreeTask = NotifyTask.Create(_manager.SetTreeAsync());
                SetTreeTask.TaskCompleted.ContinueWith(treeTask =>
                {
                    //FileTree = _manager.GetFiles("GitRemote/");
                    FileTree = _manager.GetFiles("ForkHub/");
                    OnPropertyChanged(nameof(FileTree));
                });
            });

            _navigationService = navigationService;
            ListItemTappedCommand = new DelegateCommand(OnListItemTapped);
            BotPanelTapped = new DelegateCommand(OnBotPanelTapped);
            MessagingCenter.Subscribe<string>(this, ConstantsService.Messages.HardwareBackPressed, OnHardwareBackPressed);
            MessagingCenter.Subscribe<Grid>(this, ConstantsService.Messages.TakePathPartsGrid, SetPathPartsGrid);
            MessagingCenter.Subscribe<SelectBranchPopUpModel>(this, ConstantsService.Messages.TakeBranchModelFromPopUpPage, OnBranchSelected);
        }

        private async void OnBranchSelected(SelectBranchPopUpModel selectBranchPopUpModel)
        {
            _currentSourceType = selectBranchPopUpModel.Type;
            await _manager.SetCurrentBranchAsync(selectBranchPopUpModel.Name);
        }

        private void OnBotPanelTapped()
        {
            PopupNavigation.PushAsync(new SelectBranchPopUpPage());
            MessagingCenter.Send(_manager, ConstantsService.Messages.SendManagerToBranchPopUpPage);

        }

        private void SetPathPartsGrid(Grid grid)
        {
            _pathPartsGrid = grid;
            _partsIndexes.Add(0);

            var layout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Horizontal,
                Spacing = 3
            };

            _partsCount++;

            #region LinkLabel

            var pathPart = new HyperLinkLabel
            {
                Text = "GitRemote",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        NumberOfTapsRequired = 1,
                        Command = new DelegateCommand<int[]>(OnPathPartTapped),
                        CommandParameter = new[]{ _partsIndexes.Count - 1, _partsIndexes[_partsIndexes.Count - 1], _partsCount}
                    }
                }
            };

            #endregion

            layout.Children.Add(pathPart);
            _pathPartsGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = 20 } };
            _pathPartsGrid.Children.Add(layout, 1, 0);
            _layoutIsFull.Add(false);

        }

        private void OnPathPartTapped(int[] indexes)
        {
            var i = _pathPartsGrid.Children.Count - 2; // Cause FontIcon
            for ( ; i > indexes[0]; i-- )
            {
                var deadLayout = ( StackLayout )_pathPartsGrid.Children[i + 1];
                var doubleCount = deadLayout.Children.Count / 2f; // Cause slashes
                _partsCount -= ( int )Math.Floor(doubleCount) + 1;
                _pathPartsGrid.Children.RemoveAt(i + 1);
                var nextLayout = ( StackLayout )_pathPartsGrid.Children[_pathPartsGrid.Children.Count - 1];
                nextLayout.Children.RemoveAt(nextLayout.Children.Count - 1);
                _partsIndexes.RemoveAt(i);
                _layoutIsFull.RemoveAt(_layoutIsFull.Count - 1);
            }

            var layout = ( StackLayout )_pathPartsGrid.Children[i + 1];
            _layoutIsFull[_layoutIsFull.Count - 1] = false;

            for ( var j = layout.Children.Count - 1; j > indexes[1] * 2; j -= 2 ) // * 2 - Cause slashes
            {
                _partsCount--;
                layout.Children.RemoveAt(j);
                layout.Children.RemoveAt(j - 1);
                _partsIndexes[i]--;
            }

            var tappedPart = ( HyperLinkLabel )layout.Children[layout.Children.Count - 1];
            tappedPart.IsUnderline = false;
            tappedPart.TextColor = Color.Black;

            SetFilesOfPart(indexes[2]);
        }

        private void SetFilesOfPart(int count)
        {
            _manager.PopUpExplorerToIndex(count);
            FileTree = _manager.GetFiles(Empty);
            OnPropertyChanged(nameof(FileTree));
            if ( count == 1 )
                PathPartsGridIsVisible = false;
        }

        private void OnListItemTapped()
        {
            if ( LastTappedItem.IsFolder )
            {
                FileTree = _manager.GetFiles(LastTappedItem.Name + "/");
                OnPropertyChanged(nameof(FileTree));
                PathPartsGridIsVisible = true;
                _partsCount++;
                AddPathPart(LastTappedItem.Name);
            }
        }

        private void AddPathPart(string pathPartName)
        {
            var layout = ( StackLayout )_pathPartsGrid.Children[_layoutIsFull.Count];
            var layoutText = Empty;

            foreach ( var child in layout.Children )
            {
                layoutText = Concat(layoutText, ( ( Label )child ).Text);
            }

            var size = DependencyService.Get<IMetricsHelper>()
                .GetWidthOfString(layoutText + "/" + pathPartName + "/", ( float )( ( Label )layout.Children[0] ).FontSize);

            #region Slash

            var slash = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Text = "/",
                FontSize = 16,
                TextColor = Color.Black,
            };

            #endregion

            var previousPart = ( HyperLinkLabel )layout.Children[layout.Children.Count - 1];
            previousPart.IsUnderline = true;
            previousPart.TextColor = _hyperLinkColor;

            if ( layout.Spacing * ( layout.Children.Count + 1 ) + size < PathPartsRowWidth.Value ) // If needed space less than available
            {
                _partsIndexes[_partsIndexes.Count - 1]++;
                #region LinkLabel
                var pathPart = new HyperLinkLabel
                {
                    Text = pathPartName,
                    FontSize = 16,
                    TextColor = Color.Black,
                    IsUnderline = false,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    GestureRecognizers =
                            {
                                new TapGestureRecognizer
                                {
                                    NumberOfTapsRequired = 1,
                                    Command = new DelegateCommand<int[]>(OnPathPartTapped),
                                    CommandParameter = new[]{ _partsIndexes.Count - 1, _partsIndexes[_partsIndexes.Count - 1], _partsCount}
                                }
                            }
                };


                #endregion
                layout.Children.Add(slash);
                layout.Children.Add(pathPart);
            }
            else
            {
                _partsIndexes.Add(0);
                #region LinkLabel
                var pathPart = new HyperLinkLabel
                {
                    Text = pathPartName,
                    FontSize = 16,
                    TextColor = Color.Black,
                    IsUnderline = false,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    GestureRecognizers =
                            {
                                new TapGestureRecognizer
                                {
                                    NumberOfTapsRequired = 1,
                                    Command = new DelegateCommand<int[]>(OnPathPartTapped),
                                    CommandParameter = new[]{ _partsIndexes.Count - 1, _partsIndexes[_partsIndexes.Count - 1], _partsCount}
                                }
                            }
                };


                #endregion
                _layoutIsFull[_layoutIsFull.Count - 1] = true;

                var newLayout = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 3
                };

                layout.Children.Add(slash);
                newLayout.Children.Add(pathPart);
                _pathPartsGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = 20 } };
                _pathPartsGrid.Children.Add(newLayout, 1, _layoutIsFull.Count);
                _layoutIsFull.Add(false);
            }
        }

        private void OnHardwareBackPressed(string sender)
        {
            if ( _manager.PopUpExplorer() ) // if there is something to PopUp
            {
                FileTree = _manager.GetFiles(Empty);
                OnPropertyChanged(nameof(FileTree));
                _partsCount--;
                if ( _partsCount == 1 )
                    PathPartsGridIsVisible = false;

                var layout = ( StackLayout )_pathPartsGrid.Children[_pathPartsGrid.Children.Count - 1];
                if ( _pathPartsGrid.Children.Count > 2 ) // If current layout not firts layout (Cause FontIcon)
                {
                    if ( layout.Children.Count < 2 ) // Remove whole layout
                    {
                        _pathPartsGrid.Children.Remove(layout);
                        _layoutIsFull.RemoveAt(_layoutIsFull.Count - 1);
                        var previousLayout = ( StackLayout )_pathPartsGrid.Children[_pathPartsGrid.Children.Count - 1];
                        previousLayout.Children.RemoveAt(previousLayout.Children.Count - 1);
                        var label = ( HyperLinkLabel )previousLayout.Children[previousLayout.Children.Count - 1];
                        label.IsUnderline = false;
                        label.TextColor = Color.Black;
                        _partsIndexes.RemoveAt(_partsIndexes.Count - 1);
                    }
                    else
                    {
                        layout.Children.RemoveAt(layout.Children.Count - 1);
                        layout.Children.RemoveAt(layout.Children.Count - 1);
                        var label = ( HyperLinkLabel )layout.Children[layout.Children.Count - 1];
                        label.IsUnderline = false;
                        label.TextColor = Color.Black;
                        _partsIndexes[_partsIndexes.Count - 1]--;
                        _layoutIsFull[_layoutIsFull.Count - 1] = false;
                    }
                }
                else
                {
                    if ( layout.Children.Count > 1 ) // Remove elements in first layout
                    {
                        layout.Children.RemoveAt(layout.Children.Count - 1);
                        if ( layout.Children.Count > 1 )
                            layout.Children.RemoveAt(layout.Children.Count - 1);
                        var label = ( HyperLinkLabel )layout.Children[layout.Children.Count - 1];
                        label.IsUnderline = false;
                        label.TextColor = Color.Black;
                        _partsIndexes[_partsIndexes.Count - 1]--;
                        _layoutIsFull[_layoutIsFull.Count - 1] = false;
                    }
                }
            }
            else
            {
                MessagingCenter.Send("JustIgnore", ConstantsService.Messages.PressHardwareBack);

                MessagingCenter.Unsubscribe<string>(this, ConstantsService.Messages.HardwareBackPressed);
                MessagingCenter.Unsubscribe<Grid>(this, ConstantsService.Messages.TakePathPartsGrid);
                MessagingCenter.Unsubscribe<SelectBranchPopUpModel>(this, ConstantsService.Messages.TakeBranchModelFromPopUpPage);
            }
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            MessagingCenter.Unsubscribe<string>(this, ConstantsService.Messages.HardwareBackPressed);
            MessagingCenter.Unsubscribe<Grid>(this, ConstantsService.Messages.TakePathPartsGrid);
            MessagingCenter.Unsubscribe<SelectBranchPopUpModel>(this, ConstantsService.Messages.TakeBranchModelFromPopUpPage);
        }
    }
}
