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
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Console_MVVMTesting.ViewModels
{
    public class UserReceiverViewModel : ObservableRecipient
    {
        private readonly ILoggingService _log;
        private readonly IMessenger _messenger;
        private const string consoleColor = "LCYAN";

        private string _username = "";

        public string Username
        {
            get
            {
                _log.Log(consoleColor, $"UserReceiverViewModel.Username.get: {_username}");
                return _username;
            }

            private set
            {
                SetProperty(ref _username, value);
                _log.Log(consoleColor, $"UserReceiverViewModel.Username.set: {_username}");
            }
        }

        public Post _postProperty;
        public Post PostProperty
        {
            get
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel.Username.get: {_postProperty}");
                return _postProperty;
            }

            private set
            {
                SetProperty(ref _postProperty, value);
                _log.Log(consoleColor, $"UserReceiverViewModel.Username.set: {_postProperty}");
            }
        }


        private string _reaction;
        public string Reaction
        {
            get => _reaction;
            set => SetProperty(ref _reaction, value);
        }

        private Random rnd = new Random();


        private List<string> _reactions = new List<string>
            {
                "Oops!",
                "Ouch!",
                "Not good!",
                "Aww!",
                "D'oh!"
            };



        private bool _userReceiverStatus = false;

        public UserReceiverViewModel(ILoggingService loggingService, IMessenger messenger)
        {
            _log = loggingService;
            _log.Log(consoleColor, $"UserReceiverViewModel::UserReceiverViewModel(): Start of constructor ({this.GetHashCode():x8})");

            this._username = "Poul";
            this.IsActive = true;

            _messenger = messenger;
            _messenger.Register<UserReceiverViewModel, UsernameChangedMessage>(this, UsernameChangedMessageHandler);

            _userReceiverStatus = !_userReceiverStatus;
            LoadPostsCommand = new AsyncRelayCommand(LoadPostsAsync);

            //https://medium.com/@csharpwriter/async-lock-in-csharp-4d37e22a05d7
            SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(3);

            

            // (!) https://docs.microsoft.com/en-us/dotnet/api/microsoft.toolkit.mvvm.messaging.messages?view=win-comm-toolkit-dotnet-7.0
            // odpowiedz na request:  //////// CurrentUsernameRequestMessage //////////
            _messenger.Register<UserReceiverViewModel, CurrentUsernameRequestMessage>(this, UsernameMessageHandler);

            _messenger.Register<UserReceiverViewModel, StatusRequestMessage>(this, StatusMessageHandler);


            //_postProperty = new Post();
            //////// PropertyChangedPostMessage //////////
            _messenger.Register<UserReceiverViewModel, PropertyChangedPostMessage>(this, PropertyChangedPostMessageHandler);



            // szukac Messenger.Register<CasualtyMessage, string>(this, "blanket", m => { OnCasualtyMessageReceived(); });
            // tutej https://xamlbrewer.wordpress.com/category/mvvm/

            _messenger.Register<CasualtyMessage, string>(this, "blanket", (r, m) => { RunBlanket(); });
            _messenger.Register<CasualtyMessage, string>(this, "grzypki", (r, m) => { RunGrzypki(); });
            _messenger.Register<CasualtyMessage, string>(this, "pillow", (r, m) => { RunPillow(); });
            _messenger.Register<CasualtyMessage, string>(this, "something", (r, m) => { RunSomething(); });
            _messenger.Register<CasualtyMessage>(this, (r, m) => { RunSomething(); });



            //_messenger.UnregisterAll(this);
            //_messenger.Unregister<UsernameChangedMessage>(this);
            //_messenger.Unregister<CurrentUsernameRequestMessage>(this);
            //_messenger.Unregister<StatusRequestMessage>(this);
            //_messenger.Unregister<PropertyChangedPostMessage>(this);

            _log.Log(consoleColor, $"UserReceiverViewModel::UserReceiverViewModel(): End of constructor ({this.GetHashCode():x8})");
        }

        private void RunGrzypki()
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::RunGrzypki() ({this.GetHashCode():x8})");
            _userReceiverStatus = false;
        }

        private void RunBlanket()
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::RunBlanket() ({this.GetHashCode():x8})");
            _userReceiverStatus = false;
            //Reaction = _reactions[rnd.Next(0, 5)];

            //_messenger.Send<CasualtyMessage, bool>(new CasualtyMessage(), true);
            //_messenger.Send<CasualtyMessage, bool>(new CasualtyMessage(), false);
        }

        private void RunPillow()
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::RunPillow() ({this.GetHashCode():x8})");
            Reaction = _reactions[rnd.Next(0, 5)];
            _userReceiverStatus = true;
        }

        private void RunSomething()
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::RunSomething() ({this.GetHashCode():x8})");
            Reaction = _reactions[rnd.Next(0, 5)];
            _userReceiverStatus = true;
        }


        private void PropertyChangedPostMessageHandler(UserReceiverViewModel recipient, PropertyChangedPostMessage message)
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::PropertyChangedPostMessageHandler(): start of PropertyChangedPostMessageHandler()  ({this.GetHashCode():x8})");

            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() ({this.GetHashCode():x8})");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.Sender.GetType()}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.PropertyName}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() ({message.Sender.GetHashCode():x8})");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.OldValue.SelfText}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.OldValue.Title}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.OldValue.Thumbnail}");

            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.NewValue.Title}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.NewValue.Thumbnail}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::Receive() {message.NewValue.SelfText}");

            //_log.Log(consoleColor, $"1. UserReceiverViewModel::Receive(): message.Sender.GetType(): {message.Sender.GetType()}");
            //_log.Log(consoleColor, $"2. UserReceiverViewModel::Receive(): typeof(UserSenderViewModel): {typeof(UserSenderViewModel)}");
            //_log.Log(consoleColor, $"3. UserReceiverViewModel::Receive(): message.PropertyName : {message.PropertyName }");
            //_log.Log(consoleColor, $"4. UserReceiverViewModel::Receive(): nameof(UserSenderViewModel.SelectedPost): {nameof(UserSenderViewModel.SelectedPost)}");
            //_log.Log(consoleColor, $"5. UserReceiverViewModel::Receive(): nameof(UserSenderViewModel.MySenderPublicProperyName): " +
            //    $"{nameof(UserSenderViewModel.MySenderPublicProperyName)}");
            //_log.Log(consoleColor, $"6. UserReceiverViewModel::Receive(): nameof(LCSocketViewModel.MyLCSocketPublicProperyName): " +
            //    $"{nameof(UserReceiver2ViewModel.MyLCSocketPublicProperyName)}");

            if (message.Sender.GetType() == typeof(UserSenderViewModel) &&
                message.PropertyName == nameof(UserSenderViewModel.SelectedPost))
            {
                this.PostProperty = message.NewValue;

                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Title: {this.PostProperty.Title}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Thumbnail: {this.PostProperty.Thumbnail}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.SelfText: {this.PostProperty.SelfText}");
            }

            else if (message.Sender.GetType() == typeof(UserSenderViewModel) &&
                message.PropertyName == nameof(UserSenderViewModel.MySenderPublicProperyName))
            {
                this.PostProperty = message.NewValue;

                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Title: {this.PostProperty.Title}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Thumbnail: {this.PostProperty.Thumbnail}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.SelfText: {this.PostProperty.SelfText}");

            }

            else if (message.Sender.GetType() == typeof(UserReceiver2ViewModel) &&
                message.PropertyName == nameof(UserReceiver2ViewModel.MyLCSocketPublicProperyName))
            {

                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): ==> pszet: jeste fsrodku1 <==");
                this.PostProperty = message.NewValue;

                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Title: {this.PostProperty.Title}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.Thumbnail: {this.PostProperty.Thumbnail}");
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): PostProperty.SelfText: {this.PostProperty.SelfText}");
                
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): ==> ipo: jeste fsrodku1 <==");
            }

            else if (message.Sender.GetType() == typeof(EastTesterViewModel) &&
                message.PropertyName == nameof(EastTesterViewModel.MyEastTesterPublicProperyName))
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): ==> pszet: jeste fsrodku2 <==");
                this.PostProperty = message.NewValue;

                //_log.Log(consoleColor, $"===> UserReceiverViewModel::Receive(): PostProperty.Title: {this.PostProperty.Title}");
                //_log.Log(consoleColor, $"===> UserReceiverViewModel::Receive(): PostProperty.Thumbnail: {this.PostProperty.Thumbnail}");
                //_log.Log(consoleColor, $"===> UserReceiverViewModel::Receive(): PostProperty.SelfText: {this.PostProperty.SelfText}");

                //_log.Log(consoleColor, $"UserReceiverViewModel::Receive(): ==> ipo: jeste fsrodku2 <==");
            }



            _messenger.Send<MyTestMessage<MyEnum>>();   // tesz nie dziala stont

            _log.Log(consoleColor, $"UserReceiverViewModel::PropertyChangedPostMessageHandler(): end of PropertyChangedPostMessageHandler()  ({this.GetHashCode():x8})");
        }




        // ValueChangedMessage dostaje wartosc ale nic nie zwraca
        private void UsernameChangedMessageHandler(UserReceiverViewModel recipient, UsernameChangedMessage ucm)
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler() ({this.GetHashCode():x8})");
            //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler() ucm.Value: {ucm.Value}");

            //ucm.Value.Replace('I', 'i');
            //ucm.Value = "huj";

            if (ucm.Value== "Emil")
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler(): pszyszet Emil.");
                _userReceiverStatus = true;
            }
            else if (ucm.Value == "BIANCA")
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler(): pszyszua Bianca.");
                _userReceiverStatus = true;
            }
            else if (ucm.Value == "Tata")
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler(): pszyszet tata.");
                _userReceiverStatus = false;
            }
            else if (ucm.Value == "Matka")
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler(): pszyszua Matka.");
                _userReceiverStatus = false;
            }
            else
            {
                //_log.Log(consoleColor, $"UserReceiverViewModel::UsernameChangedMessageHandler(): pszyszet ktus inny.");
                _userReceiverStatus = false;
            }

        }


        private void StatusMessageHandler(UserReceiverViewModel recipient, StatusRequestMessage message)
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::ReturnStatusMessageHandler() ({this.GetHashCode():x8})");
            message.Reply(recipient._userReceiverStatus);
        }



        // RequestMessage skoleji to co innego.
        private void UsernameMessageHandler(UserReceiverViewModel recipient, CurrentUsernameRequestMessage curm)
        {
            //_log.Log(consoleColor, $"UserReceiverViewModel::ReturnUsernameMessageHandler() ({this.GetHashCode():x8})");
            //_log.Log(consoleColor, $"UserReceiverViewModel::ReturnUsernameMessageHandler() {curm}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::ReturnUsernameMessageHandler() {curm.HasReceivedResponse}");
            //_log.Log(consoleColor, $"UserReceiverViewModel::ReturnUsernameMessageHandler() ({curm.Response})");

            curm.Reply(recipient.Username.Replace('I', 'i'));
        }

   



        public ObservableCollection<Post> Posts { get; } = new ObservableCollection<Post>();

        // // C:\Users\pkr\.nuget\packages\nito.asyncex.coordination\5.1.0\lib\netstandard2.0\Nito.AsyncEx.Coordination.dll
        //private readonly AsyncLock LoadingLock = new();
        public IAsyncRelayCommand LoadPostsCommand { get; }

        private async Task LoadPostsAsync()
        {
            //using (await LoadingLock.LockAsync())
            //{
            //    try
            //    {
            //        var response = await RedditService.GetSubredditPostsAsync(SelectedSubreddit);

            //        Posts.Clear();

            //        foreach (var item in response.Data!.Items!)
            //        {
            //            Posts.Add(item.Data!);
            //        }
            //    }
            //    catch
            //    {
            //        // Whoops!
            //    }
            //}


        }

    }
}
