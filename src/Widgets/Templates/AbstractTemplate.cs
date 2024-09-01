using Microsoft.Windows.Widgets.Providers;

namespace WidgetsForUniGetUI.Templates
{
    internal abstract class AbstractTemplate
    {
        WidgetUpdateRequestOptions options;
        public AbstractTemplate(GenericWidget widget)
        {
            options = new WidgetUpdateRequestOptions(widget.Id);
        }

        public void UpdateWidget()
        {
            options.Data = GenerateTemplateData();
            options.Template = GenerateTemplate();

            Logger.Log(options.Data);
            Logger.Log(options.Template);

            WidgetManager.GetDefault().UpdateWidget(options);
        }

        private string GenerateTemplate()
        {
            return $$"""
            {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.5",
                "body": [
                    {{GenerateTemplateBody()}}
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

        protected abstract string GenerateTemplateBody();
        protected abstract string GenerateTemplateData();

    }
}
