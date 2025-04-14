using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Samples.Benchmarks.Http.Clients;

public class Native
{
    [Benchmark]
    public async Task Request()
    {
        try
        {
            // URL da API
            string url = "https://viacep.com.br/ws/01001000/json/";

            // Fazendo uma requisição GET
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            // Verificando se a resposta foi bem-sucedida
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Resposta da API:");
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Erro na requisição: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro: {ex.Message}");
        }
    }
}