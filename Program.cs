using Azure.Core;
using CarRental;
using CarRental.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

public class Program
{
    public static void Main(string[] args)
    {
        // Настройка Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Создание и настройка приложения
        var builder = WebApplication.CreateBuilder(args);

        // Используем Serilog как логгер
        builder.Host.UseSerilog();

        // Настройка логирования
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Добавление кэша для сессий
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddMemoryCache();

        // Настройка сессий
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Регистрация контекста базы данных
        builder.Services.AddDbContext<CarRentalContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Регистрация сервиса CachedClientsService
        builder.Services.AddScoped<CachedClientsService>();

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Middleware для обработки пустых маршрутов
        app.Use(async (context, next) =>
        {
            // Проверка, есть ли адрес в запросе
            if (string.IsNullOrEmpty(context.Request.Path) || context.Request.Path == "/")
            {
                await context.Response.WriteAsync("Пустой адрес, продолжаем обработку.");
            }

            // Передача управления следующему middleware
            await next();
        });

        // Настройка middleware
        app.UseRouting();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.UseSession();

        // Установка кодировки для HTTP-ответов
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await next();
        });

        // Вывод информации о клиенте
        app.Map("/info", appBuilder =>
        {
            appBuilder.Run(async (context) =>
            {
                string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                                     "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                     "<BODY><H1>Информация:</H1>";
                strResponse += "<BR> Сервер: " + context.Request.Host;
                strResponse += "<BR> Путь: " + context.Request.PathBase;
                strResponse += "<BR> Протокол: " + context.Request.Protocol;
                strResponse += "<BR><A href='/home'>Главная</A></BODY></HTML>";

                await context.Response.WriteAsync(strResponse);
            });
        });


        app.Map("/home", appBuilder =>
        {
            appBuilder.Run(async (context) =>
            {
                // Формирование HTML-строки
                string htmlString = "<HTML><HEAD><TITLE>Главная</TITLE></HEAD>" +
                                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                    "<BODY><H1>Главная</H1>";
                htmlString += "<BR><A href='/carClasses'>Классы автомобилей</A></BR>";
                htmlString += "<BR><A href='/cars'>Автомобили</A></BR>";
                htmlString += "<BR><A href='/clients'>Клиенты</A></BR>";
                htmlString += "<BR><A href='/rentalAgreements'>Договоры аренды</A></BR>";
                htmlString += "<BR><A href='/rentalHistories'>История аренды</A></BR>";
                htmlString += "<BR><A href='/maintenances'>Техническое обслуживание</A></BR>";
                htmlString += "<BR><A href='/searchform1'>Данные клиента</A></BR>";
                htmlString += "<BR><A href='/searchform2'>Данные автомобиля</A></BR>";
                htmlString += "</BODY></HTML>";

                // Установка заголовка контента
                context.Response.ContentType = "text/html; charset=utf-8";

                // Вывод HTML-контента
                await context.Response.WriteAsync(htmlString);
            });
        });


        app.Map("/cars", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<Car> cars = await cachedClientsService.GetCarsAsync("Cars20");

            // Проверка на наличие автомобилей
            if (cars == null || !cars.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Автомобили не найдены."); // Возврат 404
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>Автомобили</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                "<BODY><H1>Список автомобилей</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Код автомобиля</TH>";
            htmlString += "<TH>Код класса автомобиля</TH>";
            htmlString += "<TH>Марка</TH>";
            htmlString += "<TH>Модель</TH>";
            htmlString += "<TH>Номер автомобиля</TH>";
            htmlString += "<TH>Год изготовления</TH>";
            htmlString += "<TH>Тариф за аренду в день</TH>";
            htmlString += "<TH>Статус</TH>";
            htmlString += "</TR>";

            // Данные автомобилей
            foreach (var car in cars)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{car.CarId}</TD>";
                htmlString += $"<TD>{car.ClassId}</TD>";
                htmlString += $"<TD>{car.Brand}</TD>";
                htmlString += $"<TD>{car.Model}</TD>";
                htmlString += $"<TD>{car.LicensePlate}</TD>";
                htmlString += $"<TD>{car.Year}</TD>";
                htmlString += $"<TD>{car.RentalCostPerDay}</TD>";
                htmlString += $"<TD>{car.Status}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE>";
            htmlString += "<BR><A href='/home'>На Главную</A></BR>";
            htmlString += "</BODY></HTML>";

            // Вывод данных
            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/clients", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<Client> clients = await cachedClientsService.GetClientsAsync("Clients20");

            if (clients == null || !clients.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Клиенты не найдены.");
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>Клиенты</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8' />" +
                                "<BODY><H1>Список клиентов</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Код клиента</TH>";
            htmlString += "<TH>Имя клиента</TH>";
            htmlString += "<TH>Паспорт</TH>";
            htmlString += "<TH>Телефон</TH>";
            htmlString += "</TR>";

            // Данные клиентов
            foreach (var client in clients)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{client.ClientId}</TD>";
                htmlString += $"<TD>{client.FullName}</TD>";
                htmlString += $"<TD>{client.LicenseNumber}</TD>";
                htmlString += $"<TD>{client.PhoneNumber}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE><BR><A href='/home'>На Главную</A></BR></BODY></HTML>";

            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/carClasses", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<CarClass> carClass = await cachedClientsService.GetClassesAsync("Class20");

            // Проверка на наличие классов
            if (carClass == null || !carClass.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Классы не найдены."); // Возврат 404
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>Класс автомобилей</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8' />" +
                                "<BODY><H1>Список классов автомобилей</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Код класса</TH>";
            htmlString += "<TH>Имя класса</TH>";
            htmlString += "<TH>Описание</TH>";
            htmlString += "</TR>";

            // Данные
            foreach (var CarClas in carClass)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{CarClas.ClassId}</TD>";
                htmlString += $"<TD>{CarClas.Name}</TD>";
                htmlString += $"<TD>{CarClas.Description}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE><BR><A href='/home'>На Главную</A></BR></BODY></HTML>";

            // Вывод данных
            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/rentalAgreements", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<RentalAgreement> rentalAgreements = await cachedClientsService.GetRentalAgreementsAsync("RentalAgreements20");

            if (rentalAgreements == null || !rentalAgreements.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Договоры аренды не найдены."); // Возврат 404
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>Договоры аренды</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                "<BODY><H1>Список договоров аренды</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Номер договора </TH>";
            htmlString += "<TH>Код клиента</TH>";
            htmlString += "<TH>Код автомобиля</TH>";
            htmlString += "<TH>Дата начала аренды</TH>";
            htmlString += "<TH>Планируемая дата окончания аренды</TH>";
            htmlString += "<TH>Фактическая дата окончания аренды</TH>";
            htmlString += "<TH>Стоимость аренды</TH>";
            htmlString += "</TR>";

            // Данные
            foreach (var rentalAgreement in rentalAgreements)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{rentalAgreement.RentalAgreementId}</TD>";
                htmlString += $"<TD>{rentalAgreement.ClientId}</TD>";
                htmlString += $"<TD>{rentalAgreement.CarId}</TD>";
                htmlString += $"<TD>{rentalAgreement.StartDateTime}</TD>";
                htmlString += $"<TD>{rentalAgreement.PlannedEndDateTime}</TD>";
                htmlString += $"<TD>{rentalAgreement.ActualEndDateTime}</TD>";
                htmlString += $"<TD>{rentalAgreement.TotalAmount}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE>";
            htmlString += "<BR><A href='/home'>На Главную</A></BR>";
            htmlString += "</BODY></HTML>";

            // Вывод данных
            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/rentalHistories", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<RentalHistory> rentalHistoryes = await cachedClientsService.GetRentalHistoryesAsync("rentalHistories20");

            if (rentalHistoryes == null || !rentalHistoryes.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("История аренды не найдена."); // Возврат 404
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>История аренды</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                "<BODY><H1>История аренды автомобилей</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Номер истории аренды</TH>";
            htmlString += "<TH>Код клиента</TH>";
            htmlString += "<TH>Дата начала аренды</TH>";
            htmlString += "<TH>Фактическая дата окончания аренды</TH>";
            htmlString += "<TH>Стоимость аренды</TH>";
            htmlString += "</TR>";

            // Данные
            foreach (var rentalHistory in rentalHistoryes)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{rentalHistory.RentalHistoryId}</TD>";
                htmlString += $"<TD>{rentalHistory.ClientId}</TD>";
                htmlString += $"<TD>{rentalHistory.StartDateTime}</TD>";
                htmlString += $"<TD>{rentalHistory.ActualEndDateTime}</TD>";
                htmlString += $"<TD>{rentalHistory.TotalAmount}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE>";
            htmlString += "<BR><A href='/home'>На Главную</A></BR>";
            htmlString += "</BODY></HTML>";

            // Вывод данных
            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/maintenances", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            IEnumerable<Maintenance> maintenances = await cachedClientsService.GetMaintenancesAsync("maintenances20");

            if (maintenances == null || !maintenances.Any())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Техобслуживание не найдено."); // Возврат 404
                return;
            }

            string htmlString = "<HTML><HEAD><TITLE>Техническое обслуживание автомобиля</TITLE></HEAD>" +
                                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                "<BODY><H1>Техническое обслуживание автомобиля</H1>" +
                                "<TABLE BORDER=1>";

            // Заголовки таблицы
            htmlString += "<TR>";
            htmlString += "<TH>Код технического обслуживания</TH>";
            htmlString += "<TH>Код автомобиля</TH>";
            htmlString += "<TH>Дата технического обслуживания</TH>";
            htmlString += "<TH>Вид технического обслуживания</TH>";
            htmlString += "<TH>Стоимость технического обслуживания</TH>";
            htmlString += "</TR>";

            foreach (var maintenance in maintenances)
            {
                htmlString += "<TR>";
                htmlString += $"<TD>{maintenance.Id}</TD>";
                htmlString += $"<TD>{maintenance.CarId}</TD>";
                htmlString += $"<TD>{maintenance.MaintenanceDate}</TD>";
                htmlString += $"<TD>{maintenance.Description}</TD>";
                htmlString += $"<TD>{maintenance.Cost}</TD>";
                htmlString += "</TR>";
            }

            htmlString += "</TABLE>";
            htmlString += "<BR><A href='/home'>На Главную</A></BR>";
            htmlString += "</BODY></HTML>";

            // Вывод данных
            await context.Response.WriteAsync(htmlString);
        });


        app.Map("/searchform1", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<CachedClientsService>>();

            Client client = context.Session.Get<Client>("client") ?? new Client();

            // Получение кэшированных клиентов
            var cachedClients = await cachedClientsService.GetClientsAsync("cachedClients");

            // Если кэшированных клиентов нет, получаем первых 20 клиентов из базы данных
            if (cachedClients == null || !cachedClients.Any())
            {
                logger.LogInformation("Нет кэшированных клиентов, загрузка первых 20 клиентов из базы данных.");
                cachedClients = await cachedClientsService.GetAllClientsAsync();
            }

            // Проверка, есть ли данные в куках
            if (context.Request.Cookies.ContainsKey("FormData"))
            {
                var cookieData = context.Request.Cookies["FormData"].Split(';');
                if (cookieData.Length >= 4)
                {
                    client.ClientId = int.TryParse(cookieData[0], out int id) ? id : 0;
                    client.FullName = cookieData[1];
                    client.LicenseNumber = cookieData[2];
                    client.PhoneNumber = cookieData[3];
                }
            }

            // Формирование строки для вывода динамической HTML формы
            string strResponse = "<HTML><HEAD><TITLE>Пользователь</TITLE></HEAD>" +
                                 "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                 "<BODY><FORM method='get' action='searchform1'>" +
                                 "ID:<BR><INPUT type='text' name='Id' value='" + client.ClientId + "'>" +
                                 "<BR>ФИО:<BR><INPUT type='text' name='FullName' value='" + client.FullName + "'>" +
                                 "<BR>Удостоверение:<BR><INPUT type='text' name='LicenseNumber' value='" + client.LicenseNumber + "'>" +
                                 "<BR>Телефон:<BR><INPUT type='text' name='PhoneNumber' value='" + client.PhoneNumber + "'>" +
                                 "<BR><BR>Список клиентов:<BR><SELECT name='CachedClients'>";

            // Добавление выпадающего списка для кэшированных клиентов
            foreach (var cachedClient in cachedClients)
            {
                strResponse += $"<OPTION value='{cachedClient.ClientId}'>{cachedClient.FullName}</OPTION>";
            }

            strResponse += "</SELECT>"; // Закрываем тег SELECT
            strResponse += "<BR><INPUT type='submit' value='Сохранить в куки'>" +
                           "</FORM>";
            strResponse += "<BR><A href='/home'>Главная</A></BODY></HTML>";

            // Обработка сохранения формы
            if (context.Request.Query.ContainsKey("Id"))
            {
                client.ClientId = int.TryParse(context.Request.Query["Id"], out int id) ? id : 0;
            }

            client.FullName = context.Request.Query["FullName"];
            client.LicenseNumber = context.Request.Query["LicenseNumber"];
            client.PhoneNumber = context.Request.Query["PhoneNumber"];

            // Сохранение в куки
            var cookieValue = $"{client.ClientId};{client.FullName};{client.LicenseNumber};{client.PhoneNumber}";
            context.Response.Cookies.Append("FormData", cookieValue, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) });

            // Запись в Session данных объекта Client
            context.Session.Set<Client>("client", client);

            // Асинхронный вывод динамической HTML формы
            await context.Response.WriteAsync(strResponse);
        });


        app.Map("/searchform2", async (context) =>
        {
            // Получаем сервис из контейнера зависимостей
            var cachedClientsService = context.RequestServices.GetRequiredService<CachedClientsService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<CachedClientsService>>();

            // Получаем объект Car из сессии или создаем новый
            Car car = context.Session.Get<Car>("Car") ?? new Car();

            // Получение кэшированных автомобилей
            var cachedCars = await cachedClientsService.GetCarsAsync("cachedCars");

            // Если кэшированных автомобилей нет, получаем первых 20 марок автомобилей из базы данных
            if (cachedCars == null || !cachedCars.Any())
            {
                logger.LogInformation("Нет кэшированных автомобилей, загрузка первых 20 марок автомобилей из базы данных.");
                cachedCars = await cachedClientsService.GetAllCarsAsync();
            }

            // Проверка, есть ли данные в форме
            if (context.Request.Method == HttpMethods.Get && context.Request.Query.ContainsKey("Id"))
            {
                var idValue = context.Request.Query["Id"];
                if (int.TryParse(idValue, out int id)) // Преобразуем StringValues в int
                {
                    car.CarId = id; // Сохранение Id в объект
                }
            }

            // Преобразование Year
            if (context.Request.Query.ContainsKey("Year"))
            {
                var yearValue = context.Request.Query["Year"];
                if (int.TryParse(yearValue, out int year))
                {
                    car.Year = year; // Сохранение года в объект
                }
            }

            car.Model = context.Request.Query["Model"];

            // Сохранение объекта Car в сессии
            context.Session.Set<Car>("Car", car);

            // Формирование строки для вывода динамической HTML формы
            string strResponse = "<HTML><HEAD><TITLE>Форма автомобиля</TITLE></HEAD>" +
                                 "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                 "<BODY><FORM method='get' action='/searchform2'>" +
                                 "ID:<BR><INPUT type='text' name='Id' value='" + car.CarId + "'>" +
                                 "<BR>Модель автомобиля:<BR><INPUT type='text' name='Model' value='" + car.Model + "'>" +
                                 "<BR>Год автомобиля:<BR><INPUT type='text' name='Year' value='" + car.Year + "'>" +
                                 "<BR><BR>Список марок автомобилей:<BR><SELECT name='CachedCars'>";

            // Добавление выпадающего списка для кэшированных автомобилей
            foreach (var cachedCar in cachedCars)
            {
                strResponse += $"<OPTION value='{cachedCar.CarId}'>{cachedCar.Model}</OPTION>";
            }

            strResponse += "</SELECT>";
            strResponse += "<BR><INPUT type='submit' value='Сохранить в Session'>" +
                           "</FORM>";
            strResponse += "<BR><A href='/home'>Главная</A></BODY></HTML>";

            // Асинхронный вывод динамической HTML формы
            await context.Response.WriteAsync(strResponse);
        });


        // Запуск приложения
        app.Run();
    }
}
