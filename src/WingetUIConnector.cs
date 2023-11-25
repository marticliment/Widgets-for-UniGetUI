using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Devices.Printers;
using Windows.Management.Deployment;
using Windows.Media.Protection.PlayReady;
using static System.Net.Mime.MediaTypeNames;

namespace WingetUIWidgetProvider
{

    internal class WingetUIConnector
    {
        private double minimum_required_host_version = 2.119;

        public event EventHandler<UpdatesCheckFinishedEventArgs>? UpdateCheckFinished;

        private string SessionToken = "";
        private bool is_connected_to_host = false;
        private bool update_cache_is_valid = false;
        private Package[] cached_updates = new Package[0];

        private System.Timers.Timer CacheExpirationTimer = new System.Timers.Timer();

        public Dictionary<string, string> WidgetSourceReference = new Dictionary<string, string>()
        {
            {Widgets.All, ""},
            {Widgets.Winget, "Winget"},
            {Widgets.Scoop, "Scoop"},
            {Widgets.Chocolatey, "Chocolatey"},
            {Widgets.Pip, "Pip"},
            {Widgets.Npm, "Npm"},
            {Widgets.Dotnet, ".NET Tool"},
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
            Console.WriteLine("Updates expired!");
            CacheExpirationTimer.Stop();
        }

        async public void GetAvailableUpdates(GenericWidget Widget, bool DeepCheck = false)
        {
            UpdatesCheckFinishedEventArgs result = new UpdatesCheckFinishedEventArgs(Widget);
            string AllowedSource = WidgetSourceReference[Widget.Name];
            Package[] found_updates;


            // Connect to WingetUI if needed

            try
            {


                if (!is_connected_to_host)
                {

                    WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(Widget.Id);
                    updateOptions.Template = Templates.BaseTemplate;
                    updateOptions.Data = Templates.GetData_IsLoading();
                    WidgetManager.GetDefault().UpdateWidget(updateOptions);

                    StreamReader reader = new StreamReader(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\.wingetui\\CurrentSessionToken");
                    SessionToken = reader.ReadToEnd().ToString().Replace("\n", "").Trim();
                    reader.Close();

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:7058//");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage task = await client.GetAsync("/widgets/v1/get_wingetui_version?token=" + SessionToken);
                    if (task.IsSuccessStatusCode)
                    {
                        double host_version;
                        host_version = double.Parse(await task.Content.ReadAsStringAsync(), System.Globalization.CultureInfo.InvariantCulture);
                        Console.WriteLine("Found WingetUI " + host_version.ToString());

                        if (host_version < minimum_required_host_version)
                        {
                            result.Succeeded = is_connected_to_host = false;
                            result.ErrorReason = "WingetUI " + minimum_required_host_version.ToString(System.Globalization.CultureInfo.InvariantCulture) + " is required. You are running WingetUI " + host_version.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            if (UpdateCheckFinished != null)
                                UpdateCheckFinished(this, result);
                            return;
                        }
                        else
                        {
                            is_connected_to_host = true;
                        }
                    }
                    else
                    {
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
                Console.WriteLine(ex.Message);
                result.Succeeded = is_connected_to_host = false;
                result.ErrorReason = "NO_WINGETUI";
                if (UpdateCheckFinished != null)
                    UpdateCheckFinished(this, result);
                return;
            }

            // Get fresh updates from the host if cached ones are not valid

            if (!update_cache_is_valid || DeepCheck)
            {
                try
                {
                    WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(Widget.Id);
                    updateOptions.Template = Templates.BaseTemplate;
                    updateOptions.Data = Templates.GetData_IsLoading();
                    WidgetManager.GetDefault().UpdateWidget(updateOptions);

                    Console.WriteLine("Fetching updates from server");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:7058//");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage task = await client.GetAsync("/widgets/v1/get_updates?token=" + SessionToken);

                    string outputString = await task.Content.ReadAsStringAsync();

                    if (!task.IsSuccessStatusCode)
                    {
                        throw new Exception("Update fetching failed with code "+task.StatusCode.ToString());
                    }

                    string purifiedString = outputString.Replace("\",\"status\":\"success\"}", "").Replace("{\"packages\":\"", "").Replace("\n", "").Trim();


                    string[] packageStrings = purifiedString.Split("&&");
                    int updateCount = packageStrings.Length;

                    cached_updates = new Package[updateCount];
                    for (int i = 0; i < updateCount; i++)
                    {
                        Package package = new Package(packageStrings[i]);
                        cached_updates[i] = package;
                    }
                    update_cache_is_valid = true;
                    CacheExpirationTimer.Stop();
                    CacheExpirationTimer.Start();
                }
                catch (Exception ex)
                {
                    result.ErrorReason = "CANNOT_FETCH_UPDATES: " + ex.Message;
                    result.Succeeded = false;
                    if (UpdateCheckFinished != null)
                        UpdateCheckFinished(this, result);
                    Console.WriteLine("Failed to fetch updates!");
                    Console.WriteLine(ex);
                    return;
                }
            }

            // Handle the updates and return only the requested ones.

            try
            {
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
                return;

            }
            catch (Exception ex)
            {
                result.ErrorReason = "CANNOT_PROCESS_UPDATES: " + ex.Message;
                result.Succeeded = false;
                if (UpdateCheckFinished != null)
                    UpdateCheckFinished(this, result);
                Console.WriteLine("Failed to process updates!");
                Console.WriteLine(ex);
                return;
            }
        }
        
        async public void OpenWingetUI()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/open_wingetui?token=" + SessionToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ResetConnection();
            }
        }

        async public void ViewOnWingetUI()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/view_on_wingetui?token=" + SessionToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdatePackage(Package package)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                cached_updates = cached_updates.Where((val, idx) => val != package).ToArray(); // Remove that widget from the current list

                await client.GetAsync("/widgets/v1/update_package?token=" + SessionToken + "&id=" + package.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdateAllPackages()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/update_all_packages?token=" + SessionToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ResetConnection();
            }
        }

        async public void UpdateAllPackagesForSource(string source)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/v1/update_all_packages_for_source?token=" + SessionToken + "&source=" + source);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                Console.WriteLine("Can't construct package, given packageString=" + packageString);
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
