using System;
using System.IO;
using System.IO.Compression;

namespace SimplePlanes2ModManager.Services
{
    internal static class ZipInstallService
    {
        public static void ExtractZipToDirectorySafely(string zipPath, string targetDirectory)
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
                ExtractArchiveEntries(archive, fullTargetDirectory);
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

        private static void ExtractArchiveEntries(ZipArchive archive, string fullTargetDirectory)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string normalizedEntryName = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(normalizedEntryName))
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
