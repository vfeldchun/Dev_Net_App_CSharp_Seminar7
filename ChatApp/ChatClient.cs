using ChatCommon;
using ChatNetwork;
using System.Net;

namespace ChatApp
{
    public class ChatClient : ChatBase
    {
        private readonly IMessageSource _messageSource;
        private readonly IPEndPoint _serverEndPoint;
        private readonly User _user;
        private List<User> _users = [];

        public ChatClient(string userName, IPEndPoint serverEndPoint, IMessageSource messageSource) 
        {         
            _messageSource = messageSource;
            _serverEndPoint = serverEndPoint;
            _user = new User { Name = userName };
        }

        private async Task RegisterUser()
        {
            var registerMessage = new Message { Command = Command.Register, FromName = _user.Name, ToName = "Server" };
            await _messageSource.Send(registerMessage, _serverEndPoint, CancellationToken);
        }

        public override async Task Start()
        {        
            new Task(async () => await ListenerAsync()).Start();
            
            await RegisterUser();

            while (!CancellationToken.IsCancellationRequested)
            {
                string userInput = (Console.ReadLine()) ?? string.Empty;
                Message? message = null;

                if (userInput != String.Empty)
                {

                    switch (userInput.ToLower())
                    {
                        case "list":
                            message = new Message {
                                Command = Command.List,
                                FromName = _user.Name,
                                ToName = "Server"
                            };
                            break;
                        case "exit":
                            message = new Message
                            {
                                Command = Command.Exit,
                                FromName = _user.Name,
                                ToName = "Server"
                            };

                            break;
                        default:
                            // Если пользователь адресует комманду другому пользователю через имя и :
                            if (userInput.Split(':').Length > 1)
                            {
                                message = new Message
                                {
                                    Command = Command.Message,
                                    FromName = _user.Name,
                                    ToName = userInput.Split(':')[0].Trim(),
                                    Text = userInput.Split(':')[1].Trim(),
                                };
                            }
                            // В противном случае отправка сообщение идет всем пользователям
                            else
                            {
                                message = new Message
                                {
                                    Command = Command.None,
                                    FromName = _user.Name,                                    
                                    Text = userInput
                                };
                            }
                            break;
                    }

                }

                if (message is not null)                
                    await _messageSource.Send(message, _serverEndPoint, CancellationToken);

                if (message?.Command == Command.Exit)
                {
                    CancellationTokenSource.Cancel();
                    await Task.Delay(1000);
                }
            }
        }

        protected override async Task ListenerAsync()
        {
            while(!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _messageSource.Receive(CancellationToken);

                    if (result.Message == null)
                        throw new Exception("Message is null");

                    if (result.Message.Command == Command.Message)
                    {
                        await _messageSource.Send(
                            new Message() { Command = Command.Confirmation, Id = result.Message.Id, FromName = _user.Name }, 
                            _serverEndPoint, 
                            CancellationToken);
                    }

                    Console.WriteLine(result.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
