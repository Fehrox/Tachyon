﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TachyonCommon;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class TachyonDesignTimeBinder
{
    private static bool _bound = false;
    static TachyonDesignTimeBinder()
    {
        
        if (!_bound) {
            CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
            _bound = true;
        }
        
    }

    private static void OnAssemblyCompilationStarted(string builtAssembly)
    {
        var fileInfo = new FileInfo(builtAssembly);
        if(!fileInfo.Exists) return;

        var assembly = Assembly.LoadFile(fileInfo.FullName);
        var interfaceTypes = assembly.GetTypes()
            .Where(t => t.IsInterface);
        foreach (var foundType in interfaceTypes) {
            var isInterop = foundType
                .GetCustomAttributes(false)
                .Any(a => a is InteropAttribute);
            if (isInterop)
            {
                var interopInterfaceFile =
                    FindScriptForInterface(foundType);
                UpdateTachyonBindings(interopInterfaceFile);
            }
        }
    }
    
    private static FileInfo FindScriptForInterface(Type interfaceType)
    {
        var scriptFiles = Directory.GetFiles(
            "Assets/",
            "*.cs", 
            SearchOption.AllDirectories
        );

        foreach (var script in scriptFiles) {
            
            var code = File.ReadAllText(script);
            var hasInterop =  code.Contains("[Interop]");
            var hasInterface = Regex.IsMatch(
                code, 
                "interface\\s*" + interfaceType.Name
            );

            if(hasInterop && hasInterface)
                return new FileInfo(script);
        }

        return null;
    }

    private static void UpdateTachyonBindings(FileInfo interopInterfaceFile)
    {
        var binder = FindBinder();
        if (binder == null) {
            Debug.LogError("Tachyon-Binder missing.");
        } else {
            var bindingsDir = new DirectoryInfo(
                binder.Directory?.Parent + "/Bindings/");
            ProcessStart(
                binder,
                interopInterfaceFile, 
                bindingsDir
            );
        }
    }

    private static void ProcessStart(FileInfo executableFile, FileInfo interfaceFile, DirectoryInfo outputDirectory)
    {
        var execFile = executableFile.FullName;
        ProcessStartInfo startInfo;
        
        try {
            startInfo = new ProcessStartInfo(execFile);
        } catch {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                execFile = execFile.Replace("&", "^&");
                startInfo = new ProcessStartInfo("cmd", $"/c start {execFile}") { CreateNoWindow = true };
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                startInfo = new ProcessStartInfo("xdg-open", execFile);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                startInfo = new ProcessStartInfo("open", execFile);
            } else {
                throw;
            }
        }

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;

        // Interface file.
        startInfo.Arguments += interfaceFile.FullName;
        startInfo.Arguments += " ";
        
        // Generated Biding folder.
        if (!Directory.Exists(outputDirectory.FullName))
            Directory.CreateDirectory(outputDirectory.FullName);
        startInfo.Arguments += outputDirectory.FullName; 
        
        // Run binding generation process.
        var process = Process.Start(startInfo);
        
        // Output
        var stdout = process.StandardOutput.ReadToEnd();
        if(!string.IsNullOrEmpty(stdout))
            Debug.Log(stdout);
        var stdErr = process.StandardError.ReadToEnd();
        if(!string.IsNullOrEmpty(stdErr))
            Debug.LogError(stdErr);

    }

    private static FileInfo FindBinder()
    {
        var currentDirectory = new FileInfo(".");
        var files = Directory.GetFiles(
            currentDirectory.FullName,
            "Tachyon-Binder",
            SearchOption.AllDirectories);
    
        if (!files.Any())
            return null;
    
        var tachyonBinder = files.Single();
        return new FileInfo(tachyonBinder);
    }

}