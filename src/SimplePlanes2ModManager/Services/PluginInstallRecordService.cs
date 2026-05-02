using SimplePlanes2ModManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace SimplePlanes2ModManager.Services
{
    internal sealed class PluginInstallRecordService
    {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly string _recordsPath;

        public PluginInstallRecordService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string recordsDirectory = Path.Combine(appData, "SimplePlanes2ModManager");
            Directory.CreateDirectory(recordsDirectory);
            _recordsPath = Path.Combine(recordsDirectory, "installed-plugins.json");
        }

        public PluginInstallRecord[] LoadRecords()
        {
            if (!File.Exists(_recordsPath))
            {
                return new PluginInstallRecord[0];
            }

            try
            {
                PluginInstallRecord[] records = _serializer.Deserialize<PluginInstallRecord[]>(File.ReadAllText(_recordsPath));
                return records ?? new PluginInstallRecord[0];
            }
            catch
            {
                return new PluginInstallRecord[0];
            }
        }

        public void SaveRecord(PluginInstallRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.id))
            {
                throw new ArgumentException("Plugin install record is invalid.", "record");
            }

            List<PluginInstallRecord> records = new List<PluginInstallRecord>(LoadRecords());
            bool replacedRecord = false;
            for (int index = 0; index < records.Count; index++)
            {
                if (IsSamePluginRecord(records[index], record))
                {
                    records[index] = record;
                    replacedRecord = true;
                    break;
                }
            }

            if (!replacedRecord)
            {
                records.Add(record);
            }

            SaveRecords(records.ToArray());
        }

        public void SaveRecords(PluginInstallRecord[] records)
        {
            if (records == null)
            {
                records = new PluginInstallRecord[0];
            }

            File.WriteAllText(_recordsPath, _serializer.Serialize(records));
        }

        public PluginInstallRecord FindRecordForPluginDirectory(string pluginDirectoryName)
        {
            if (string.IsNullOrEmpty(pluginDirectoryName))
            {
                return null;
            }

            PluginInstallRecord[] records = LoadRecords();
            for (int index = 0; index < records.Length; index++)
            {
                PluginInstallRecord record = records[index];
                if (record == null)
                {
                    continue;
                }

                string recordDirectoryName = Path.GetFileName(NormalizePackagePath(record.pluginDirectory).TrimEnd('/'));
                if (string.Equals(recordDirectoryName, pluginDirectoryName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(record.id, pluginDirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    return record;
                }
            }

            return null;
        }

        private static bool IsSamePluginRecord(PluginInstallRecord existingRecord, PluginInstallRecord newRecord)
        {
            if (existingRecord == null)
            {
                return false;
            }

            if (string.Equals(existingRecord.id, newRecord.id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrEmpty(existingRecord.pluginDirectory) &&
                   !string.IsNullOrEmpty(newRecord.pluginDirectory) &&
                   string.Equals(
                       NormalizePackagePath(existingRecord.pluginDirectory).TrimEnd('/'),
                       NormalizePackagePath(newRecord.pluginDirectory).TrimEnd('/'),
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePackagePath(string relativePath)
        {
            return (relativePath ?? string.Empty).Replace('\\', '/');
        }
    }
}
