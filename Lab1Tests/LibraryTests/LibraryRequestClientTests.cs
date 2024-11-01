using FluentAssertions;
using Library;
using NSubstitute;

namespace LibraryTests;

public class LibraryRequestClientTests
{
    [Fact]
    public async Task SendAsync_ShouldReturnResult_WhenHandleOperationResultIsCalled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("Test", new byte[] { 1, 2, 3 });
        CancellationToken cancellationToken = CancellationToken.None;
        var requestId = Guid.NewGuid();
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                client.HandleOperationResult((Guid)callInfo[0], new byte[] { 4, 5, 6 });
            });
        ResponseModel response = await client.SendAsync(request, cancellationToken).ConfigureAwait(true);
        response.Data.Should().BeEquivalentTo(new byte[] { 4, 5, 6 });
    }

    [Fact]
    public async Task SendAsync_ShouldThrowException_WhenHandleOperationErrorIsCalled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("Test", new byte[] { 1, 2, 3 });
        CancellationToken cancellationToken = CancellationToken.None;
        var requestId = Guid.NewGuid();
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                client.HandleOperationError((Guid)callInfo[0], new InvalidOperationException("Error"));
            });
        Func<Task> act = async () => await client.SendAsync(request, cancellationToken).ConfigureAwait(true);
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Error")
            .ConfigureAwait(true);
    }

    [Fact]
    public void SendAsync_ShouldThrowTaskCanceledException_WhenTokenIsAlreadyCancelled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("Test", new byte[] { 1, 2, 3 });
        var cts = new CancellationTokenSource();
        cts.Cancel();
        Func<Task> act = async () => await client.SendAsync(request, cts.Token).ConfigureAwait(true);
        act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowTaskCanceledException_WhenTokenIsCancelledLater()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("Test", new byte[] { 1, 2, 3 });
        var cts = new CancellationTokenSource();
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                Task.Delay(100).ContinueWith(_ => cts.Cancel());
            });
        Func<Task> act = async () => await client.SendAsync(request, cts.Token).ConfigureAwait(true);
        await act.Should().ThrowAsync<TaskCanceledException>().ConfigureAwait(true);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResult_WhenHandleOperationResultIsCalledInBeginOperation()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("TestMethod", new byte[] { 1, 2, 3 });
        var requestId = Guid.NewGuid();
        byte[] responseData = new byte[] { 4, 5, 6 };
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                client.HandleOperationResult(
                    (Guid)callInfo[0],
                    responseData);
            });
        ResponseModel response = await client.SendAsync(request, CancellationToken.None).ConfigureAwait(true);
        response.Data.Should().BeEquivalentTo(responseData);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowException_WhenHandleOperationErrorIsCalledInBeginOperation()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("TestMethod", new byte[] { 1, 2, 3 });
        var requestId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Test exception");
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                client.HandleOperationError(
                    (Guid)callInfo[0],
                    expectedException);
            });
        Func<Task> act = async () => await client.SendAsync(request, CancellationToken.None).ConfigureAwait(true);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception").ConfigureAwait(true);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowTaskCancelledException_WhenTokenIsCancelledInBeginOperation()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new LibraryRequestClient(service);
        var request = new RequestModel("TestMethod", new byte[] { 1, 2, 3 });
        var requestId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                cts.Cancel();
            });
        Func<Task> act = async () => await client.SendAsync(request, cts.Token).ConfigureAwait(true);
        await act.Should().ThrowAsync<TaskCanceledException>().ConfigureAwait(true);
    }
}