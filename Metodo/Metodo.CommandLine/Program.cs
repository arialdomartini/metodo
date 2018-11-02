using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Metodo.CommandLine
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var repositoryPath = args[0];
            Console.WriteLine($"Repository path: {repositoryPath}");
            using (var repo = new Repository(repositoryPath))
            {
                var report = AuthorsPerFiles(repo);

                foreach (var kv in report.Where(r => r.Value.Count() > 1))
                {
                    Console.WriteLine($"File: {kv.Key}");
                    kv.Value.ToList().ForEach(v => 
                        Console.WriteLine($"\t\t{v}"));
                }
            }
        }

        private static Dictionary<string, IEnumerable<string>> AuthorsPerFiles(Repository repo) => 
            repo.Commits.Aggregate(
                new Dictionary<string, IEnumerable<string>>(),
                (a, c) => Count(repo, c, a));

        private static Dictionary<string, IEnumerable<string>> Count(Repository repo, Commit c, Dictionary<string, IEnumerable<string>> a)
        {
            var authorsPerFile = GetAuthorsPerFile(repo, c);
            var author = authorsPerFile.Item1;
            var files = authorsPerFile.Item2;
            foreach (var file in files)
            {
                if (!a.ContainsKey(file))
                    a.Add(file, new List<string>());
                a[file] = a[file].Append(author).Distinct();
            }

            return a;
        }

        private static (string, IEnumerable<string>) GetAuthorsPerFile(Repository repo, Commit commit)
        {
            var parent = commit.Parents.FirstOrDefault();

            // FIXME this skips a commit, which is wrong
            if (parent == null)
                return ("none", new List<string> {"none"});
            var treeChanges = repo.Diff.Compare<TreeChanges>(
                parent.Tree, 
                commit.Tree);

            var paths = treeChanges.Select(tc => tc.Path);

            return (commit.Author.Name,  paths);
        }
    }
}