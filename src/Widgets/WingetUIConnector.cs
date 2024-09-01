using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.Widgets.Providers;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Timers;
using Widgets_for_UniGetUI;
using Widgets_for_UniGetUI.Templates;

namespace WingetUIWidgetProvider
{

    internal class WingetUIConnector
    {
        private const double MINIMUM_REQUIRED_HOST_VERSION = 3.119;
        private const string MINIMUM_REQUIRED_HOST_VERSION_STRING = "3.1.2-beta0";

        public event EventHandler<UpdatesCheckFinishedEventArgs>? UpdateCheckFinished;

        private string SessionToken = "";
        private bool is_connected_to_host = false;
        private bool update_cache_is_valid = false;
        private Package[] cached_updates = new Package[0];

        private System.Timers.Timer CacheExpirationTimer = new();

        public Dictionary<string, string> WidgetSourceReference = new()
        {
            {Widgets.All, ""},
            {Widgets.Winget, "Winget"},
            {Widgets.Scoop, "Scoop"},
            {Widgets.Chocolatey, "Chocolatey"},
            {Widgets.Pip, "Pip"},
            {Widgets.Npm, "Npm"},
            {Widgets.Dotnet, ".NET Tool"},
            {Widgets.Powershell, "PowerShell"},
        };

        public WingetUIConnector()
        {
            CacheExpirationTimer.Elapsed += OnCacheExpire;
            CacheExpirationTimer.Interval = 15000;
        }

        public void ResetConnection()
        {
            is_connected_to_host = false;
            ResetCachedUpdates();
        }

        public void ResetCachedUpdates()
        {
            cached_updates = new Package[0];
            update_cache_is_valid = false;
        }


        public void OnCacheExpire(object? source, ElapsedEventArgs? e)
        {
            ResetCachedUpdates();
            Logger.Log("Updates expired");
            CacheExpirationTimer.Stop();
        }

        async public void GetAvailableUpdates(GenericWidget Widget, bool DeepCheck = false)
        {
            Logger.Log("BEGIN GetAvailableUpdates(). Widget.Name=" + Widget.Name + ", DeepCheck=" + DeepCheck.ToString());
            UpdatesCheckFinishedEventArgs result = new(Widget);
            string AllowedSource = WidgetSourceReference[Widget.Name];
            Package[] found_updates;

            // Connect to UniGetUI if needed

            try
            {
                if (!is_connected_to_host)
                {
                    Logger.Log("GetAvailableUpdates: BEGIN connection to the host");

                    new Template_LoadingPage(Widget).UpdateWidget();

                    string old_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wingetui", "CurrentSessionToken");
                    string new_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UniGetUI", "CurrentSessionToken");

                    string SessionTokenFile;
                    if(!File.Exists(new_path))
                        SessionTokenFile = old_path;
                    else if(!File.Exists(old_path))
                        SessionTokenFile = new_path;
                    else
                    {
                        FileInfo old_path_data = new(old_path);
                        DateTime old_created = old_path_data.LastWriteTimeUtc; //File Creation
                        FileInfo new_path_data = new(new_path);
                        DateTime new_created = new_path_data.LastWriteTimeUtc; //File Creation
                        SessionTokenFile = old_created > new_created ? old_path : new_path;
                    }

                    StreamReader reader = new(SessionTokenFile);
                    SessionToken = reader.ReadToEnd().ToString().Replace("\n", "").Trim();
                    reader.Close();

                    HttpClient client = new();
                    client.BaseAddress = new Uri("http://localhost:7058//");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage task = await client.GetAsync("/widgets/v1/get_wingetui_version?token=" + SessionToken);
                    if (task.IsSuccessStatusCode)
                    {
                        double host_version;
                        host_version = double.Parse(await task.Content.ReadAsStringAsync(), System.Globalization.CultureInfo.InvariantCulture);
                        Logger.Log("Found UniGetUI " + host_version.ToString());

                        if (host_version < MINIMUM_REQUIRED_HOST_VERSION)
                        {
                            string minVersion = MINIMUM_REQUIRED_HOST_VERSION.ToString(CultureInfo.InvariantCulture);
                            string minVersionName = MINIMUM_REQUIRED_HOST_VERSION_STRING;

                            Logger.Log("GetAvailableUpdates: ABORTED: MINIMUM_REQUIRED_HOST_VERSION " 
                                + $"{minVersion} ({minVersionName}) was not met by the host (host is {host_version})");
                            result.Succeeded = is_connected_to_host = false;
                            result.ErrorReason = "UniGetUI (formerly WingetUI) "
                                + $"{minVersion} ({minVersionName}) is required. You are running UniGetUI version {host_version.ToString(CultureInfo.InvariantCulture)})";
                            if (UpdateCheckFinished != null) UpdateCheckFinished(this, result);
                            return;
                        }
                        else
                        {
                            Logger.Log("GetAvailableUpdates: SUCCESS Connected to host successfully.");
                            is_connected_to_host = true;
                        }
                    }
                    else if (task.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Logger.Log("GetAvailableUpdates: ABORTED connection to UniGetUI due to UNAUTHORIZED");
                        result.Succeeded = is_connected_to_host = false;
                        result.ErrorReason = "UNAUTHORIZED";
                        if (UpdateCheckFinished != null)
                            UpdateCheckFinished(this, result);
                        return;
                    }
                    else
                    {
                        Logger.Log("GetAvailableUpdates: ABORTED connection to UniGetUI due to StatusCode=" + task.StatusCode);
                        result.Succeeded = is_connected_to_host = false;
                        result.ErrorReason = "NO_WINGETUI";
                        if (UpdateCheckFinished != null)
                            UpdateCheckFinished(this, result);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetAvailableUpdates: ABORTED connection to host: An exception was thrown");
                Logger.Log(ex.Message);
                result.Succeeded = is_connected_to_host = false;
                result.ErrorReason = "NO_WINGETUI";
                if (UpdateCheckFinished != null)
                    UpdateCheckFinished(this, result);
                Logger.Log("END of GetAvailableUpdates()");
                return;
            }

            // Get fresh updates from the host if cached ones are not valid

            if (!update_cache_is_valid || DeepCheck)
            {
                try
                {
                    Logger.Log("GetAvailableUpdates: BEGIN retrieving updates from the host");

                    new Template_LoadingPage(Widget).UpdateWidget();
                    
                    Logger.Log("Fetching updates from server");
                    HttpClient client = new();
                    client.BaseAddress = new Uri("http://localhost:7058//");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage task = await client.GetAsync("/widgets/v1/get_updates?token=" + SessionToken);

                    string outputString = await task.Content.ReadAsStringAsync();

                    if (!task.IsSuccessStatusCode)
                    {
                        Logger.Log("GetAvailableUpdates: ABORT checking for updates due to StatusCode=" + task.StatusCode.ToString());
                        throw new Exception("Update fetching failed with code "+task.StatusCode.ToString());
                    }

                    string purifiedString = outputString.Replace("\",\"status\":\"success\"}", "").Replace("{\"packages\":\"", "").Replace("\n", "").Trim();


                    string[] packageStrings = purifiedString.Split("&&");
                    int updateCount = packageStrings.Length;

                    cached_updates = new Package[updateCount];
                    for (int i = 0; i < updateCount; i++)
                    {
                        Package package = new(packageStrings[i]);
                        cached_updates[i] = package;
                    }
                    update_cache_is_valid = true;
                    CacheExpirationTimer.Stop();
                    CacheExpirationTimer.Start();
                    Logger.Log("GetAvailableUpdates: SUCCESS checking for updates successfully");
                }
                catch (Exception ex)
                {
                    Logger.Log("GetAvailableUpdates: ABORTED checking for updates: An exception was thrown.");
                    result.ErrorReason = "CANNOT_FETCH_UPDATES: " + ex.Message;
                    result.Succeeded = false;
                    if (UpdateCheckFinished != null)
                        UpdateCheckFinished(this, result);
                    Logger.Log("Failed to fetch updates!");
                    Logger.Log(ex);
                    Logger.Log("END of GetAvailableUpdates()");
                    return;
                }
            }

            // Handle the updates and return only the requested ones.

            try
            {
                Logger.Log("GetAvailableUpdates: BEGIN parsing updates");
                found_updates = cached_updates;

                Package[] valid_updates = new Package[found_updates.Length];

                int skippedPackages = 0;
                
                for(int i = 0; i < found_updates.Length; i++)
                {
                    if (AllowedSource == "" || AllowedSource == found_updates[i].ManagerName)
                        valid_updates[i - skippedPackages] = found_updates[i];
                    else
                        skippedPackages++;
                }

                Package[] updates = new Package[found_updates.Length - skippedPackages];
                for(int i = 0;i < found_updates.Length - skippedPackages; i++)
                    updates[i] = valid_updates[i];

                result.Updates = updates;
                result.Count = found_updates.Length - skippedPackages;
                result.Succeeded = true;
                result.ErrorReason = "";
                if (UpdateCheckFinished != null)
                    UpdateCheckFinished(this, result);
                Logger.Log("GetAvailableUpdates: SUCCESS parsing updates.");
                Logger.Log("END of GetAvailableUpdates()");
                return;

            }
            catch (Exception ex)
            {
                Logger.Log("GetAvailableUpdates: ABORT parsing updates due to an exception thrown.");
                result.ErrorReason = "CANNOT_PROCESS_UPDATES: " + ex.Message;
                result.Succeeded = false;
                if (UpdateCheckFinished != null)
                    UpdateCheckFinished(this, result);
                Logger.Log("Failed to process updates!");
                Logger.Log(ex);
                Logger.Log("END of GetAvailableUpdates()");
                return;
            }
        }
        
        public void OpenWingetUI()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "unigetui://showUniGetUI",
                    UseShellExecute = true
                });
                /*HttpClient client = new();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/open_wingetui?token=" + SessionToken);*/
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ResetConnection();
            }
        }

        public void ViewOnWingetUI()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "unigetui://showUpdatesPage",
                    UseShellExecute = true
                });
                /*HttpClient client = new();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/view_on_wingetui?token=" + SessionToken);*/
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdatePackage(Package package)
        {
            try
            {
                HttpClient client = new();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                cached_updates = cached_updates.Where((val, idx) => val != package).ToArray(); // Remove that widget from the current list

                await client.GetAsync("/widgets/v1/update_package?token=" + SessionToken + "&id=" + package.Id);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdateAllPackages()
        {
            try
            {
                HttpClient client = new();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/update_all_packages?token=" + SessionToken);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdateAllPackagesForSource(string source)
        {
            try
            {
                HttpClient client = new();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/update_all_packages_for_source?token=" + SessionToken + "&source=" + source);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ResetConnection();
            }
        }
    }

    public class Package
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string NewVersion{ get; set; }
        public string Source { get; set; }
        public string ManagerName { get; set; }
        public string Icon {  get; set; }
        public bool isValid = true;

        public Package(string packageString)
        {
            try
            {
                string[] packageParts = packageString.Split('|');
                Name = packageParts[0];
                Id = packageParts[1];
                Version = packageParts[2];
                NewVersion = packageParts[3];
                Source = packageParts[4];
                ManagerName = packageParts[5];
                if (packageParts[6] != "")
                    Icon = packageParts[6];
                else
                    Icon = "https://marticliment.com/resources/widgets/package_color.png";
            } catch
            {
                isValid = false;
                Name = "";
                Id = "";
                Version = "";
                NewVersion = "";
                Source = "";
                ManagerName = "";
                Icon = "https://marticliment.com/resources/widgets/package_color.png";
                Logger.Log("Can't construct package, given packageString=" + packageString);
            }
        }
    }

    public class UpdatesCheckFinishedEventArgs : EventArgs
    {
        public Package[] Updates { get; set; }
        public int Count { get; set; }
        public bool Succeeded { get; set; }
        public GenericWidget widget { get; set; }
        public string ErrorReason { get; set; } = "";

        public UpdatesCheckFinishedEventArgs(GenericWidget widget)
        {
            Updates = new Package[0];
            this.widget = widget;
        }
    }
}
