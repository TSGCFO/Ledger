using Microsoft.Maui.Controls;

namespace Ledger.ViewModels
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate AiTemplate { get; set; }
        public DataTemplate TypingTemplate { get; set; }
        public DataTemplate ErrorTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ChatMessage message)
            {
                if (message.IsTyping)
                    return TypingTemplate;

                if (message.IsError)
                    return ErrorTemplate;

                return message.IsFromUser ? UserTemplate : AiTemplate;
            }

            return AiTemplate;
        }
    }
}