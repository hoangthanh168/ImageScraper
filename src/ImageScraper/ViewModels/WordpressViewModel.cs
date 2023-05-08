using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using ImageScraper.Services;
using Prism.Commands;
using Sdl.MultiSelectComboBox.API;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageScraper.ViewModels
{
    public class WordpressViewModel : ViewModelBase
    {
        #region Properties

        private string _url = "";

        public string Url
        {
            get { return _url; }
            set
            {
                SetProperty(ref _url, value);
                _userSettings.Url = value;
            }
        }

        private string _username = "";

        public string Username
        {
            get { return _username; }
            set
            {
                SetProperty(ref _username, value);
                _userSettings.Username = value;
            }
        }

        private string _password = "";

        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                _userSettings.Password = value; ;
            }
        }

        private int _selectedUserCount;

        public int SelectedUserCount
        {
            get { return _selectedUserCount; }
            set
            {
                SetProperty(ref _selectedUserCount, value);
            }
        }

        private int _selectedCategoriesCount;

        public int SelectedCategoriesCount
        {
            get { return _selectedCategoriesCount; }
            set
            {
                SetProperty(ref _selectedCategoriesCount, value);
            }
        }

        private int _selectedTagsCount;

        public int SelectedTagsCount
        {
            get { return _selectedTagsCount; }
            set
            {
                SetProperty(ref _selectedTagsCount, value);
            }
        }

        private string _loginStatus = "Not logged in";

        public string LoginStatus
        {
            get { return _loginStatus; }
            set
            {
                SetProperty(ref _loginStatus, value);
            }
        }

        private bool _isLoginProgressActive = false;

        public bool IsLoginProgressActive
        {
            get { return _isLoginProgressActive; }
            set
            {
                SetProperty(ref _isLoginProgressActive, value);
            }
        }

        private int _selectedStatusTypeIndex;

        public int SelectedStatusTypeIndex
        {
            get { return _selectedStatusTypeIndex; }
            set
            {
                SetProperty(ref _selectedStatusTypeIndex, value);
                _userSettings.SelectedStatusTypeIndex = _selectedStatusTypeIndex;
            }
        }

        private ObservableCollection<string> _users;

        public ObservableCollection<string> Users
        {
            get { return _users ?? (_users = new ObservableCollection<string>()); }
            set
            {
                SetProperty(ref _users, value);
            }
        }

        private ObservableCollection<string> _categories;

        public ObservableCollection<string> Categories
        {
            get { return _categories ?? (_categories = new ObservableCollection<string>()); }
            set
            {
                SetProperty(ref _categories, value);
            }
        }

        private ObservableCollection<string> _tags;

        public ObservableCollection<string> Tags
        {
            get { return _tags ?? (_tags = new ObservableCollection<string>()); }
            set
            {
                SetProperty(ref _tags, value);
            }
        }

        private ObservableCollection<string> _selectedUsers = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedUsers
        {
            get { return _selectedUsers; }
            set
            {
                SetProperty(ref _selectedUsers, value);
            }
        }

        private ObservableCollection<string> _selectedCategories = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedCategories
        {
            get { return _selectedCategories; }
            set
            {
                SetProperty(ref _selectedCategories, value);
            }
        }

        private ObservableCollection<string> _selectedTags = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedTags
        {
            get { return _selectedTags; }
            set
            {
                SetProperty(ref _selectedTags, value);
            }
        }

        public List<string> StatusTypes
        {
            get { return GetStatusTypes(); }
        }

        private List<string> GetStatusTypes()
        {
            string[] statusTypes = { "Pubish", "Draft" };
            return statusTypes.ToList();
        }

        #endregion Properties

        public DelegateCommand LoginCommand { get; set; }
        public DelegateCommand SelectedUsersChangeCommand { get; set; }
        public DelegateCommand ClearUsersCommand { get; set; }
        public DelegateCommand SelectedCategoriesChangeCommand { get; set; }
        public DelegateCommand ClearCategoriesCommand { get; set; }
        public DelegateCommand SelectedTagsChangeCommand { get; set; }
        public DelegateCommand ClearTagsCommand { get; set; }
        public ISuggestionProvider UserSuggestionProvider { get; set; }
        public ISuggestionProvider CategoriesSuggestionProvider { get; set; }
        public ISuggestionProvider TagsSuggestionProvider { get; set; }

        private UserSettings _userSettings;

        public bool isConnected;

        public WordpressViewModel(UserSettings userSettings)
        {
            _userSettings = userSettings;
            LoadSetting();
            LoginCommand = new DelegateCommand(Login);
            ClearUsersCommand = new DelegateCommand(ClearUsers);
            SelectedUsersChangeCommand = new DelegateCommand(SelectedUsersChanged);
            ClearCategoriesCommand = new DelegateCommand(ClearCategories);
            ClearTagsCommand = new DelegateCommand(ClearTags);
            SelectedCategoriesChangeCommand = new DelegateCommand(SelectedCategoriesChanged);
            SelectedTagsChangeCommand = new DelegateCommand(SelectedTagsChanged);
        }

        private bool CanLogin() =>
          !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) &&
          !IsLoginProgressActive;

        private async void Login()
        {
            if (CanLogin())
            {
                LoginStatus = "Logging in...";
                IsLoginProgressActive = true;

                var account = new WordpressAccount
                {
                    Username = Username,
                    Password = Password,
                    Url = Url
                };
                isConnected = await WordpressHelper.Instance.LoginAsync(account);

                if (isConnected)
                {
                    try
                    {
                        await Task.WhenAll(GetAndSetUsers(), GetAndSetCategories(), GetAndSetTags());
                        LoginStatus = "Logged in";
                    }
                    catch (System.Exception e)
                    {
                        MessageBox.Show(e.Message, "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginStatus = "Not logged in";
                        isConnected = false;
                    }
                }
                else
                {
                    LoginStatus = "Not logged in";
                }

                IsLoginProgressActive = false;
            }
        }

        private void SelectedUsersChanged()
        {
            SelectedUserCount = SelectedUsers.Count;
        }

        private void SelectedCategoriesChanged()
        {
            SelectedCategoriesCount = SelectedCategories.Count;
        }

        private void SelectedTagsChanged()
        {
            SelectedTagsCount = SelectedCategories.Count;
        }

        private void LoadSetting()
        {
            Username = _userSettings.Username;
            Password = _userSettings.Password;
            Url = _userSettings.Url;
            SelectedStatusTypeIndex = _userSettings.SelectedStatusTypeIndex;
        }

        public void ClearUsers()
        {
            SelectedUsers = new ObservableCollection<string>();
        }

        public void ClearCategories()
        {
            SelectedCategories = new ObservableCollection<string>();
        }

        public void ClearTags()
        {
            SelectedTags = new ObservableCollection<string>();
        }

        private async Task GetAndSetUsers()
        {
            var users = await WordpressHelper.Instance.GetUsersAsync();
            Users.Clear();
            Users.AddRange(users);
            UserSuggestionProvider = new CustomSuggestionProvider(Users, users);
        }

        private async Task GetAndSetCategories()
        {
            var categories = await WordpressHelper.Instance.GetCategoriesAsync();
            Categories.Clear();
            Categories.AddRange(categories);
            CategoriesSuggestionProvider = new CustomSuggestionProvider(Categories, categories);
        }

        private async Task GetAndSetTags()
        {
            var tags = await WordpressHelper.Instance.GetTagsAsync();
            Tags.Clear();
            Tags.AddRange(tags);
            TagsSuggestionProvider = new CustomSuggestionProvider(Tags, tags);
        }
    }
}