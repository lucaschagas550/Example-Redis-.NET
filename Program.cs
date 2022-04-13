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

            //PUB/SUB

            var sub1 = db.Multiplexer.GetSubscriber();
            var sub2 = db.Multiplexer.GetSubscriber();

            //first subscribe, until we publish
            //subscribe to a test message
            await sub1.SubscribeAsync("test", (channel, message) => {
                Console.WriteLine("Sub1 Got notification: " + (string)message);
            });

            await sub2.SubscribeAsync("test", (channel, message) => {
                Console.WriteLine("Sub2 Got notification: " + (string)message);
            });

            //criando um publish
            var publish = db.Multiplexer.GetSubscriber();
            
            //publicando no canal de teste a mensagem
            var count = await publish.PublishAsync("test", "Hello there I am a test message");
            Console.WriteLine($"Number of listeners for test {count}");



            //padrão bate com a canal, tudo que começa com a A termina com C
            await sub1.SubscribeAsync(new RedisChannel("a*c", RedisChannel.PatternMode.Pattern), (channel, message) => {
                Console.WriteLine($"Sub1 Got pattern a*c notification: {message}");
            });
            
            await sub2.SubscribeAsync(new RedisChannel("a*c", RedisChannel.PatternMode.Pattern), (channel, message) => {
                Console.WriteLine($"Sub2 Got pattern a*c notification: {message}");
            });

            count = await publish.PublishAsync("a*c", "Hello there I am a a*c message");
            Console.WriteLine($"Number of listeners for a*c {count}");

            await publish.PublishAsync("abc", "Hello there I am a abc message");
            await publish.PublishAsync("a1234567890c", "Hello there I am a a1234567890c message");
            await publish.PublishAsync("ab", "Hello I am a lost message"); //essa mensagem nunca será enviada

            
            //correspondencia automatica com padrões, tudo que começa com as letras primeiras letras iguais
            await sub1.SubscribeAsync(new RedisChannel("zyx*", RedisChannel.PatternMode.Auto), (channel, message) => {
                Console.WriteLine($"Sub1 Got Literal pattern zyx* notification: {message}");
            });

            await publish.PublishAsync("zyxabc", "Hello there I am a zyxabc message");
            await publish.PublishAsync("zyx1234", "Hello there I am a zyxabc message");


            Console.ReadKey();
        }
    }
}
