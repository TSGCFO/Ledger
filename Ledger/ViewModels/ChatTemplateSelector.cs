using Microsoft.Maui.Controls;

namespace Ledger.ViewModels
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; } = null!;
        public DataTemplate AiTemplate { get; set; } = null!;
        public DataTemplate TypingTemplate { get; set; } = null!;
        public DataTemplate ErrorTemplate { get; set; } = null!;

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