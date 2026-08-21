using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace AuraCommerce.Tests;

public class ReviewTests
{
    [Fact]
    public void InvalidRating_FailsViewModelValidation()
    {
        var model = new ReviewInputViewModel { ProductId = 1, Rating = 0, Comment = "Invalid" };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Rating)));
    }

    [Fact]
    public async Task DuplicateUserProductReview_IsRejectedByDatabase()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Reviews.Add(new Reviews
        {
            Id = 100,
            UserId = 2,
            ProductId = 1,
            Rating = 5,
            Comment = "First",
            CreatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();
        database.Context.Reviews.Add(new Reviews
        {
            Id = 101,
            UserId = 2,
            ProductId = 1,
            Rating = 4,
            Comment = "Duplicate",
            CreatedAt = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }
}
