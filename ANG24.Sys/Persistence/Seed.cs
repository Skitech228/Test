using ANG24.Sys.Domain.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ANG24.Sys.Persistence
{
    public sealed class Seed
    {
        public static async Task SeedData(ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (db != null && db.Prefixes != null)
            {
                if (!userManager.Users.Any() && !db.Prefixes.Any())
                {
                    await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                    await roleManager.CreateAsync(new IdentityRole("Operator"));
                    await roleManager.CreateAsync(new IdentityRole("Guest"));
                    var admin = new User
                    {
                        UserName = "admin",
                        Email = "admin@angstrem.tech"
                    };
                    var guest = new User
                    {
                        UserName = "guest",
                        Email = "guest@angstrem.tech"
                    };
                    await userManager.CreateAsync(admin, "admin");
                    await userManager.CreateAsync(guest, "guest");
                    await userManager.AddToRolesAsync(admin, new[] { "SuperAdmin", "Operator", "Guest" });
                    await userManager.AddToRoleAsync(guest, "Guest");
                    var prefixes = new Prefix[]
                    {
                    new Prefix { Name = "Дека",  CutName = "да", Factor = Math.Pow(10, 1) },
                    new Prefix { Name = "Гекто", CutName = "г", Factor = Math.Pow(10, 2) },
                    new Prefix { Name = "Кило",  CutName = "к", Factor = Math.Pow(10, 3) },
                    new Prefix { Name = "Мега",  CutName = "М", Factor = Math.Pow(10, 6) },
                    new Prefix { Name = "Гига",  CutName = "Г", Factor = Math.Pow(10, 9) },
                    new Prefix { Name = "Тера",  CutName = "Т", Factor = Math.Pow(10, 12) },
                    new Prefix { Name = "Деци",  CutName = "д", Factor = Math.Pow(10, -1) },
                    new Prefix { Name = "Санти", CutName = "с", Factor = Math.Pow(10, -2) },
                    new Prefix { Name = "Милли", CutName = "м", Factor = Math.Pow(10, -3) },
                    new Prefix { Name = "Микро", CutName = "мк", Factor = Math.Pow(10, -6) },
                    new Prefix { Name = "Нано",  CutName = "н", Factor = Math.Pow(10, -9) },
                    new Prefix { Name = "Пико",  CutName = "п", Factor = Math.Pow(10, -12) }
                    };
                    var modules = new Module[]
                    {
                    new Module { Name = "Испытания переменным напряжением", Synonym = "HVMAC" },
                    new Module { Name = "Испытания постоянным напряжением", Synonym = "HVMDC" },
                    new Module { Name = "Испытания постоянным напряжением (140кВ)", Synonym = "HVMDCHi" },
                    new Module { Name = "Высоковольтный прожиг", Synonym = "HVBurn" },
                    new Module { Name = "Совместный прожиг", Synonym = "JoinBurn" },
                    new Module { Name = "Прожиг", Synonym = "Burn" },
                    new Module { Name = "Измерения", Synonym = "Meas" },
                    new Module { Name = "Низковольтные измерения", Synonym = "LVMeas" },
                    new Module { Name = "Мост переменного тока", Synonym = "Bridge" },
                    new Module { Name = "Генератор высоковольтных импульсов", Synonym = "GVI" },
                    new Module { Name = "Рефлектометр", Synonym = "Reflect" },
                    new Module { Name = "ГП-500", Synonym = "GP500" },
                    new Module { Name = "Мост СА640", Synonym = "SA640" },
                    new Module { Name = "Мост СА540", Synonym = "SA540" },
                    new Module { Name = "Испытания СНЧ", Synonym = "VLF" },
                    new Module { Name = "Тангенс 2000", Synonym = "Tangent2000" },
                    };
                    await db.Prefixes.AddRangeAsync(prefixes);
                    await db.Modules?.AddRangeAsync(modules);
                    var deviceParameters = new List<DeviceParameter>
                {
                    new DeviceParameter
                    {
                        Parameter = "Испытательное напряжение",
                        Synonym = "V",
                        Unit = new Unit
                        {
                            Name = "Килловольт",
                            Synonim = "В",
                            Prefix = prefixes[2] // кило
                        }
                    },
                    new DeviceParameter
                    {
                        Parameter = "Ток утечки",
                        Synonym = "A",
                        Unit = new Unit
                        {
                            Name = "Миллиамперы",
                            Synonim = "A",
                            Prefix = prefixes[8] // Милли
                        }
                    }
                };
                    Device device = new Device
                    {
                        Name = "СВИ 100/140",
                        Zav_N = 0,
                        VerificationDate = DateTime.Now,
                        NextVerificationDate = DateTime.Now,
                        DeviceGroup = new DeviceGroup { Name = "ВИ устройство" },
                        Synonym = "HVMAC",
                        DeviceParameters = deviceParameters,
                        Modules = new List<Module>
                    {
                        modules[0]
                    }
                    };
                    db.Devices?.Add(device);
                    var session = new Session
                    {
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                        User = new User
                        {
                            Name = "",
                            CreateDate = DateTime.Now,
                        },
                    };
                    session.LogDatas = new List<LogData>
                    { new LogData
                        {
                            CreateDate = DateTime.Now,
                            LogType = "idk",
                            Message = "",
                            Session = session
                        }};
                    db.Sessions?.Add(session);

                    await db.SaveChangesAsync();
                }
            } //
        }
    }
}
