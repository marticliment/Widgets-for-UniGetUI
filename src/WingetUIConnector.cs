using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Newtonsoft.Json;
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
using Windows.Management.Deployment;
using Windows.Media.Protection.PlayReady;
using static System.Net.Mime.MediaTypeNames;

namespace WingetUIWidgetProvider
{

    internal class WingetUIConnector
    {
        public event EventHandler<UpdatesCheckFinishedEventArgs>? UpdateCheckFinished;
        public event EventHandler<ConnectionEventArgs>? Connected;

        private string SessionToken = "";
        private bool was_connected = false;
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
            was_connected = false;
            ResetCachedUpdates();
        }

        public void ResetCachedUpdates()
        {
            cached_updates = new Package[0];
            update_cache_is_valid = false;
        }

        async public void Connect(GenericWidget widget)
        {
            ConnectionEventArgs args = new ConnectionEventArgs(widget);
            try
            {
                if (!was_connected)
                {
                    StreamReader reader = new StreamReader(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\.wingetui\\CurrentSessionToken");
                    SessionToken = reader.ReadToEnd().ToString().Replace("\n", "").Trim();
                    reader.Close();

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:7058//");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage task = await client.GetAsync("/widgets/attempt_connection?token="+SessionToken);
                    if (task.IsSuccessStatusCode)
                        args.Succeeded = was_connected = true;
                    else
                        args.Succeeded = was_connected = false;
                }
                else
                {
                    args.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                args.Succeeded = was_connected = false;
            }
            if(Connected != null)
                Connected(this, args);
        }

        public void OnCacheExpire(object? source, ElapsedEventArgs? e)
        {
            ResetCachedUpdates();
            Console.WriteLine("Updates expired!");
            CacheExpirationTimer.Stop();
        }

        async public Task<Package[]> FetchAvailableUpdates(GenericWidget widget)
        {
            try
            {
                Console.WriteLine("Fetching updates from server");
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage task = await client.GetAsync("/widgets/get_updates?token=" + SessionToken);

                string outputString = await task.Content.ReadAsStringAsync();

                string purifiedString = outputString.Replace("\",\"status\":\"success\"}", "").Replace("{\"packages\":\"", "").Replace("\n", "").Trim();


                string[] packageStrings = purifiedString.Split("&&");
                int updateCount = packageStrings.Length;

                Package[] updates = new Package[updateCount];
                for (int i = 0; i < updateCount; i++)
                {
                    Package package = new Package(packageStrings[i]);
                    updates[i] = package;
                }
                update_cache_is_valid = true;
                cached_updates = updates;

                CacheExpirationTimer.Stop();
                CacheExpirationTimer.Start();

                return cached_updates;
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                update_cache_is_valid = false;
                cached_updates = new Package[0];
                return cached_updates;
            }
        }

        async public void GetAvailableUpdates(GenericWidget Widget, bool DeepCheck = false)
        {
            UpdatesCheckFinishedEventArgs args = new UpdatesCheckFinishedEventArgs(Widget);
            try
            {
                string AllowedSource = WidgetSourceReference[Widget.Name];

                Package[] found_updates;
                Console.WriteLine(update_cache_is_valid);
                if (!update_cache_is_valid || DeepCheck)
                {
                    found_updates = await FetchAvailableUpdates(Widget);
                    if (!update_cache_is_valid)
                        throw new Exception("FetchAvailableUpdates failed");
                }
                else
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

                args.Updates = updates;
                args.Count = found_updates.Length - skippedPackages;
                args.Succeeded = true;
            }
            catch (Exception ex)
            {
                args.Updates = new Package[0];
                args.Count = 0;
                args.Succeeded = false;
                Console.WriteLine(ex.ToString());
                ResetConnection();
            }
            if (UpdateCheckFinished != null)
                UpdateCheckFinished(this, args);

        }
        
        async public void OpenWingetUI()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:7058//");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.GetAsync("/widgets/open_wingetui?token=" + SessionToken);
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

                await client.GetAsync("/widgets/view_on_wingetui?token=" + SessionToken);
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

                await client.GetAsync("/widgets/update_package?token=" + SessionToken + "&id=" + package.Id);
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

                await client.GetAsync("/widgets/update_all_packages?token=" + SessionToken);
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

                await client.GetAsync("/widgets/update_all_packages_for_source?token=" + SessionToken + "&source=" + source);
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
        public GenericWidget widget {  get; set; }

        public UpdatesCheckFinishedEventArgs(GenericWidget widget)
        {
            Updates = new Package[0];
            this.widget = widget;
        }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(GenericWidget widget)
        {
            this.widget = widget;
        }

        public bool Succeeded = true;
        public GenericWidget widget { get; set; }
    }
}
