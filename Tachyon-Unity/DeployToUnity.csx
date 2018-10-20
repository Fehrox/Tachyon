
void DeployToUnity(){
    var clientIO = "Tachyon-Client-IO";
    var clientRPC  = "Tachyon-Client-RPC";
    var clientCommon = "Tachyon-Common";
    var clientCommonHash = "Tachyon-Common/Hashing";

    var destinationDrive = "Tachyon-Unity/Assets/Tachyon";

    var solutions = new string[] { clientIO, clientRPC, clientCommon, clientCommonHash };
    foreach (var solution in solutions)
    {
        var solutionDir = new DirectoryInfo(solution);
        foreach(var filePath in solutionDir.GetFiles("*.cs")){  
            var destDir = new DirectoryInfo(destinationDrive);
            Console.WriteLine("Copied " + filePath.FullName + " to " + destDir.FullName + filePath.Name);
            File.Copy(filePath.FullName, destDir.FullName + "/" + filePath.Name, true);
        }
    }
}

DeployToUnity();