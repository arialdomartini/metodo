using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var allButTheFirstCommit = repo.Commits.Take(repo.Commits.Count() -1);
            
            allButTheFirstCommit.ToList().ForEach(
                commit => { PrintAffectedLines(repo, commit); });
        }

        private static void PrintAffectedLines(Repository repo, Commit commit)
        {
            Console.WriteLine($"Commit: {commit.Sha}");
            var changes = repo.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);

            var changedFiles = repo.Diff.Compare<TreeChanges>(commit.Parents.First().Tree, commit.Tree);
       
            changes.ToList().ForEach(change =>
            {
                var treeEntryChanges = changedFiles.First(c => c.Path == change.Path);

                if (treeEntryChanges.Path.EndsWith(".cs"))
                {
                    var blob = repo.Lookup<Blob>(treeEntryChanges.Oid);
                    if (blob != null)
                    {
                        Console.WriteLine($"\t\t{treeEntryChanges.Path}");
                        var file = blob.GetContentText();
                        //                    Console.WriteLine(file);
                        var spans = Span.From(change.Patch);
                        spans.ToList().ForEach(s =>
                            Console.WriteLine($"\t\t\t-{s.OldFrom}, {s.NewTo} +{s.NewFrom}, {s.NewTo}"));
                    }
                }
            });
        }

        public static void PrintMethodsInfo(List<string> args)
        {
            var filePath = args.First();
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            var root = tree.GetCompilationUnitRoot();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var m in methods)
            {
                var d = new Dictionary<string, string>
                {
                    {"method", m.Identifier.Text},
                    {"body start", m.GetLocation().GetLineSpan().StartLinePosition.Line.ToString()},
                    {"body end", m.GetLocation().GetLineSpan().EndLinePosition.Line.ToString()}
                };

                var sb = new StringBuilder();
                foreach (var kv in d)
                {
                    sb.Append($"{kv.Key}: {kv.Value}        ");
                }

                Console.WriteLine(sb.ToString());
            }

            Console.ReadLine();
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