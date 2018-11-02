using System;
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
                PrintLog(repo);
            }
        }

        private static void PrintLog(Repository repo)
        {
            foreach (var c in repo.Commits.Take(15))
            {
                Console.WriteLine($"commit {c.Id} Author: {c.Author.Name} <{c.Author.Email}>");
            }
        }
    }
}