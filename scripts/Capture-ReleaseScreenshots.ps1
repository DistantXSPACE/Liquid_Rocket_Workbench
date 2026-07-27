[CmdletBinding()]
param(
    [string]$ExecutablePath,
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ExecutablePath))
{
    $ExecutablePath = Join-Path $repositoryRoot `
        "publish\LiquidRocketWorkbench-1.0.0-win-x64\LiquidRocketWorkbench.exe"
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $repositoryRoot "docs\screenshots"
}

$resolvedExecutable = (Resolve-Path -LiteralPath $ExecutablePath).Path
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing

function Wait-ForElement
{
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string]$Name,
        [int]$TimeoutSeconds = 10
    )

    $condition = New-Object `
        System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            $Name)
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do
    {
        $element = $Root.FindFirst(
            [System.Windows.Automation.TreeScope]::Descendants,
            $condition)
        if ($null -ne $element)
        {
            return $element
        }

        Start-Sleep -Milliseconds 100
    }
    while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out waiting for UI Automation element '$Name'."
}

function Show-InScrollViewer
{
    param(
        [System.Windows.Automation.AutomationElement]$Element
    )

    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $scrollContainer = $walker.GetParent($Element)
    $scrollPattern = $null
    while ($null -ne $scrollContainer)
    {
        if ($scrollContainer.TryGetCurrentPattern(
            [System.Windows.Automation.ScrollPattern]::Pattern,
            [ref]$scrollPattern))
        {
            break
        }

        $scrollContainer = $walker.GetParent($scrollContainer)
    }

    if ($null -eq $scrollContainer -or $null -eq $scrollPattern)
    {
        throw "No scroll container contains '$($Element.Current.Name)'."
    }

    for ($attempt = 0; $attempt -lt 150; $attempt++)
    {
        $elementBounds = $Element.Current.BoundingRectangle
        $viewportBounds = $scrollContainer.Current.BoundingRectangle
        $targetTop = $viewportBounds.Top + 70
        if (-not $Element.Current.IsOffscreen -and
            $elementBounds.Top -le $targetTop)
        {
            break
        }

        if (-not $scrollPattern.Current.VerticallyScrollable)
        {
            throw "The calculation workspace cannot scroll to '$($Element.Current.Name)'."
        }

        $scrollPattern.Scroll(
            [System.Windows.Automation.ScrollAmount]::NoAmount,
            [System.Windows.Automation.ScrollAmount]::SmallIncrement)
        Start-Sleep -Milliseconds 30
    }

    if ($Element.Current.IsOffscreen)
    {
        throw "Could not bring '$($Element.Current.Name)' into view."
    }

    Start-Sleep -Milliseconds 300
}

function Save-WindowScreenshot
{
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$Path
    )

    $rectangle = $Window.Current.BoundingRectangle
    $width = [Math]::Max(1, [int][Math]::Ceiling($rectangle.Width))
    $height = [Math]::Max(1, [int][Math]::Ceiling($rectangle.Height))
    $bitmap = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try
    {
        $graphics.CopyFromScreen(
            [int][Math]::Floor($rectangle.Left),
            [int][Math]::Floor($rectangle.Top),
            0,
            0,
            $bitmap.Size)
    }
    finally
    {
        $graphics.Dispose()
    }

    try
    {
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally
    {
        $bitmap.Dispose()
    }
}

$applicationProcess = Start-Process `
    -FilePath $resolvedExecutable `
    -PassThru

try
{
    $null = $applicationProcess.WaitForInputIdle(10000)
    $windowCondition = New-Object `
        System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $applicationProcess.Id)
    $window = $null
    $deadline = [DateTime]::UtcNow.AddSeconds(10)
    $rootElement =
        [System.Windows.Automation.AutomationElement]::RootElement
    do
    {
        $window = $rootElement.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $windowCondition)
        if ($null -eq $window)
        {
            Start-Sleep -Milliseconds 100
        }
    }
    while ($null -eq $window -and [DateTime]::UtcNow -lt $deadline)

    if ($null -eq $window)
    {
        throw "The published application window was not found."
    }

    $transform = $window.GetCurrentPattern(
        [System.Windows.Automation.TransformPattern]::Pattern)
    if ($transform.Current.CanResize)
    {
        $transform.Resize(1040, 680)
    }

    if ($transform.Current.CanMove)
    {
        $transform.Move(0, 0)
    }

    Start-Sleep -Milliseconds 300
    Save-WindowScreenshot `
        -Window $window `
        -Path (Join-Path $OutputDirectory "01-input-workflow.png")

    $calculate = Wait-ForElement `
        -Root $window `
        -Name "Calculate engine performance"
    $invoke = $calculate.GetCurrentPattern(
        [System.Windows.Automation.InvokePattern]::Pattern)
    $invoke.Invoke()

    $success = Wait-ForElement `
        -Root $window `
        -Name "Successful calculation state"
    $summary = Wait-ForElement `
        -Root $window `
        -Name "Performance summary"
    Show-InScrollViewer -Element $summary
    Save-WindowScreenshot `
        -Window $window `
        -Path (Join-Path $OutputDirectory "02-performance-summary.png")

    $profiles = Wait-ForElement `
        -Root $window `
        -Name "Normalized nozzle flow profiles"
    Show-InScrollViewer -Element $profiles
    Save-WindowScreenshot `
        -Window $window `
        -Path (Join-Path $OutputDirectory "03-nozzle-profiles.png")

    $altitude = Wait-ForElement `
        -Root $window `
        -Name "Thrust versus standard altitude plot"
    Show-InScrollViewer -Element $altitude
    Save-WindowScreenshot `
        -Window $window `
        -Path (Join-Path $OutputDirectory "04-thrust-altitude.png")

    $windowTitle = $window.Current.Name
    $windowWidth = [int]$window.Current.BoundingRectangle.Width
    $windowHeight = [int]$window.Current.BoundingRectangle.Height
    $successState = $success.Current.Name
    $windowPattern = $window.GetCurrentPattern(
        [System.Windows.Automation.WindowPattern]::Pattern)
    $windowPattern.Close()
    if (-not $applicationProcess.WaitForExit(5000))
    {
        throw "The published application did not exit after its window was closed."
    }

    if ($applicationProcess.ExitCode -ne 0)
    {
        throw "The published application exited with code $($applicationProcess.ExitCode)."
    }

    [pscustomobject]@{
        Executable = $resolvedExecutable
        WindowTitle = $windowTitle
        Width = $windowWidth
        Height = $windowHeight
        SuccessState = $successState
        ExitCode = $applicationProcess.ExitCode
        Screenshots = (
            Get-ChildItem -LiteralPath $OutputDirectory `
                -Filter "*.png" `
                -File
        ).Count
        OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
    }
}
finally
{
    if ($null -ne $applicationProcess -and -not $applicationProcess.HasExited)
    {
        Stop-Process -Id $applicationProcess.Id -Force
    }
}
