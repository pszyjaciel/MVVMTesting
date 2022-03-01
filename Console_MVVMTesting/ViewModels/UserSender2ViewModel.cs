using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;


namespace Console_MVVMTesting.ViewModels
{
    public class UserSender2ViewModel : ObservableObject
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private const string consoleColor = "DRED";


        public UserSender2ViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;

            //_messenger.Send(new GenericMessage(MessageOp.Reset));

            MyPerson myPerson3 = new MyPerson("Wladimir", "Putin", 55, true);
            _messenger.Send(myPerson3);

            _messenger.Send(new InitMessage());
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): InitMessage  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage());    // public static TMessage Send<TMessage>(this IMessenger messenger, TMessage message) where TMessage : class;
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): ResetMessage  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage(true));
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): ResetMessage(true)  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage(false));
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): ResetMessage(false)  ({this.GetHashCode():x8})");

            _messenger.Send(new OperationMessage<bool>());      // public static class IMessengerExtensions: public static TMessage Send<TMessage>(this IMessenger messenger, TMessage message) where TMessage : class;
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): OperationMessage()  ({this.GetHashCode():x8})");

            _messenger.Send(new OperationMessage<bool>(MessageOp.Exit));
            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): OperationMessage(MessageOp.Exit)  ({this.GetHashCode():x8})");

            //_messenger.Send<MyTestMessage<MyEnum>>();

            //_messenger.Send(MyEnum.majRzal);    // public interface IMessenger: TMessage Send<TMessage, TToken>(TMessage message, TToken token)



            _log.Log(consoleColor, $"UserSender2ViewModel::UserSender2ViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }

    }
}
