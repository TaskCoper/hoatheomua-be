using System.Globalization;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.Utils.JsonConverters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "dd/MM/yyyy";

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
        => writer.WriteValue(value.ToString(Format, CultureInfo.InvariantCulture));

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        => DateOnly.ParseExact(reader.Value as string ?? string.Empty, Format, CultureInfo.InvariantCulture);
}

/*
 
 Problem: Json.NET không có converter built-in cho DateOnly
   
   DateOnly là struct mới trong .NET 6, ra đời sau Newtonsoft.Json rất lâu, nên Json.NET không biết cách serialize nó thành string ngày tháng thông thường. Nó sẽ fallback về reflection-based serialization — tức là serialize tất cả public properties của struct đó ra JSON object.
 
 Trước đây
 public class Employee
 {
       public string Name { get; set; }
       public DateOnly HireDate { get; set; }
  }

   var emp = new Employee { Name = "Tân", HireDate = new DateOnly(2026, 8, 21) };
   string json = JsonConvert.SerializeObject(emp);
   Console.WriteLine(json);
   
   Kết quả KHÔNG mong muốn (vì DateOnly có các public property Year, Month, Day, DayOfWeek, DayOfYear, DayNumber):
   
   {
     "Name": "Tân",
     "HireDate": {
       "Year": 2026,
       "Month": 8,
       "Day": 21,
       "DayOfWeek": 5,
       "DayOfYear": 233,
       "DayNumber": 739847
     }
   }
   
   → Thay vì mong đợi "HireDate": "21/08/2026", ta lại nhận được cả một object lộn xộn, không đúng format API cần.
   
   Tệ hơn, khi deserialize ngược lại từ JSON string date ("HireDate": "2026-08-21") mà không có converter, Json.NET sẽ throw exception vì nó không tìm được constructor phù hợp để dựng DateOnly từ string đó — request bị fail hoàn toàn.
   
   Sau khi apply DateOnlyJsonConverter, nó giúp gì
   
   public class Employee
   {
       public string Name { get; set; }
   
       [JsonConverter(typeof(DateOnlyJsonConverter))]
       public DateOnly HireDate { get; set; }
   }
   
   var json = JsonConvert.SerializeObject(emp);
   Console.WriteLine(json);
   
   {"Name":"Tân","HireDate":"21/08/2026"}
 */