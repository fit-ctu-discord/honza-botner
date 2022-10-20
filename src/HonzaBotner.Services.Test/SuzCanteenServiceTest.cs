using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Xunit;

namespace HonzaBotner.Services.Test;

public class SuzCanteenServiceTest
{
    private static readonly CanteenDto Technicka = new(3, "Technická menza", true);

    [Fact]
    public async void ListCanteens()
    {
        ICanteenService canteenService = new SuzCanteenService(new HttpClient()); // TODO: Init

        var canteens = (await canteenService.ListCanteensAsync()).ToList();

        Assert.NotEmpty(canteens);
        Assert.All(canteens, dto =>
        {
            Assert.NotEmpty(dto.Name);
            Assert.NotEqual(0, dto.Id);
        });

        Assert.Contains(Technicka, canteens);
    }

    [Fact]
    public async void GetCurrentMenu()
    {
        ICanteenService canteenService = new SuzCanteenService(new HttpClient()); // TODO: Init
        var canteens = await canteenService.ListCanteensAsync(true);

        foreach (CanteenDto canteen in canteens)
        {
            var canteenWithMenu = await canteenService.GetCurrentMenuAsync(canteen);
            // Method modifies only today dishes
            Assert.Equal(canteen, canteenWithMenu with { TodayDishes = canteen.TodayDishes });
            Assert.NotNull(canteenWithMenu);

            foreach (var dish in canteenWithMenu.TodayDishes!)
            {
                Assert.NotEmpty(dish.DishType);
                Assert.NotEqual("Jiné", dish.DishType);
                Assert.NotEmpty(dish.Name);
                Assert.NotEmpty(dish.StudentPrice);
                Assert.NotEmpty(dish.OtherPrice);
            }
        }
    }
}
