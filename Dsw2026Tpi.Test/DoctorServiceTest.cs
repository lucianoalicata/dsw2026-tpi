using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Test
{
    public class DoctorServiceTest
    {
        private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
        private readonly DoctorService _service;

        public DoctorServiceTest()
        {
            _service = new DoctorService(_mockPersistence);
        }

        [Fact]
        public async Task GetAll_CuandoExistenDoctores_EntoncesRetornaLaPaginacionMapeadaCorrectamente()
        {
            // arrange 
            var specialty = new Specialty("Cardiología", "Estudio del corazón");
            var doctor = new Doctor("Dr. Alejandro Rivera", "MED-4920", specialty);

            var pagination = new Pagination<Doctor>(10, 1, 1, new List<Doctor> { doctor });

            _mockPersistence.Paginate(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<Expression<Func<Doctor, bool>>>(),
                Arg.Any<Expression<Func<Doctor, string?>>>(),
                Arg.Any<string[]>())
                .Returns(Task.FromResult(pagination));

            // act
            var result = await _service.GetAll(10, 1);

            // assert
            var response = Assert.Single(result.Data);
            Assert.Equal(doctor.Id, response.Id);
            Assert.Equal(doctor.Name, response.Name);
            Assert.Equal(doctor.LicenseNumber, response.LicenseNumber);
            Assert.NotNull(response.Specialty);
            Assert.Equal(specialty.Name, response.Specialty.Name);
        }

        [Fact]
        public async Task GetAll_CuandoNoHayDoctoresRegistrados_EntoncesRetornaUnaPaginacionVacia()
        {
            // arrange
            _mockPersistence.Paginate(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<Expression<Func<Doctor, bool>>>(),
                Arg.Any<Expression<Func<Doctor, string?>>>(),
                Arg.Any<string[]>())
                .Returns(Task.FromResult(Pagination<Doctor>.Empty));

            // act
            var result = await _service.GetAll(10, 1);

            // assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Total);
        }
    }
}