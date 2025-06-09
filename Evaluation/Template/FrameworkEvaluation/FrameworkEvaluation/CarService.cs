using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkEvaluation
{
    public class CarService
    {
        private readonly IRepository<Car> carRepository;
        private readonly ITransactionManager transactionManager;
        private readonly IApiGateway apiGateway;
        private readonly IEventStore eventStore;

        public CarService(IRepository<Car> carRepository, ITransactionManager transactionManager, IApiGateway apiGateway, IEventStore eventStore)
        {
            this.carRepository = carRepository;
            this.transactionManager = transactionManager;
            this.apiGateway = apiGateway;
            this.eventStore = eventStore;
        }

        public async Task<Guid> CreateCar(string model, int year)
        {
            var car = new Car(model, year);
            await carRepository.CreateAsync(car);
            return car.Id;
        }

        public async Task<Car?> GetCar(Guid carId)
        {
            return await carRepository.ReadByIdAsync(carId);
        }

        public async Task UpdateCarYear(Guid carId, int year)
        {
            var car = await carRepository.ReadByIdAsync(carId);
            car.Year = year;
            await carRepository.UpdateAsync(car);
        }

        public async Task DeleteCar(Guid carId)
        {
            var car = await carRepository.ReadByIdAsync(carId);
            await carRepository.DeleteAsync(car);
        }

        public async Task SwitchCarModels(Car car1, Car car2)
        {
            var year1 = car1.Year;
            var year2 = car2.Year;
            car1.Year = year2;
            car2.Year = year1;

            transactionManager.Begin();
            try
            {
                await carRepository.UpdateAsync(car1);
                await carRepository.UpdateAsync(car2);
                await transactionManager.CommitAsync();
            }
            catch
            {
                await transactionManager.RollbackAsync();
                car1.Year = year1;
                car2.Year = year2;
                throw;  
            }
        }

        public Task <IReadOnlyCollection<Car>> FetchCarsExternally()
        {
            return apiGateway.GetAsync<IReadOnlyCollection<Car>>("/cars");
        }

        public async Task<int> NrOfEventsTheLastHour()
        {
            var events = await eventStore.GetEventsFromUntilAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            return events.Count();           
        }
    }
}
