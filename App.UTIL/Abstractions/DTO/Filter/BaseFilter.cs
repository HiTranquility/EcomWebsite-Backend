using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace App.UTIL.Abstractions.DTO.Filter;

public abstract class BaseFilter : IFilter
{
    private int _page;
    private int _pageSize;
    private string? _keyword;

    protected BaseFilter()
    {
        _page = DefaultPage;
        _pageSize = DefaultPageSize;
    }

    // --- Config overrideable ---
    protected virtual int DefaultPage => 1;
    protected virtual int DefaultPageSize => 10;
    protected virtual int MinPageSize => 1;
    protected virtual int MaxPage => int.MaxValue;
    protected virtual int MaxPageSize => 100;
    protected virtual int KeywordMaxLength => 100;

    // --- Query params ---
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int Page
    {
        get => _page;
        set => _page = value;
    }

    [FromQuery(Name = "pageSize")]
    [Range(1, int.MaxValue)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value;
    }

    [FromQuery(Name = "keyword")]
    public string? Keyword
    {
        get => _keyword;
        set => _keyword = value;
    }

    // --- Normalize: ONLY here, không clamp trong getter ---
    public virtual void Normalize()
    {
        // Page
        if (_page < 1) _page = DefaultPage;
        if (_page > MaxPage) _page = MaxPage;

        // PageSize
        if (_pageSize < MinPageSize) _pageSize = MinPageSize;
        if (_pageSize > MaxPageSize) _pageSize = MaxPageSize;

        // Keyword
        if (string.IsNullOrWhiteSpace(_keyword))
        {
            _keyword = null;
        }
        else
        {
            _keyword = _keyword.Trim();
            if (_keyword.Length > KeywordMaxLength)
                _keyword = _keyword[..KeywordMaxLength];
        }
    }
}
