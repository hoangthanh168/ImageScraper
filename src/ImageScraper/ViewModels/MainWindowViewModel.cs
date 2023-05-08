using AutoUpdaterDotNET;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using ImageScraper.Services;
using Prism.Commands;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ImageScraper.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _version = "";

        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        private string _ping;

        public string Ping
        {
            get { return _ping; }
            set
            {
                SetProperty(ref _ping, value);
            }
        }

        private IRegionManager _regionManager;
        private IRegionNavigationJournal _journal;
        private UserSettings _userSettings;
        private UpdateInfoEventArgs updateInfoEventArgs = null;
        private System.Timers.Timer timer;
        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand WindowClosingCommand { get; set; }
        public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand GoForwardImageEditorCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager, UserSettings userSettings)
        {
            _regionManager = regionManager;
            _userSettings = userSettings;
            GetVersion();
            NavigateCommand = new DelegateCommand<string>(Navigate);
            WindowClosingCommand = new DelegateCommand(On_WindowClosing);
            WindowLoadedCommand = new DelegateCommand(On_WindowLoaded);
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Kiểm tra ping trước khi bắt đầu Timer nếu app mở lần đầu
            OnTimedEvent(null, null);
            // Bật Timer để kiểm tra ping sau mỗi 5 giây
            timer.Enabled = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/hoangthanh168/ImageScraper/main/update.xml");
        }

        [DebuggerStepThrough]
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Kiểm tra tốc độ mạng
            Ping ping = new Ping();
            PingReply reply = ping.Send("www.google.com");

            // Hiển thị kết quả tốc độ mạng
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Ping = $"{reply.RoundtripTime}";
            });
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    updateInfoEventArgs = args;
                    System.Windows.Forms.MessageBox.Show(
                        $@"Có phiên bản mới {args.CurrentVersion} sẵn sàng. Bạn đang sử dụng phiên bản {args.InstalledVersion}. Đây là một bản cập nhật bắt buộc. Ứng dụng sẽ tự động cập nhật khi đóng. Nhấn Ok để tiếp tục sử dụng ứng dụng.", @"Cập nhật sẵn sàng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            else
            {
                if (args.Error is WebException)
                {
                    System.Windows.Forms.MessageBox.Show(
                        @"Có vấn đề khi kết nối đến máy chủ cập nhật. Vui lòng kiểm tra kết nối internet của bạn và thử lại sau.",
                        @"Kiểm tra cập nhật thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fvi.FileVersion;
        }
        bool isFirstNavigate = false;
        //tạm thời chỉ tính một viewmodel nên lười sửa IsFirstNavigate
        private void Navigate(string navigatePath)
        {
            if (navigatePath == "Setting")
            {
                _regionManager.RequestNavigate("SettingRegion", navigatePath);
            }
            else
            {
                if (navigatePath != null)
                {
                    NavigationParameters parameters = new NavigationParameters();
                    parameters.Add("IsFirstNavigate", isFirstNavigate);
                    _regionManager.RequestNavigate("ContentRegion", navigatePath, parameters);
                }
                   
            }
        }

        private void On_WindowClosing()
        {
            _userSettings.SaveSettings();
            if (updateInfoEventArgs != null)
            {
                if (updateInfoEventArgs.IsUpdateAvailable)
                {
                    AutoUpdater.DownloadUpdate(updateInfoEventArgs);
                }
            }
        }

        public async Task ShowUpdateMessageAsync()
        {
            await Task.Run(() =>
            {
                if (File.Exists("update_message.txt"))
                {
                    var message = File.ReadAllText("update_message.txt");
                    MessageBoxService.ShowInformation(message);
                }
            });
        }

        private async void On_WindowLoaded()
        {
            _regionManager.RequestNavigate("ContentRegion", "Home");
        }
    }
}