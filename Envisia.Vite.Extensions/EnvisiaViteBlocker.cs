using System.Threading.Tasks;

namespace Envisia.Vite.Extensions
{
    internal class EnvisiaViteBlocker
    {
        public TaskCompletionSource<bool> CompletionSource = new TaskCompletionSource<bool>();
    }
}