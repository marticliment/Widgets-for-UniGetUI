using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widgets_for_UniGetUI.Templates;
using WingetUIWidgetProvider;

namespace Widgets_for_UniGetUI.Templates
{
    internal class Template_NoUpdatesFound : AbstractTemplate
    {
        public Template_NoUpdatesFound(GenericWidget widget) : base(widget)
        { }

        protected override string GenerateTemplateBody()
        {
            return TemplateBody;
        }

        protected override string GenerateTemplateData()
        {
            return "{}";
        }
        
        public const string TemplateBody = $$"""
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
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "width": "auto",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "type": "Action.Execute",
                                                "title": "Check again",
                                                "verb": "{{Verbs.Reload}}"
                                            }
                                        ]
                                    }
                                ],
                                "spacing": "None",
                                "height": "stretch"
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
                                        "wrap": true
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "width": "auto",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "type": "Action.Execute",
                                                "title": "Show UniGetUI",
                                                "verb": "{{Verbs.OpenUniGetUI}}"
                                            }
                                        ]
                                    }
                                ],
                                "spacing": "None",
                                "height": "stretch"
                            }
                        ],
                        "spacing": "Small"
                    }
                ],
                "verticalContentAlignment": "Center",
                "style": "default",
                "id": "NoUpdatesDiv",
                "height": "stretch"
            }
            """;

    }
}
