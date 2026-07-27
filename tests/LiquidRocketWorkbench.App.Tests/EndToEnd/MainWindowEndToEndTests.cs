using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.Tests.EndToEnd;

[Collection(WpfEndToEndCollection.Name)]
public sealed class MainWindowEndToEndTests
{
    [Fact]
    public Task HappyPath_EditableInputsReachCoreAndRenderCompleteResult()
    {
        return WpfApplicationHost.RunAsync(
            async () =>
            {
                var (window, viewModel) = CreateWindow();
                try
                {
                    await SetTextAsync(
                        FindByAutomationName<TextBox>(
                            window,
                            "Propellant label"),
                        "E2E LOX / Methane");
                    await SetTextAsync(
                        FindByAutomationName<TextBox>(
                            window,
                            "Ambient pressure in kilopascals"),
                        "90");
                    await SetTextAsync(
                        FindByAutomationName<TextBox>(
                            window,
                            "Optional target mass flow in kilograms per second"),
                        string.Empty);

                    var calculateButton =
                        FindByAutomationName<Button>(
                            window,
                            "Calculate engine performance");
                    Assert.True(calculateButton.IsEnabled);

                    InvokeButton(calculateButton);
                    await WaitForWorkflowStateAsync(
                        viewModel,
                        CalculationWorkflowState.Success);
                    await Dispatcher.Yield(DispatcherPriority.DataBind);
                    window.UpdateLayout();

                    var headline = Assert.IsType<
                        HeadlinePerformanceViewModel>(
                            viewModel.HeadlineResult);
                    var details = Assert.IsType<
                        DetailedPerformanceViewModel>(
                            viewModel.DetailedResult);

                    Assert.Equal("E2E LOX / Methane", headline.PropellantLabel);
                    Assert.Equal(90, headline.SelectedAmbientPressureKilopascals);
                    Assert.InRange(
                        headline.SelectedThrustKilonewtons,
                        22.20,
                        22.21);
                    Assert.InRange(details.ExitMachNumber, 4.355, 4.356);
                    Assert.False(details.HasTargetComparison);
                    Assert.NotNull(viewModel.NozzleProfiles);
                    Assert.NotNull(viewModel.ThrustAltitudePlot);

                    var successfulState =
                        FindByAutomationName<TextBlock>(
                            window,
                            "Successful calculation state");
                    Assert.True(successfulState.IsVisible);
                    AssertVisibleText(window, "E2E LOX / Methane");
                    AssertVisibleText(
                        window,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0:N2} kN",
                            headline.SelectedThrustKilonewtons));
                    AssertVisibleText(
                        window,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Mach {0:N4}",
                            details.ExitMachNumber));
                    AssertVisibleText(
                        window,
                        CalculationDiagnosticCodes.IdealFlowAssumptions);
                    Assert.Equal(
                        "Recalculate performance",
                        calculateButton.Content);
                }
                finally
                {
                    window.Close();
                }
            });
    }

    [Fact]
    public Task Accessibility_InteractiveControlsExposeNamesStatusAndErrors()
    {
        return WpfApplicationHost.RunAsync(
            async () =>
            {
                var (window, _) = CreateWindow();
                try
                {
                    var interactiveControls = Descendants(window)
                        .OfType<Control>()
                        .Where(
                            control =>
                                control.IsVisible
                                && control is TextBox or ComboBox or Button)
                        .ToArray();
                    var automationNames = interactiveControls
                        .Select(AutomationProperties.GetName)
                        .ToArray();

                    Assert.True(interactiveControls.Length >= 15);
                    Assert.All(
                        automationNames,
                        name => Assert.False(string.IsNullOrWhiteSpace(name)));
                    Assert.Equal(
                        automationNames.Length,
                        automationNames.Distinct().Count());

                    Assert.Equal(
                        "Alt+C",
                        AutomationProperties.GetAcceleratorKey(
                            FindByAutomationName<Button>(
                                window,
                                "Calculate engine performance")));
                    Assert.Equal(
                        "Alt+S",
                        AutomationProperties.GetAcceleratorKey(
                            FindByAutomationName<Button>(
                                window,
                                "Save current result for comparison")));
                    Assert.Equal(
                        "Alt+L",
                        AutomationProperties.GetAcceleratorKey(
                            FindByAutomationName<Button>(
                                window,
                                "Clear saved operating points")));

                    var validationStatus =
                        FindByAutomationName<TextBlock>(
                            window,
                            "Input validation status");
                    var applicationStatus =
                        FindByAutomationName<TextBlock>(
                            window,
                            "Application status");
                    Assert.Equal(
                        AutomationLiveSetting.Polite,
                        AutomationProperties.GetLiveSetting(validationStatus));
                    Assert.Equal(
                        AutomationLiveSetting.Polite,
                        AutomationProperties.GetLiveSetting(applicationStatus));

                    var chamberPressure =
                        FindByAutomationName<TextBox>(
                            window,
                            "Chamber pressure in megapascals");
                    await SetTextAsync(chamberPressure, "invalid");

                    Assert.Equal(
                        "Chamber pressure must be a number.",
                        AutomationProperties.GetHelpText(chamberPressure));
                }
                finally
                {
                    window.Close();
                }
            });
    }

    [Fact]
    public Task MinimumWindowLayout_HasNoHorizontalOverflowOrClippedControls()
    {
        return WpfApplicationHost.RunAsync(
            async () =>
            {
                var (window, _) = CreateWindow();
                try
                {
                    window.Width = window.MinWidth;
                    window.Height = window.MinHeight;
                    await Dispatcher.Yield(DispatcherPriority.Render);
                    window.UpdateLayout();

                    var workspace =
                        FindByAutomationName<ScrollViewer>(
                            window,
                            "Calculation workspace");
                    Assert.Equal(
                        ScrollBarVisibility.Disabled,
                        workspace.HorizontalScrollBarVisibility);
                    Assert.Equal(0, workspace.ScrollableWidth, precision: 3);

                    var introduction = Descendants(window)
                        .OfType<TextBlock>()
                        .Single(
                            textBlock =>
                                textBlock.Text.StartsWith(
                                    "Build a complete operating point",
                                    StringComparison.Ordinal));
                    Assert.True(
                        introduction.ActualWidth <= workspace.ViewportWidth);

                    var modelBoundary =
                        FindByAutomationName<Border>(
                            window,
                            "Model boundary summary");
                    var applicationStatus =
                        FindByAutomationName<TextBlock>(
                            window,
                            "Application status");
                    var modelBoundaryText = Descendants(modelBoundary)
                        .OfType<TextBlock>()
                        .Single(
                            textBlock =>
                                textBlock.Text
                                    == "Constant-property ideal gas with "
                                        + "choked throat flow.");
                    var boundaryBounds = modelBoundary
                        .TransformToAncestor(window)
                        .TransformBounds(
                            new Rect(
                                new Size(
                                    modelBoundary.ActualWidth,
                                    modelBoundary.ActualHeight)));
                    var statusBounds = applicationStatus
                        .TransformToAncestor(window)
                        .TransformBounds(
                            new Rect(
                                new Size(
                                    applicationStatus.ActualWidth,
                                    applicationStatus.ActualHeight)));
                    var boundaryTextBounds = modelBoundaryText
                        .TransformToAncestor(window)
                        .TransformBounds(
                            new Rect(
                                new Size(
                                    modelBoundaryText.ActualWidth,
                                    modelBoundaryText.ActualHeight)));
                    Assert.True(
                        boundaryTextBounds.Bottom
                            <= boundaryBounds.Bottom - 13.5);
                    Assert.True(
                        boundaryBounds.Bottom <= statusBounds.Top + 0.5);

                    foreach (var control in Descendants(window)
                        .OfType<Control>()
                        .Where(
                            control =>
                                control.IsVisible
                                && control is TextBox or ComboBox or Button))
                    {
                        var bounds = control
                            .TransformToAncestor(window)
                            .TransformBounds(
                                new Rect(
                                    new Size(
                                        control.ActualWidth,
                                        control.ActualHeight)));
                        Assert.True(
                            bounds.Left >= -0.5,
                            $"{AutomationProperties.GetName(control)} "
                                + $"starts outside the window at {bounds.Left}.");
                        Assert.True(
                            bounds.Right <= window.ActualWidth + 0.5,
                            $"{AutomationProperties.GetName(control)} "
                                + $"ends outside the window at {bounds.Right}.");
                    }
                }
                finally
                {
                    window.Close();
                }
            });
    }

    [Fact]
    public Task GermanCulture_FormatsRenderedValuesAndKeepsMetricUnits()
    {
        return WpfApplicationHost.RunAsync(
            async () =>
            {
                var originalCulture = CultureInfo.CurrentCulture;
                var originalUiCulture = CultureInfo.CurrentUICulture;
                var originalDefaultCulture =
                    CultureInfo.DefaultThreadCurrentCulture;
                var originalDefaultUiCulture =
                    CultureInfo.DefaultThreadCurrentUICulture;
                var germanCulture = CultureInfo.GetCultureInfo("de-DE");
                CultureInfo.DefaultThreadCurrentCulture = germanCulture;
                CultureInfo.DefaultThreadCurrentUICulture = germanCulture;
                CultureInfo.CurrentCulture = germanCulture;
                CultureInfo.CurrentUICulture = germanCulture;

                MainWindow? window = null;
                try
                {
                    (window, var viewModel) = CreateWindow();
                    InvokeButton(
                        FindByAutomationName<Button>(
                            window,
                            "Calculate engine performance"));
                    await WaitForWorkflowStateAsync(
                        viewModel,
                        CalculationWorkflowState.Success);
                    await Dispatcher.Yield(DispatcherPriority.DataBind);
                    window.UpdateLayout();

                    Assert.Equal(
                        "de-DE",
                        window.Language.IetfLanguageTag,
                        ignoreCase: true);
                    AssertVisibleText(window, "21,31 kN");
                    AssertVisibleText(window, "19,635 cm²");
                    Assert.Empty(
                        Descendants(window)
                            .OfType<TextBlock>()
                            .Where(
                                textBlock =>
                                    textBlock.IsVisible
                                    && textBlock.Text == "21.31 kN")
                            .Select(
                                textBlock =>
                                    textBlock.DataContext?.GetType().Name
                                        ?? "<no data context>"));

                    var renderedText = Descendants(window)
                        .OfType<TextBlock>()
                        .Where(static textBlock => textBlock.IsVisible)
                        .Select(static textBlock => textBlock.Text)
                        .ToArray();
                    foreach (var unit in new[]
                    {
                        "MPa",
                        "kPa",
                        "mm",
                        "cm²",
                        "K",
                        "J/(kg·K)",
                        "m/s",
                        "kg/s",
                        "kN",
                        "s Isp",
                        "%",
                        "km",
                    })
                    {
                        Assert.Contains(
                            renderedText,
                            text => text.Contains(
                                unit,
                                StringComparison.Ordinal));
                    }
                }
                finally
                {
                    window?.Close();
                    CultureInfo.CurrentCulture = originalCulture;
                    CultureInfo.CurrentUICulture = originalUiCulture;
                    CultureInfo.DefaultThreadCurrentCulture =
                        originalDefaultCulture;
                    CultureInfo.DefaultThreadCurrentUICulture =
                        originalDefaultUiCulture;
                }
            });
    }

    [Fact]
    public Task ThemeTextColors_MeetWcagAaContrastOnCommonSurfaces()
    {
        return WpfApplicationHost.RunAsync(
            () =>
            {
                var resources = Assert.IsType<ResourceDictionary>(
                    Application.Current.Resources);
                var accent = Assert.IsType<SolidColorBrush>(
                    resources["AccentBrush"]);
                var accentSoft = Assert.IsType<SolidColorBrush>(
                    resources["AccentSoftBrush"]);
                var muted = Assert.IsType<SolidColorBrush>(
                    resources["MutedTextBrush"]);
                var success = Assert.IsType<SolidColorBrush>(
                    resources["SuccessBrush"]);
                var successSoft = Assert.IsType<SolidColorBrush>(
                    resources["SuccessSoftBrush"]);
                var palePanel = Color.FromRgb(0xF6, 0xF8, 0xFB);

                AssertContrastAtLeast(accent.Color, Colors.White, 4.5);
                AssertContrastAtLeast(accent.Color, accentSoft.Color, 4.5);
                AssertContrastAtLeast(Colors.White, accent.Color, 4.5);
                AssertContrastAtLeast(muted.Color, Colors.White, 4.5);
                AssertContrastAtLeast(muted.Color, palePanel, 4.5);
                AssertContrastAtLeast(
                    success.Color,
                    successSoft.Color,
                    4.5);

                return Task.CompletedTask;
            });
    }

    [Theory]
    [InlineData(
        "Chamber pressure in megapascals",
        "not-a-number",
        "Chamber pressure in megapascals",
        "Chamber pressure must be a number.",
        "8")]
    [InlineData(
        "Ambient pressure in kilopascals",
        "9000",
        "Chamber pressure in megapascals",
        "Chamber pressure must be greater than ambient pressure.",
        "90")]
    public Task InvalidInput_DisablesCalculationAndRecoversThroughBinding(
        string editedControlName,
        string invalidValue,
        string errorControlName,
        string expectedMessage,
        string recoveredValue)
    {
        return WpfApplicationHost.RunAsync(
            async () =>
            {
                var (window, viewModel) = CreateWindow();
                try
                {
                    var editedControl =
                        FindByAutomationName<TextBox>(
                            window,
                            editedControlName);
                    var errorControl =
                        FindByAutomationName<TextBox>(
                            window,
                            errorControlName);
                    var calculateButton =
                        FindByAutomationName<Button>(
                            window,
                            "Calculate engine performance");

                    await SetTextAsync(editedControl, invalidValue);

                    Assert.Equal(
                        CalculationWorkflowState.Empty,
                        viewModel.WorkflowState);
                    Assert.True(viewModel.Inputs.HasErrors);
                    Assert.True(Validation.GetHasError(errorControl));
                    Assert.Contains(
                        Validation.GetErrors(errorControl),
                        error =>
                            Equals(error.ErrorContent, expectedMessage));
                    AssertVisibleText(window, expectedMessage);
                    Assert.False(calculateButton.IsEnabled);
                    Assert.Null(viewModel.HeadlineResult);
                    Assert.Null(viewModel.DetailedResult);

                    await SetTextAsync(editedControl, recoveredValue);

                    Assert.False(viewModel.Inputs.HasErrors);
                    Assert.False(Validation.GetHasError(errorControl));
                    Assert.True(calculateButton.IsEnabled);

                    InvokeButton(calculateButton);
                    await WaitForWorkflowStateAsync(
                        viewModel,
                        CalculationWorkflowState.Success);
                    await Dispatcher.Yield(DispatcherPriority.DataBind);
                    window.UpdateLayout();

                    Assert.True(viewModel.HasResult);
                    Assert.True(
                        FindByAutomationName<TextBlock>(
                            window,
                            "Successful calculation state").IsVisible);
                }
                finally
                {
                    window.Close();
                }
            });
    }

    private static (MainWindow Window, MainWindowViewModel ViewModel)
        CreateWindow()
    {
        var inputs = new EngineInputViewModel(new EngineInputsValidator());
        var viewModel = new MainWindowViewModel(
            new EnginePerformanceCalculator(),
            inputs,
            minimumLoadingDuration: TimeSpan.Zero);
        var window = new MainWindow(viewModel)
        {
            ShowActivated = false,
            ShowInTaskbar = false,
            Opacity = 0,
        };

        window.Show();
        window.UpdateLayout();
        return (window, viewModel);
    }

    private static async Task SetTextAsync(
        TextBox textBox,
        string value)
    {
        textBox.Text = value;
        BindingExpression? bindingExpression =
            textBox.GetBindingExpression(TextBox.TextProperty);
        Assert.NotNull(bindingExpression);
        bindingExpression.UpdateSource();
        await Dispatcher.Yield(DispatcherPriority.DataBind);
    }

    private static void InvokeButton(Button button)
    {
        var peer = new ButtonAutomationPeer(button);
        var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(
            peer.GetPattern(PatternInterface.Invoke));
        invokeProvider.Invoke();
    }

    private static async Task WaitForWorkflowStateAsync(
        MainWindowViewModel viewModel,
        CalculationWorkflowState expected)
    {
        var stopwatch = Stopwatch.StartNew();
        while (viewModel.WorkflowState != expected)
        {
            if (stopwatch.Elapsed >= TimeSpan.FromSeconds(5))
            {
                throw new TimeoutException(
                    $"The workflow did not reach {expected}. "
                    + $"Current state: {viewModel.WorkflowState}.");
            }

            await Task.Delay(10);
        }
    }

    private static T FindByAutomationName<T>(
        DependencyObject root,
        string automationName)
        where T : DependencyObject
    {
        return Descendants(root)
            .OfType<T>()
            .Single(
                element =>
                    AutomationProperties.GetName(element)
                        == automationName);
    }

    private static void AssertVisibleText(
        DependencyObject root,
        string expectedText)
    {
        Assert.Contains(
            Descendants(root).OfType<TextBlock>(),
            textBlock =>
                textBlock.IsVisible
                && textBlock.Text == expectedText);
    }

    private static void AssertContrastAtLeast(
        Color foreground,
        Color background,
        double minimumRatio)
    {
        var foregroundLuminance = RelativeLuminance(foreground);
        var backgroundLuminance = RelativeLuminance(background);
        var ratio =
            (Math.Max(foregroundLuminance, backgroundLuminance) + 0.05)
            / (Math.Min(foregroundLuminance, backgroundLuminance) + 0.05);

        Assert.True(
            ratio >= minimumRatio,
            $"Contrast ratio {ratio:N2}:1 is below "
                + $"{minimumRatio:N1}:1.");
    }

    private static double RelativeLuminance(Color color)
    {
        return (0.2126 * Linearize(color.R / 255d))
            + (0.7152 * Linearize(color.G / 255d))
            + (0.0722 * Linearize(color.B / 255d));
    }

    private static double Linearize(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static IEnumerable<DependencyObject> Descendants(
        DependencyObject root)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            yield return child;

            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class WpfEndToEndCollection
{
    public const string Name = "WPF end-to-end";
}

internal static class WpfApplicationHost
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly Lazy<Dispatcher> UiDispatcher =
        new(StartDispatcher, LazyThreadSafetyMode.ExecutionAndPublication);

    public static async Task RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        await Gate.WaitAsync();
        try
        {
            await UiDispatcher.Value
                .InvokeAsync(action, DispatcherPriority.Normal)
                .Task
                .Unwrap();
        }
        finally
        {
            Gate.Release();
        }
    }

    private static Dispatcher StartDispatcher()
    {
        var ready = new TaskCompletionSource<Dispatcher>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(
            () =>
            {
                try
                {
                    var application =
                        new LiquidRocketWorkbench.App.App();
                    application.InitializeComponent();
                    application.ShutdownMode =
                        ShutdownMode.OnExplicitShutdown;
                    var dispatcher = Dispatcher.CurrentDispatcher;
                    ready.SetResult(dispatcher);
                    Dispatcher.Run();
                }
                catch (Exception exception)
                {
                    ready.TrySetException(exception);
                }
            })
        {
            IsBackground = true,
            Name = "LiquidRocketWorkbench.WpfEndToEnd",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return ready.Task.GetAwaiter().GetResult();
    }
}
