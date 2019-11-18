using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MitmDofus
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Server server = new Server();
                Task.Run(async () => {
                    server.StartListening();
                });
                Console.ReadLine();
            }
            
        }
    }
}
