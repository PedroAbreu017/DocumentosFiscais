namespace DocumentosFiscais.Core.Models;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = [];
    
    public static ServiceResult<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static ServiceResult<T> ErrorResult(string error) => new() { Success = false, ErrorMessage = error };
}

public class PagedResult<T>
{
    private int _totalPages;
    
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    
    public int TotalPages 
    { 
        get => _totalPages > 0 ? _totalPages : (int)Math.Ceiling((double)TotalCount / PageSize);
        set => _totalPages = value;
    }
    
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Warnings { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}