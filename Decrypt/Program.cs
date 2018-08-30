using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                try
                {
                    Console.WriteLine(new StringEncryption().Decrypt(args[0]));
                }
                catch (Exception)
                { }
            }
            else
            {
                Console.Write("string to encrypt: ");
                var plain = Console.ReadLine();
                try
                {
                    Console.WriteLine(new StringEncryption().Encrypt(plain));
                }
                catch (Exception)
                { }
            }
        }
    }
}
