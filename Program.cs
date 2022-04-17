using StackExchange.Redis;

namespace ExampleRedis
{
    public class Program
    {
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.56.101:6379,allowAdmin=true");
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
            var keys = redis.GetServer("192.168.56.101", 6379).Keys();
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
            #region 
            var sub1 = db.Multiplexer.GetSubscriber();
            var sub2 = db.Multiplexer.GetSubscriber();

            //inscrição
            await sub1.SubscribeAsync("test", (channel, message) =>
            {
                Console.WriteLine("Sub1 Got notification: " + (string)message);
            });

            await sub2.SubscribeAsync("test", (channel, message) =>
            {
                Console.WriteLine("Sub2 Got notification: " + (string)message);
            });

            //criando um publish
            var publish = db.Multiplexer.GetSubscriber();

            //publicando no canal de teste a mensagem
            var count = await publish.PublishAsync("test", "Hello there I am a test message");
            Console.WriteLine($"Number of listeners for test {count}");



            //padrão bate com a canal, tudo que começa com a A termina com C
            await sub1.SubscribeAsync(new RedisChannel("a*c", RedisChannel.PatternMode.Pattern), (channel, message) =>
            {
                Console.WriteLine($"Sub1 Got pattern a*c notification: {message}");
            });

            await sub2.SubscribeAsync(new RedisChannel("a*c", RedisChannel.PatternMode.Pattern), (channel, message) =>
            {
                Console.WriteLine($"Sub2 Got pattern a*c notification: {message}");
            });

            count = await publish.PublishAsync("a*c", "Hello there I am a a*c message");
            Console.WriteLine($"Number of listeners for a*c {count}");

            await publish.PublishAsync("abc", "Hello there I am a abc message");
            await publish.PublishAsync("a1234567890c", "Hello there I am a a1234567890c message");
            await publish.PublishAsync("ab", "Hello I am a lost message"); //essa mensagem nunca será enviada


            //correspondencia automatica com padrões, tudo que começa com as letras primeiras letras iguais
            await sub1.SubscribeAsync(new RedisChannel("zyx*", RedisChannel.PatternMode.Auto), (channel, message) =>
            {
                Console.WriteLine($"Sub1 Got Literal pattern zyx* notification: {message}");
            });

            await publish.PublishAsync("zyxabc", "Hello there I am a zyxabc message");
            await publish.PublishAsync("zyx1234", "Hello there I am a zyxabc message");
            #endregion


            //Manipulando uma lista
            #region
            await db.ListLeftPushAsync("site:lista_top3", "palmeiras campeao libertadores 2023");
            await db.ListLeftPushAsync("site:lista_top3", "palmeiras campeao recopa sul americana 2023");
            await db.ListLeftPushAsync("site:lista_top3", "palmeiras campeao paulista 2023");
            await db.ListLeftPushAsync("site:lista_top3", "rivais de sp ficam loucos");

            //0 até -1 que é o ultimo elemento da lista, -2 e -1 seria o penultimo e ultimo elemento
            var rangeList = await db.ListRangeAsync("site:lista_top3", 0, -1);
            Console.WriteLine("\n");
            foreach (var item in rangeList)
            {
                Console.WriteLine($"Lista: {item}");
            }

            var oldNews = await db.ListRightPopAsync("site:lista_top3");
            Console.WriteLine($"\nRemovendo noticia mais antiga: {oldNews}");

            var newList = await db.ListRangeAsync("site:lista_top3", 0, 2);
            Console.WriteLine("\n");
            foreach (var item in newList)
            {
                Console.WriteLine($"Lista Top 3 noticias: {item}");
            }

            var deleteKeyList = await db.KeyDeleteAsync("site:lista_top3");
            Console.WriteLine($"\nLista deletada? {deleteKeyList}");
            #endregion

            //Manipulando Conjuntos
            #region
            await db.SetAddAsync("conjunto:amigos_lucas", "patrick");
            await db.SetAddAsync("conjunto:amigos_lucas", "felipe");
            await db.SetAddAsync("conjunto:amigos_lucas", "lucas");
            await db.SetAddAsync("conjunto:amigos_lucas", "daniel");
            await db.SetAddAsync("conjunto:amigos_lucas", "vinicius");

            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "lucas");
            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "joao");
            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "ricardo");
            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "daniel");
            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "bruno");
            await db.SetAddAsync("conjunto:amigos_exercito_brasileiro", "caio");

            Console.WriteLine("\n");
            var totalMembersLucas = await db.SetMembersAsync("conjunto:amigos_lucas");
            var totalMembersEB = await db.SetMembersAsync("conjunto:amigos_exercito_brasileiro");

            Console.WriteLine("\n");
            foreach (var item in totalMembersLucas)
            {
                Console.WriteLine($"Amigo do Lucas: {item}");
            }

            Console.WriteLine("\n");
            foreach (var item in totalMembersEB)
            {
                Console.WriteLine($"Amigos EB: {item}");
            }

            Console.WriteLine("\n");
            var IsFriendLucas = await db.SetContainsAsync("conjunto:amigos_lucas", "patrick");
            Console.WriteLine($"É amigo do lucas? {IsFriendLucas}");

            var IsFriendEB = await db.SetContainsAsync("conjunto:amigos_exercito_brasileiro", "Jhonathan");
            Console.WriteLine($"É amigo do EB? {IsFriendEB}");

            Console.WriteLine("\n");
            var removeFriendEB = await db.SetRemoveAsync("conjunto:amigos_exercito_brasileiro", "caio");
            Console.WriteLine($"Amizade removida? {removeFriendEB}");

            //Amigos em comum nos dois conjuntos
            Console.WriteLine("\n");
            var mutualFriends = await db.SetCombineAsync(SetOperation.Intersect, "conjunto:amigos_lucas", "conjunto:amigos_exercito_brasileiro");
            foreach(var item in mutualFriends)
            {
                Console.WriteLine($"Amigos em comum: {item}");
            }

            //Recupera os diferentes amigos do primeiro conjunto para o segundo, resultando em patrick, vinicius e felipe
            Console.WriteLine("\n");
            var differentFriends = await db.SetCombineAsync(SetOperation.Difference, "conjunto:amigos_lucas", "conjunto:amigos_exercito_brasileiro");
            foreach (var item in differentFriends)
            {
                Console.WriteLine($"Pessoas não conhecidas: {item}");
            }


            var deleteKeySetLucas = await db.KeyDeleteAsync("conjunto:amigos_lucas");
            Console.WriteLine($"\nConjunto de amigos do lucas deletado? {deleteKeySetLucas}"); 
            
            var deleteKeySetEb = await db.KeyDeleteAsync("conjunto:amigos_exercito_brasileiro");
            Console.WriteLine($"\nConjunto de amigos do EB deletado? {deleteKeySetEb}");
            #endregion


            Console.ReadKey();
        }
    }
}
