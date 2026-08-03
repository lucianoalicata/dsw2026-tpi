
using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;


namespace Dsw2026Tpi.Test
{
    public class SpecialtyServiceTest
    {
        private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
        private readonly SpecialtyService _service;

        public SpecialtyServiceTest()
        {
            _service = new SpecialtyService(_mockPersistence);
        }

        [Fact]
        public async Task Add_CuandoLosDatosSonValidos_EntoncesCreaLaEspecialidadYLaRetorna()
        {
            var request = new SpecialtyModel.Request("Odontología", "Diagnóstico y tratamiento de enfermedades de los dientes y encías");

            _mockPersistence.First(
                Arg.Any<Expression<Func<Specialty, bool>>>(),
                Arg.Any<string[]>())
                .Returns(Task.FromResult<Specialty?>(null));

            var result = await _service.Add(request);

            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            await _mockPersistence.Received().Add(Arg.Any<Specialty>());
        }

        [Fact]
        public async Task Add_CuandoYaExisteUnaEspecialidadConEseNombre_EntoncesLanzaUnaConflictException()
        {
            var request = new SpecialtyModel.Request("Odontología", "Diagnóstico y tratamiento de enfermedades de los dientes y encías");
            var existente = new Specialty("Odontología", "Ya fue cargada anteriormente");

            _mockPersistence.First(
                Arg.Any<Expression<Func<Specialty, bool>>>(),
                Arg.Any<string[]>())
                .Returns(Task.FromResult<Specialty?>(existente));

            await Assert.ThrowsAsync<ConflictException>(() => _service.Add(request));
        }
    }
}
