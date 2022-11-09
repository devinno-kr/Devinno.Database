<br />

> # Devinno.Database [![NuGet stable version](https://badgen.net/nuget/v/Devinno.Database)](https://nuget.org/packages/Devinno.Database)

## 개요
  * 데이터베이스 라이브러리
    <br />
    <br />  

## 참조
  * [Microsoft.Data.Sqlite.Core](https://learn.microsoft.com/ko-kr/dotnet/standard/data/sqlite/?tabs=netcore-cli) [5.0.9](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/5.0.9)
  * [MySql.Data](https://dev.mysql.com/downloads/) [8.0.26](https://www.nuget.org/packages/MySql.Data/8.0.26)
  * [System.Data.SqlClient](https://github.com/dotnet/corefx) [4.8.2](https://www.nuget.org/packages/System.Data.SqlClient/4.8.2)
    <br />
    <br />  

## 목차
  * Devinno.Database
    * [MySQL](#MySQL)
    * [MsSQL](#MsSQL)
    * [SQLite](#SQLite)
  <br />  
  <br />  

## 사용법
### 1. Devinno.Database
#### 1.1. MySQL
MySQL DB 기능 수행

* **샘플코드**
```csharp
static void Main(string[] args)
{
    var db = new MySQL
    {
        DatabaseName = "db_test",
        Host = "127.0.0.1",
        ID = "root",
        Password = "mysql"
    };

    var tbl = "tblTest";

    Console.WriteLine("DropTable");         db.DropTable(tbl);
    Console.WriteLine("CreateTable");       db.CreateTable<Data>(tbl);
    Console.WriteLine("Insert");            db.Insert<Data>(tbl, new Data
                                            {
                                                Humidity = 70,
                                                Temperature = 36.5,
                                                Operation = true,
                                                Count = 10,
                                            });
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Update");            var d = db.Select<Data>(tbl).FirstOrDefault();
                                            d.Humidity = 100;
                                            d.OpenClose = true;
                                            d.Operation = false;
                                            d.Count = 15;
                                            d.Description = "Test";
                                            db.Update<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Delete");            db.Delete<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));

    Console.ReadKey();
}

static void Print(List<Data> ls) => ls.ForEach((v) => Console.WriteLine(v));

public class Data
{
    [SqlKey(AutoIncrement = true)]
    public int Id { get; set; }

    public double Humidity { get; set; }
    public double Temperature { get; set; }
    public bool OpenClose { get; set; }
    public bool Operation { get; set; }
    public int? Count { get; set; }
    public string? Description { get; set; }

    public override string ToString() => 
        $"{Id},{Humidity},{Temperature},{OpenClose},{Operation},{Count},{Description}";
}
```

* **결과**
```
DropTable
CreateTable
Insert
1,70,36.5,False,True,10,
Update
1,100,36.5,True,False,15,Test
Delete

```

* **설명** 
``` 
DropTable, CreateTable, Select, Insert, Update, Delete 테스트
```
<br /> 

#### 1.2. MsSQL
MsSQL DB 기능 수행

* **샘플코드**
```csharp
static void Main(string[] args)
{
    var db = new MsSQL
    {
        DatabaseName = "db_test",
        Host = @"DESKTOP\SQLEXPRESS",
        ID = @"DESKTOP\user",
        IntegratedSecurity = true
    };

    var tbl = "tblTest";

    Console.WriteLine("DropTable");         db.DropTable(tbl);
    Console.WriteLine("CreateTable");       db.CreateTable<Data>(tbl);
    Console.WriteLine("Insert");            db.Insert<Data>(tbl, new Data
                                            {
                                                Humidity = 70,
                                                Temperature = 36.5,
                                                Operation = true,
                                                Count = 10,
                                            });
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Update");            var d = db.Select<Data>(tbl).FirstOrDefault();
                                            d.Humidity = 100;
                                            d.OpenClose = true;
                                            d.Operation = false;
                                            d.Count = 15;
                                            d.Description = "Test";
                                            db.Update<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Delete");            db.Delete<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));

    Console.ReadKey();
}

static void Print(List<Data> ls) => ls.ForEach((v) => Console.WriteLine(v));

public class Data
{
    [SqlKey(AutoIncrement = true)]
    public int Id { get; set; }

    public double Humidity { get; set; }
    public double Temperature { get; set; }
    public bool OpenClose { get; set; }
    public bool Operation { get; set; }
    public int? Count { get; set; }
    public string? Description { get; set; }

    public override string ToString() =>
        $"{Id},{Humidity},{Temperature},{OpenClose},{Operation},{Count},{Description}";
}
```

* **결과**
```
DropTable
CreateTable
Insert
1,70,36.5,False,True,10,
Update
1,100,36.5,True,False,15,Test
Delete

```

* **설명** 
``` 
DropTable, CreateTable, Select, Insert, Update, Delete 테스트
```
<br /> 

#### 1.3. SQLite
SQLite DB 기능 수행

* **샘플코드**
```csharp
static void Main(string[] args)
{
    var db = new SQLite { FileName = "db.sqlite" };
    var tbl = "tblTest";

    Console.WriteLine("DropTable");         db.DropTable(tbl);
    Console.WriteLine("CreateTable");       db.CreateTable<Data>(tbl);
    Console.WriteLine("Insert");            db.Insert<Data>(tbl, new Data
                                            {
                                                Humidity = 70,
                                                Temperature = 36.5,
                                                Operation = true,
                                                Count = 10,
                                            });
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Update");            var d = db.Select<Data>(tbl).FirstOrDefault();
                                            d.Humidity = 100;
                                            d.OpenClose = true;
                                            d.Operation = false;
                                            d.Count = 15;
                                            d.Description = "Test";
                                            db.Update<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));
    Console.WriteLine("Delete");            db.Delete<Data>(tbl, d);
                                            Print(db.Select<Data>(tbl));

    Console.ReadKey();
}

static void Print(List<Data> ls) => ls.ForEach((v) => Console.WriteLine(v));

public class Data
{
    [SqlKey(AutoIncrement = true)]
    public int Id { get; set; }

    public double Humidity { get; set; }
    public double Temperature { get; set; }
    public bool OpenClose { get; set; }
    public bool Operation { get; set; }
    public int? Count { get; set; }
    public string? Description { get; set; }

    public override string ToString() =>
        $"{Id},{Humidity},{Temperature},{OpenClose},{Operation},{Count},{Description}";
}
```

* **결과**
```
DropTable
CreateTable
Insert
1,70,36.5,False,True,10,
Update
1,100,36.5,True,False,15,Test
Delete

```

* **설명** 
``` 
DropTable, CreateTable, Select, Insert, Update, Delete 테스트
```

<br /> 
