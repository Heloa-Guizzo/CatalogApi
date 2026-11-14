using CatalogAPI.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace CatalogAPI.Services;

public class ElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "games";

    public ElasticsearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task IndexGameAsync(Game game)
    {
        await _client.IndexAsync(game, idx => idx.Index(IndexName).Id(game.Id));
    }

    public async Task DeleteGameAsync(Guid id)
    {
        await _client.DeleteAsync<Game>(id, d => d.Index(IndexName));
    }

    public async Task<IReadOnlyCollection<Game>> SearchGamesAsync(string term)
    {
        var response = await _client.SearchAsync<Game>(s => s
                    .Indices(IndexName)
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Fields(new[] { "name^2", "genre" })
                            .Query(term)
                            .Fuzziness(new Fuzziness("AUTO"))
                        )
                    )
                );

        return response.Documents;
    }
}