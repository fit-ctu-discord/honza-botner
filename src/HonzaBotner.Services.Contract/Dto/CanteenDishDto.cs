namespace HonzaBotner.Services.Contract.Dto;

public record CanteenDishDto(
    string DishType,
    string Name,
    string Amount,
    string StudentPrice,
    string OtherPrice,
    string PhotoLink = "");
