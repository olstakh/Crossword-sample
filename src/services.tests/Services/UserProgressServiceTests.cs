using CrossWords.Services.Models;
using CrossWords.Services;
using CrossWords.Services.Abstractions;
using CrossWords.Services.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Neovolve.Logging.Xunit;

namespace CrossWords.Services.Tests;

public class UserProgressServiceTests
{
    private readonly Mock<IUserProgressRepository> _mockUserRepository = new(MockBehavior.Strict);
    private readonly Mock<ICrosswordService> _mockCrosswordService = new(MockBehavior.Strict);

    private readonly IUserProgressService _userProgressService;

    private readonly string _testUserId = "test-user";

    public UserProgressServiceTests()
    {
        _userProgressService = new UserProgressService(
            _mockCrosswordService.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public void GetUserProgress_SolvedPuzzles()
    {
        var userSolvedPuzzles = new HashSet<string>() { "puzzle1", "puzzle2" };
        var availablePuzzles = new List<string>() { "puzzle1", "puzzle3", "puzzle4"};
        _mockUserRepository
            .Setup(x => x.GetSolvedPuzzles(_testUserId))
            .Returns(userSolvedPuzzles);
        _mockCrosswordService
            .Setup(x => x.GetAvailablePuzzleIds(null))
            .Returns(availablePuzzles);

        var result = _userProgressService.GetUserProgress(_testUserId);

        Assert.Equal(_testUserId, result.UserId);
        Assert.Equal(["puzzle1"], result.SolvedPuzzleIds);
        Assert.Equal(2, result.TotalPuzzlesSolved);
    }

    [Fact]
    public void GetUserProgress_RecordSolvedPuzzles_ForwardsToUserRepository()
    {
        string solvedPuzzle = "solved-puzzle";
        _mockUserRepository
            .Setup(x => x.RecordSolvedPuzzle(_testUserId, solvedPuzzle))
            .Verifiable();

        _userProgressService.RecordSolvedPuzzle(_testUserId, solvedPuzzle);

        _mockUserRepository.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetUserProgress_HasSolvedPuzzle_ForwardsToUserRepository(bool isSolved)
    {
        string solvedPuzzle = "solved-puzzle";
        _mockUserRepository
            .Setup(x => x.IsPuzzleSolved(_testUserId, solvedPuzzle))
            .Returns(isSolved)
            .Verifiable();

        var result = _userProgressService.HasSolvedPuzzle(_testUserId, solvedPuzzle);

        Assert.Equal(isSolved, result);
        _mockUserRepository.VerifyAll();
    }
}