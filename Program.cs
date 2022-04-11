using StackExchange.Redis;

namespace ExampleRedis
{
    public class Program
    {
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
        // new ConfigurationOptions{
        //     EndPoints = {"localhost:6379,allowAdmin=true"}                
        // });

        static async Task Main(string[] args)
        {
            var db = redis.GetDatabase();
            var pong = await db.PingAsync();
            Console.WriteLine(pong);

            //Recuperando todas as keys
            List<string> listKeys = new List<string>();
            var keys = redis.GetServer("localhost", 6379).Keys();
            listKeys.AddRange(keys.Select(key => (string)key).ToList());

            foreach (var key in listKeys)
            {
                Console.WriteLine(key);
            }

            //Criando uma key e setando valor
            await db.StringSetAsync("NetRedis:11-04-2022", "Linux e usando Redis com .NET");

            //Recuperar valor de uma key
            var getValue = await db.StringGetAsync("NetRedis:11-04-2022");
            Console.WriteLine($"\nValor da Key: {getValue}\n");

            //Recuperando hash da key
            string searchKeyValue = "resultado:11-04-22";
            var allValuesKeys = await db.HashValuesAsync(searchKeyValue);
            foreach (var item in allValuesKeys)
            {
                Console.WriteLine($"{searchKeyValue}    {item}");
            }

            //Deletando um hash de uma key
            var delete = await db.HashDeleteAsync(searchKeyValue, "valores1, 22, 33,44");
            Console.WriteLine($"\nDeletado? {delete}\n");

            //Recupera Hash da key com o valor e nome da hash
            var allHashForKey = await db.HashGetAllAsync(searchKeyValue);
            foreach (var item in allHashForKey)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }
    }
}
