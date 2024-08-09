using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Examine;
using Examine.Search;
using System.Linq;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using TheAthletic.core.Models.Account;
using TheAthletic.models;

namespace TheAthletic.Controllers
{
    public class SearchController : RenderController
    {
        private readonly IExamineManager _examineManager;
        private readonly IPublishedContentQuery _contentQuery;

        public SearchController(
            ILogger<SearchController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IExamineManager examineManager,
            IPublishedContentQuery contentQuery)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _examineManager = examineManager;
            _contentQuery = contentQuery;
        }

        [HttpGet]
        public IActionResult Search()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            string queryString = HttpContext.Request.Query["query"];
            var searchResults = SearchContent(queryString);

            var viewModel = new SearchViewModel
            {
                Query = queryString,
                SearchResults = searchResults.Select(result => new SearchResultViewModel
                {
                    Url = result.Url(),
                    Name = result.Name,
                    BodyText = result.Value<string>("bodyText")
                }).ToList(),
                HasSearched = !string.IsNullOrEmpty(queryString)
            };

            ViewData["SearchResults"] = viewModel;

            return CurrentTemplate(currentPage);
        }

        private IEnumerable<IPublishedContent> SearchContent(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<IPublishedContent>();

            var allResults = new List<ISearchResults>();

            var searchFields = new[]
            {
                "nodeName", "bodyText","selectgalleryImages","GalleryImages","blockListContent", "title", "title","bodytextaboutus","newsTitle","fullContent","homecontent","publishedDate","imageCaption"
            };

            if (_examineManager.TryGetIndex("ExternalIndex", out var externalIndex))
            {
                var externalSearcher = externalIndex.Searcher;
                var externalQuery = externalSearcher.CreateQuery("content")
                .GroupedOr(searchFields, searchTerm)
                .Or().Field("_content", searchTerm);
                var externalResults = externalQuery.Execute();
                //var externalResults = externalQuery.GroupedOr(searchFields, searchTerm).Execute();
                allResults.Add(externalResults);
            }

            if (_examineManager.TryGetIndex("InternalIndex", out var internalIndex))
            {
                var internalSearcher = internalIndex.Searcher;
                var internalQuery = internalSearcher.CreateQuery("content")
                .GroupedOr(searchFields, searchTerm)
                .Or().Field("_content",searchTerm);
                var internalResults = internalQuery.Execute();
                //var internalResults = internalQuery.GroupedOr(searchFields, searchTerm).Execute();
                allResults.Add(internalResults);
            }

            var combinedResults = allResults.SelectMany(r => r).Distinct(new SearchResultComparer());

            return combinedResults.Select(result => _contentQuery.Content(result.Id)).Where(c => c != null);
        }

        private class SearchResultComparer : IEqualityComparer<ISearchResult>
        {
            public bool Equals(ISearchResult x, ISearchResult y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(ISearchResult obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}