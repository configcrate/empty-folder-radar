$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$demo = Join-Path $root 'build\demo-workspace'
if (Test-Path $demo) { Remove-Item $demo -Recurse -Force }
New-Item -ItemType Directory -Force "$demo\exports\old-preview\frames", "$demo\drafts\unused", "$demo\assets\kept" | Out-Null
Set-Content "$demo\assets\kept\logo.txt" 'keep'
$app = Start-Process "$root\dist\empty-folder-radar.exe" -ArgumentList ('"' + $demo + '"') -PassThru
try {
    Start-Sleep -Seconds 2
    $app.Refresh()
    Add-Type -AssemblyName System.Drawing
    Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class EmptyRadarCapture {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr handle, int attribute, out RECT rect, int size);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr handle, IntPtr device, uint flags);
}
'@
    $rect = New-Object EmptyRadarCapture+RECT
    [EmptyRadarCapture]::DwmGetWindowAttribute($app.MainWindowHandle, 9, [ref]$rect, 16) | Out-Null
    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    $bitmap = New-Object System.Drawing.Bitmap $width, $height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $device = $graphics.GetHdc()
    [EmptyRadarCapture]::PrintWindow($app.MainWindowHandle, $device, 2) | Out-Null
    $graphics.ReleaseHdc($device)
    New-Item -ItemType Directory -Force "$root\assets" | Out-Null
    $output = "$root\assets\screenshot-zh.png"
    $bitmap.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}
finally {
    if ($app -and -not $app.HasExited) { $app.Kill() }
}
Write-Host "Rendered: $output"
