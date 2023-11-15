using System.Collections.Generic;

namespace HonzaBotner.Services.Contract.Dto;

public record CanteenDto(int Id, string Name, bool Open, IReadOnlyList<CanteenDishDto>? TodayDishes = null);
