namespace proyecto_SISIE.Helpers;

public static class PageHelper
{
    public static (int Page, int PageSize) Clamp(int page, int pageSize, int maxPageSize = 100)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > maxPageSize) pageSize = maxPageSize;
        return (page, pageSize);
    }
}
