using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace Metodo.CommandLine
{
    internal static class Program
    {
        private static Dictionary<string, Action<Repository>> _actions = new Dictionary<string, Action<Repository>>
        {
            ["--authors"] = PrintAuthorsPerFile,
            ["--methods"] = PrintMethods
        };

        private static int Main(string[] args)
        {
            var repositoryPath = args[1];
            Console.WriteLine($"Repository path: {repositoryPath}");
            using (var repo = new Repository(repositoryPath))
            {
                var action = _actions.FirstOrDefault(a => a.Key == args[0]).Value;
                if (action == null)
                {
                    Console.WriteLine("Option not supported");
                    return -1;
                }

                action(repo);
                return 0;
            }
        }

        private static void PrintMethods(Repository repo)
        {
            repo.Commits.Take(repo.Commits.Count() -1 ).ToList().ForEach(commit =>
            {
                Console.WriteLine($"Commit: {commit.Sha}");
                var changes = repo.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);

                changes.ToList().ForEach(c =>
                {
                    var spans = Span.From(c.Patch);
                    spans.ToList().ForEach(s =>
                        Console.WriteLine($"-{s.OldFrom}, {s.NewTo} +{s.NewFrom}, {s.NewTo}"));
                });
            });
        }

        
        private static void PrintAuthorsPerFile(Repository repo)
        {
            var report = GetAuthorsPerFiles(repo);

            foreach (var kv in report.Where(r => r.Value.Count() > 1))
            {
                Console.WriteLine($"File: {kv.Key}");
                kv.Value.ToList().ForEach(v =>
                    Console.WriteLine($"\t\t{v}"));
            }
        }

        private static Dictionary<string, IEnumerable<string>> GetAuthorsPerFiles(Repository repo) => 
            repo.Commits.Aggregate(
                new Dictionary<string, IEnumerable<string>>(),
                (a, c) => DetectAuthors(repo, c, a));

        private static Dictionary<string, IEnumerable<string>> DetectAuthors(Repository repo, Commit c, Dictionary<string, IEnumerable<string>> a)
        {
            var authorsPerFile = GetAuthorsPerFile(repo, c);
            var author = authorsPerFile.Item1;
            var files = authorsPerFile.Item2;
            foreach (var file in files)
            {
                    a.Add(file, new List<string>());
                if (!a.ContainsKey(file))
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