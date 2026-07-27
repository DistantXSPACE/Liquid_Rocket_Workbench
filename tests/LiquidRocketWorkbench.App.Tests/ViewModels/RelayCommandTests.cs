using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class RelayCommandTests
{
    [Fact]
    public void Constructor_WithNullExecute_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RelayCommand(null!));
    }

    [Fact]
    public void Execute_WhenEnabled_InvokesAction()
    {
        var calls = 0;
        var command = new RelayCommand(() => calls++);

        command.Execute(parameter: null);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Execute_WhenDisabled_DoesNotInvokeAction()
    {
        var calls = 0;
        var command = new RelayCommand(
            () => calls++,
            canExecute: () => false);

        command.Execute(parameter: null);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void RaiseCanExecuteChanged_NotifiesSubscribers()
    {
        var notifications = 0;
        var command = new RelayCommand(() => { });
        command.CanExecuteChanged += (_, _) => notifications++;

        command.RaiseCanExecuteChanged();

        Assert.Equal(1, notifications);
    }
}
