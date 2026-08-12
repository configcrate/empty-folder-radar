param([Parameter(Mandatory=$true)][string]$OutputPath)
$ErrorActionPreference='Stop'; Add-Type -AssemblyName System.Drawing
$size=256; $b=New-Object System.Drawing.Bitmap $size,$size; $g=[System.Drawing.Graphics]::FromImage($b); $g.SmoothingMode='AntiAlias'; $g.Clear([System.Drawing.Color]::FromArgb(15,18,24))
$mint=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(92,225,181)); $ink=New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(10,43,35)),16; $ink.StartCap='Round';$ink.EndCap='Round';$ink.LineJoin='Round'
$g.FillRectangle($mint,22,22,212,212); $g.DrawLine($ink,45,92,105,92);$g.DrawLine($ink,105,92,124,112);$g.DrawLine($ink,124,112,206,112);$g.DrawLine($ink,206,112,187,197);$g.DrawLine($ink,187,197,50,197);$g.DrawLine($ink,50,197,45,92);$g.DrawEllipse($ink,88,42,92,92);$g.DrawLine($ink,156,111,196,151)
$g.Dispose();$mint.Dispose();$ink.Dispose();$h=$b.GetHicon();$i=[System.Drawing.Icon]::FromHandle($h);$s=[System.IO.File]::Create($OutputPath);$i.Save($s);$s.Dispose();$i.Dispose();$b.Dispose()
