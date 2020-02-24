# https://github.com/Hubert-Rybak/dotnet-warp
dotnet-warp -l aggressive --verbose --rid linux-x64 --output ../UnityPackages/TachyonCommon-UnityPackage/Tachyon-Binder-Linux
dotnet-warp -l aggressive --verbose --rid osx-x64 --output ../UnityPackages/TachyonCommon-UnityPackage/Tachyon-Binder-OSX
dotnet-warp -l aggressive --verbose --rid win-x64 --output ../UnityPackages/TachyonCommon-UnityPackage/Tachyon-Binder-Win.exe
