using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LamarCompiler;
using TachyonClientBinder;
using TachyonCommon;

namespace TachyonBinder
{
    class Program
    {
        static int Main(string[] args)
        {
            //Console.WriteLine(string.Join(", ", args));
            
            if (args.Length != 2) {
                Console.Error.WriteLine(
                    "Invalid argument(s) provided. \n"+
                    "Expecting: Tachyon-Binder {sourceFile} {outputPath}.");
                return -1;
            }

            var argStr = args.First();
            var exists = File.Exists(argStr);
            if (exists)
            {
                var file = new FileInfo(argStr); 
//                Console.WriteLine("Generating bindings for " + file.FullName);
                var codeStr = File.ReadAllText(file.FullName);

                var assemblyGenerator = new AssemblyGenerator();
                assemblyGenerator.ReferenceAssemblyContainingType<InteropAttribute>();
                var assembly = assemblyGenerator.Generate(codeStr);

                var interfaceTypes = assembly.GetTypes()
                    .Where(t => t.IsInterface);
                foreach (var type in interfaceTypes)
                {
//                    Console.WriteLine(type.FullName);
                    var destDir = args.Length > 1
                        ? new DirectoryInfo(args[1])
                        : file.Directory; 
                    var result = GenerateClientBindingCode(type, destDir);
                    if (result != 0)
                        return result;
                }
            }

//            Console.WriteLine("Tachyon binding generation complete.");
            return 0;
        }

        private static int GenerateClientBindingCode(Type type, DirectoryInfo destDir)
        {
            var genericBindWriter = typeof(ClientBindingWriter<>)
                .MakeGenericType(type);

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
                                "/" + type.Name + "ClientBinding.cs";
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