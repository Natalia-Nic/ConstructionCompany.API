// Data/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using ConstructionCompany.API.Models;

namespace ConstructionCompany.API.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Создание ролей если их нет
            string[] roleNames = { "Admin", "Contractor", "Client" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Создание администратора если нет пользователей
            if (!userManager.Users.Any())
            {
                var adminUser = new User
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    FullName = "Администратор Системы",
                    Phone = "+79160000000",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                // Создание тестового подрядчика
                var contractorUser = new User
                {
                    UserName = "contractor@test.com",
                    Email = "contractor@test.com",
                    FullName = "Строительная Компания ООО",
                    Phone = "+79167654321",
                    Role = "Contractor",
                    CreatedAt = DateTime.UtcNow
                };

                var contractorResult = await userManager.CreateAsync(contractorUser, "Contractor123!");
                if (contractorResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(contractorUser, "Contractor");
                }

                // Создание тестового клиента
                var clientUser = new User
                {
                    UserName = "client@test.com", 
                    Email = "client@test.com",
                    FullName = "Иван Клиентов",
                    Phone = "+79161234567",
                    Role = "Client",
                    CreatedAt = DateTime.UtcNow
                };

                var clientResult = await userManager.CreateAsync(clientUser, "Client123!");
                if (clientResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(clientUser, "Client");
                }
            }

            // Создание тестовых проектов если их нет
            if (!context.Projects.Any())
            {
                var projects = new List<Project>
                {
                    new Project
                    {
                        Name = "Дом 'Альфа'",
                        Description = "Современный двухэтажный коттедж с панорамными окнами и просторной гостиной. Идеален для большой семьи.",
                        Price = 4500000.00m,
                        ImageUrl = "/house-alpha.jpg",
                        PlanUrl = "/assets/plan-alpha.jpg", 
                        Specifications = "120м², 2 этажа, 3 спальни, 2 санузла, гараж",
                        Area = 120,
                        Bedrooms = 3,
                        Bathrooms = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Project
                    {
                        Name = "Коттедж 'Бета'", 
                        Description = "Уютный одноэтажный дом с камином и террасой. Отличный вариант для молодой семьи.",
                        Price = 3200000.00m,
                        ImageUrl = "/house-beta.jpg",
                        PlanUrl = "/assets/plan-beta.jpg",
                        Specifications = "85м², 1 этаж, 2 спальни, 1 санузел, терраса",
                        Area = 85,
                        Bedrooms = 2,
                        Bathrooms = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Project
                    {
                        Name = "Особняк 'Гамма'",
                        Description = "Просторный трехэтажный особняк с бассейном и сауной. Для тех кто ценит комфорт и роскошь.",
                        Price = 7800000.00m,
                        ImageUrl = "/house-gamma.jpg", 
                        PlanUrl = "/assets/plan-gamma.jpg",
                        Specifications = "220м², 3 этажа, 5 спален, 3 санузла, бассейн",
                        Area = 220,
                        Bedrooms = 5,
                        Bathrooms = 3,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Projects.AddRange(projects);
                await context.SaveChangesAsync();
            }
        }
    }
}