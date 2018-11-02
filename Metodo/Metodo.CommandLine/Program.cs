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
                var totalCommits = repo.Commits.Count();
                var count = 0;
                var report = repo.Commits.AsParallel().Aggregate(
                    new Dictionary<string, IEnumerable<string>>(),
                    (a, c) =>
                    {
                        Console.WriteLine($"{totalCommits}/{count++}");
                        var r = GetAuthorsPerFile(repo, c);
                        var author = r.Item1;
                        var files = r.Item2;
                        if(!a.ContainsKey(author))
                            a.Add(author, new List<string>());
                        a[author] = a[author].Concat(files).Distinct();
                        return a;
                    });


                foreach (var kv in report)
                {
                    Console.WriteLine($"Author: {kv.Key}");
                    kv.Value.ToList().ForEach(v => 
                        Console.WriteLine($"  {v}"));
                }
            }
        }

        private static (string, IEnumerable<string>) GetAuthorsPerFile(Repository repo, Commit commit)
        {
            var parent = commit.Parents.FirstOrDefault();
            if (parent == null)
                return ("none", new List<string> {"none"});
            var treeChanges = repo.Diff.Compare<TreeChanges>(
                parent.Tree, 
                commit.Tree);

            var paths = treeChanges.Select(tc => tc.Path);

            return (commit.Author.Name,  paths);
        }

        public static void Bo(Repository repo)
        {
            repo.Branches.ToList().ForEach(b =>
            {
                Console.WriteLine($"branch: {b.FriendlyName}, commits: {b.Commits.Count()}");
            });

            foreach (var c in repo.Commits.Take(15))
            {
                Console.WriteLine($"commit {c.Id} Author: {c.Author.Name} <{c.Author.Email}>");
            }
        }
    }
}