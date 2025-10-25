using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarRental
{
    public class CachedClientsService
    {
        private readonly CarRentalContext db;
        private readonly IMemoryCache cache;
        private readonly int _rowsNumber;
        private readonly ILogger<CachedClientsService> _logger;

        public CachedClientsService(CarRentalContext context, IMemoryCache memoryCache, ILogger<CachedClientsService> logger)
        {
            db = context;
            cache = memoryCache;
            _rowsNumber = 20;
            _logger = logger;
        }

        public async Task<IEnumerable<Client>> GetClientsAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<Client> clients))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                clients = await db.Clients.Take(_rowsNumber).ToListAsync();
                if (clients != null)
                {
                    cache.Set(cacheKey, clients,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return clients;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            _logger.LogInformation("Получение первых 20 клиентов из базы данных в обратном порядке.");

            // Получаем клиентов, сортируя в обратном порядке
            var clients = await db.Clients
                .OrderByDescending(c => c.ClientId) // Сортировка по ClientId в обратном порядке
                .Take(_rowsNumber)
                .ToListAsync(); // Получение первых 20 клиентов

            // Логируем количество полученных клиентов
            _logger.LogInformation($"Получено {clients.Count()} клиентов.");

            return clients;
        }

        public async Task<IEnumerable<Car>> GetCarsAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<Car> cars))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                cars = await db.Cars.Take(_rowsNumber).ToListAsync(); // Асинхронный вызов
                if (cars != null)
                {
                    cache.Set(cacheKey, cars,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return cars;
        }

        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            _logger.LogInformation("Получение первых 20 марок автомобилей из базы данных в обратном порядке.");

            var cars = await db.Cars
                .OrderByDescending(c => c.CarId)
                .Take(_rowsNumber)
                .ToListAsync();

            // Логируем количество полученных клиентов
            _logger.LogInformation($"Получено {cars.Count()} клиентов.");

            return cars;
        }

        public async Task<IEnumerable<CarClass>> GetClassesAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<CarClass> carClass))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                carClass = await db.CarClasses.Take(_rowsNumber).ToListAsync();
                if (carClass != null)
                {
                    cache.Set(cacheKey, carClass,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return carClass;
        }

        public async Task<IEnumerable<RentalAgreement>> GetRentalAgreementsAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<RentalAgreement> rentalAgreement))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                rentalAgreement = await db.RentalAgreements.Take(_rowsNumber).ToListAsync(); // Асинхронный вызов
                if (rentalAgreement != null)
                {
                    cache.Set(cacheKey, rentalAgreement,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return rentalAgreement;
        }

        public async Task<IEnumerable<RentalHistory>> GetRentalHistoryesAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<RentalHistory> rentalHistory))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                rentalHistory = await db.RentalHistories.Take(_rowsNumber).ToListAsync(); // Асинхронный вызов
                if (rentalHistory != null)
                {
                    cache.Set(cacheKey, rentalHistory,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return rentalHistory;
        }

        public async Task<IEnumerable<Maintenance>> GetMaintenancesAsync(string cacheKey)
        {
            if (!cache.TryGetValue(cacheKey, out IEnumerable<Maintenance> maintenance))
            {
                _logger.LogInformation("Загрузка данных из базы данных.");
                maintenance = await db.Maintenances.Take(_rowsNumber).ToListAsync(); // Асинхронный вызов
                if (maintenance != null)
                {
                    cache.Set(cacheKey, maintenance,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(262)));
                    _logger.LogInformation("Данные кэшированы на 262 секунды.");
                }
            }
            else
            {
                _logger.LogInformation("Получение данных из кэша.");
            }
            return maintenance;
        }


    }
}
