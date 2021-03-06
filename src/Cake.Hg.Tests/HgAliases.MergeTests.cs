﻿using System.IO;
using Cake.Hg.Tests.Fakes;
using NUnit.Framework;
using Mercurial;
using System.Linq;

namespace Cake.Hg.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Self)]
    public class HgMergeTests : RepositoryTestsBase
    {
        [Test]
        public void HgMerge_ShouldMergeWithCurrentBranch()
        {
            var path = Repository.Path;
            var context = new FakeCakeContext();
            
            context.HgInit(path);
            var repository = context.Hg(Repository.Path);
            
            //default branch
            File.WriteAllText(path + "/dummy.txt", "123");
            context.HgCommit(path, "Initial commit");
            var firstCommit = context.HgTip(path);

            File.WriteAllText(path + "/dummy.txt", "213");
            context.HgCommit(path, "Dummy commit");
            var defaultCommit = context.HgTip(path);

            repository.Update(firstCommit.Hash);

            //dev branch
            repository.Branch("dev");
            File.WriteAllText(path + "/next.txt", "111");
            context.HgCommit(path, "dev commit");
            var devCommit = context.HgTip(path);

            //back to default
            repository.Update("default");

            //merge
            var result = context.HgMerge(path, "dev");

            //check if merge was ok
            var mergeCommit = context.HgTip(path);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(MergeResult.Success));
                Assert.That(mergeCommit.LeftParentHash, Is.EqualTo(defaultCommit.Hash));
                Assert.That(mergeCommit.RightParentHash, Is.EqualTo(devCommit.Hash));
            });

        }

        [Test]
        public void HgMerge_ShouldMergeNonCurrentBranches()
        {
            var path = Repository.Path;
            var context = new FakeCakeContext();

            context.HgInit(path);
            var repository = context.Hg(Repository.Path);

            //default branch
            File.WriteAllText(path + "/dummy.txt", "123");
            context.HgCommit(path, "Initial commit");
            var firstCommit = context.HgTip(path);

            //other branch
            repository.Branch("other");
            File.WriteAllText(path + "/dummy.txt", "213");
            context.HgCommit(path, "Dummy commit");
            var otherCommit = context.HgTip(path);

            repository.Update(firstCommit.Hash);

            //dev branch
            repository.Branch("dev");
            File.WriteAllText(path + "/next.txt", "111");
            context.HgCommit(path, "dev commit");
            var devCommit = context.HgTip(path);

            //back to default
            repository.Update("default");

            //merge
            var result = context.HgMerge(path, "dev", "other");

            //check if merge was ok
            var mergeCommit = context.HgTip(path);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(MergeResult.Success));
                Assert.That(mergeCommit.Branch, Is.EqualTo("other"));
                Assert.That(mergeCommit.LeftParentHash, Is.EqualTo(otherCommit.Hash));
                Assert.That(mergeCommit.RightParentHash, Is.EqualTo(devCommit.Hash));
            });

        }

        [Test]
        public void HgMerge_ShouldFailIfMergeConflict()
        {
            var path = Repository.Path;
            var context = new FakeCakeContext();
            
            context.HgInit(path);
            var repository = context.Hg(Repository.Path);
            
            //default branch
            File.WriteAllText(path + "/dummy.txt", "123");
            context.HgCommit(path, "Initial commit");
            var firstCommit = context.HgTip(path);

            File.WriteAllText(path + "/dummy.txt", "213");
            context.HgCommit(path, "Dummy commit");
            var defaultCommit = context.HgTip(path);

            repository.Update(firstCommit.Hash);

            //dev branch
            repository.Branch("dev");
            File.WriteAllText(path + "/dummy.txt", "111");
            context.HgCommit(path, "dev commit");
            var devCommit = context.HgTip(path);

            //back to default
            repository.Update("default");

            //merge
            var result = context.HgMerge(path, "dev");

            //check if merge was not performed
            var currentCommit = repository.Log(RevSpec.ByBranch("default").Max).First();
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(MergeResult.UnresolvedFiles));
                Assert.That(repository.Status().Where(s => s.State == FileState.Modified).Count(), 
                    Is.EqualTo(0));
                Assert.That(currentCommit.Hash, Is.EqualTo(defaultCommit.Hash));
            });

        }

        [Test]
        public void HgMerge_ShouldFailIfAlreadyMerged()
        {
            var path = Repository.Path;
            var context = new FakeCakeContext();

            context.HgInit(path);
            var repository = context.Hg(Repository.Path);

            //default branch
            File.WriteAllText(path + "/dummy.txt", "123");
            context.HgCommit(path, "Initial commit");
            var firstCommit = context.HgTip(path);

            File.WriteAllText(path + "/dummy.txt", "213");
            context.HgCommit(path, "Dummy commit");
            var defaultCommit = context.HgTip(path);

            repository.Update(firstCommit.Hash);

            //dev branch
            repository.Branch("dev");
            File.WriteAllText(path + "/next.txt", "111");
            context.HgCommit(path, "dev commit");
            var devCommit = context.HgTip(path);

            //back to default
            repository.Update("default");

            //merge
            var result = context.HgMerge(path, "dev");

            //check if first merge was ok
            var mergeCommit = context.HgTip(path);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(MergeResult.Success));
                Assert.That(mergeCommit.LeftParentHash, Is.EqualTo(defaultCommit.Hash));
                Assert.That(mergeCommit.RightParentHash, Is.EqualTo(devCommit.Hash));
            });

            result = context.HgMerge(path, "dev");
            //check if second merge was not performed
            var tip = context.HgTip(path);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(MergeResult.UnresolvedFiles));
                Assert.That(tip.Hash, Is.EqualTo(mergeCommit.Hash));
            });

        }
    }
}