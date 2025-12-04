using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Filter;
using Microsoft.AspNetCore.Mvc;

namespace App.BLL.Dtos.OrderDto;

public class OrderFilter : BaseFilter
{
    protected override int MinPageSize => 5;
    protected override int MaxPageSize => 50;
    protected override int DefaultPageSize => 10;

    [FromQuery(Name = "status")]
    [StringLength(32)]
    public string? Status { get; set; }
}


