# https://github.com/Hubert-Rybak/dotnet-warp
dotnet-warp -l aggressive --verbose --rid win-x64 --output ../Tachyon-Unity/Assets/Modules/Tachyon/Editor/Tachyon-Binder-Win.exe
dotnet-warp -l aggressive --verbose --rid linux-x64 --output ../Tachyon-Unity/Assets/Modules/Tachyon/Editor/Tachyon-Binder-Linux
dotnet-warp -l aggressive --verbose --rid osx-x64 --output ../Tachyon-Unity/Assets/Modules/Tachyon/Editor/Tachyon-Binder-OSX