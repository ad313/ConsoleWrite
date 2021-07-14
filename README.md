# ConsoleWrite
Console Write Table

```
    ConsoleWriteTable
                .AddHeader("one1", "two", "three", "aa", "放松放松放松")
                .SetHeaderColor(ConsoleColor.Red)
                .AddRow(1, 2, 3, 4)
                .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh", "gagaga")
                .Write(TextAlign.Center);
                
                
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
```
