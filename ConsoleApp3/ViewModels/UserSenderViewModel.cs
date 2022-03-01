using Console_MVVMTesting.Helpers;
using Console_MVVMTesting.Messages;
using Console_MVVMTesting.Models;
using Console_MVVMTesting.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Console_MVVMTesting.ViewModels
{
    public class UserSenderViewModel : ObservableObject
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private const string consoleColor = "LRED";

        public ICommand SendUserMessageCommand { get; }

        private string _username;


        /// <summary>
        /// Gets or sets the currently selected post, if any.
        /// </summary>
        private Post _selectedPost;
        public Post SelectedPost
        {
            get => _selectedPost;
            set => SetProperty(ref _selectedPost, value);
        }


        private Post _mySenderPrivateProperyName;
        public Post MySenderPublicProperyName
        {
            get => _mySenderPrivateProperyName;
            set => SetProperty(ref _mySenderPrivateProperyName, value);
        }
        //SetProperty<T>([NotNullIfNotNull("newValue")] ref T field, T newValue, bool broadcast, [CallerMemberName] string? propertyName = null);


        public string Username
        {
            get
            {
                _log.Log(consoleColor, $"UserSenderViewModel.Username.get: {_username}");
                return _username;
            }

            private set
            {
                SetProperty(ref _username, value);
                _log.Log(consoleColor, $"UserSenderViewModel.Username.set: {_username}");
            }
        }


        public UserSenderViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): Start of constructor  ({this.GetHashCode():x8})");

            _messenger = messenger;


            /////////// StatusRequestMessage : RequestMessage<bool> ///////////
            StatusRequestMessage srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(1).request: {srm.Response}");


            ///////// UsernameChangedMessage : ValueChangedMessage<string> /////////
            _username = "Emil";
            _messenger.Send(new UsernameChangedMessage(Username));

            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(2).request: {srm.Response}");



            _username = "BIANCA";
            _messenger.Send(new UsernameChangedMessage(_username));     // raczej totu

            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(3).request: {srm.Response}");




            _username = "Tata";
            _messenger.Send(new UsernameChangedMessage(_username));

            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(4).request: {srm.Response}");




            _username = "Matka";
            _messenger.Send(new UsernameChangedMessage(_username));

            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(5).request: {srm.Response}");


            _messenger.Send<CasualtyMessage, string>(new CasualtyMessage(), "grzypki");
            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(6).request: {srm.Response}");

            _messenger.Send<CasualtyMessage, string>(new CasualtyMessage(), "blanket");
            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(7).request: {srm.Response}");


            _messenger.Send<CasualtyMessage, string>(new CasualtyMessage(), "pillow");
            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(8).request: {srm.Response}");

            _messenger.Send<CasualtyMessage, string>(new CasualtyMessage(), "something");
            srm = _messenger.Send(new StatusRequestMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(9).request: {srm.Response}");


            ///////// PropertyChangedPostMessage : PropertyChangedMessage<Post> /////////
            //_selectedPost = new Post { Title = "staryTajtyl1", Thumbnail = "majOldTambnejl1" };
            //_messenger.Send(new PropertyChangedPostMessage(this, "SelectedPost", _selectedPost,
            //    new Post { Title = "nowyTajtyl1", Thumbnail = "majNjuTambnejl1" }));

            _log.Log(consoleColor, $"UserSenderViewModel::Receive() this: {this}");

            //_mySenderPrivateProperyName = new Post { Title = "staryTajtyl2", Thumbnail = "majOldTambnejl2", SelfText="jakis tam SelfTest2" };
            //_messenger.Send(new PropertyChangedPostMessage(typeof(Console_MVVMTesting.ViewModels.UserSenderViewModel), "MyPublicProperyName", _mySenderPrivateProperyName,
            //    new Post { Title = "nowyTajtyl2", Thumbnail = "majNjuTambnejl2", SelfText = "A new selftext2" }));

            //_messenger.Send(new PropertyChangedPostMessage(this, "MySenderPublicProperyName", _mySenderPrivateProperyName,
            //    new Post { Title = "nowyTajtyl2", Thumbnail = "majNjuTambnejl2", SelfText = "A new selftext2" }));




            /////////// send request: CurrentUsernameRequestMessage : RequestMessage<string> ///////////
            CurrentUsernameRequestMessage curm = _messenger.Send(new CurrentUsernameRequestMessage());
            if (curm.HasReceivedResponse)
            {
                _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel().request: {curm.Response}");
            }


            /////////// StatusRequestMessage : RequestMessage<bool> ///////////
            srm = _messenger.Send(new StatusRequestMessage());
            //_log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(1).request: {srm.Response}");

            srm = _messenger.Send(new StatusRequestMessage());  // RequestMessage<string>
                                                                //_log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(2).request: {srm.Response}");


            //_messenger.Register<CasualtyMessage, bool>(this, true, (r, m) => { RunBlanketStatusTrue(); });
            //_messenger.Register<CasualtyMessage, bool>(this, false, (r, m) => { RunBlanketStatusFalse(); });


            //_messenger.Send(new GenericMessage(MessageOp.Reset));

            _messenger.Send(new InitMessage());
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): InitMessage  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage());    // public static TMessage Send<TMessage>(this IMessenger messenger, TMessage message) where TMessage : class;
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): ResetMessage  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage(true));
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): ResetMessage(true)  ({this.GetHashCode():x8})");

            _messenger.Send(new ResetMessage(false));
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): ResetMessage(false)  ({this.GetHashCode():x8})");

            _messenger.Send(new OperationMessage<bool>());      // public static class IMessengerExtensions: public static TMessage Send<TMessage>(this IMessenger messenger, TMessage message) where TMessage : class;
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): OperationMessage()  ({this.GetHashCode():x8})");

            _messenger.Send(new OperationMessage<bool>(MessageOp.Exit));
            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): OperationMessage(MessageOp.Exit)  ({this.GetHashCode():x8})");

            //_messenger.Send<MyTestMessage<MyEnum>>();

            //_messenger.Send(MyEnum.majRzal);    // public interface IMessenger: TMessage Send<TMessage, TToken>(TMessage message, TToken token)








            //_messenger.Send(new ResetMessage<bool>(MessageOp.Reset));

            //_messenger.Send(new OperationMessage(mo1));
            //_messenger.Send(new OperationMessage(mo2));

            _log.Log(consoleColor, $"UserSenderViewModel::UserSenderViewModel(): End of constructor  ({this.GetHashCode():x8})");
        }

        private void RunBlanketStatusTrue()
        {
            _log.Log(consoleColor, $"UserSenderViewModel::RunBlanketStatusTrue()  ({this.GetHashCode():x8})");
        }

        private void RunBlanketStatusFalse()
        {
            _log.Log(consoleColor, $"UserSenderViewModel::RunBlanketStatusFalse()  ({this.GetHashCode():x8})");
        }

    }
}
