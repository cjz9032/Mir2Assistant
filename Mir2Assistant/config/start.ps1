param(
    [string]$Arguments = "CqJHmIicTR62d+9dSOk58cTKgI7HznPbN6DJTvZprSAELnz5oDnMgNt5wBjuFk1qAxqanu//vSGjdVAYZn1QVPj5eBNHqu8r4kAsaof5+vscS8Zg1/EMOLMUwqpHS2YUm5UOhb889exVRhd96hTJFIy2GNszQaAk4ncba891PYNsKBxPjF9PeD+eX8L/GJU6ZIHi/GdjTYUxiBMd20cBkivf1tc3SbFSLabzUQjMLFYEX0f3dTqz8T3pAGRR5eGdIC5UiEbcGJowco1ftRbqqTIKMqyHUk3ie7SiX0uDpRK3DjxXN2dVyAqfucJ6HiftllA5LrVZb77XUr4JOt631s2Ku6zEgZhYDYRH9Kip8qxiRMPXSzzNfaP5gCaNQfxPfePxDlcHectMqT+XV2LzEkJWEEEnF79SyHKT6Uiz99UeHtZ11MDST5NVTwie/bOnFyu2CT7xScxoWT+yyuw3d7tNGAkt1fqlTtXTlc2/BT5ps9phS154s8TsyjcWNDoXWhXPgrknoFSJVhtQbl0qxBUvXFVYE2wDh6D+pe5kBMLOF8PRc82m4PcpGy+vimoRVJCkGamn757CLu4Bg2G4sda4RxQmk2dtPSu17irMsc/Cxmebkv3W76xPKdUeNgFYF+oWJDCkwfj7iFvzolHb1KaSuCHRnOb6O+mvW51BGB5tkxKzrXt2XIcNr1DWwQrkZT3HKQsyY1vILGR0U2xfE+Z9wQCVwwCOsERTh9RxZt2Iht6vKWV0DDn2TWfVevPPl5Du3QU+Y7lp3WmIe+GwLZjueViLnJIL46EmpMKXA1/s+zSTW9s1No5eSoVoRmYN6FH7Wds1xvMSn2JovV3OE7zo+DMLC69xLopScUHBKVeQQOLHVkLvqVepYGdQ0fg3tc17vxphP046OdSzdxxJ1OB8xNtK+dgH9nxaYBPZOCWb7Wutz8kWJaO8lziTLMHD6jfR77lkt2HmO0ENJb36REyfeWaCQ5cnlLnlhSy9MK5HH3P95dreEORsNZnNWBYN",
    [string]$Account = "",
    [string]$Password = "",
    [string]$CharacterName = ""
)

# 日志文件路径
$logPath = ".\logs\start_ps1.log"
if (!(Test-Path -Path (Split-Path $logPath))) {
    New-Item -Path (Split-Path $logPath) -ItemType Directory | Out-Null
}
function Write-Log {
    param([string]$msg)
    $time = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
    Add-Content -Path $logPath -Value "$time $msg"
}

try {
    Write-Log "==== 脚本启动 ===="
    Write-Log "参数: Arguments=$Arguments, Account=$Account, Password=$Password, CharacterName=$CharacterName"

    # 切换到游戏目录
    cd G:\cq\cd
    Write-Log "已切换到目录: G:\cq\cd"

    # 记录账号信息到临时文件，以便后续使用
    if ($Account -ne "") {
        $accountInfo = @{
            Account = $Account
            Password = $Password
            CharacterName = $CharacterName
        } | ConvertTo-Json

        $tempFile = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "mir2_account_$Account.json")
        $accountInfo | Out-File -FilePath $tempFile -Encoding utf8
        Write-Log "已写入账号信息到临时文件: $tempFile"
    }

    # 启动游戏进程，传入参数
    Write-Log "准备启动游戏进程: ./ZC.H $Arguments"
    Start-Process -FilePath "./ZC.H" -ArgumentList $Arguments -NoNewWindow
    Write-Log "已调用Start-Process"
}
catch {
    Write-Log "发生异常: $_"
}