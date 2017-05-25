
using System;
using System.IO;
using System.Windows.Forms;

internal class Data {

    public enum InitResult {
        resSuccess,
        resNoExe,
        resNoToolPath,
        resNoPathConfig
    }

//    public static string pathConfigFileName = "test.conf";

    public string tablePath
    {
        get { return configData.tablePath.Value; }
        set {
            configData.tablePath.Value = value;
            SaveConfig();
        }
    }

    public string tableOutPath {
        get { return configData.tableOutPath.Value; }
        set {
            configData.tableOutPath.Value = value;
            SaveConfig();
        }
    }

    public string filter
    {
        get { return configData.filter.Value; }
        set
        {
            configData.filter.Value = value;
            SaveConfig();
        }
    }

    public int configId;
    public Config config = new Config();

    private Config.Data configData;

    public InitResult Init() {
        config.Init();
        configId = this.config.GetCurrentConfigId();
        configData = config.GetConfig(this.configId);

        return InitResult.resSuccess;
    }

    public string PathFormat(string s) {
        return s.Replace("/", "\\");
    }

    private void SaveConfig() {
        config.SetConfig(this.configId, configData);
    }
}

