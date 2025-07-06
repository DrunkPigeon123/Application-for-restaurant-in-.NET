using System;
using restaurant.Models;
using restaurant.DTO;

namespace restaurant
{
    public static class CreateRecipeDtoFactory
    {
        public static RecipeDto FromMenuItem(MenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException(nameof(menuItem));

            return new RecipeDto
            {
                Name = menuItem.Name ?? string.Empty, // Handle potential null
                Ingredients = menuItem.Ingredients ?? string.Empty, // Handle potential null
                Instructions = menuItem.Instructions ?? string.Empty // Handle potential null
            };
        }
    }
}