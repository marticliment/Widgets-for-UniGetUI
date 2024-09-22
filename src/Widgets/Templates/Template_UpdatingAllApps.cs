namespace WidgetsForUniGetUI.Templates
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
                                "text": "The updates will be ready soon. You can check their progress on UniGetUI",
                                "wrap": true,
                                "horizontalAlignment": "Center",
                                "size": "Small"
                            }
                        ],
                        "height": "stretch"
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
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
                                                "title": "Refresh",
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
                            }
                        ],
                        "spacing": "Small"
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
