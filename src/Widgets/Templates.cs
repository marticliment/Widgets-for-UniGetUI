using ABI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Printing.PrintSupport;
using Windows.UI.Text;
using WingetUIWidgetProvider;
using static System.Net.Mime.MediaTypeNames;

namespace WingetUIWidgetProvider
{

    public static class Verbs
    {
        public const string Reload = "Reload";
        public const string OpenUniGetUI = "OpenUniGetUI";
        public const string ViewUpdatesOnUniGetUI = "ViewUpdatesOnUniGetUI";
        public const string UpdateAll = "UpdateAll";
        public const string UpdatePackage = "UpdateIndex";
    }

    public static class Pages
    {
        public const string NotInstalled = "NoUniGetUI";
        public const string LoadingPackages = "IsLoading";
        public const string NoUpdatesFound = "NoUpdatesFound";
        public const string UpdatesList = "UpdatesList";
        public const string ErrorOccurred = "ErrorOccurred";
        public const string UpdatingPackages = "UpdatesInCourse";
    }

    public class Widgets
    {
        public const string All = "updates_all";
        public const string Winget = "updates_winget";
        public const string Scoop = "updates_scoop";
        public const string Chocolatey = "updates_chocolatey";
        public const string Pip = "updates_pip";
        public const string Npm = "updates_npm";
        public const string Dotnet = "updates_dotnet";
        public const string Powershell = "updates_powershell";
    }

    public class Templates
    {

        public static string GetData_NoUniGetUI()
        {
            return $$"""
                { 
                    "{{Pages.NotInstalled}}": true 
                }
                """;
        }
        private const string NoWingetUI = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "Could not communicate with UniGetUI",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "fontType": "Default",
                                "size": "Default",
                                "weight": "Bolder"
                            },
                            {
                                "type": "Image",
                                "url": "https://marticliment.com/resources/widgets/unigetui_new.png",
                                "horizontalAlignment": "Center",
                                "size": "Medium",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            },
                            {
                                "type": "TextBlock",
                                "text": "UniGetUI (formerly WingetUI) is required for this widget to work.\nPlease make sure that UniGetUI is installed and running on the background",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "size": "Small"
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "height": "stretch"
                    },
                    {
                        "type": "ActionSet",
                        "actions": [
                            {
                                "type": "Action.Execute",
                                "title": "Retry",
                                "verb": "{{Verbs.Reload}}"
                            }
                        ],
                        "horizontalAlignment": "Center",
                        "verticalContentAlignment": "Center",
                        "$when": "${$host.widgetSize!=\"small\"}",
                        "size": "Medium"
                    }
                ],
                "verticalContentAlignment": "Center",
                "style": "default",
                "id": "NoWingetUIDiv",
                "height": "stretch",
                "$when": "${$root.{{Pages.NotInstalled}}}"
            }
            """;


        public static string GetData_IsLoading()
        {
            return $$"""
                { 
                    "{{Pages.LoadingPackages}}": true
                }
                """;
        }
        private const string IsLoading = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "Checking for updates...",
                                "wrap": true,
                                "fontType": "Default",
                                "size": "Default",
                                "weight": "Bolder",
                                "horizontalAlignment": "Center"
                            },
                            {
                                "type": "Image",
                                "url": "https://marticliment.com/resources/widgets/sandclock.png",
                                "horizontalAlignment": "Center",
                                "size": "Medium",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            },
                            {
                                "type": "TextBlock",
                                "text": "This won't take long",
                                "wrap": true,
                                "size": "Small",
                                "horizontalAlignment": "Center"
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "height": "stretch"
                    }
                ],
                "id": "LoadingDiv",
                "height": "stretch",
                "verticalContentAlignment": "Center",
                "$when": "${$root.{{Pages.LoadingPackages}}}"
            }
            """;


        public static string GetData_NoUpdatesFound()
        {
            return $$"""
                { 
                    "{{Pages.NoUpdatesFound}}": true 
                }
                """;
        }
        private const string NoUpdatesFound = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "Hooray! No updates were found!",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "fontType": "Default",
                                "size": "Default",
                                "weight": "Bolder"
                            },
                            {
                                "type": "Image",
                                "url": "https://marticliment.com/resources/widgets/laptop_checkmark.png",
                                "horizontalAlignment": "Center",
                                "size": "Medium",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            },
                            {
                                "type": "TextBlock",
                                "text": "Everything seems to be up-to-date",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "size": "Small",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "height": "stretch"
                    },
                    {
                        "type": "ActionSet",
                        "actions": [
                            {
                                "type": "Action.Execute",
                                "title": "Check again",
                                "verb": "{{Verbs.Reload}}"
                            },
                            {
                                "type": "Action.Execute",
                                "title": "Show UniGetUI",
                                "verb": "{{Verbs.OpenUniGetUI}}"
                            }
                        ],
                        "horizontalAlignment": "Center"
                    }
                ],
                "verticalContentAlignment": "Center",
                "style": "default",
                "id": "NoUpdatesDiv",
                "$when": "${$root.{{Pages.NoUpdatesFound}}}",
                "height": "stretch"
            }
            """;


        private static string GeneratePackageStructure(int index)
        {
            string package = $$"""
            {
                "type": "TableRow",
                "cells": [
                    {
                        "type": "TableCell",
                        "items": [
                            {
                                "type": "Image",
                                "url": "${Icon{{index.ToString()}}}",
                                "width": "24px",
                                "height": "24px",
                                "horizontalAlignment": "Center",
                                "altText": "📦"
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "minHeight": "32px",
                        "spacing": "None"
                    },
                    {
                        "type": "TableCell",
                        "items": [
                            {
                                "type": "Container",
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": "${PackageName{{index.ToString()}}}",
                                        "wrap": false,
                                        "spacing": "None",
                                        "fontType": "Default",
                                        "size": "Small",
                                        "weight": "Bolder",
                                        "color": "Accent"
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": "From ${Version{{index.ToString()}}} to ${NewVersion{{index.ToString()}}}",
                                        "fontType": "Default",
                                        "size": "Small",
                                        "weight": "Lighter",
                                        "isSubtle": true,
                                        "spacing": "None",
                                        "wrap": false,
                                        "style": "default"
                                    }
                                ]
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "spacing": "None"
                    },
                    {
                        "type": "TableCell",
                        "items": [
                            {
                                "type": "ActionSet",
                                "actions": [
                                    {
                                        "type": "Action.Execute",
                                        "title": "🡇",
                                        "verb": "{{Verbs.UpdatePackage + index.ToString()}}",
                                        "data": {},
                                        "tooltip": "Update this package",
                                        "spacing": "None"
                                    }
                                ],
                                "horizontalAlignment": "Center",
                                "spacing": "None"
                            }
                        ],
                        "verticalContentAlignment": "Center",
                        "minHeight": "32px",
                        "spacing": "None"
                    }
                ],
            "$when": "${$root.Package{{index.ToString()}}Visisble}",
            "spacing": "None"
            }
            """;
            return package;
        }
        public static string GetData_UpdatesList(int count, Package[] upgradablePackages)
        {
            string data = $$"""
                { 
                    "{{Pages.UpdatesList}}": true,  
                    "count": "{{count}}"
                """;

            for (int i = 0; i < upgradablePackages.Length; i++)
            {
                if (upgradablePackages[i] != null)
                {
                    data += "," + $"""
                         "Package{i}Visisble": true,
                         "PackageName{i}":    "{upgradablePackages[i].Name}",
                         "Version{i}":        "{upgradablePackages[i].Version}",
                         "Icon{i}":           "{upgradablePackages[i].Icon}",
                         "NewVersion{i}":     "{upgradablePackages[i].NewVersion}"

                     """;
                }
            }

            if ((count - upgradablePackages.Length) >= 1)
                return data += "," + $$"""
                    "upgradablePackages": "{{count - upgradablePackages.Length}} packages more can be updated"
                }
                """;
            else
                return data += "," + """
                    "upgradablePackages": "\n"
                }
                """;
        }
        private static string GetUpdatesListTemplate(int numOfPackages)
        {
            string basestr = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "TextBlock",
                        "text": "Available Updates: ${count}",
                        "wrap": true,
                        "weight": "bolder",
                        "size": "medium"
                    },
                    {
                        "type": "Table",
                        "columns": [
                            {
                                "width": "32px"
                            },
                            {
                                "width": 1
                            },
                            {
                                "width": "42px"
                            }
                        ],
                        "rows": [
            """;

            for (int i = 0; i < numOfPackages; i++)
                basestr += GeneratePackageStructure(i) + ((i + 1 == numOfPackages) ? "\n" : ",\n");

            basestr += $$"""
                        ],
                        "spacing": "None",
                        "showGridLines": false,
                        "verticalCellContentAlignment": "Top"
                    },
                    {
                        "type": "Container",
                        "verticalContentAlignment": "Center",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "${upgradablePackages}",
                                "horizontalAlignment": "Center",
                                "spacing": "None",
                                "weight": "Lighter",
                                "isSubtle": true
                            }
                        ],
                        "height": "stretch",
                        "spacing": "None"
                    },
                    {
                        "type": "ActionSet",
                        "actions": [
                            {
                            "type": "Action.Execute",
                                "title": "Update all",
                                "verb": "{{Verbs.UpdateAll}}"
                            },
                            {
                                "type": "Action.Execute",
                                "title": "Reload",
                                "verb": "{{Verbs.Reload}}"
                            },
                            {
                                "type": "Action.Execute",
                                "title": "UniGetUI",
                                "verb": "{{Verbs.ViewUpdatesOnUniGetUI}}"
                            }
                        ],
                        "id": "buttons",
                        "horizontalAlignment": "Center",
                        "spacing": "None"
                    }
                ],
                "height": "stretch",
                "$when": "${$root.{{Pages.UpdatesList}}}",
                "spacing": "None",
                "verticalContentAlignment": "Center"
            }
            """;

            return basestr;
        }

        public static string GetData_ErrorOccurred(string error)
        {
            return $$"""
                { 
                    "{{Pages.ErrorOccurred}}": true, 
                    "errorcode": "{{error}}"
                }
                """;
        }

        private const string ErrorOccurred = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "Something went wrong...",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "fontType": "Default",
                                "size": "Default",
                                "weight": "Bolder"
                            },
                            {
                                "type": "Image",
                                "url": "https://marticliment.com/resources/error.png",
                                "horizontalAlignment": "Center",
                                "size": "Medium",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            },
                            {
                                "type": "TextBlock",
                                "text": "An error occurred with this widget: ${errorcode}",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "size": "Small",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            }
                        ],
                        "height": "stretch"
                    },
                    {
                        "type": "ActionSet",
                        "actions": [
                            {
                                "type": "Action.Execute",
                                "title": "Try again",
                                "verb": "{{Verbs.Reload}}"
                            }
                        ],
                        "horizontalAlignment": "Center"
                    }
                ],
                "verticalContentAlignment": "Center",
                "style": "default",
                "id": "ErrorOccurredDiv",
                "$when": "${$root.{{Pages.ErrorOccurred}}}",
                "height": "stretch"
            }
            """;

        public static string GetData_UpdatesInCourse()
        {
            return $$"""
                {
                    "{{Pages.UpdatingPackages}}": true 
                }
                """;
        }
        private const string UpdatesInCourse = $$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "Container",
                        "items": [
                            {
                                "type": "TextBlock",
                                "text": "Your packages are being updated!",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "fontType": "Default",
                                "size": "Default",
                                "weight": "Bolder"
                            },
                            {
                                "type": "Image",
                                "url": "https://marticliment.com/resources/widgets/laptop_checkmark.png",
                                "horizontalAlignment": "Center",
                                "size": "Medium",
                                "$when": "${$host.widgetSize!=\"small\"}"
                            },
                            {
                                "type": "TextBlock",
                                "text": "The updates will be ready soon. You can check their progress on UniGetUI (formerly WingetUI)",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "size": "Small"
                            }
                        ],
                        "height": "stretch"
                    },
                    {
                        "type": "ActionSet",
                        "actions": [
                            {
                                "type": "Action.Execute",
                                "title": "Refresh",
                                "verb": "{{Verbs.Reload}}"
                            }
                        ],
                        "horizontalAlignment": "Center",
                        "$when": "${$host.widgetSize!=\"small\"}",
                        "size": "Medium"
                    }
                ],
                "verticalContentAlignment": "Center",
                "style": "default",
                "id": "UpdatesOnTheGo",
                "height": "stretch",
                "$when": "${$root.{{Pages.UpdatingPackages}}}"
            }
            """;


        public const string BaseTemplate = $$"""
            {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.5",
                "body": [
                    {{NoWingetUI}},
                    {{IsLoading}},
                    {{NoUpdatesFound}},
                    {{UpdatesInCourse}},
                    {{ErrorOccurred}}
                ],
                "rtl": false,
                "refresh": {
                    "action": {
                        "type": "Action.Execute",
                        "verb": "{{Verbs.Reload}}"
                    }
                }
            }
            """;

        public static string GetUpdatesTemplate(int numOfUpdates)
        {
            return $$"""
            {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.5",
                "body": [
                    {{GetUpdatesListTemplate(numOfUpdates)}}
                ],
                "rtl": false,
                "refresh": {
                    "action": {
                        "type": "Action.Execute",
                        "verb": "{{Verbs.Reload}}"
                    }
                }
            }
            """;
        }
    }
}
