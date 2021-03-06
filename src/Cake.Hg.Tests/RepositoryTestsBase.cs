﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Mercurial;
using NUnit.Framework;

namespace Cake.Hg.Tests
{
    public abstract class RepositoryTestsBase
    {
        protected Repository Repository { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            Repository = CreateTempRepository();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DeleteTempRepository(Repository);
        }

        protected void WriteTextAndCommit(string fileName, string contents, string commitMessage, bool addRemove)
        {
            File.WriteAllText(Path.Combine(Repository.Path, fileName), contents);
            Repository.Commit(
                new CommitCommand
                {
                    Message = commitMessage,
                    AddRemove = addRemove,
                });
        }

        protected string ReadText(string path)
        {
            var fullPath = Path.Combine(Repository.Path, path);
            return File.ReadAllText(fullPath);
        }

        protected string GetResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"Cake.HgVersionTests.Resources.{name}"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private string GetCommitMessage(FileInfo fileInfo, string commitMessage)
        {
            if (!string.IsNullOrEmpty(commitMessage))
                return commitMessage;

            return fileInfo.Exists ? $"change {fileInfo.Name}" : $"create {fileInfo.Name}";
        }

        private static Repository CreateTempRepository()
        {
            var repoPath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid()
                    .ToString()
                    .Replace("-", string.Empty)
                    .ToLowerInvariant());
            
            Directory.CreateDirectory(repoPath);
            return new Repository(repoPath);
        }

        private static void DeleteTempRepository(Repository repository)
        {
            for (int index = 1; index < 5; index++)
            {
                try
                {
                    if (Directory.Exists(repository.Path))
                        Directory.Delete(repository.Path, true);
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("exception while cleaning up repository directory: " 
                                    + ex.GetType().Name + ": " +
                                    ex.Message);
                    
                    Thread.Sleep(1000);
                }
            }
        }
    }
}