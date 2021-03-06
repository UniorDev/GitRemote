﻿using GitRemote.Services;
using Prism.Mvvm;
using Xamarin.Forms;

namespace GitRemote.Models
{
    public class StarredRepositoryModel : BindableBase
    {
        public FormattedString CustomFormattedText => new FormattedString
        {
            Spans =
            {
                new Span { Text = StarredRepositoryPath, FontSize=16 },
                new Span { Text = StarredRepositoryName, FontAttributes = FontAttributes.Bold, FontSize=16 }
            }
        };

        #region ElementsPropertiesDeclaretion

        public bool IsDescription { get; set; }

        public string StarredRepositoryTypeIcon => StarredRepositoryType == "Fork"
             ? FontIconsService.Octicons.RepoForked
             : ( StarredRepositoryType == "Private"
                 ? FontIconsService.Octicons.Lock
                 : FontIconsService.Octicons.Repo );

        private string _starredRepositoryType;

        public string StarredRepositoryType
        {
            get { return _starredRepositoryType; }
            set { SetProperty(ref _starredRepositoryType, value); }
        }

        private string _starredRepositoryName;

        private string _ownerName;

        public string OwnerName
        {
            get { return _ownerName; }
            set { SetProperty(ref _ownerName, value); }
        }

        public string StarredRepositoryName
        {
            get { return _starredRepositoryName; }
            set { SetProperty(ref _starredRepositoryName, value); }
        }

        private string _starredRepositoryPath;

        public string StarredRepositoryPath
        {
            get { return _starredRepositoryPath; }
            set { SetProperty(ref _starredRepositoryPath, value); }
        }

        private string _starredRepositoryDescription;

        public string StarredRepositoryDescription
        {
            get { return _starredRepositoryDescription; }
            set { SetProperty(ref _starredRepositoryDescription, value); }
        }

        private string _starredRepositoryLanguage;

        public string StarredRepositoryLanguage
        {
            get { return _starredRepositoryLanguage; }
            set { SetProperty(ref _starredRepositoryLanguage, value); }
        }

        private string _starredRepositoryStarIcon;

        public string StarredRepositoryStarIcon
        {
            get { return _starredRepositoryStarIcon; }
            set { SetProperty(ref _starredRepositoryStarIcon, value); }
        }

        private string _starredRepositoryStarsCount;

        public string StarredRepositoryStarsCount
        {
            get { return _starredRepositoryStarsCount; }
            set { SetProperty(ref _starredRepositoryStarsCount, value); }
        }

        private string _starredRepositoryForkIcon;

        public string StarredRepositoryForkIcon
        {
            get { return _starredRepositoryForkIcon; }
            set { SetProperty(ref _starredRepositoryForkIcon, value); }
        }

        private string _starredRepositoryForksCount;

        public string StarredRepositoryForksCount
        {
            get { return _starredRepositoryForksCount; }
            set { SetProperty(ref _starredRepositoryForksCount, value); }
        }
        #endregion

    }
}
