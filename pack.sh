#!/bin/sh

cd BlueCloud.Extensions

rm -f *.nupkg

dotnet build BlueCloud.Extensions.csproj -c Release

nuget pack BlueCloud.Extensions.nuspec
