using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class AsyncRelayCommandTests
{
    [Fact]
    public void Constructor_WithNullExecute_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AsyncRelayCommand(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WhenEnabled_AwaitsDelegate()
    {
        var calls = 0;
        var command = new AsyncRelayCommand(
            () =>
            {
                calls++;
                return Task.CompletedTask;
            });

        await command.ExecuteAsync();

        Assert.Equal(1, calls);
        Assert.False(command.IsExecuting);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotInvokeDelegate()
    {
        var calls = 0;
        var command = new AsyncRelayCommand(
            () =>
            {
                calls++;
                return Task.CompletedTask;
            },
            canExecute: () => false);

        await command.ExecuteAsync();

        Assert.Equal(0, calls);
        Assert.False(command.IsExecuting);
    }

    [Fact]
    public async Task ExecuteAsync_WhileRunning_IsNonReentrant()
    {
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var command = new AsyncRelayCommand(
            async () =>
            {
                calls++;
                entered.TrySetResult();
                await release.Task;
            });

        var firstExecution = command.ExecuteAsync();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(command.IsExecuting);
        Assert.False(command.CanExecute(null));
        await command.ExecuteAsync();
        Assert.Equal(1, calls);

        release.TrySetResult();
        await firstExecution;

        Assert.False(command.IsExecuting);
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void RaiseCanExecuteChanged_NotifiesSubscribers()
    {
        var notifications = 0;
        var command = new AsyncRelayCommand(
            static () => Task.CompletedTask);
        command.CanExecuteChanged += (_, _) => notifications++;

        command.RaiseCanExecuteChanged();

        Assert.Equal(1, notifications);
    }
}
