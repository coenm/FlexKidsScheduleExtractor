namespace FlexKids.Console
{
    using System.Security.Cryptography;
    using System.Text;
    using FlexKids.Core.Scheduler;

    public class Sha1Hash : IHash
    {
        public static readonly IHash Instance = new Sha1Hash();

        private Sha1Hash()
        {
        }

        public string Hash(string input)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                _ = sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
