using System;
using System.Collections.Generic;

namespace ConsoleWrite.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var sb = ConsoleWriteTable
                .AddHeader("one1", "two", "three", "aa")
                .AddRow(1, 2, 3, 4)
                .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh", "gagaga")
                .ToString(TextAlign.Center);

            Console.WriteLine(sb.ToString());


            ConsoleWriteTable
                .AddHeader("one1", "two", "three", "aa", "放松放松放松")
                .SetHeaderColor(ConsoleColor.Red)
                .AddRow(1, 2, 3, 4)
                .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh", "gagaga")
                .Write(TextAlign.Center);
            
            Console.WriteLine();

            var list = new List<TestClass>()
            {
                new TestClass()
                {
                    Id = 1,
                    Age = 18,
                    Name = "jahhahha",
                    Des = "是否是否是"
                },
                new TestClass()
                {
                    Id = 2,
                    Age = 18,
                    Name = "水灌水灌水灌水",
                    //Des = "有任何成果报告的小公司123"
                }
            };

            ConsoleWriteTable.From(list).Write(TextAlign.Center);
            Console.WriteLine();
            ConsoleWriteTable.From(list).Write(TextAlign.Left);
            Console.WriteLine();
            ConsoleWriteTable.From(list)
                .SetHeaderColor(ConsoleColor.Red)
                .Write(TextAlign.Right);


            Console.ReadLine();
        }
    }

    public class TestClass
    {
        public int Id { get; set; }


        public string Name { get; set; }


        public int Age { get; set; }

        public string Des { get; set; }
    }
}