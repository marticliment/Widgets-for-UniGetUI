using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widgets_for_UniGetUI.Templates;
using WingetUIWidgetProvider;

namespace Widgets_for_UniGetUI.Templates
{
    internal class Template_MissingUniGetUI : AbstractTemplate
    {
        public Template_MissingUniGetUI(GenericWidget widget) : base(widget)
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
                "height": "stretch"
            }
            """;
    }
}
