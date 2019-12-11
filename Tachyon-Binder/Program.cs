using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LamarCompiler;
using TachyonClientBinder;

namespace TachyonBinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var argStr = args.First();
            var exists = File.Exists(argStr);
            if (exists)
            {
                var file = new FileInfo(argStr); 
                Console.WriteLine("Generating bindings for " + file.Name);
                var codeStr = File.ReadAllText(file.FullName);

                var assemblyGenerator = new AssemblyGenerator();
                var assembly = assemblyGenerator.Generate(codeStr);

                var interfaceTypes = assembly.GetTypes()
                    .Where(t => t.IsInterface);
                foreach (var type in interfaceTypes)
                {
                    var destDir = args.Length > 1
                        ? new DirectoryInfo(args[1])
                        : file.Directory; 
                    GenerateClientBindingCode(type, destDir);
                }
            }

            Console.WriteLine("Tacnyon binding generation complete.");
            Console.ReadKey();
        }

        private static void GenerateClientBindingCode(Type type, DirectoryInfo destDir)
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

            if (destDir == null) return;
            
            var generatedFile = destDir.FullName +
                                "/" + type.Name + "ClientBinding.cs";
            File.WriteAllText(generatedFile, generatedCode);

            var generatedFileInfo = new FileInfo(generatedFile);
            if (generatedFileInfo.Exists)
                Console.WriteLine("Generated " + generatedFileInfo.FullName);
        }
    }
}