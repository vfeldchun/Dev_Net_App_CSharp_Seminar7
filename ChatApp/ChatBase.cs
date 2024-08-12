
namespace ChatApp
{
    public abstract class ChatBase
    {
        protected CancellationTokenSource CancellationTokenSource {  get; set; } = new CancellationTokenSource();
        protected CancellationToken CancellationToken => CancellationTokenSource.Token;

        protected abstract Task ListenerAsync();
        public abstract Task Start();
    }
}
