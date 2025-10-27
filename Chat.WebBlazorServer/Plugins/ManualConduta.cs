using System.ComponentModel;
using System.Text.Json.Serialization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace Chat.WebBlazorServer.Plugins
{
    public class ManualConduta
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly SearchIndexClient _indexClient;

        public ManualConduta(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, SearchIndexClient indexClient)
        {
            _embeddingGenerator = embeddingGenerator;
            _indexClient = indexClient;
        }

        [KernelFunction("manual_conduta")]
        [Description("use para pesquisar o documento do manual de conduta da empresa para a consulta fornecida.")]
        [return: Description("retorna uma lista de resultados onde conteúdo são os dados encontrados na pesquisa, Citação é o nome do documento onde o resultado foi encontrado e Pontuação é a porcentagem decimal de quão confiável o resultado corresponde à consulta")]
        public async Task<IList<ManualCondutaSearchResults>> SearchAsync([Description("o prompt original otimizado para uma pesquisa vetorial")] string query)
        {
            // Convert string query to vector
            var embedding = await _embeddingGenerator.GenerateAsync(query);

            // Get AI Search client for index
            SearchClient searchClient = _indexClient.GetSearchClient("Nome dado ao index de AI Search - atenção neste ponto pois é o nome do index que você criou no portal do Azure");

            // Configure request parameters
            VectorizedQuery vectorQuery = new(embedding.Vector);

            vectorQuery.Fields.Add("contentVector"); // contentVector é um dos fields do index que guarda os vetores


            // Configure Search Options

            SearchOptions searchOptions = new()
            {
                Size = 5,
                VectorSearch = new() { Queries = { vectorQuery } }
            };


            // Perform search request
            Response<SearchResults<IndexSchema>> response = await searchClient.SearchAsync<IndexSchema>(searchOptions);

            var searchResults = new List<ManualCondutaSearchResults>();

            //interate over AI Search result
            await foreach (SearchResult<IndexSchema> result in response.Value.GetResultsAsync())
            {

                // Only add results with score >= 0.8
                if (result.Score < 0.8)
                    continue;


                searchResults.Add(new ManualCondutaSearchResults()
                {
                    Content = result.Document.Content,
                    Citation = result.Document.FilePath,
                    Score = result.Score
                });


            }
            return searchResults;
        }

        // Dar matches nos campos do Index do AI Search
        private sealed class IndexSchema
        {
            [JsonPropertyName("parent_id")]
            public string? ParentId { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("filepath")]
            public string? FilePath { get; set; }
        }
    }
}


public class ManualCondutaSearchResults
{
    public string? Content { get; set; }
    public string? Citation { get; set; }
    public double? Score { get; set; }

}