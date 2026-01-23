using LiloShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LiloShop.Services
{
    public class CategoryService
    {
        private readonly Database _database;

        public CategoryService(Database database)
        {
            _database = database;
        }

        public List<Category> GetAllCatgeories()
        {
            return _database.Categories.Include(x => x.Products).ToList();
        }

        public void CreateCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Du måste ange ett kategorinamn");
            }
            if (_database.Categories.Any(c => c.Name.ToLower() == name.ToLower()))
            {
                throw new ArgumentException("Kategorin finns redan");
            }

            _database.Categories.Add(new Category { Name = name });
            _database.SaveChanges();
        }

        public Category GetCategoryId(int id)
        {
            return _database.Categories.FirstOrDefault(c => c.Id == id);
        }

        public void Update(int categoryId, string newName)
        {
            var catory = GetCategoryId(categoryId);
            if (catory == null)
            {
                throw new ArgumentException($"Det finns ingen kategori med id: {categoryId}");
            }

            catory.Name = newName;
            _database.SaveChanges();
        }
    }
}
