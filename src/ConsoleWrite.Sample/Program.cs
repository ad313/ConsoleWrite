using System;

namespace ConsoleWrite.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //var table = new ConsoleWriteTable111("one", "two", "three");
            //table.AddRow(1, 2, 3)
            //    .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh");

            //table.Write();



            var str = new ConsoleWriteTable().AddHeader("one1", "two", "three","aa")
                .AddRow(1, 2, 3,4)
                .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh","gagaga")
                .ToString();

            Console.WriteLine(str);

            Console.ReadLine();
        }
    }
}