using System;
using System.IO;
using System.IO.Compression;

namespace SimplePlanes2ModManager.Services
{
    internal static class ZipInstallService
    {
        public static void ExtractZipToDirectorySafely(string zipPath, string targetDirectory)
        {
            ExtractZipToDirectorySafely(zipPath, targetDirectory, new string[0]);
        }

        public static void ExtractZipToDirectorySafely(string zipPath, string targetDirectory, string[] skippedRootFiles)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                throw new FileNotFoundException("Zip file does not exist.");
            }

            if (string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                throw new DirectoryNotFoundException("Target directory does not exist.");
            }

            string fullTargetDirectory = EnsureTrailingSeparator(Path.GetFullPath(targetDirectory));
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                ValidateArchiveEntries(archive, fullTargetDirectory);
                ExtractArchiveEntries(archive, fullTargetDirectory, skippedRootFiles);
            }
        }

        public static void ExtractZipStreamToDirectorySafely(Stream zipStream, string targetDirectory)
        {
            if (zipStream == null)
            {
                throw new ArgumentNullException("zipStream");
            }

            if (string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                throw new DirectoryNotFoundException("Target directory does not exist.");
            }

            string fullTargetDirectory = EnsureTrailingSeparator(Path.GetFullPath(targetDirectory));
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read, false))
            {
                ValidateArchiveEntries(archive, fullTargetDirectory);
                ExtractArchiveEntries(archive, fullTargetDirectory, new string[0]);
            }
        }

        private static void ValidateArchiveEntries(ZipArchive archive, string fullTargetDirectory)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.FullName))
                {
                    continue;
                }

                string normalizedEntryName = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                if (Path.IsPathRooted(normalizedEntryName) || normalizedEntryName.Contains(".." + Path.DirectorySeparatorChar))
                {
                    throw new InvalidOperationException("Zip package contains an unsafe path: " + entry.FullName);
                }

                string destinationPath = Path.GetFullPath(Path.Combine(fullTargetDirectory, normalizedEntryName));
                if (!destinationPath.StartsWith(fullTargetDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Zip package tries to write outside the game directory: " + entry.FullName);
                }
            }
        }

        private static void ExtractArchiveEntries(ZipArchive archive, string fullTargetDirectory, string[] skippedRootFiles)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string normalizedEntryName = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(normalizedEntryName))
                {
                    continue;
                }

                if (ShouldSkipArchiveEntry(normalizedEntryName, skippedRootFiles))
                {
                    continue;
                }

                string destinationPath = Path.GetFullPath(Path.Combine(fullTargetDirectory, normalizedEntryName));
                bool isDirectory = normalizedEntryName.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                                   normalizedEntryName.EndsWith("/", StringComparison.Ordinal);

                if (isDirectory)
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                entry.ExtractToFile(destinationPath, true);
            }
        }

        private static bool ShouldSkipArchiveEntry(string normalizedEntryName, string[] skippedRootFiles)
        {
            if (skippedRootFiles == null || skippedRootFiles.Length == 0)
            {
                return false;
            }

            if (normalizedEntryName.IndexOf(Path.DirectorySeparatorChar) >= 0)
            {
                return false;
            }

            for (int index = 0; index < skippedRootFiles.Length; index++)
            {
                if (string.Equals(normalizedEntryName, skippedRootFiles[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string EnsureTrailingSeparator(string directoryPath)
        {
            if (directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return directoryPath;
            }

            return directoryPath + Path.DirectorySeparatorChar;
        }
    }
}
