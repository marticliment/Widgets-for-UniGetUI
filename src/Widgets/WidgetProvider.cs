using Microsoft.Windows.Widgets;
using Microsoft.Windows.Widgets.Providers;
using WidgetsForUniGetUI.Templates;

namespace WidgetsForUniGetUI
{

    internal class WidgetProvider : IWidgetProvider
    {
        public static Dictionary<string, GenericWidget> RunningWidgets = new();

        WingetUIConnector UniGetUI;

        public WidgetProvider()
        {
            UniGetUI = new WingetUIConnector();
            UniGetUI.UpdateCheckFinished += UniGetUI_UpdateCheckFinished;

            WidgetInfo[] runningWidgets = WidgetManager.GetDefault().GetWidgetInfos();

            foreach (WidgetInfo? widgetInfo in runningWidgets)
            {
                WidgetContext widgetContext = widgetInfo.WidgetContext;
                string widgetId = widgetContext.Id;
                string widgetName = widgetContext.DefinitionId;
                if (!RunningWidgets.ContainsKey(widgetId))
                {
                    GenericWidget runningWidgetInfo = new(widgetId, widgetName);
                    try
                    {
                        runningWidgetInfo.isActive = true;
                        runningWidgetInfo.size = widgetInfo.WidgetContext.Size;
                        runningWidgetInfo.customState = 0;
                    }
                    catch
                    {
                        Logger.Log("Failed to import old widget!");
                    }
                    RunningWidgets[widgetId] = runningWidgetInfo;
                }
            }
        }

        private void StartLoadingRoutine(GenericWidget widget)
        {
            new Template_LoadingPage(widget).UpdateWidget();
            Logger.Log("Calling to UniGetUI.GetAvailableUpdates(widget) from widget");
            UniGetUI.GetAvailableUpdates(widget);
        }

        private void UniGetUI_UpdateCheckFinished(object? sender, UpdatesCheckFinishedEventArgs e)
        {
            if (!e.Succeeded && e.ErrorReason == "NO_WINGETUI")
            {
                new Template_MissingUniGetUI(e.widget).UpdateWidget();
                Logger.Log("UnGetUI was not found or is not running");
            }
            else if (!e.Succeeded)
            {
                new Template_Error(e.widget, e.ErrorReason).UpdateWidget();
                Logger.Log($"Check for updates failed with error code {e.ErrorReason}");
            }
            else if (e.Count is 0)
            {
                new Template_NoUpdatesFound(e.widget).UpdateWidget();
                Logger.Log("No updates were found");
            }
            else
            {
                e.widget.AvailableUpdates = e.Updates;
                new Template_UpdatesList(e.widget).UpdateWidget();
                Logger.Log(e.widget.Name);
            }
        }

        public void CreateWidget(WidgetContext widgetContext)
        {
            string widgetId = widgetContext.Id;
            string widgetName = widgetContext.DefinitionId;
            GenericWidget runningWidgetInfo = new(widgetId, widgetName);
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

        static ManualResetEvent emptyWidgetListEvent = new(false);

        public static ManualResetEvent GetEmptyWidgetListEvent()
        {
            return emptyWidgetListEvent;
        }

        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            string widgetId = actionInvokedArgs.WidgetContext.Id;
            string data = actionInvokedArgs.Data;
            WidgetUpdateRequestOptions updateOptions = new(widgetId);
            if (RunningWidgets.ContainsKey(widgetId))
            {
                GenericWidget widget = RunningWidgets[widgetId];
                string verb = actionInvokedArgs.Verb;

                switch (verb)
                {
                    case (Verbs.Reload):
                        widget.customState = 0;
                        UniGetUI.ResetConnection();
                        StartLoadingRoutine(widget);
                        break;

                    case (Verbs.ViewUpdatesOnUniGetUI):
                        UniGetUI.ViewOnWingetUI();
                        break;

                    case (Verbs.OpenUniGetUI):
                        UniGetUI.OpenWingetUI();
                        break;

                    case (Verbs.GoBigger):
                        widget.PackageOffset += Template_UpdatesList.GetMaxPackageCount(widget.size);
                        new Template_UpdatesList(widget).UpdateWidget();
                        break;

                    case (Verbs.GoSmaller):
                        widget.PackageOffset -= Template_UpdatesList.GetMaxPackageCount(widget.size);
                        new Template_UpdatesList(widget).UpdateWidget();
                        break;

                    case (Verbs.UpdateAll):
                        widget.customState = 1;
                        if (widget.Name == Widgets.All)
                            UniGetUI.UpdateAllPackages();
                        else
                            UniGetUI.UpdateAllPackagesForSource(UniGetUI.WidgetSourceReference[widget.Name]);
                        new Template_UpdatingAllApps(widget).UpdateWidget();
                        break;

                    default:
                        if (verb.StartsWith(Verbs.UpdatePackage))
                        {
                            int index = int.Parse(verb.Split('_')[^1]);
                            Logger.Log(index);
                            UniGetUI.UpdatePackage(widget.AvailableUpdates[index]);
                            UniGetUI.GetAvailableUpdates(widget);
                        }
                        else
                        {
                            Logger.Log("INVALID VERB " + verb);
                            StartLoadingRoutine(widget);
                        }
                        break;

                }
            }

        }

        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            WidgetContext widgetContext = contextChangedArgs.WidgetContext;
            string widgetId = widgetContext.Id;
            WidgetSize widgetSize = widgetContext.Size;
            if (RunningWidgets.ContainsKey(widgetId))
            {
                GenericWidget widget = RunningWidgets[widgetId];
                widget.size = widgetContext.Size;
                UniGetUI.GetAvailableUpdates(widget);

            }
        }

        public void Activate(WidgetContext widgetContext)
        {
            string widgetId = widgetContext.Id;

            if (RunningWidgets.ContainsKey(widgetId))
            {
                GenericWidget widget = RunningWidgets[widgetId];
                widget.isActive = true;
                widget.size = widgetContext.Size;
                UniGetUI.GetAvailableUpdates(widget);
            }
        }
        public void Deactivate(string widgetId)
        {
            if (RunningWidgets.ContainsKey(widgetId))
            {
                GenericWidget widget = RunningWidgets[widgetId];
                widget.isActive = false;
            }
        }
    }

    public class GenericWidget
    {
        public GenericWidget(string widgetId, string widgetName)
        {
            AvailableUpdates = [];
            this.Id = widgetId;
            this.Name = widgetName;
        }

        public string Id { get; set; }
        public int PackageOffset = 0;
        public string Name { get; set; }
        public WidgetSize size { get; set; }
        public int customState = 0;
        public bool isActive = false;
        public Package[] AvailableUpdates { get; set; }

    }

}
