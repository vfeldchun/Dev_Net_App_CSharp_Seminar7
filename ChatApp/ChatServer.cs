
using ChatCommon;
using ChatDb;
using ChatDb.Models;
using ChatNetwork;
using System.Text;

namespace ChatApp
{
    public class ChatServer : ChatBase
    {        
        private readonly IMessageSource _messageSource;        
        private List<User> _users;        

        public ChatServer(IMessageSource messageSource)
        {
            _messageSource = messageSource;      
            _users = new List<User>();
        }

        public override async Task Start()
        {
            GetCurrentUserList();
            await Task.CompletedTask;
            await Task.Run(ListenerAsync);
        }

        
        protected override async Task ListenerAsync()
        {
            Console.WriteLine("Server is ready for message exchange!");

            // Завершение работы сервера по нажатию клавишы Esc
            new Task(() =>
            {
                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        break;
                }
                
                CancellationTokenSource.Cancel();
                // Отправка сообщения о завершении работы в консоль сервера                                         
                Console.WriteLine("x" + "Server got Esc and are in shutdown process!!!");

                Task.Delay(1000);                
                //Environment.Exit(0);
            }).Start();

            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _messageSource.Receive(CancellationToken);

                    if (result.Message == null)
                        throw new Exception("Message is null");

                    switch (result.Message.Command)
                    {
                        case Command.Register:
                            await RegisterUserAsync(result);
                            break;
                        case Command.List:
                            await ListUsersAsync(result);
                            break;
                        case Command.Message:
                            await RelyMessageAsync(result);
                            break;
                        case Command.None:
                            await RelyMessageToAllAsync(result);
                            break;
                        case Command.Confirmation:
                            await HandleConfirmationAsync(result);
                            break;
                        case Command.Exit:
                            RemoveEndPoint(result);
                            //ShutdownServer();
                            break;
                    }
                    

                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("operation was canceled"))
                        Console.WriteLine(e);
                }
            }
        }

        private void RemoveEndPoint(ReceiveResult result)
        {
            var exitedUser = _users.FirstOrDefault(u => u.Name == result.Message!.FromName);

            if (exitedUser != null)
            {
                exitedUser.EndPoint = null;
            }
        }

        private async Task RelyMessageToAllAsync(ReceiveResult result)
        {
            var cloneResult = new ReceiveResult(result.EndPoint, result.Message);

            foreach (User user in _users)
            {
                if (user.Name != result.Message?.FromName)
                {
                    cloneResult.Message!.ToName = user.Name;
                    await RelyMessageAsync(cloneResult);                    
                }
            }
        }

        private void GetCurrentUserList()
        {
            using (var context = new ChatContext())
            {
                var users = context.Users.Select(x => x).ToList();
                foreach (var user in users)
                {
                    _users.Add(new User() { Id = user.Id, Name = user.Name });
                }
            }
        }

        private void ShutdownServer()
        {
            CancellationTokenSource.Cancel();
            Console.WriteLine("Server is shutting down!");
            Thread.Sleep(1000);
        }

        private async Task HandleConfirmationAsync(ReceiveResult result)
        {
            if (result.Message!.Id > 0)
            {
                using (var ctx = new ChatContext())
                {
                    var msg = ctx.Messages.FirstOrDefault(m => m.Id == result.Message!.Id);
                    msg!.Received = true;

                    ctx.SaveChanges();
                }
            }
            await Task.CompletedTask;
        }

        private async Task RelyMessageAsync(ReceiveResult result)
        {
            int? msgId = null;
            var recipientUser = _users.FirstOrDefault(u => u.Name == result.Message!.ToName);

            if (recipientUser != null)
            {
                using (var ctx = new ChatContext())
                {
                    var fromUser = ctx.Users.First(x => x.Name == result.Message!.FromName);
                    var toUser = ctx.Users.First(x => x.Name == result.Message!.ToName);

                    var msg = new ChatDb.Models.MessageEntity {
                        Text = result.Message!.Text,
                        SenderId = fromUser.Id, 
                        RecipientId = toUser.Id, 
                        Received = false,                         
                        CreatedAt = DateTime.UtcNow
                    };

                    ctx.Messages.Add(msg);

                    ctx.SaveChanges();                    

                    msgId = msg.Id;
                }
            }

            if (recipientUser?.EndPoint != null)
            {
                await _messageSource.Send(
                    new Message() { Id = msgId!, Command = Command.Message, FromName = result.Message!.FromName, 
                        ToName = result.Message!.ToName, Text = result.Message.Text },
                    recipientUser.EndPoint!,
                    CancellationToken);
            }
            else
            {
                await _messageSource.Send(
                    new Message() { Command = Command.Confirmation, FromName = "Server", ToName = result.Message!.FromName, 
                        Text = $"Message will be delivery to user {result.Message!.ToName} when he will back to online!" },
                    result.EndPoint!,
                    CancellationToken);
            }
            
        }

        private async Task ListUsersAsync(ReceiveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            foreach (var user in _users)
            {
                sb.Append($"{user.Name}, ");
            }
            sb.Append("}");

            await _messageSource.Send(
                new Message() { Command = Command.List, FromName = "Server", ToName = result.Message!.FromName, Text = $"List of registred users: {sb.ToString()}" },
                result.EndPoint!,
                CancellationToken);
        }

        private async Task RegisterUserAsync(ReceiveResult result)
        {
            User? user = null;
            // Tckb в БД есть пользователи то ищем в списке загруженных из базы пользователей
            if (_users is not null)
            {
                user = _users.FirstOrDefault(u => u.Name == result.Message!.FromName);
            }                            

            if (user is null)
            {
                // Если пользователя нет то добавлем
                using (var context = new ChatContext())
                {
                    context.Users.Add(new UserEntity() { Name = result.Message!.FromName });
                    context.SaveChanges();

                    var row = context.Users.FirstOrDefault(x => x.Name == result.Message!.FromName);

                    // user = new User() { Id = row.Id, Name = result.Message!.FromName, EndPoint = result.EndPoint };
                    user = User.FromModels(row!);
                    user.EndPoint = result.EndPoint;
                    _users?.Add(user);
                }                          
            }
            else
            {
                // Если пользователь есть то добавляем EndPoint
                user.EndPoint = result.EndPoint;                
            }
                

            await _messageSource.Send(
                new Message() { Command = Command.Confirmation, FromName = "Server", ToName = user.Name, Text = $"User {user.Name} Registred with Id={user.Id}"},
                user.EndPoint!,
                CancellationToken);

            using (var ctx = new ChatContext())
            {                
                var notReceivedMessages = ctx.Messages.Where(r => r.Received == false && r.RecipientId == user.Id).Select(x => x).ToList();

                if (notReceivedMessages.Count > 0)
                {
                    await _messageSource.Send(
                        new Message() { Command = Command.Message, FromName = "Server", ToName = user.Name, Text = $"У вас есть {notReceivedMessages.Count} не полученных сообщений" }, 
                        user.EndPoint,
                        CancellationToken);

                    foreach (var msg in notReceivedMessages)
                    {
                        var fromUser = ctx.Users.Where(r => r.Id == msg.SenderId).Select(x => x.Name).SingleOrDefault();
                        var newMessage = new Message() { Id = msg.Id, Command = Command.Message, Text = msg.Text, FromName = fromUser!, ToName = user.Name, CreatedAt = msg.CreatedAt };
                        
                        await _messageSource.Send(newMessage, user.EndPoint, CancellationToken);
                    }
                }
            }
        }
    }
}
