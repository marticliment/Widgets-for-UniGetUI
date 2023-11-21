using Microsoft.Windows.Widgets;
using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.Management.Deployment;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Pickers;

namespace WingetUIWidgetProvider
{

    internal class WidgetProvider : IWidgetProvider
    {
        public static Dictionary<string, GenericWidget> RunningWidgets = new Dictionary<string, GenericWidget>();

        WingetUIConnector WingetUI;

        public WidgetProvider()
        {
            WingetUI = new WingetUIConnector();
            WingetUI.UpdateCheckFinished += WingetUI_UpdateCheckFinished;
            WingetUI.Connected += WingetUI_Connected;

            var runningWidgets = WidgetManager.GetDefault().GetWidgetInfos();

            foreach (var widgetInfo in runningWidgets)
            {
                var widgetContext = widgetInfo.WidgetContext;
                var widgetId = widgetContext.Id;
                var widgetName = widgetContext.DefinitionId;
                if (!RunningWidgets.ContainsKey(widgetId))
                {
                    GenericWidget runningWidgetInfo = new GenericWidget(widgetId, widgetName);
                    try
                    {
                        runningWidgetInfo.isActive = true;
                        runningWidgetInfo.size = widgetInfo.WidgetContext.Size;
                        runningWidgetInfo.customState = 0;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to import old widget!");
                    }
                    RunningWidgets[widgetId] = runningWidgetInfo;
                }
            }
        }

        private void StartLoadingRoutine(GenericWidget widget)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(widget.Id);
            updateOptions.Data = "{ \"IsLoading\": true }";
            Console.WriteLine("Starting load routine...");
            updateOptions.Template = Templates.BaseTemplate;
            WingetUI.Connect(widget);
            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }

        private void WingetUI_Connected(object? sender, ConnectionEventArgs e)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(e.widget.Id);
            if (!e.Succeeded)
            {
                updateOptions.Data = Templates.GetData_NoWingetUI();
                Console.WriteLine("Could not connect to WingetUI");
            }
            else
            {
                updateOptions.Data = Templates.GetData_IsLoading();
                Console.WriteLine("Connected to WingetUI");
                WingetUI.GetAvailableUpdates(e.widget);
            }

            updateOptions.Template = Templates.BaseTemplate;
            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }

        private void WingetUI_UpdateCheckFinished(object? sender, UpdatesCheckFinishedEventArgs e)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(e.widget.Id);

            updateOptions.Template = Templates.BaseTemplate;
            if (!e.Succeeded)
            {
                updateOptions.Data = Templates.GetData_ErrorOccurred("UPDATE_CHECK_FAILED");
                Console.WriteLine("Could not check for updates");
                WidgetManager.GetDefault().UpdateWidget(updateOptions);
            }
            else if (e.Count == 0)
            {
                updateOptions.Data = Templates.GetData_NoUpdatesFound();
                Console.WriteLine("No updates were found");
                WidgetManager.GetDefault().UpdateWidget(updateOptions);
            }
            else
            {
                e.widget.AvailableUpdates = e.Updates;
                DrawUpdates(e.widget);
            }
        }

        private void DrawUpdates(GenericWidget widget)
        { 
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(widget.Id);
            
            Console.WriteLine("Showing available updates...");
            updateOptions.Template = Templates.UpdatesTemplate;
            string packages = "";
            Package[] upgradablePackages = new Package[widget.AvailableUpdates.Length];
            int nullPackages = 0;
            for (int i = 0; i < widget.AvailableUpdates.Length; i++)
            {
                if (widget.AvailableUpdates[i].Name == "")
                {
                    nullPackages += 1;
                }
                else
                {
                    upgradablePackages[i] = widget.AvailableUpdates[i];
                    if (widget.size == WidgetSize.Medium && i == (3 + nullPackages) && widget.AvailableUpdates.Length > (3 + nullPackages))
                    {
                        i++;
                        packages += (widget.AvailableUpdates.Length - i).ToString() + " more packages can also be upgraded";
                        i = widget.AvailableUpdates.Length;
                    }
                    else if (widget.size == WidgetSize.Large && i == (7 + nullPackages) && widget.AvailableUpdates.Length > (7 + nullPackages) && widget.AvailableUpdates.Length > 7)
                    {
                        i++;
                        packages += (widget.AvailableUpdates.Length - i).ToString() + " more packages can also be upgraded";
                        i = widget.AvailableUpdates.Length;
                    }
                }
            }
            if ((widget.AvailableUpdates.Length - nullPackages) == 0)
            {
                updateOptions.Template = Templates.BaseTemplate;
                updateOptions.Data = Templates.GetData_NoUpdatesFound();
            } else {
                updateOptions.Data = Templates.GetData_UpdatesList(widget.AvailableUpdates.Length, upgradablePackages);
            }
            Console.WriteLine(widget.Name);
            Console.WriteLine(updateOptions.Template);
            Console.WriteLine(updateOptions.Data);
            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }

        public void CreateWidget(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id;
            var widgetName = widgetContext.DefinitionId;
            GenericWidget runningWidgetInfo = new GenericWidget(widgetId, widgetName);
            RunningWidgets[widgetId] = runningWidgetInfo;
            StartLoadingRoutine(runningWidgetInfo);
        }

        public void DeleteWidget(string widgetId, string customState)
        {
            RunningWidgets.Remove(widgetId);

            if (RunningWidgets.Count == 0)
            {
                emptyWidgetListEvent.Set();
            }
        }

        static ManualResetEvent emptyWidgetListEvent = new ManualResetEvent(false);

        public static ManualResetEvent GetEmptyWidgetListEvent()
        {
            return emptyWidgetListEvent;
        }

        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            var widgetId = actionInvokedArgs.WidgetContext.Id;
            var data = actionInvokedArgs.Data;
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(widgetId);
            if (RunningWidgets.ContainsKey(widgetId))
            {
                GenericWidget widget = RunningWidgets[widgetId];
                var verb = actionInvokedArgs.Verb;

                switch (verb)
                {
                    case (Verbs.Reload):
                        widget.customState = 0;
                        WingetUI.ResetConnection();
                        StartLoadingRoutine(widget);
                        break;

                    case (Verbs.ViewUpdatesOnWingetUI):
                        WingetUI.ViewOnWingetUI();
                        break;

                    case (Verbs.OpenWingetUI):
                        WingetUI.OpenWingetUI();
                        break;

                    case (Verbs.UpdateAll):
                        widget.customState = 1;
                        if (widget.Name == Widgets.All)
                            WingetUI.UpdateAllPackages();
                        else
                            WingetUI.UpdateAllPackagesForSource(WingetUI.WidgetSourceReference[widget.Name]);
                        updateOptions.Data = Templates.GetData_UpdatesInCourse();
                        updateOptions.Template = Templates.BaseTemplate;
                        WidgetManager.GetDefault().UpdateWidget(updateOptions);
                        break;

                    default:
                        if (verb.Contains(Verbs.UpdatePackage))
                        {
                            int index = int.Parse(verb.Replace(Verbs.UpdatePackage, ""));
                            Console.WriteLine(index);
                            WingetUI.UpdatePackage(widget.AvailableUpdates[index]);
                            widget.AvailableUpdates = widget.AvailableUpdates.Where((val, idx) => idx != index).ToArray(); // Remove that widget from the current list
                            DrawUpdates(widget);
                        } else
                        {
                            Console.WriteLine("INVALID VERB " + verb);
                            StartLoadingRoutine(widget);
                        }
                        break;

                }
            }
            
        }

        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            var widgetContext = contextChangedArgs.WidgetContext;
            var widgetId = widgetContext.Id;
            var widgetSize = widgetContext.Size;
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var widget = RunningWidgets[widgetId];
                widget.size = widgetContext.Size;
                StartLoadingRoutine(widget);

            }
        }

        public void Activate(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id;

            if (RunningWidgets.ContainsKey(widgetId))
            {
                var widget = RunningWidgets[widgetId];
                widget.isActive = true;
                widget.size = widgetContext.Size;
                StartLoadingRoutine(widget);
            }
        }
        public void Deactivate(string widgetId)
        {
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var widget = RunningWidgets[widgetId];
                widget.isActive = false;
            }
        }
    }

    public class GenericWidget
    {
        public GenericWidget(string widgetId, string widgetName) {
            AvailableUpdates = new Package[0];
            this.Id = widgetId;
            this.Name = widgetName;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public WidgetSize size { get; set; }
        public int customState = 0;
        public bool isActive = false;
        public Package[] AvailableUpdates { get; set; }

    }

}
