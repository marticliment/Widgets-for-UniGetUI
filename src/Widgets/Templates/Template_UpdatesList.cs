﻿using Microsoft.Windows.Widgets;

namespace WidgetsForUniGetUI.Templates
{
    internal class Template_UpdatesList : AbstractTemplate
    {
        List<Package> _packages = new();
        GenericWidget _widget;
        int _totalPackages;
        int maxPackages;
        bool CanGoSmaller;
        bool CanGoBigger;

        public static int GetMaxPackageCount(WidgetSize size)
        {
            return size switch
            {
                WidgetSize.Medium => 3,
                WidgetSize.Large => 7,
                _ => 1
            };
        }

        public Template_UpdatesList(GenericWidget widget) : base(widget)
        {
            _widget = widget;
            _totalPackages = widget.AvailableUpdates.Length;

            maxPackages = GetMaxPackageCount(widget.size);
            Logger.Log("Showing available updates...");

            if (widget.PackageOffset > (_totalPackages - maxPackages)) widget.PackageOffset = (_totalPackages - maxPackages);
            if (widget.PackageOffset < 0) widget.PackageOffset = 0;

            CanGoSmaller = widget.PackageOffset > 0;
            CanGoBigger = widget.PackageOffset + maxPackages < _totalPackages;

            for (int i = widget.PackageOffset; i < _totalPackages; i++)
            {
                if (widget.AvailableUpdates[i].Name != "")
                {
                    _packages.Add(widget.AvailableUpdates[i]);
                    if (_packages.Count >= maxPackages)
                        break;
                }
            }
        }

        protected override string GenerateTemplateBody()
        {
            string basestr = $$$"""
            {
                "type": "Container",
                "items": [
                    {
                        "type": "TextBlock",
                        "text": "Available Updates: ${count}",
                        "wrap": true,
                        "weight": "Bolder",
                        "size": "Default",
                        "horizontalAlignment": "Center",
                        "spacing": "Large"
                    },
                    {
                        "type": "Container",
                        "items": [
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
                                    "width": "45px"
                                }
                            ],
                            "rows": [
            """;

            for (int i = 0; i < _packages.Count; i++)   /* Add only a comma before the newline 
                                                         * if the package is not the last one */
                basestr += GeneratePackageStructure(i) + ((i + 1 == _packages.Count) ? "\n" : ",\n");

            basestr += $$"""
                            ],
                            "spacing": "None",
                            "showGridLines": false,
                            "verticalCellContentAlignment": "Top",
                            "firstRowAsHeaders": false
                        }
                    ],
                    "height": "stretch",
                    "spacing": "Medium",
                    "verticalContentAlignment": "Top"
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "spacing": "None",
                                "width": "38px",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "title": "🠜",
                                                "card": {
                                                    "type": "AdaptiveCard"
                                                },
                                                "type": "Action.Execute",
                                                "verb": "{{Verbs.GoSmaller}}",
                                                "isEnabled": true
                                            }
                                        ]
                                    }
                                ],
                                "isVisible": {{(CanGoSmaller ? "true" : "false")}}
                            },
                            {
                                "type": "Column",
                                "spacing": "None",
                                "width": "38px",
                                "items": [],
                                "isVisible": {{(CanGoSmaller ? "false" : "true")}}
                            },
                            {
                                "type": "Column",
                                "width": "stretch",
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": "${upgradablePackages}",
                                        "horizontalAlignment": "Center",
                                        "spacing": "None",
                                        "weight": "Lighter",
                                        "isSubtle": true,
                                        "wrap": true
                                    }
                                ],
                                "verticalContentAlignment": "Center",
                                "horizontalAlignment": "Center",
                                "spacing": "None"
                            },
                            {
                                "type": "Column",
                                "spacing": "None",
                                "width": "38px",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "title": "🠞",
                                                "card": {
                                                    "type": "AdaptiveCard"
                                                },
                                                "type": "Action.Execute",
                                                "verb": "{{Verbs.GoBigger}}",
                                                "isEnabled": true
                                            }
                                        ]
                                    }
                                ],
                                "isVisible": {{(CanGoBigger ? "true" : "false")}}
                            },
                            {
                                "type": "Column",
                                "spacing": "None",
                                "width": "38px",
                                "items": [
                                ],
                                "isVisible": {{(CanGoBigger ? "false" : "true")}}
                            }
                        ],
                        "spacing": "Small"
                    },
                    {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "auto",
                            "spacing": "None",
                            "height": "stretch",
                            "items": [
                                {
                                    "type": "ActionSet",
                                    "spacing": "None",
                                    "actions": [
                                        {
                                            "type": "Action.Execute",
                                            "title": "Update all",
                                            "verb": "{{Verbs.UpdateAll}}"
                                        }
                                    ]
                                }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "spacing": "None",
                            "height": "stretch",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": " ",
                                    "wrap": true,
                                    "size": "Small",
                                    "weight": "Lighter",
                                    "spacing": "None",
                                    "horizontalAlignment": "Center"
                                }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "auto",
                            "spacing": "None",
                            "height": "stretch",
                            "items": [
                                {
                                    "type": "ActionSet",
                                    "spacing": "None",
                                    "actions": [
                                        {
                                            "type": "Action.Execute",
                                            "title": "↻",
                                            "verb": "{{Verbs.Reload}}"
                                        },
                                        {
                                            "type": "Action.Execute",
                                            "title": "UniGetUI",
                                            "verb": "{{Verbs.ViewUpdatesOnUniGetUI}}"
                                        }
                                    ]
                                }
                            ]
                        }
                    ],
                    "spacing": "Small"
                }
                ],
                "height": "stretch",
                "spacing": "None",
                "verticalContentAlignment": "Center"
            }
            """;

            return basestr;
        }

        protected override string GenerateTemplateData()
        {
            string data = $$"""
                { 
                    "count": "{{_totalPackages}}"
                """;

            for (int i = 0; i < _packages.Count; i++)
            {
                if (_packages[i] is Package p)
                {
                    data += "," + $"""
                         "Package{i}Visisble": true,
                         "PackageName{i}":    "{p.Name}",
                         "Version{i}":        "{p.Version}",
                         "Icon{i}":           "{p.Icon}",
                         "NewVersion{i}":     "{p.NewVersion}"

                     """;
                }
            }

            int extraPackages = _totalPackages - _packages.Count;
            if (extraPackages >= 1)
                return data += "," + $$"""
                    "upgradablePackages": "{{_widget.PackageOffset + 1}} - {{_widget.PackageOffset + maxPackages}} out of {{_totalPackages}}"
                }
                """;
            else
                return data += "," + $$"""
                    "upgradablePackages": "{{1}} - {{_totalPackages}} out of {{_totalPackages}}"
                }
                """;
        }

        private string GeneratePackageStructure(int index)
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
                                "url": "${Icon{{index}}}",
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
                                        "text": "${PackageName{{index}}}",
                                        "wrap": false,
                                        "spacing": "None",
                                        "fontType": "Default",
                                        "size": "Small",
                                        "weight": "Bolder",
                                        "color": "Accent"
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": "From ${Version{{index}}} to ${NewVersion{{index}}}",
                                        "fontType": "Default",
                                        "size": "Small",
                                        "weight": "Lighter",
                                        "isSubtle": true,
                                        "spacing": "None",
                                        "wrap": false,
                                        "style": "default"
                                    }
                                ],
                                "spacing": "None"
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
                                        "title": "🠟",
                                        "verb": "{{Verbs.UpdatePackage}}_{{_widget.PackageOffset + index}}",
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
            "$when": "${$root.Package{{index}}Visisble}",
            "spacing": "None"
            }
            """;
            return package;
        }


    }
}
