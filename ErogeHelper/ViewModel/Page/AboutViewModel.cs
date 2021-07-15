using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common;
using UpdateChecker;
using UpdateChecker.VersionComparers;

namespace ErogeHelper.ViewModel.Page
{
    public class AboutViewModel : PropertyChangedBase
    {
        public AboutViewModel()
        {
            Task.Run(async () => await CheckUpdateAsync().ConfigureAwait(false));
        }

        private string _checkUpdateStatus = "Check update ...";
        private Brush _brushColor = Brushes.Gray;
        private bool _canJumpRelease;
        public Brush BrushColor
        { get => _brushColor; set { _brushColor = value; NotifyOfPropertyChange(() => BrushColor); } }
        public string CheckUpdateStatus
        { get => _checkUpdateStatus; set { _checkUpdateStatus = value; NotifyOfPropertyChange(() => CheckUpdateStatus); } }
        public bool CanJumpRelease
        { get => _canJumpRelease; set { _canJumpRelease = value; NotifyOfPropertyChange(() => CanJumpRelease); } }

        private async Task CheckUpdateAsync()
        {
            var version = Utils.AppVersion;
            var updateChecker = new GitHubReleasesUpdateChecker(
                    @"luojunyuan", // Owner
                    @"Eroge-Helper", // Repo
                    false,  // Is pre-release
                    version ?? "error", // Current app version string
                    tag => tag.Replace(@"v", string.Empty), // Tag to version string
                    new DefaultVersionComparer() // Version comparer
            );
            try
            {
                var responseOk = await updateChecker.CheckAsync(default);
                //var res = await updaterChecker.CheckAsync(new HttpClient(), new CancellationToken());
                if (responseOk)
                {
                    // Update Found
                    var latestVersion = updateChecker.LatestVersion;
                    //var latestVersionUrl = updateChecker.LatestVersionUrl;
                    //var assetsUrl = updateChecker.LatestRelease?.assets?.Select(asset => asset.browser_download_url);
                    Log.Debug($"Found new {latestVersion}");

                    CheckUpdateStatus = $"Found new version {latestVersion}";
                    BrushColor = Brushes.Orange;
                    CanJumpRelease = true;
                    // ToolTip: Click me jump to release page
                }
                else
                {
                    CheckUpdateStatus = "Latest version";
                    BrushColor = Brushes.Green;
                }
            }
            catch (HttpRequestException ex)
            {
                CheckUpdateStatus = "Check Failed";
                BrushColor = Brushes.Red;
                Log.Warn(ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                // Network exception or cannot find any correct tag.
                Log.Error(ex);
            }
        }
    }
}