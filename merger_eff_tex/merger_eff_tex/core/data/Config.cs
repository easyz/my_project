using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

internal class Config {

    private const string FILE_NAME = "config.xml";

    public class Data {
        public XAttribute name = new XAttribute("name", "主配置");
        public XAttribute toolPath = new XAttribute("toolPath", string.Empty);
        public XAttribute tablePath = new XAttribute("tablePath", string.Empty);
        public XAttribute filter = new XAttribute("filter", string.Empty);
        public XAttribute tableOutPath = new XAttribute("tableOutPath", string.Empty);

        public Data(XElement element) {
            name = element.Attribute("name");
            toolPath = element.Attribute("toolPath");
            tablePath = element.Attribute("tablePath");
            filter = element.Attribute("filter");
            tableOutPath = element.Attribute("tableOutPath") ?? tableOutPath;
        }

        public Data() {
        }

        public object[] ToArray() {
            return new object[] {
                name, toolPath, tablePath, filter, tableOutPath
            };
        }
    }

    public void AddNewConfig(string name, string tablePath) {
        XDocument document = XDocument.Load(FILE_NAME);
        XElement root = document.Root;
        XElement element2 = root.Element("configs");

        Data data = new Data();
        data.name.Value = name;
        data.toolPath.Value = string.Empty;
        data.tablePath.Value = tablePath;
        data.filter.Value = string.Empty;

        element2.Add(new XElement("config", data.ToArray()));
        XElement element3 = root.Element("currentId");
        if (element3 != null) {
            element3.Value = Convert.ToString((int)(element2.Elements("config").Count<XElement>() - 1));
        }
        document.Save(FILE_NAME);
    }

    public void DeleteConfig(int id) {
        XDocument document = XDocument.Load(FILE_NAME);
        XElement root = document.Root;
        IEnumerable<XElement> source = root.Element("configs").Elements("config");
        if (source.Count<XElement>() >= id) {
            source.ElementAt<XElement>(id).Remove();
            XElement element2 = root.Element("currentId");
            if (element2 != null) {
                element2.Value = Convert.ToString((int)(id - 1));
            }
            document.Save(FILE_NAME);
        }
    }

    public Config.Data GetConfig(int configId) {
        IEnumerable<XElement> source = XDocument.Load(FILE_NAME).Root.Element("configs").Elements("config");
        if (source.Count<XElement>() > configId) {
            XElement element = source.ElementAt<XElement>(configId);
            return new Data(element);
        }
        return new Data();
    }

    public string[] GetConfigNames() {
        XElement element = XDocument.Load(FILE_NAME).Root.Element("configs");
        IEnumerable<XElement> source = element.Elements("config");
        if (source.Count<XElement>() == 0) {
            return null;
        }
        string[] strArray = new string[source.Count<XElement>()];
        int index = 0;
        foreach (XElement element2 in element.Elements("config")) {
            strArray[index] = new Data(element2).name.Value;
            index++;
        }
        return strArray;
    }

    public int GetCurrentConfigId() {
        XElement element = XDocument.Load(FILE_NAME).Root.Element("currentId");
        if (element != null) {
            return Convert.ToInt32(element.Value);
        }
        return -1;
    }

    public void Init() {
        if (!File.Exists(FILE_NAME)) {
            object[] content = new object[1];
            object[] objArray2 = new object[2];
            objArray2[0] = new XElement("currentId", new XText("0"));

            Data data = new Data();
            data.name.Value = "主配置";
            data.toolPath.Value = string.Empty;
            data.tablePath.Value = string.Empty;
            data.filter.Value = string.Empty;

            objArray2[1] = new XElement("configs", new XElement("config", data.ToArray()));
            content[0] = new XElement("Config", objArray2);
            new XDocument(content).Save(FILE_NAME);
        }
    }

    public void SetConfig(int configId, Data data) {
        XDocument document = XDocument.Load(FILE_NAME);
        IEnumerable<XElement> source = document.Root.Element("configs").Elements("config");
        if (source.Count<XElement>() >= configId) {
            XElement local1 = source.ElementAt<XElement>(configId);
            local1.ReplaceAll(data.ToArray());
            document.Save(FILE_NAME);
        }
    }

    public void SetCurrentConfigId(int id) {
        XDocument document1 = XDocument.Load(FILE_NAME);
        XElement element = document1.Root.Element("currentId");
        if (element != null) {
            element.Value = Convert.ToString(id);
        }
        document1.Save(FILE_NAME);
    }
}
