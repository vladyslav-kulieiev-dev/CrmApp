using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using NSubstitute;
using System.Text;
using System.Text.RegularExpressions;

namespace CrmApp.Tests.Services
{
    // Reference example: copy this structure when testing other services.
    public class ContractorLicensesServiceTests
    {
        private readonly IContractorLicensesRepository _repo = Substitute.For<IContractorLicensesRepository>();
        private readonly ContractorLicensesService _sut;

        public ContractorLicensesServiceTests()
        {
            _sut = new ContractorLicensesService(_repo);
        }

        [Fact]
        public async Task Add_BlankSerialNumber_GeneratesSerial()
        {
            // Arrange
            var license = new ContractorLicenses { CatalogItemId = 42, SerialNumber = "" };

            // Act
            var result = await _sut.Add(license);

            // Assert
            Assert.Matches(@"^CI42-\d{8}-[A-HJ-NP-Z2-9]{6}$", result.Data!.SerialNumber);
        }

        [Fact]
        public async Task Add_BlankSerialNumber_UsesOnlyUnambiguousCharacters()
        {
            // Arrange
            // One serial has only 6 random chars, so a disallowed char would rarely show up.
            // 200 serials (1200 chars) make missing one practically impossible.
            var randomParts = new StringBuilder();

            // Act
            for (var i = 0; i < 200; i++)
            {
                var result = await _sut.Add(new ContractorLicenses { CatalogItemId = 42 });
                var match = Regex.Match(result.Data!.SerialNumber, @"^CI42-\d{8}-(?<random>.{6})$");
                Assert.True(match.Success, $"Unexpected serial format: {result.Data.SerialNumber}");
                randomParts.Append(match.Groups["random"].Value);
            }

            // Assert
            Assert.Matches("^[A-HJ-NP-Z2-9]+$", randomParts.ToString());
        }

        [Fact]
        public async Task Add_ValidLicense_SavesAndReturnsSuccess()
        {
            // Arrange
            var license = new ContractorLicenses { Id = 99, CatalogItemId = 1, SerialNumber = "SN-1" };
            var before = DateTime.UtcNow;

            // Act
            var result = await _sut.Add(license);

            // Assert
            Assert.IsType<SuccessResultDTO<ContractorLicenses>>(result);
            Assert.True(result.Succeeded);
            Assert.Equal(0, license.Id);
            Assert.Equal("SN-1", license.SerialNumber);
            Assert.InRange(license.CreatedAt, before, DateTime.UtcNow);
            await _repo.Received(1).AddAsync(license, Arg.Any<CancellationToken>());
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Update_NotFound_ReturnsErrorAndDoesNotSave()
        {
            // Arrange
            _repo.GetAsync(5, Arg.Any<CancellationToken>()).Returns((ContractorLicenses?)null);

            // Act
            var result = await _sut.Update(new ContractorLicenses { Id = 5 });

            // Assert
            Assert.IsType<ErrorResultDTO<ContractorLicenses>>(result);
            Assert.False(result.Succeeded);
            await _repo.DidNotReceive().UpdateAsync(Arg.Any<ContractorLicenses>(), Arg.Any<CancellationToken>());
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
