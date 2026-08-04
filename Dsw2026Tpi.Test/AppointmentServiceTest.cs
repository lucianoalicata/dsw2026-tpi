using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Dsw2026Tpi.Application.Dtos.AppointmentModel;

namespace Dsw2026Tpi.Test
{
    public class AppointmentServiceTest
    {
        private readonly IPersistence _mockPersistence=Substitute.For<IPersistence>();
        private readonly ILogger<AppointmentService> _loggerMock = Substitute.For<ILogger<AppointmentService>>();
        private readonly AppointmentService _service;

        public AppointmentServiceTest()
        {
            _service = new AppointmentService(_mockPersistence, _loggerMock);
        }

        [Fact]
        public async Task BookAppointment_CuandoElDniEsInvalidoPorNoTenerLongitudEntre7Y10Caracteres_EntoncesLanzaUnaValidationException()
        {
            //arrange
            var request = new Request(
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                new PatientDto(456), 
                "consulta anual");

            //act y assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.BookAppointment(request));
        }
    }
}
