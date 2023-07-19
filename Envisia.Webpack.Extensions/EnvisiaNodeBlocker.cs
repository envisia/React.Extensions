using System.Threading.Tasks;

namespace Envisia.Webpack.Extensions
{
    internal class EnvisiaNodeBlocker
    {
        public TaskCompletionSource<bool> CompletionSource = new TaskCompletionSource<bool>();
    }
}