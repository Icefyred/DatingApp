using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions{
    public static class HttpExtensions{
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages){
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var options = new JsonSerializerOptions{
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            //the name of the header is left to the developer
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
            //since we're adding a custom header we need to add a cause header
            //the name of the header that will receive the Pagination header info needs to be the exact same as it's written
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}