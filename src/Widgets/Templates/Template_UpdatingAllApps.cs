using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widgets_for_UniGetUI.Templates;
using WingetUIWidgetProvider;

namespace Widgets_for_UniGetUI.Templates
{
    internal class Template_UpdatingAllApps : AbstractTemplate
    {
        public Template_UpdatingAllApps(GenericWidget widget) : base(widget)
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
                "height": "stretch"
            }
            """;
    }
}
