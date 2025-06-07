using BaseballApi.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet("{searchQuery}")]
        public async Task<ActionResult<IEnumerable<SearchResult>>> Search(string searchQuery)
        {
            if (searchQuery.Length < 3)
            {
                return Ok(new List<SearchResult>
                {
                    new() {
                        Name = "Mike Trout",
                        Description = "Angels 2010-2025",
                        Type = SearchResultType.Player,
                        Id = 0
                    },
                    new() {
                        Name = "Shohei Ohtani",
                        Description = "Angels 2018-2023; Dodgers 2024-2025",
                        Type = SearchResultType.Player,
                        Id = 1
                    }
                }
                );
            }
            else
            {
                return Ok(new List<SearchResult>
                {
                    new() {
                        Name = "Mike Trout",
                        Description = "Angels 2010-2025",
                        Type = SearchResultType.Player,
                        Id = 0
                    },
                    new() {
                        Name = "Shohei Ohtani",
                        Description = "Angels 2018-2023; Dodgers 2024-2025",
                        Type = SearchResultType.Player,
                        Id = 1
                    },
                    new() {
                        Name = "Los Angeles Angels",
                        Description = "Los Angeles Angels of Anaheim, California",
                        Type = SearchResultType.Team,
                        Id = 2
                    }
                });
            }
        }
    }
}
