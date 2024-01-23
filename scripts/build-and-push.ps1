$versionInput = Read-Host "Tag (e.g. 1.2.3)"
if($versionInput.Split(".").Length -ne 3) {
    Write-Error "Invalid version format. Expected: 1.2.3"
    exit 1
}

$latesInput = Read-Host "Latest (y/n). Default: n"
$latest = $false
if($latesInput -eq "y") {
    $latest = $true
}
elseif($latesInput -ne "n" -and $latesInput -ne "") {
    Write-Error "Invalid input. Expected: y/n or empty" 
    exit 1
}

$repository = "sietsetro/ip-dns-sync"

Write-Host "Tags: ${repository}:$versionInput $(If ($latest) {`"${repository}:latest`"} Else {''})"
$command = "docker build -t ${repository}:$versionInput $(If ($latest) {`"-t ${repository}:latest`"} Else {''}) ."

$dockerFilePath = Resolve-Path "../src"
Push-Location $dockerFilePath

try {
    Write-Host "Building docker image..."
    iex $command
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Pop-Location
    Write-Host ""
}


Write-Host "Pushing docker image..."
docker push ${repository}:$versionInput
if($latest) {
    docker push ${repository}:latest
}
