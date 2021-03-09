using System.Text.Json;
using Helpers;
using Microsoft.AspNetCore.Http;

namespace Extensions
{
    public static class HttpExtensions
    {
        // add pagination header to the response
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            // create pagination header
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // add to response header
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));

            // make this new header available
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}