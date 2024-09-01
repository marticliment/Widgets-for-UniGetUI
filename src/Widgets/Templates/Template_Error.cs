using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widgets_for_UniGetUI.Templates;
using WingetUIWidgetProvider;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Widgets_for_UniGetUI.Templates
{
    internal class Template_Error : AbstractTemplate
    {
        string _errorMessage;
        public Template_Error(GenericWidget widget, string errorMessage) : base(widget)
        {
            _errorMessage = errorMessage;
        }

        protected override string GenerateTemplateBody()
        {
            return TemplateBody;
        }

        protected override string GenerateTemplateData()
        {
            return $$"""
                {  
                    "errorcode": "{{_errorMessage}}"
                }
                """;
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
                "height": "stretch"
            }
            """;
    }
}
