using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LamarCompiler;
using TachyonClientBinder;
using TachyonCommon;
using TachyonServerBinder;

namespace TachyonBinder
{
    class Program
    {
        static int Main(string[] args)
        {
            
            System.Diagnostics.Debugger.Launch(); 
            
            if (args.Length != 3) {
                var actualArgs = string.Join(' ', args);
                Console.Error.WriteLine(
                    "Invalid argument(s) provided ("+actualArgs+"). \n"+
                    "Expecting: Tachyon-Binder {sourceFile} {outputPath} {--host|--client}.");
                return -1;
            }

            var argStr = args.First();
            var exists = File.Exists(argStr);
            if (exists)
            {
                var file = new FileInfo(argStr);
                var codeStr = File.ReadAllText(file.FullName);

                var assemblyGenerator = new AssemblyGenerator();
                var attr = typeof(GenerateBindingsAttribute).Assembly;
                assemblyGenerator.ReferenceAssemblyContainingType<GenerateBindingsAttribute>();
                var assembly = assemblyGenerator.Generate(codeStr);

                var interfaceTypes = assembly.GetTypes()
                    .Where(t => t.IsInterface);
                foreach (var type in interfaceTypes) {
                    var destDir = args.Length > 1
                        ? new DirectoryInfo(args[1])
                        : file.Directory;
                    
                    if (args[2] == "--host") {
                        var result = GenerateHostBindingCode(type, destDir);
                        if (result != 0)
                            return result;
                    } else if (args[2] == "--client") {
                        var result = GenerateClientBindingCode(type, destDir);
                        if (result != 0)
                            return result;
                    } else {
                        Console.Error.WriteLine("Host or client not specified.");
                        return -1;
                    }
                    
                }
            }
            
            return 0;
        }
        
        private static int GenerateHostBindingCode(Type type, DirectoryInfo destDir) {
            var genericBindWriter = typeof(HostBindingWriter<>)
                .MakeGenericType(type);
            return GenerateBindingCode(
                type, destDir,
                genericBindWriter, 
                "HostBinding.cs"
            );
        }
        
        private static int GenerateClientBindingCode(Type type, DirectoryInfo destDir) {
            var genericBindWriter = typeof(ClientBindingWriter<>)
                .MakeGenericType(type);
            return GenerateBindingCode(
                type, destDir,
                genericBindWriter, 
                "ClientBinding.cs"
            );
        }

        private static int GenerateBindingCode(
            Type type, 
            DirectoryInfo destDir, 
            Type genericBindWriter,
            string bindingAppendage
        ) {
            var bindingFlags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.FlattenHierarchy;

            var generateMethod = genericBindWriter
                .GetMethod("GenerateCode", bindingFlags);

            var generatedCode = (string) generateMethod
                ?.Invoke(null, new object[0]);

            if (destDir == null) return -1;

            var generatedFile = destDir.FullName +
                "/" + type.Name + bindingAppendage;
            File.WriteAllText(generatedFile, generatedCode);

            var generatedFileInfo = new FileInfo(generatedFile);
            if (generatedFileInfo.Exists) {
                //Console.WriteLine("Generated " + generatedFileInfo.FullName);
                return 0;
            }

            return -1;
        }
    }
}