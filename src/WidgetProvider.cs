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
            WingetUI.GetAvailableUpdates(widget);
            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }

        private void WingetUI_UpdateCheckFinished(object? sender, UpdatesCheckFinishedEventArgs e)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(e.widget.Id);

            updateOptions.Template = Templates.BaseTemplate;
            if (!e.Succeeded)
            {
                if (e.ErrorReason == "NO_WINGETUI")
                {
                    updateOptions.Data = Templates.GetData_NoWingetUI();
                }
                else
                {   
                    updateOptions.Data = Templates.GetData_ErrorOccurred(e.ErrorReason);
                    WidgetManager.GetDefault().UpdateWidget(updateOptions);
                }

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
                Console.WriteLine("Showing available updates...");
                updateOptions.Template = Templates.UpdatesTemplate;
                string packages = "";
                Package[] upgradablePackages = new Package[e.widget.AvailableUpdates.Length];
                int nullPackages = 0;
                for (int i = 0; i < e.widget.AvailableUpdates.Length; i++)
                {
                    if (e.widget.AvailableUpdates[i].Name == "")
                    {
                        nullPackages += 1;
                    }
                    else
                    {
                        upgradablePackages[i] = e.widget.AvailableUpdates[i];
                        if (e.widget.size == WidgetSize.Medium && i == (3 + nullPackages) && e.widget.AvailableUpdates.Length > (3 + nullPackages))
                        {
                            i++;
                            packages += (e.widget.AvailableUpdates.Length - i).ToString() + " more packages can also be upgraded";
                            i = e.widget.AvailableUpdates.Length;
                        }
                        else if (e.widget.size == WidgetSize.Large && i == (7 + nullPackages) && e.widget.AvailableUpdates.Length > (7 + nullPackages) && e.widget.AvailableUpdates.Length > 7)
                        {
                            i++;
                            packages += (e.widget.AvailableUpdates.Length - i).ToString() + " more packages can also be upgraded";
                            i = e.widget.AvailableUpdates.Length;
                        }
                    }
                }
                if ((e.widget.AvailableUpdates.Length - nullPackages) == 0)
                {
                    updateOptions.Template = Templates.BaseTemplate;
                    updateOptions.Data = Templates.GetData_NoUpdatesFound();
                }
                else
                {
                    updateOptions.Data = Templates.GetData_UpdatesList(e.widget.AvailableUpdates.Length, upgradablePackages);
                }
                Debug.WriteLine(e.widget.Name);
                Debug.WriteLine(updateOptions.Template);
                Debug.WriteLine(updateOptions.Data);
                WidgetManager.GetDefault().UpdateWidget(updateOptions);
            }
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
                            WingetUI.GetAvailableUpdates(widget);
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
                WingetUI.GetAvailableUpdates(widget);

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
                WingetUI.GetAvailableUpdates(widget);
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
