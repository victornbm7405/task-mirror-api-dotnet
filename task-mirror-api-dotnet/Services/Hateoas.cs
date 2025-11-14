using System.Collections.Generic;

namespace TaskMirror.Services;

public record Link(string Rel, string Href, string Method);

public static class Hateoas
{
    public static IEnumerable<Link> BuildListLinks(string basePath, int page, int pageSize, int total)
    {
        var links = new List<Link>
        {
            new("self", $"{basePath}?page={page}&pageSize={pageSize}", "GET"),
        };

        var totalPages = (total + pageSize - 1) / pageSize;
        if (page < totalPages)
        {
            links.Add(new("next", $"{basePath}?page={page + 1}&pageSize={pageSize}", "GET"));
        }

        if (page > 1)
        {
            links.Add(new("prev", $"{basePath}?page={page - 1}&pageSize={pageSize}", "GET"));
        }

        return links;
    }

    public static IEnumerable<Link> BuildResourceLinks(string basePath, int id)
    {
        return new List<Link>
        {
            new("self", $"{basePath}/{id}", "GET"),
            new("update", $"{basePath}/{id}", "PUT"),
            new("delete", $"{basePath}/{id}", "DELETE"),
        };
    }
}
