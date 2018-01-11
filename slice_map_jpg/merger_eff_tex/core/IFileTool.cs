
    using System.Collections.Generic;

public interface IFileTool {
        bool Handle(string[] handleFiles, string outPath);
        List<string> GetFiles(string dir);
    }
