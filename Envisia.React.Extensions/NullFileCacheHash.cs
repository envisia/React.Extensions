using React;

namespace Envisia.React.Extensions
{
    public class NullFileCacheHash : IFileCacheHash
    {
        public string CalculateHash(string input)
        {
            return string.Empty;
        }

        public bool ValidateHash(string cacheContents, string hash)
        {
            return false;
        }

        public string AddPrefix(string hash)
        {
            return string.Empty;
        }
    }
}