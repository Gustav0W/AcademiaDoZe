using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

public sealed class BancoPreferencesUpdateMessage(string value) : ValueChangedMessage<string>(value)    
{
}
